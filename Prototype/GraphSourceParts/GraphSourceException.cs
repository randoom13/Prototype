namespace Prototype.Main.GraphSourceParts
{
    public class GraphSourceException : InvalidOperationException
    {
        public GraphSourceException() : base() { }

        public GraphSourceException(string? message) : base(message) { }
    }
}
