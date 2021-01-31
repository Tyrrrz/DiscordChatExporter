using System.Collections.Generic;

namespace DiscordChatExporter.Domain.Discord.Models.Common
{
    public partial class PositionBasedComparer : IComparer<IHasIdAndPosition>
    {
        public int Compare(IHasIdAndPosition? x, IHasIdAndPosition? y)
        {
            int result;
            if (x != null)
            {
                result = x.Position.CompareTo(y?.Position);
                if(result == 0)
                {
                    result = x.Id.Value.CompareTo(y?.Id.Value);
                }
            }
            else
            {
                result = y == null ? 0 : -1;
            }
            return result;
        }
    }

    public partial class PositionBasedComparer
    {
        public static PositionBasedComparer Instance { get; } = new();
    }
}