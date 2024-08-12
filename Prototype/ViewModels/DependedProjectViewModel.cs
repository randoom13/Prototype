using System.Collections.ObjectModel;
using System.Diagnostics;
namespace Prototype.Main.ViewModels
{
    [DebuggerDisplay("Id = {Description}  {Info}")]
    public class DependedProjectViewModel : BindableBase, INodeViewModel
    {
        public DependedProjectViewModel(string description)
        {
            Description = description;
            Info = "";
        }

        public string Description { get; private set; }
        public string Info { get; set; }

        public bool CanExpand => Children.Any();

        private bool _isExpanded = false;
        public bool IsExpanded 
        { 
            get => _isExpanded;
            set 
            {
                _isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
             }
        }

        public void AddRange(IEnumerable<INodeViewModel> items, string text) 
        {
            if (string.IsNullOrEmpty(text) || IsMatched(text, false))
            {
                Children.AddRange(items);
            } else
            {
                Children.AddRange(items.Where(it => it.IsMatched(text, false)));
            }
        }


        public IEnumerable<INodeViewModel> ToFlatList()
        {
            yield return this;
            if (CanExpand && IsExpanded)
            {
                foreach (var item in Children)
                {
                    yield return item;
                }
            }
        }

        public  bool IsMatched(string text, bool withChild)
        {
            return Description.Contains(text) || (withChild && Children.Any(it => it.IsMatched(text, false)));
        }

        private ObservableCollection<INodeViewModel> _children = new ObservableCollection<INodeViewModel>();

        public ObservableCollection<INodeViewModel> Children => _children;

    }
}
