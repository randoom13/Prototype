namespace Prototype.Main.GraphSourceParts
{
    public class GraphInfos
    {
        public Dictionary<string, Dictionary<string, HashSet<ClassInfo>>> ProjectsDependencies { get; private set; }

        public string TargetDirecory { get; private set; }

        public GraphInfos(Dictionary<string, Dictionary<string, HashSet<ClassInfo>>> projectsDependencies, string targetDirecory)
        {
            TargetDirecory = targetDirecory;
            ProjectsDependencies = projectsDependencies;
        }
    }
}
