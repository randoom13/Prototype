using log4net;
using System.Diagnostics;

namespace Prototype.Main.LoggerParts
{
    internal class CustomLogger : ICustomLogger
    {
        private ILog? _logger = null;

        private static bool VerifyInitialized()
        {
            if (!LoggerConfigurator.IsConfigured)
                LoggerConfigurator.Config();

            return LoggerConfigurator.IsConfigured;
        }

        private ILog? GetLogger()
        {
            if (_logger == null && VerifyInitialized())
            {
                StackTrace stack = new StackTrace(1);
                Type type = stack.GetFrames().Select(it => it?.GetMethod()?.ReflectedType).
                    Where(it => it != null).FirstOrDefault(it => !ReferenceEquals(typeof(CustomLogger), it))
                 ?? typeof(CustomLogger);

                _logger = LogManager.GetLogger(type);
            }
            return _logger;
        }

        public void Debug(object? message)
        {
            GetLogger()?.Debug(message);
        }

        public void Warn(object? message)
        {
            GetLogger()?.Warn(message);
        }

        public void Warn(object? message, Exception? exception)
        {
            GetLogger()?.Warn(message, exception);
        }

        public void Error(object? message)
        {
            GetLogger()?.Error(message);
        }

        public void Error(object? message, Exception? exception)
        {
            GetLogger()?.Error(message, exception);
        }
    }
}
