using System.Diagnostics;

namespace Prototype.Main.ViewModels
{
    [DebuggerDisplay("Id = {Description} {Info}")]
    public class ProjectViewModel : BindableBase
    {
        private readonly Caliburn.Micro.BindableCollection<INodeViewModel> _projects = new Caliburn.Micro.BindableCollection<INodeViewModel>();
        private readonly Caliburn.Micro.BindableCollection<INodeViewModel> _flatList = new Caliburn.Micro.BindableCollection<INodeViewModel>();
      
        public bool IsMatched(string text)
        {
            return Description.Contains(text);
        }

        private bool _showLines = false;
        public bool ShowLines
        {
            get => _showLines;
            set
            {
                _showLines = value;
                RaisePropertyChanged(nameof(ShowLines));
            }
        }

        private bool _canShowLines = true;
        public bool CanShowLines
        {
            get => _canShowLines;
            set
            {
                _canShowLines = value;
                RaisePropertyChanged(nameof(CanShowLines));
            }
        }

        public ProjectViewModel(string description) 
        {
            Description = description;
            ExpandedCommand = new DelegateCommand<object>(OnExpanded);
        }

        public Caliburn.Micro.BindableCollection<INodeViewModel> FlatList => _flatList;

        public string Description { get; private set; }

        public string Info { get; set; } = string.Empty;

        public void RefreshFlatList(IEnumerable<INodeViewModel> items)
        {
            _projects.AddRange(items);
            RefreshFlatList();
        }

        private void OnExpanded(object arg) => RefreshFlatList();

        private void RefreshFlatList() 
        {
            FlatList.IsNotifying = false;
            FlatList.Clear();
            FlatList.AddRange(_projects.SelectMany(it => it.ToFlatList()));
            FlatList.IsNotifying = true;
            FlatList.Refresh();
        }

        public DelegateCommand<object> ExpandedCommand { get; }
    }
}
