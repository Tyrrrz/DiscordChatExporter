﻿using System;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class GenericExtensions
    {
        public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> transform) => transform(input);

        public static T? NullIf<T>(this T value, Func<T, bool> predicate) where T : struct =>
            !predicate(value)
                ? value
                : (T?) null;
    }
}