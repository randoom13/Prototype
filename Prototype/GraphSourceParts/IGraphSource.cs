namespace Prototype.Main.GraphSourceParts
{ 
    public class GraphSourceProgressInfo
    {
        public int FilesCount { get; private set; }
        public int CurrentFileNumber { get; private set; }
        public string Description { get; private set; }
        public CancellationToken Token { get; set; }

        public GraphSourceProgressInfo(int filesCount, int currentFileNumber = default, string? description = default)
        {
            FilesCount = filesCount;
            CurrentFileNumber = currentFileNumber;
            Description = description ?? "";
        }
    }

    internal interface IGraphSource
    {
        Task<GraphInfos> BuildAsync(string folderPath, 
            IProgress<GraphSourceProgressInfo>? progressProvider, CancellationToken token);
    }
}
