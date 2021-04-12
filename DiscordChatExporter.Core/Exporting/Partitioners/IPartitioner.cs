﻿using DiscordChatExporter.Core.Exporting.Partitioners;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting
{
    public interface IPartitioner
    {
        bool IsLimitReached(ExportPartitioningContext context);
    }
}
