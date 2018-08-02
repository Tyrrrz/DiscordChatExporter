using PowerArgs;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        public static void Main(string[] args) => Args.InvokeAction<Verbs>(args);
    }
}