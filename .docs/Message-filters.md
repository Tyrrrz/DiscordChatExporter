# Message filters

You can use a special notation to filter messages that you want to have included in an export. The notation syntax is designed to mimic Discord's search query syntax, but with additional capabilities.

To configure a filter, specify it in advanced export parameters when using the GUI or by passing the `--filter` option when using the CLI. For the CLI version, see also [caveats](#cli-caveats).

## Examples

- Filter by user

```console
from:Tyrrrz
```

- Filter by user (with discriminator)

```console
from:Tyrrrz#1234
```

- Filter by message content (allowed values: `link`, `embed`, `file`, `video`, `image`, `sound`)

```console
has:image
```

- Filter by mentioned user (same rules apply as with `from:` filter)

```console
mentions:Tyrrrz#1234
```

- Filter by contained text (has word "hello" and word "world" somewhere in the message text):

```console
hello world
```

- Filter by contained text (has the string "hello world" somewhere in the message text):

```console
"hello world"
```

- Combine multiple filters ('and'):

```console
from:Tyrrrz has:image
```

- Same thing but with an explicit operator:

```console
from:Tyrrrz & has:image
```

- Combine multiple filters ('or'):

```console
from:Tyrrrz | from:"96-LB"
```

- Combine multiple filters using groups:

```console
(from:Tyrrrz | from:"96-LB") has:image
```

- Negate a filter:

```console
-from:Tyrrrz | -has:image
```

- Negate a grouped filter:

```console
-(from:Tyrrrz has:image)
```

- Escape special characters (`-` is escaped below, so it's not parsed as negation operator):

```console
from:96\-LB
```

## CLI Caveats

In most cases, you will need to enclose your filter in quotes (`"`) to escape characters that may have special meaning in your shell:

```console
DiscordChatExporter.Cli export [...] --filter "from:Tyrrrz has:image"
```

If you need to include quotes inside the filter itself as well, use single quotes (`'`) for those instead:

```console
DiscordChatExporter.Cli export [...] --filter "from:Tyrrrz 'hello world'"
```

Additionally, negated filters (those that start with `-`) may cause parsing issues even when enclosed in quotes. To avoid this, use the tilde (`~`) character instead of the dash (`-`):

```console
DiscordChatExporter.Cli export [...] --filter ~from:Tyrrrz
```
