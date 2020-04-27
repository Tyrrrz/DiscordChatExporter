using System;
using System.Net.Http;
using DiscordChatExporter.Domain.Discord.Models;

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
            const string message = "Not found.";
            return new DiscordChatExporterException(message);
        }

        internal static DiscordChatExporterException ChannelEmpty(string channel)
        {
            var message = $"Channel '{channel}' contains no messages for the specified period.";
            return new DiscordChatExporterException(message);
        }

        internal static DiscordChatExporterException ChannelEmpty(Channel channel) =>
            ChannelEmpty(channel.Name);
    }
}