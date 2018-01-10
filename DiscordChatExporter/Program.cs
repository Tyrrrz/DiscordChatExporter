using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using AmmySidekick;

namespace DiscordChatExporter
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // GUI
            if (!args.Any())
            {
                var app = new App();
                app.InitializeComponent();
                RuntimeUpdateHandler.Register(app, $"/{Ammy.GetAssemblyName(app)};component/App.g.xaml");
                app.Run();
            }
            // CLI
            else
            {
                var cli = new Cli();
                cli.Run(args);
            }
        }

        public static string GetResourceString(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
                throw new MissingManifestResourceException($"Could not find resource [{resourcePath}].");

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}