using DiscordChatExporter.Messages;
using GalaSoft.MvvmLight;

namespace DiscordChatExporter.ViewModels
{
    public class ErrorViewModel : ViewModelBase, IErrorViewModel
    {
        public string Message { get; private set; }

        public ErrorViewModel()
        {
            MessengerInstance.Register<ShowErrorMessage>(this, m => Message = m.Message);
        }
    }
}