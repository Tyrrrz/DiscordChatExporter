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
}
