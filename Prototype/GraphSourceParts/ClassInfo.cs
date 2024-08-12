namespace Prototype.Main.GraphSourceParts
{
    public class ClassInfo
    {
        public string Name { get; private set; }
        public int Frequency { get; private set; }
        public ClassInfo(string name)
        {
            Name = name;
            Frequency = 1;
        }

        public void IncreaseFrequency()
        {
            Frequency++;
        }

        public override bool Equals(object? obj)
        {
            return obj is ClassInfo item &&
                   Name == item.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
