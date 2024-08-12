namespace Prototype.Main.ViewModels
{
    public interface INodeViewModel
    {
        bool CanExpand { get; }
        bool IsExpanded { get; }

        string Description { get; }     
        bool IsMatched(string text, bool withChild);

        IEnumerable<INodeViewModel> ToFlatList();
    }
}
