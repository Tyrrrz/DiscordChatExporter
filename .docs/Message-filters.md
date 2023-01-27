# Message filters

You can use a special notation to filter messages that you want to have included in an export. The notation syntax is designed to mimic Discord's search query syntax, but with additional capabilities.

To configure a filter, specify it in advanced export parameters when using the GUI or by passing the `--filter` option when using the CLI.

## Examples

- Filter by user

```
from:Tyrrrz
```

- Filter by user (with discriminator)

```
from:Tyrrrz#1234
```

- Filter by message content (allowed values: `link`, `embed`, `file`, `video`, `image`, `sound`)

```
has:image
```

- Filter by mentioned user (same rules apply as with `from:` filter)

```
mentions:Tyrrrz#1234
```

- Filter by contained text (has word "hello" and word "world" somewhere in the message text):

```
hello world
```

- Filter by contained text (has the string "hello world" somewhere in the message text):

```
"hello world"
```

- Combine multiple filters ('and'):

```
from:Tyrrrz has:image
```

- Same thing but with an explicit operator:

```
from:Tyrrrz & has:image
```

- Combine multiple filters ('or'):

```
from:Tyrrrz | from:"96-LB"
```

- Combine multiple filters using groups:

```
(from:Tyrrrz | from:"96-LB") has:image
```

- Negate a filter:

```
-from:Tyrrrz | -has:image
```

- Negate a grouped filter:

```
-(from:Tyrrrz has:image)
```

- Escape special characters (`-` is escaped below so it's not parsed as negation operator):

```
from:96\-LB
```

## CLI Caveat

When using the CLI and the specified filter starts with a negation (for example `-from:Tyrrrz`), you need to quote the value and prepend it with a space (i.e. `" -from:Tyrrrz"`). This prevents the beginning of the input from being incorrectly treated as a value passed to the `-f|--format` option.

In other words, this breaks:

```
DiscordChatExporter.Cli export [...] --filter "-from:Tyrrrz"
```

This works (note the space):

```
DiscordChatExporter.Cli export [...] --filter " -from:Tyrrrz"
```

Additionally, when using the CLI, it's recommended to use single quotes for contingent string of characters rather than double quotes (so it doesn't collide with the double quotes used as escaping mechanism in terminals). E.g.:

This breaks:

```
DiscordChatExporter.Cli export [...] --filter "from:"Some name with spaces""
```

This works:

```
DiscordChatExporter.Cli export [...] --filter "from:'Some name with spaces'"
```
