using System.Collections.Generic;

namespace DiscordChatExporter.Core.Discord.Data.Common;

public class IdBasedEqualityComparer : IEqualityComparer<IHasId>
{
    public static IdBasedEqualityComparer Instance { get; } = new();

    public bool Equals(IHasId? x, IHasId? y) => x?.Id == y?.Id;

    public int GetHashCode(IHasId obj) => obj.Id.GetHashCode();
}