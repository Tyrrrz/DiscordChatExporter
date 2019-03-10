using System;

namespace DiscordChatExporter.Cli.Internal
{
    internal class InlineProgress : IProgress<double>, IDisposable
    {
        private readonly int _posX;
        private readonly int _posY;

        public InlineProgress()
        {
            // If output is not redirected - save initial cursor position
            if (!Console.IsOutputRedirected)
            {
                _posX = Console.CursorLeft;
                _posY = Console.CursorTop;
            }
        }

        public void Report(double progress)
        {
            // If output is not redirected - reset cursor position and write progress
            if (!Console.IsOutputRedirected)
            {
                Console.SetCursorPosition(_posX, _posY);
                Console.WriteLine($"{progress:P1}");
            }
        }

        public void Dispose()
        {
            // If output is not redirected - reset cursor position
            if (!Console.IsOutputRedirected)
                Console.SetCursorPosition(_posX, _posY);

            // Inform about completion
            Console.WriteLine("Completed ✓");
        }
    }
}