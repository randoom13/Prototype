namespace Prototype.Main
{
    internal interface ISelectedFoldersStorage
    {
        Task<IEnumerable<string>> LoadFoldersAsync();
        void Save(IEnumerable<string> folders);
    }
}
