using System.IO;

namespace Prototype.Main
{
    internal class ApplicationCommand : IApplicationCommand
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigator _navigator;

        public DelegateCommand<object> SelectFolderCommand { get; }

        internal ApplicationCommand(IEventAggregator eventAggregator, INavigator navigator)
        {
            _navigator = navigator;
            _eventAggregator = eventAggregator;
           SelectFolderCommand = new DelegateCommand<object>(OnSelectFolder);
        }

        private void OnSelectFolder(object arg)
        {
            string? selectedPath = arg as string;
            if (string.IsNullOrEmpty(selectedPath) || selectedPath.All(it => it == '.')
                || !Directory.Exists(selectedPath))
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                selectedPath = "";
                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    selectedPath = dialog.SelectedPath ?? "";
                }
            }
            if (!string.IsNullOrEmpty(selectedPath))
            {
                _navigator.NavigateFrom(this, selectedPath);
            }
        }
    }
}
