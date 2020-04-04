using System.Windows.Navigation;
using DiscordChatExporter.Gui.Internal;

namespace DiscordChatExporter.Gui.Views
{
    public partial class RootView
    {
        public RootView()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessEx.StartShellExecute(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}