using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/topics/permissions#role-object
    public partial class Role : IHasId
    {
        public Snowflake Id { get; }

        public string Name { get; }

        public int Position { get; }

        public Color? Color { get; }

        public Role(
            Snowflake id,
            string name,
            int position,
            Color? color)
        {
            Id = id;
            Name = name;
            Position = position;
            Color = color;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => Name;
    }

    public partial class Role
    {
        public static Role Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
            var name = json.GetProperty("name").GetNonWhiteSpaceString();
            var position = json.GetProperty("position").GetInt32();

            var color = json
                .GetPropertyOrNull("color")?
                .GetInt32()
                .Pipe(System.Drawing.Color.FromArgb)
                .ResetAlpha()
                .NullIf(c => c.ToRgb() <= 0);

            return new Role(id, name, position, color);
        }
    }
}