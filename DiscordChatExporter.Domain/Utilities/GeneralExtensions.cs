using System;

namespace DiscordChatExporter.Domain.Utilities
{
    public static class GeneralExtensions
    {
        public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> transform) => transform(input);
    }
}