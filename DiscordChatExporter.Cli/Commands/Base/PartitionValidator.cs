using ByteSizeLib;
using CliFx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Cli.Commands.Base
{
    class PartitionValidator : ArgumentValueValidator<string>
    {
        public override ValidationResult Validate(string value)
        {
            return int.TryParse(value, out _) ? ValidationResult.Ok() :
                ByteSize.TryParse(value, out _) ?
                ValidationResult.Ok() :
                ValidationResult.Error(@"The value must be numeric or represent a file size (e.g. 25mb).");
        }
    }
}
