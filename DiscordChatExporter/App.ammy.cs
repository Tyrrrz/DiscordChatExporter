using System.Windows;

namespace DiscordChatExporter
{
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Locator.Init();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Locator.Cleanup();
        }
    }
}