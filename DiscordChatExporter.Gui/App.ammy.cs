using System.Windows;

namespace DiscordChatExporter.Gui
{
    public partial class App
    {
        private Container Container => (Container) Resources["Container"];

        private void App_Startup(object sender, StartupEventArgs e)
        {
            Container.Init();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Container.Cleanup();
        }
    }
}