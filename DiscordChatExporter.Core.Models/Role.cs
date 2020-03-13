
namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/topics/permissions#role-object

    public partial class Role : IHasId
    {
        public string Id { get; }

        public string Name { get; }

        public int Color { get; }

        public string ColorAsHex { get => "#"+Color.ToString("X6"); }
        public string ColorAsRgb { get => $"{Color>>8}, {(Color & 0xff00)>>4}, {Color & 0xff}"; }

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
        private static Role? _everyone;
        public static Role Everyone
        {
            get => _everyone ?? (_everyone = new Role("", "@everyone", 0, 0));
        }
        public static Role CreateDeletedRole(string id) => new Role(id, "deleted-role", 0, -1);
    }
}