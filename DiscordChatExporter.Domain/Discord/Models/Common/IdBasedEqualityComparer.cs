using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Domain.Discord.Models.Common
{
    public partial class IdBasedEqualityComparer : IEqualityComparer<IHasId>
    {
        public bool Equals(IHasId? x, IHasId? y) => StringComparer.Ordinal.Equals(x?.Id, y?.Id);

        public int GetHashCode(IHasId obj) => StringComparer.Ordinal.GetHashCode(obj.Id);
    }

    public partial class IdBasedEqualityComparer
    {
        public static IdBasedEqualityComparer Instance { get; } = new();
    }
}