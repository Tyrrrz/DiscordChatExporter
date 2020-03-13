
namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/topics/permissions#role-object

    public partial class Role : IHasId
    {
        public string Id { get; }

        public string Name { get; }

        public int Color { get; }

        public string ColorAsHex { get => "#"+Color.ToString("X6"); }

        public int Position { get; }

        public Role(string id, string name, int color, int position)
        {
            Id = id;
            Name = name;
            Color = color;
            Position = position;
        }

        public override string ToString() => Name;
    }

    public partial class Role
    {
        public static Role CreateDeletedRole(string id) => new Role(id, "deleted-role", 0xffffff, -1);
    }
}