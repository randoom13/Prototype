using Prototype.Main.LoggerParts;
using System.IO;
using System.Text.Json;
namespace Prototype.Main
{
    internal class SelectedFoldersStorage : ISelectedFoldersStorage
    {
        private readonly ICustomLogger _logger;
        private readonly string _directory;
        private const string FILE_NAME = "Folders.json";

        public SelectedFoldersStorage(ICustomLogger logger)
        {
            _logger = logger;
            _directory = Path.Combine(System.Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData), "Prototype");
        }

        public async Task<IEnumerable<string>> LoadFoldersAsync()
        {
            IEnumerable<string> result = Enumerable.Empty<string>();
            try
            {
                var text = await File.ReadAllTextAsync(Path.Combine(_directory, FILE_NAME)) ?? string.Empty;
                result = JsonSerializer.Deserialize<IEnumerable<string>>(text)?
                    .Where(Directory.Exists) ?? Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load folders", ex);
            }
            return result;
        }

        public void Save(IEnumerable<string> folders)
        {
            Task.Run(async () =>
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                try
                {
                    if (!Directory.Exists(_directory))
                        Directory.CreateDirectory(_directory);

                    string jsonString = JsonSerializer.Serialize(folders, options);
                    await File.WriteAllTextAsync(Path.Combine(_directory, FILE_NAME), jsonString);
                }
                catch (Exception ex) 
                {
                    _logger.Error("Failed to save folders", ex);
                }
            });
        }
    }
}
