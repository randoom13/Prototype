
using Prototype.Main.GraphSourceParts;
using Prototype.Main.LoggerParts;
namespace Prototype.Main.ViewModels
{
    internal class LoaderViewModel : BaseNavigateViewModel, IProgress<GraphSourceProgressInfo>
    {
        private readonly IGraphSource _graphSource;
        private readonly INavigator _navigator;
        private CancellationTokenSource? _analyzerCts;

        [Dependency]
        public ICustomLogger? Logger { get; set; }

        [Dependency]
        public IMessageDialogService? MessageDialogService { get; set; }

        public LoaderViewModel(IGraphSource source, IEventAggregator eventAggregator, INavigator navigator)
        {
            _graphSource = source;
            eventAggregator.GetEvent<SelectedFolderEvent>().Subscribe(OnSelectedFolder);
            _navigator = navigator;
            Progress = new ProgressViewModel();
        }

        private void OnSelectedFolder(string selectedPath)
        {
            var old = _analyzerCts;
            old?.Cancel();
            _analyzerCts = new CancellationTokenSource();
            Scan(selectedPath, _analyzerCts.Token);
            old?.Dispose();
        }

        public void Scan(string folderPath, CancellationToken token)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentOutOfRangeException(nameof(folderPath));

            TaskScheduler taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Run(async () => await _graphSource.BuildAsync(folderPath,
                   this, token)).ContinueWith(async result =>
                  await ScanCompletedAsync(folderPath, result, token), taskScheduler);
        }

        private async Task ScanCompletedAsync(string folderPath, Task<GraphInfos> task, CancellationToken token)
        {
            try
            {
                var graphInfos = await task;
                _navigator.NavigateFrom(this, graphInfos);
            }
            catch (Exception ex)
            {
                Logger!.Error($"Failed to analyze infomation from {folderPath}", ex);
                if (!token.IsCancellationRequested)
                    Progress.Reset();

                if (ex is OperationCanceledException)
                    return;

                var message = ex is GraphSourceException ? ex.Message :
                    $"Could not analyze information from {folderPath}";
                Progress.Description = message;
                message = ex is GraphSourceException ? ex.Message :
                $"Could not analyze information from {folderPath}. Details:{ex.Message}";
                await MessageDialogService!.ShowMessageAsync("Prototype", message);
            }
        }

        public ProgressViewModel Progress { get; private set; }

        public void Report(GraphSourceProgressInfo value)
        {
            if (value.Token.IsCancellationRequested)
                return;

          //  System.Diagnostics.Debug.WriteLine($"{value.CurrentFileNumber}/{value.FilesCount} ({value.Description})");
            Progress.Current = value.CurrentFileNumber;
            Progress.Maximum = value.FilesCount;
            Progress.Description = value.Description;
        }
    }
}
