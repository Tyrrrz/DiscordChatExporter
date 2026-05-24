FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

ARG TARGETARCH
ARG VERSION=0.0.0

WORKDIR /src

COPY favicon.ico .
COPY NuGet.config .
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY DiscordChatExporter.Core DiscordChatExporter.Core
COPY DiscordChatExporter.Cli DiscordChatExporter.Cli

RUN dotnet publish DiscordChatExporter.Cli \
    -p:Version=$VERSION \
    -p:CSharpier_Bypass=true \
    --configuration Release \
    --self-contained \
    --use-current-runtime \
    --arch "$TARGETARCH" \
    --output /opt/publish

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS run

RUN apk add --no-cache bash jq icu-libs icu-data-full tzdata

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

WORKDIR /workspace

COPY --from=build /opt/publish /opt/app
COPY config /opt/dce-config
COPY scripts/run-discord-scrape.sh /opt/dce-scheduler/run-discord-scrape.sh

RUN chmod 755 /opt/dce-scheduler/run-discord-scrape.sh

ENTRYPOINT ["/opt/dce-scheduler/run-discord-scrape.sh"]
CMD ["help"]
