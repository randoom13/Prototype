using System.Diagnostics;

namespace Prototype.Main.ViewModels
{
    [DebuggerDisplay("Id = {Description} {Info}")]
    public class ClassViewModel : BindableBase, INodeViewModel
    {
        public ClassViewModel(string description, int info)
        {
            Description = description;
            Info = info;
        }

        public int Info { get; private set; }
        public bool CanExpand => false;
        public bool IsExpanded { get; set; } = false;
        public string Description { get; private set; }

        public bool IsMatched(string text, bool withChild) => Description.Contains(text);

        public IEnumerable<INodeViewModel> ToFlatList()
        {
            yield return this;
        }
    }
}
