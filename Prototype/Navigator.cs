using Prototype.Main.GraphSourceParts;
using Prototype.Main.ViewModels;

namespace Prototype.Main
{
    // InitialViewModel => LoaderViewModel => StatisticsViewModel
    // BottomBar => => LoaderViewModel 
    public class Navigator : INavigator
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;

        internal Navigator(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
        }

        public void NavigateFrom(object source, object? data = default) 
        {
            if (source is MainWindowViewModel)
            {
                Navigate<InitialViewModel>();
            }
            else if (source is IApplicationCommand && data is string)
            {
                Navigate<LoaderViewModel>();
                _eventAggregator.GetEvent<SelectedFolderEvent>().Publish((string)data);
            }
            else if (source is LoaderViewModel && data is GraphInfos)
            {
                Navigate<StatisticsViewModel>();
                _eventAggregator.GetEvent<BuildStatisticEvent>().Publish((GraphInfos)data!);
            }
        }

        private void Navigate<TViewModel>() where TViewModel : BindableBase
        {
            var viewName = typeof(TViewModel).Name.Replace("ViewModel", "View");
            _regionManager.RequestNavigate("ContentRegion", viewName);
        }
    }

}
