using System.Windows;

namespace DiscordChatExporter
{
    public partial class App
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            Locator.Init();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Locator.Cleanup();
        }
    }
}