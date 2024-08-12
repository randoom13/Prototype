namespace Prototype.Main
{
    internal interface IApplicationCommand
    {
        DelegateCommand<object> SelectFolderCommand { get; }
    }
}
