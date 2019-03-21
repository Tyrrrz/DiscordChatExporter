FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY DiscordChatExporter.Core.Markdown DiscordChatExporter.Core.Markdown
COPY DiscordChatExporter.Core DiscordChatExporter.Core
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli
COPY DiscordChatExporter.sln favicon.ico ./
RUN dotnet build DiscordChatExporter.Cli -c Release -f netcoreapp2.1

FROM build AS app
WORKDIR /app
COPY --from=build /src/DiscordChatExporter.Cli/bin/Release/netcoreapp2.1 ./
ENTRYPOINT ["dotnet", "DiscordChatExporter.Cli.dll"]