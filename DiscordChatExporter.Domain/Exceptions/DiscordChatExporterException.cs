using System;
using System.Net.Http;

namespace DiscordChatExporter.Domain.Exceptions
{
    public partial class DiscordChatExporterException : Exception
    {
        public bool IsCritical { get; }

        public DiscordChatExporterException(string message, bool isCritical = false)
            : base(message)
        {
            IsCritical = isCritical;
        }
    }

    public partial class DiscordChatExporterException
    {
        internal static DiscordChatExporterException FailedHttpRequest(HttpResponseMessage response)
        {
            var message = $@"
Failed to perform an HTTP request.

{response.RequestMessage}

{response}";

            return new DiscordChatExporterException(message.Trim(), true);
        }

        internal static DiscordChatExporterException Unauthorized()
        {
            const string message = "Authentication token is invalid.";
            return new DiscordChatExporterException(message);
        }

        internal static DiscordChatExporterException Forbidden()
        {
            const string message = "Access is forbidden.";
            return new DiscordChatExporterException(message);
        }

        internal static DiscordChatExporterException NotFound()
        {
            const string message = "Requested resource does not exist.";
            return new DiscordChatExporterException(message);
        }

        internal static DiscordChatExporterException ChannelIsEmpty()
        {
            var message = $"No messages for the specified period.";
            return new DiscordChatExporterException(message);
        }
    }
}