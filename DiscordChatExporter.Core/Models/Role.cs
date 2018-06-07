namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/topics/permissions#role-object

    public partial class Role
    {
        public string Id { get; }

        public string Name { get; }

        public Role(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => Name;
    }

    public partial class Role
    {
        public static Role CreateDeletedRole(string id) =>
            new Role(id, "deleted-role");
    }
}