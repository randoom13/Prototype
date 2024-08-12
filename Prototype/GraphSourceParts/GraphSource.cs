using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;

namespace Prototype.Main.GraphSourceParts
{
    internal partial class GraphSource : IGraphSource
    {
        private static GraphInfos BuildResult(Graphs graph)
        {
            var result = new Dictionary<string, Dictionary<string, HashSet<ClassInfo>>>();
            var dependentDirs = new HashSet<string>();
            foreach (var pair in graph.Dependencies)
            {
                var mainPath = pair.Key.Path;
                var projPath = graph.ProjDirs.FirstOrDefault(it => PathUtility.isParentDirectory(it, mainPath));
                if (projPath == null)
                {
                    System.Diagnostics.Debug.WriteLine($"{pair} was skipped on level 1!");
                    continue;
                }
                Dictionary<string, HashSet<ClassInfo>>? stats;
                if (!result.TryGetValue(projPath, out stats))
                {
                    stats = new Dictionary<string, HashSet<ClassInfo>>();
                }
                foreach (var reference in pair.Value)
                {
                    String? referenceDir = null;
                    if (!graph.DirectoryByNameSpaces.TryGetValue(reference.Path, out referenceDir))
                    {
                        System.Diagnostics.Debug.WriteLine($"{reference} was skipped on level 2!");
                        continue;
                    }
                    var refProjPath = graph.ProjDirs.FirstOrDefault(it => PathUtility.isParentDirectory(it, referenceDir));
                    if (refProjPath == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"{reference} was skipped on level 3!");
                        continue;
                    }
                    dependentDirs.Add(refProjPath);
                    HashSet<ClassInfo>? files;
                    if (stats.TryGetValue(refProjPath, out files))
                    {
                        var classInfo = files.FirstOrDefault(it => it.Name == reference.Name);
                        if (classInfo != null)
                        {
                            classInfo.IncreaseFrequency();
                        }
                        else
                        {
                            files.Add(new ClassInfo(reference.Name));
                        }
                    }
                    else
                        stats[refProjPath] = new HashSet<ClassInfo>() { new ClassInfo(reference.Name) };
                }
                if (stats.Any())
                    result[projPath] = stats;
            }
            foreach (string notMentionedProj in dependentDirs.Except(result.Keys))
            {
                result[notMentionedProj] = new Dictionary<string, HashSet<ClassInfo>>();
            }
            return new GraphInfos(result, graph.TargetDirecory);
        }

        public async Task<GraphInfos> BuildAsync(string folderPath, IProgress<GraphSourceProgressInfo>? progressProvider, CancellationToken token)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var graph = await AnalyzeFolderAsync(folderPath, progressProvider, token);
            stopWatch.Stop();
            System.Diagnostics.Debug.WriteLine($"{stopWatch.ElapsedMilliseconds} - Analyze");
            var result = BuildResult(graph);
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            // class to fix the issue with writting dictionary
            options.Converters.Add(new JsonNonStringKeyDictionaryConverterFactory());
            string jsonString = JsonSerializer.Serialize(graph, options);

            // TODO : Write the JSON to a file for debug reason and it should be deleted
            File.WriteAllText("graph0.json", jsonString);
            jsonString = JsonSerializer.Serialize(graph, options);
            File.WriteAllText("graph.json", jsonString);
            return result;
        }

        private static string CalculateNameSpace(ClassDeclarationSyntax? classSyntax)
        {
            var classNamespaces = classSyntax?.Ancestors()?.OfType<NamespaceDeclarationSyntax>()?
         .Select(it => it.Name.ToFullString().Trim())?.ToList() ?? Enumerable.Empty<string>();
            return classNamespaces.Any() ? string.Join(".", classNamespaces.Reverse()) : "";
        }

        // improved version with DataFlow
        private static async Task<Graphs> AnalyzeFolderAsync2(string folderPath, IProgress<GraphSourceProgressInfo>? progressProvider, CancellationToken token)
        {
            var projFiles = Directory.GetFiles(folderPath, "*.csproj", SearchOption.AllDirectories);
            if (!projFiles.Any())
            {
                throw new GraphSourceException("There are no project files!");
            }
            var projDirs = projFiles.Select(Path.GetDirectoryName).Where(it => it != null).OfType<string>().ToList() ?? Enumerable.Empty<string>();
            if (!projDirs.Any())
            {
                throw new GraphSourceException("Could not calculate directories for project files!");
            }
            var files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
            if (!projFiles.Any())
            {
                throw new GraphSourceException("There are no *cs. files!");
            }
            token.ThrowIfCancellationRequested();
            progressProvider?.Report(new GraphSourceProgressInfo(files.Length) { Token = token });

            int currentFilesCount = 0;
            var progressBlock = new ActionBlock<string>(n =>
            {
                while (true)
                {
                    var oldCount = currentFilesCount;
                    var newCount = oldCount + 1;
                    if (Interlocked.CompareExchange(ref currentFilesCount, newCount, oldCount) == oldCount)
                    {
                        if (newCount > 800)
                        {
                        }
                        progressProvider?.Report(new GraphSourceProgressInfo(files.Length, newCount, n) { Token = token });
                        break;
                    }
                }
            });
            var dirByNameSpace = new ConcurrentDictionary<string, string>();
            var dirBlock = new ActionBlock<KeyValuePair<string, string>>(n =>
            {
                dirByNameSpace.TryAdd(n.Key, n.Value);
            });
            var opt = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount / 2, 1),
                EnsureOrdered = false
            };
            var collector = new TransformBlock<string, IEnumerable<KeyValuePair<DependencyItem, DependencyItem>>>(file =>
            {

                var ui = new List<KeyValuePair<DependencyItem, DependencyItem>>();
                token.ThrowIfCancellationRequested();
                //progressProvider?.Report(new GraphSourceProgressInfo(files.Length, index, file) { Token = token });
                progressBlock.Post(file);
                var code = File.ReadAllText(file);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var root = syntaxTree.GetRoot() as CompilationUnitSyntax;
                token.ThrowIfCancellationRequested();
                var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString().Replace("-", "_"))
                    .AddSyntaxTrees(syntaxTree)
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classes = root!.DescendantNodes().OfType<ClassDeclarationSyntax>();
                var dir = Path.GetDirectoryName(file) ?? "";
                foreach (var classSyntax in classes)
                {
                    token.ThrowIfCancellationRequested();
                    var className = classSyntax.Identifier.Text;
                    var currentNameSpace = CalculateNameSpace(classSyntax);
                    if (!string.IsNullOrEmpty(currentNameSpace) && !string.IsNullOrEmpty(dir))
                    {
                        dirBlock.Post(new KeyValuePair<string, string>(currentNameSpace, dir));

                    }
                    DependencyItem fileItem = new DependencyItem(className, dir);
                    var dependencies = FindClassDependencies(classSyntax!, semanticModel);
                    foreach (var dependency in dependencies)
                    {
                        token.ThrowIfCancellationRequested();
                        // ignore link on itself
                        if (dependency.Name != className || dependency.Path != currentNameSpace)
                            ui.Add(new KeyValuePair<DependencyItem, DependencyItem>(fileItem, dependency));
                    }
                }
                return ui;

            }, opt);
            var aggregationResults = new ConcurrentDictionary<DependencyItem, HashSet<DependencyItem>>();

            var aggregationBlock = new TransformManyBlock<IEnumerable<KeyValuePair<DependencyItem, DependencyItem>>,
                KeyValuePair<DependencyItem, HashSet<DependencyItem>>>(lines =>
            {
                // Process each line in the CSV file
                foreach (var line in lines)
                {
                    aggregationResults.AddOrUpdate(
                        line.Key,
                        (_) => new HashSet<DependencyItem>() { line.Value },
                        (_, existing) =>
                        {
                            existing.Add(line.Value);
                            return existing;
                        });
                }

                return aggregationResults;
            }, opt);
            var printBlock = new ActionBlock<KeyValuePair<DependencyItem, HashSet<DependencyItem>>>(result =>
            {
                Console.WriteLine($"Name: {result.Key}, Total amount: {result.Value.Count}");
            }, opt);
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            collector.LinkTo(aggregationBlock, linkOptions);
            aggregationBlock.LinkTo(printBlock, linkOptions);
            foreach (var file in files)
            {
                await collector.SendAsync(file);
            }
            collector.Complete();
            await Task.WhenAll(collector.Completion, aggregationBlock.Completion, printBlock.Completion);

            return new Graphs(aggregationResults.ToDictionary(it => it.Key, it => it.Value), projDirs)
            {
                TargetDirecory = folderPath,
                DirectoryByNameSpaces = dirByNameSpace.ToDictionary(it => it.Key, it => it.Value)
            };
        }

        private static async Task<Graphs> AnalyzeFolderAsync(string folderPath, IProgress<GraphSourceProgressInfo>? progressProvider, CancellationToken token)
        {
            //      var stringBuilder = new StringBuilder();
            var projFiles = Directory.GetFiles(folderPath, "*.csproj", SearchOption.AllDirectories);
            if (!projFiles.Any())
            {
                throw new GraphSourceException("There are no project files!");
            }
            var projDirs = projFiles.Select(Path.GetDirectoryName).Where(it => it != null).OfType<string>().ToList() ?? Enumerable.Empty<string>();
            if (!projDirs.Any())
            {
                throw new GraphSourceException("Could not calculate directories for project files!");
            }
            var files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
            if (!projFiles.Any())
            {
                throw new GraphSourceException("There are no *cs. files!");
            }
            token.ThrowIfCancellationRequested();
            progressProvider?.Report(new GraphSourceProgressInfo(files.Length) { Token = token });
            var classDependencies = new Dictionary<DependencyItem, HashSet<DependencyItem>>();
            Dictionary<string, string> directoryByNameSpaces = new Dictionary<string, string>();
            for (int index = 0; index < files.Length; index++)
            {
                var file = files.ElementAt(index);
                token.ThrowIfCancellationRequested();
                progressProvider?.Report(new GraphSourceProgressInfo(files.Length, index, file) { Token = token });
                var code = await File.ReadAllTextAsync(file, token);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var root = syntaxTree.GetRoot() as CompilationUnitSyntax;
                token.ThrowIfCancellationRequested();
                var compilation = CSharpCompilation.Create("temp")
                    .AddSyntaxTrees(syntaxTree)
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classes = root!.DescendantNodes().OfType<ClassDeclarationSyntax>();
                var dir = Path.GetDirectoryName(file) ?? "";
                // String? currentNameSpace = null;
                foreach (var classSyntax in classes)
                {
                    token.ThrowIfCancellationRequested();
                    var className = classSyntax.Identifier.Text;
                    string currentNameSpace = CalculateNameSpace(classSyntax);
                    if (!string.IsNullOrEmpty(currentNameSpace) && !string.IsNullOrEmpty(dir))
                    {
                        System.Diagnostics.Debug.WriteLine($"!!!!!! {className} {dir}");
                        directoryByNameSpaces[currentNameSpace] = dir;
                    }

                    DependencyItem fileItem = new DependencyItem(className, dir);
                    HashSet<DependencyItem>? result;
                    if (!classDependencies.TryGetValue(fileItem, out result))
                    {
                        result = new HashSet<DependencyItem>();
                        classDependencies[fileItem] = result!;
                    }
                    var dependencies = FindClassDependencies(classSyntax!, semanticModel);
                    foreach (var dependency in dependencies)
                    {
                        token.ThrowIfCancellationRequested();
                        // ignore link on itself
                        if (dependency.Name != className || dependency.Path != currentNameSpace)
                        {
                            //                stringBuilder.AppendLine($"{fileItem.Path} {fileItem.Name} {dependency.Path} {dependency.Name}");
                            result.Add(dependency);
                        }
                    }
                    //       if (!result.Any())
                    //      {
                    //         classDependencies.Remove(fileItem);
                    //     }
                }

            }
            //   File.WriteAllText("D:\\Work\\stat.txt", stringBuilder.ToString());
            return new Graphs(classDependencies, projDirs)
            {
                TargetDirecory = folderPath,
                DirectoryByNameSpaces = directoryByNameSpaces
            };
        }

        private static IEnumerable<DependencyItem> FindClassDependencies(ClassDeclarationSyntax classSyntax,
            SemanticModel semanticModel)
        {
            var dependencies = new HashSet<DependencyItem>();
            var root = classSyntax.SyntaxTree.GetRoot();
            var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (var identifier in identifiers)
            {
                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
                if (symbol is ITypeSymbol typeSymbol && typeSymbol.Kind == SymbolKind.NamedType)
                {
                    // Filter out types from native libraries
                    if (!IsFromNativeLibrary(typeSymbol))
                    {
                        // It is imposible to get path to dependent files 
                        dependencies.Add(new DependencyItem(typeSymbol.Name, typeSymbol.ContainingNamespace.ToString() ?? ""));
                    }
                }
            }

            return dependencies;
        }

        private static bool IsFromNativeLibrary(ITypeSymbol typeSymbol)
        {
            var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
            return IsFromNativeLibrary(namespaceName) ||
                   typeSymbol.ContainingAssembly.Identity.ToString().StartsWith("System") ||
                   typeSymbol.ContainingAssembly.Identity.ToString().StartsWith("Microsoft");
        }

        private static bool IsFromNativeLibrary(string className)
        {
            // Use a list of namespaces or assemblies that represent native libraries
            var nativeNamespaces = new[] { "System", "Microsoft" };
            return nativeNamespaces.Any(ns => className.StartsWith(ns));
        }

        private static Dictionary<string, HashSet<string>> FilterDependencies(Dictionary<string, HashSet<string>> dependencies)
        {
            // Filter out native classes and their dependencies
            var filtered = new Dictionary<string, HashSet<string>>();

            var relevantClasses = dependencies.Keys.Where(c => !IsFromNativeLibrary(c)).ToHashSet();

            foreach (var className in relevantClasses)
            {
                var filteredDependencies = dependencies[className]
                    .Where(dep => relevantClasses.Contains(dep))
                    .ToHashSet();

                if (filteredDependencies.Count > 0)
                {
                    filtered[className] = filteredDependencies;
                }
            }

            return filtered;
        }
    }
}
