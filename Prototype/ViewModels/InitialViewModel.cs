namespace Prototype.Main.ViewModels
{
    internal class InitialViewModel : BaseNavigateViewModel
    {
        public DelegateCommand<object> SelectFolderCommand { get; }

        public InitialViewModel(IApplicationCommand applicationCommand)
        {
            if (applicationCommand == null) 
                throw new ArgumentException(nameof(applicationCommand));

            SelectFolderCommand = applicationCommand.SelectFolderCommand;
        }
    }
}
