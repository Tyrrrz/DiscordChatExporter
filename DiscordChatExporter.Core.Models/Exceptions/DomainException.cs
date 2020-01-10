using System;

namespace DiscordChatExporter.Core.Models.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message)
            : base(message)
        {
        }
    }
}