using System.Collections.Generic;

namespace DiscordChatExporter.Domain.Discord.Models.Common
{
    public partial class ChannelPositionBasedComparer : IComparer<Channel>
    {
        public int Compare(Channel? x, Channel? y)
        {
            int result;
            if (x != null)
            {
                result = x.Position.CompareTo(y?.Position);
            }
            else if (y != null)
            {
                result = -y.Position.CompareTo(x?.Position);
            }
            else
            {
                result = 0;
            }
            return result;
        }
    }

    public partial class ChannelPositionBasedComparer
    {
        public static ChannelPositionBasedComparer Instance { get; } = new();
    }
}