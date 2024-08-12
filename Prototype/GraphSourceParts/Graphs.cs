namespace Prototype.Main.GraphSourceParts
{
    internal class Graphs
    {
        public Dictionary<DependencyItem, HashSet<DependencyItem>> Dependencies { get; }

        public IEnumerable<string> ProjDirs { get; }

        public Dictionary<string, Dictionary<string, HashSet<ClassInfo>>> ProjectsDependencies { get; set; } = new Dictionary<string, Dictionary<string, HashSet<ClassInfo>>>();

        public string TargetDirecory { get; set; } = "";

        public Dictionary<string, string> DirectoryByNameSpaces { get; set; } = new Dictionary<string, string>();

        public Graphs(Dictionary<DependencyItem, HashSet<DependencyItem>> dependencies, IEnumerable<string> projDirs)
        {
            Dependencies = dependencies;
            ProjDirs = projDirs;
        }
    }
}
