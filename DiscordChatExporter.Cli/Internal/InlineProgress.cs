using System;
using CliFx.Services;

namespace DiscordChatExporter.Cli.Internal
{
    internal class InlineProgress : IProgress<double>, IDisposable
    {
        private readonly IConsole _console;

        private string _lastOutput = "";
        private bool _isCompleted;

        public InlineProgress(IConsole console)
        {
            _console = console;
        }

        private void ResetCursorPosition()
        {
            foreach (var c in _lastOutput)
                _console.Output.Write('\b');
        }

        public void Report(double progress)
        {
            // If output is not redirected - reset cursor position and write progress
            if (!_console.IsOutputRedirected)
            {
                ResetCursorPosition();
                _console.Output.Write(_lastOutput = $"{progress:P1}");
            }
        }

        public void ReportCompletion() => _isCompleted = true;

        public void Dispose()
        {
            // If output is not redirected - reset cursor position
            if (!_console.IsOutputRedirected)
            {
                ResetCursorPosition();
            }

            // Inform about completion
            _console.Output.WriteLine(_isCompleted ? "Completed ✓" : "Failed X");
        }
    }
}