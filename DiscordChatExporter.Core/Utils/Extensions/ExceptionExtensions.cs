using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class ExceptionExtensions
{
    private static void PopulateChildren(this Exception exception, ICollection<Exception> children)
    {
        if (exception is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                children.Add(innerException);
                PopulateChildren(innerException, children);
            }
        }
        else if (exception.InnerException is not null)
        {
            children.Add(exception.InnerException);
            PopulateChildren(exception.InnerException, children);
        }
    }

    public static IReadOnlyList<Exception> GetSelfAndChildren(this Exception exception)
    {
        var children = new List<Exception> {exception};
        PopulateChildren(exception, children);
        return children;
    }

    public static HttpStatusCode? TryGetStatusCode(this HttpRequestException ex) =>
        // This is extremely frail, but there's no other way
        Regex
            .Match(ex.Message, @": (\d+) \(")
            .Groups[1]
            .Value
            .NullIfWhiteSpace()?
            .Pipe(s => (HttpStatusCode) int.Parse(s, CultureInfo.InvariantCulture));
}