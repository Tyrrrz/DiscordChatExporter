using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DiscordChatExporter.Domain.Discord
{
    internal class UrlBuilder
    {
        private string _path = "";

        private readonly Dictionary<string, string?> _queryParameters =
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        public UrlBuilder SetPath(string path)
        {
            _path = path;
            return this;
        }

        public UrlBuilder SetQueryParameter(string key, string? value)
        {
            var keyEncoded = WebUtility.UrlEncode(key);
            var valueEncoded = WebUtility.UrlEncode(value);
            _queryParameters[keyEncoded] = valueEncoded;

            return this;
        }

        public UrlBuilder SetQueryParameterIfNotNullOrWhiteSpace(string key, string? value) =>
            !string.IsNullOrWhiteSpace(value)
                ? SetQueryParameter(key, value)
                : this;

        public string Build()
        {
            var buffer = new StringBuilder();

            buffer.Append(_path);

            if (_queryParameters.Any())
                buffer.Append('?').AppendJoin('&', _queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            return buffer.ToString();
        }
    }
}