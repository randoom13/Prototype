using System.Windows;

namespace Prototype.Main.ViewModels
{
    internal class MainWindowViewModel : BindableBase
    {
        private const int MaxDropDownItemsCount = 3;
        private const string SelectFolderDesc = "....";

        private readonly INavigator _navigator;
        private string _dropDownContent = "Select folder";

        public DelegateCommand<object> ViewLoaded { get; }

        public Caliburn.Micro.BindableCollection<string> DropDownItems { get; }

        public DelegateCommand<object> SelectFolderCommand { get; }

        [Dependency]
        public ISelectedFoldersStorage? SelectedFoldersStorage { get; set; }

        [Dependency]
        public IMessageDialogService? MessageDialogService { get; set; }

        [Dependency]
        public IUIHelper? UIHelper { get; set; }

        public MainWindowViewModel(INavigator navigator, IEventAggregator eventAggregator,
                                    IApplicationCommand applicationCommand)
        {
            _navigator = navigator;
            SelectFolderCommand = applicationCommand.SelectFolderCommand;
            eventAggregator.GetEvent<SelectedFolderEvent>().Subscribe(OnSelectedFolder);
            ViewLoaded = new DelegateCommand<object>(OnViewLoaded);
            DropDownItems = new Caliburn.Micro.BindableCollection<string>() { SelectFolderDesc };
        }

        private void OnSelectedFolder(string selectedPath)
        {
            DropDownItems.Remove(selectedPath);
            DropDownItems.Insert(0, selectedPath);
            // SelectFolderDesc should always go first
            DropDownItems.Remove(SelectFolderDesc);
            DropDownItems.Insert(0, SelectFolderDesc);
            while (1 + MaxDropDownItemsCount < DropDownItems.Count)
                DropDownItems.RemoveAt(DropDownItems.Count - 1);

            SelectedFoldersStorage?.Save(DropDownItems.Where(it => it != SelectFolderDesc));
        }

        public string DropDownContent 
        {
            get => _dropDownContent;
            set
            {
                _dropDownContent = value;
                RaisePropertyChanged(nameof(DropDownContent));
            }
        }

		private async void OnViewLoaded(object obj)
		{
			_navigator.NavigateFrom(this);
            UIHelper?.Initialize((FrameworkElement)obj);
            MessageDialogService?.Initialize((Window)obj);
            if (SelectedFoldersStorage != null)
                DropDownItems.AddRange(await SelectedFoldersStorage!.LoadFoldersAsync());
        }
    }
}
