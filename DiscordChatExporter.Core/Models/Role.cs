namespace DiscordChatExporter.Core.Models
{
    public partial class Role
    {
        public string Id { get; }

        public string Name { get; }

        public Role(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class Role
    {
        public static Role CreateDeletedRole(string id)
        {
            return new Role(id, "deleted-role");
        }
    }
}