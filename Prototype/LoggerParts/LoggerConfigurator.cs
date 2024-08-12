using log4net;

namespace Prototype.Main.LoggerParts
{
    internal static class LoggerConfigurator
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(LoggerConfigurator));
        public static bool IsConfigured { get; private set; } = false;

        public static void Config() 
        {
            log4net.Config.XmlConfigurator.Configure();
            logger.Info("        =============  Started Logging  =============        ");
            IsConfigured = true;
        }
    }
}
