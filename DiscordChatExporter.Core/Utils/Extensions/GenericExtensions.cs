using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class GenericExtensions
{
    public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> transform) =>
        transform(input);

    public static T? NullIf<T>(this T value, Func<T, bool> predicate)
        where T : struct => !predicate(value) ? value : null;

    public static T? NullIfDefault<T>(this T value)
        where T : struct => value.NullIf(v => EqualityComparer<T>.Default.Equals(v, default));
}
