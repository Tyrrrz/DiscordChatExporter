using System;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Verbs
{
    public class UpdateAppVerb : Verb<UpdateAppOptions>
    {
        public UpdateAppVerb(UpdateAppOptions options) 
            : base(options)
        {
        }

        public override async Task ExecuteAsync()
        {
            // Get update service
            var container = new Container();
            var updateService = container.Resolve<IUpdateService>();

            // TODO: this is configured only for GUI
            // Get update version
            var updateVersion = await updateService.CheckPrepareUpdateAsync();

            if (updateVersion != null)
            {
                Console.WriteLine($"Updating to version {updateVersion}");

                updateService.NeedRestart = false;
                updateService.FinalizeUpdate();
            }
            else
            {
                Console.WriteLine("There are no application updates available.");
            }
        }
    }
}