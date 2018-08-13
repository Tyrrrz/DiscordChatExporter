using System.Threading.Tasks;

namespace DiscordChatExporter.Cli.Verbs
{
    public abstract class Verb<TOptions>
    {
        protected TOptions Options { get; }

        protected Verb(TOptions options)
        {
            Options = options;
        }

        public abstract Task ExecuteAsync();

        public virtual void Execute() => ExecuteAsync().GetAwaiter().GetResult();
    }
}