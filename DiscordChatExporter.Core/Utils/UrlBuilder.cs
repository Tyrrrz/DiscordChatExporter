using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DiscordChatExporter.Core.Utils;

public class UrlBuilder
{
    private string _path = "";

    private readonly Dictionary<string, string?> _queryParameters =
        new(StringComparer.OrdinalIgnoreCase);

    public UrlBuilder SetPath(string path)
    {
        _path = path;
        return this;
    }

    public UrlBuilder SetQueryParameter(string key, string? value, bool ignoreUnsetValue = true)
    {
        if (ignoreUnsetValue && string.IsNullOrWhiteSpace(value))
            return this;

        var keyEncoded = WebUtility.UrlEncode(key);
        var valueEncoded = WebUtility.UrlEncode(value);
        _queryParameters[keyEncoded] = valueEncoded;

        return this;
    }

    public string Build()
    {
        var buffer = new StringBuilder();

        buffer.Append(_path);

        if (_queryParameters.Any())
            buffer
                .Append('?')
                .AppendJoin('&', _queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));

        return buffer.ToString();
    }
}
