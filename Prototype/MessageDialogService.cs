using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace Prototype.Main
{
    public class MessageDialogService : IMessageDialogService
    {
        private MetroWindow? _window;

        public bool IsInitialized => _window != null;
        public void Initialize(Window window) 
        {
            if (window == null)
                throw new ArgumentException($"{nameof(window)} should be MetroWindow type");

            _window = window as MetroWindow;
        }
        
        public async Task<MessageDialogResult> ShowMessageAsync(string title, string Message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null)
        {
            if (!IsInitialized) 
                throw new InvalidOperationException("Сlass was not initialized.");

            return await _window!.ShowMessageAsync(title, Message, style, settings);
        }
    }
}
