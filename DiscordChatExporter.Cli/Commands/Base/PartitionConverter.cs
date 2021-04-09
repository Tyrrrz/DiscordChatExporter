using System;
using System.Collections.Generic;
using System.Text;
using ByteSizeLib;
using CliFx.Extensibility;
using DiscordChatExporter.Core;
using DiscordChatExporter.Core.Exporting;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public class PartitionConverter : BindingConverter<IPartitioner>
    {
        public override IPartitioner Convert(string? rawValue)
        {
            if (rawValue == null) return new NullPartitioner();

            if (ByteSize.TryParse(rawValue, out ByteSize filesize))
            {
                return new FileSizePartitioner((long)filesize.Bytes);
            }
            else
            {
                int messageLimit = int.Parse(rawValue);
                return new MessageCountPartitioner(messageLimit);
            }
        }
    }
}
