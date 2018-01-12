using DiscordChatExporter.Gui.Messages;
using GalaSoft.MvvmLight;

namespace DiscordChatExporter.Gui.ViewModels
{
    public class ErrorViewModel : ViewModelBase, IErrorViewModel
    {
        public string Message { get; private set; }

        public ErrorViewModel()
        {
            // Messages
            MessengerInstance.Register<ShowErrorMessage>(this, m =>
            {
                Message = m.Message;
            });
        }
    }
}