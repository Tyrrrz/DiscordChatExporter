using System.IO;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Rendering
{
    public interface IChatLogRenderer
    {
        Task RenderAsync(TextWriter writer);
    }
}