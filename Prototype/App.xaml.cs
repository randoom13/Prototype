using Prototype.Main.GraphSourceParts;
using Prototype.Main.LoggerParts;
using Prototype.Main.Views;
using System.Text;
using System.Windows;

namespace Prototype.Main
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private ICustomLogger? _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            LoggerConfigurator.Config();
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogUnandledException(e.Exception);
        }

        private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            LogUnandledException(e.ExceptionObject as Exception);
        }

        private void LogUnandledException(object? ex)
        {
            StringBuilder message = new StringBuilder();
            var info = ex?.GetType()?.ToString() ?? "unknown";
            message.AppendFormat("Unhandled exception of type {0} in ocurred", info);
            message.AppendLine();
            message.AppendFormat("Operating system: {0}, ProcessorCount: {1}", Environment.OSVersion.ToString(), Environment.ProcessorCount);
            var exception = ex as Exception;
            if (exception != null)
            {
                if (exception.StackTrace != null)
                    message.AppendLine(exception.StackTrace!.TrimStart(' '));
                message.AppendFormat("\tError message: {0}", exception.Message);
                _logger?.Error(message.ToString(), exception);
            }
            else 
            {
                message.AppendLine(ex?.ToString() ?? "NONE");
                _logger?.Error(message.ToString());
            }
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IGraphSource, GraphSource>();
            containerRegistry.RegisterSingleton<IMessageDialogService, MessageDialogService>();
            containerRegistry.RegisterSingleton<IUIHelper, UIHelper>();
            containerRegistry.Register<INavigator, Navigator>();
            containerRegistry.Register<ICustomLogger, CustomLogger>();
            containerRegistry.Register<ISelectedFoldersStorage, SelectedFoldersStorage>();
            containerRegistry.Register<IApplicationCommand, ApplicationCommand>();
            containerRegistry.RegisterForNavigation<InitialView>();
            containerRegistry.RegisterForNavigation<LoaderView>();
            containerRegistry.RegisterForNavigation<StatisticsView>();
            _logger = Container.Resolve<ICustomLogger>();
        }
    }

}
