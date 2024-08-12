using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace Prototype.Main
{
    //https://stackoverflow.com/questions/41121566/implement-mahapps-messagebox-with-ok-and-cancel
    internal interface IMessageDialogService
    {
        bool IsInitialized { get; }
        public void Initialize(Window window);
        Task<MessageDialogResult> ShowMessageAsync(string title, string Message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null);
    }
}
