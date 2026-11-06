using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Discord.Data;

public record ChannelConnection(Channel Channel, IReadOnlyList<ChannelConnection> Children)
{
    public static IReadOnlyList<ChannelConnection> BuildTree(IReadOnlyList<Channel> channels)
    {
        IReadOnlyList<ChannelConnection> GetChildren(Channel parent) =>
            channels
                .Where(c => c.Parent?.Id == parent.Id)
                .Select(c => new ChannelConnection(c, GetChildren(c)))
                .ToArray();

        return channels
            .Where(c => c.Parent is null)
            .Select(c => new ChannelConnection(c, GetChildren(c)))
            .ToArray();
    }

    public static IReadOnlyList<ChannelConnection> FilterTree(
        IReadOnlyList<ChannelConnection> roots,
        string query
    )
    {
        if (string.IsNullOrWhiteSpace(query))
            return roots;

        var result = new List<ChannelConnection>();

        foreach (var connection in roots)
        {
            var filteredChildren = FilterTree(connection.Children, query);
            var matches = connection.Channel.Name.Contains(
                query,
                StringComparison.OrdinalIgnoreCase
            );

            if (matches && filteredChildren.Count == 0)
            {
                result.Add(connection);
            }
            else if (filteredChildren.Count > 0)
            {
                result.Add(new ChannelConnection(connection.Channel, filteredChildren));
            }
        }

        return result;
    }
}
