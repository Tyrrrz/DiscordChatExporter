# Message filters

You can use a special notation to filter messages that you want to have included in an export. The notation syntax is designed to mimic Discord's search query syntax, but with additional capabilities.

To configure a filter, specify it in advanced export parameters when using the GUI or by passing the `--filter` option when using the CLI. For the CLI version, see also [caveats](#cli-caveats).

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

- Escape special characters (`-` is escaped below, so it's not parsed as negation operator):

```
from:96\-LB
```

## CLI Caveats

In most cases, you will need to enclose your filter in quotes (`"`) to escape characters that may have special meaning in your shell:

```
DiscordChatExporter.Cli export [...] --filter "from:Tyrrrz has:image"
```

If you need to include quotes inside the filter itself as well, use single quotes (`'`) for those instead:

```
DiscordChatExporter.Cli export [...] --filter "from:Tyrrrz 'hello world'"
```

Additionally, negated filters (those that start with `-`) may cause parsing issues even when enclosed in quotes. To avoid this, use the tilde (`~`) character instead of the dash (`-`):

```
DiscordChatExporter.Cli export [...] --filter ~from:Tyrrrz
```