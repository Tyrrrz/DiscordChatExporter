using System.Drawing;
using System.Text.Json;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/topics/permissions#role-object
    public partial class Role
    {
        public string Id { get; }

        public string Name { get; }

        public int Position { get; }

        public Color? Color { get; }

        public Role(string id, string name, int position, Color? color)
        {
            Id = id;
            Name = name;
            Position = position;
            Color = color;
        }

        public override string ToString() => Name;
    }

    public partial class Role
    {
        public static Role Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString();
            var name = json.GetProperty("name").GetString();
            var position = json.GetProperty("position").GetInt32();

            var color = json.GetPropertyOrNull("color")?
                .GetInt32()
                .Pipe(System.Drawing.Color.FromArgb)
                .ResetAlpha()
                .NullIf(c => c.ToRgb() <= 0);

            return new Role(id, name, position, color);
        }
    }
}