namespace Prototype.Main.LoggerParts
{
    internal interface ICustomLogger
    {
        void Debug(object? message);
        void Warn(object? message);
        void Warn(object? message, Exception? exception);
        void Error(object? message);
        void Error(object? message, Exception? exception);
    }
}
