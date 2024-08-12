namespace Prototype.Main.GraphSourceParts
{
    internal static class PathUtility
    {
        static internal string NormalizePath(string path) => path.Replace(@"\\", @"\");

        static internal bool isParentDirectory(string parentDirectory, string subDirectory)
        {
            if (!subDirectory.Contains(parentDirectory)) return false;

            string[] folders1 = subDirectory.Split('\\');
            string[] folders2 = parentDirectory.Split('\\');

            for (int i = 0; i < folders2.Count(); i++)
            {
                if (folders2[i] != folders1[i])
                    return false;
            }
            return true;
        }
    }
}
