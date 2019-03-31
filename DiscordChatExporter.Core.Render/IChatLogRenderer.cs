using System.IO;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Render
{
    public interface IChatLogRenderer
    {
        Task RenderAsync(TextWriter writer);
    }
}