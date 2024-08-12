using System.Diagnostics;

namespace Prototype.Main.GraphSourceParts
{
    [DebuggerDisplay("Id = {Name}  {Path}")]
    public readonly struct DependencyItem
    {
        public readonly string Name { get; }
        public readonly string Path { get; }

        public DependencyItem(string name, string directory)
        {
            Name = name;
            Path = directory;
        }

        public override bool Equals(object? obj)
        {
            return obj is DependencyItem item &&
                   Name == item.Name &&
                   Path == item.Path;
        }

        public static bool operator ==(DependencyItem? lhs, DependencyItem? rhs)
        {
            if (lhs is null)
            {
                // Only the left side is null.
                return rhs is null;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(DependencyItem? lhs, DependencyItem? rhs) => !(lhs == rhs);

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name);
        }

        public override string ToString()
        {
            return Path != "" ? System.IO.Path.Combine(Path, Name) : Name;
        }
    }
}
