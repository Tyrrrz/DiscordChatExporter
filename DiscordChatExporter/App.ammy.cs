using System;
using AmmySidekick;

namespace DiscordChatExporter
{
    public partial class App
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.InitializeComponent();

            RuntimeUpdateHandler.Register(app, "/" + Ammy.GetAssemblyName(app) + ";component/App.g.xaml");

            app.Run();
        }
    }
}