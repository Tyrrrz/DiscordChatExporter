using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Discord.Data;

public record ChannelNode(Channel Channel, IReadOnlyList<ChannelNode> Children)
{
    public static IReadOnlyList<ChannelNode> BuildTree(IReadOnlyList<Channel> channels)
    {
        IReadOnlyList<ChannelNode> GetChildren(Channel parent) =>
            channels
                .Where(c => c.Parent?.Id == parent.Id)
                .Select(c => new ChannelNode(c, GetChildren(c)))
                .ToArray();

        return channels
            .Where(c => c.Parent is null)
            .Select(c => new ChannelNode(c, GetChildren(c)))
            .ToArray();
    }
}
