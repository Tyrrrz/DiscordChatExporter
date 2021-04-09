using ByteSizeLib;
using CliFx;
using CliFx.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Cli.Commands.Base
{
    class PartitionValidator : BindingValidator<string>
    {
        public override BindingValidationError? Validate(string value)
        {
            return int.TryParse(value, out _) ? Ok() :
                ByteSize.TryParse(value, out _) ?
                Ok() :
                Error(@"The value must be numeric or represent a file size (e.g. 25mb).");
        }
    }
}
