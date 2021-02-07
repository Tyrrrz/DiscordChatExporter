using System.Collections.Generic;

namespace DiscordChatExporter.Domain.Discord.Models.Common
{
    public partial class PositionBasedComparer : IComparer<IHasIdAndPosition>
    {
        public int Compare(IHasIdAndPosition? x, IHasIdAndPosition? y)
        {
            int result = Comparer<int?>.Default.Compare(x?.Position, y?.Position);
            if (result == 0)
            {
                result = Comparer<ulong?>.Default.Compare(x?.Id.Value, y?.Id.Value);
            }
            return result;
        }
    }

    public partial class PositionBasedComparer
    {
        public static PositionBasedComparer Instance { get; } = new();
    }
}