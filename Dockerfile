FROM mono:5
WORKDIR /root/build
COPY DiscordChatExporter.sln favicon.ico ./
COPY DiscordChatExporter.Core DiscordChatExporter.Core
COPY DiscordChatExporter.Core.Markdown DiscordChatExporter.Core.Markdown
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli
RUN msbuild ./DiscordChatExporter.Cli/DiscordChatExporter.Cli.csproj /t:Restore
RUN msbuild ./DiscordChatExporter.Cli/DiscordChatExporter.Cli.csproj /p:Configuration=Release

FROM mono:5
COPY --from=0 /root/build/DiscordChatExporter.Cli/bin/Release/net461 /root/bin
WORKDIR /a
ENTRYPOINT ["mono", "/root/bin/DiscordChatExporter.Cli.exe"]
