#!/usr/bin/env sh

if [ "$(id -u)" = '0' ]; then
  chown -R dce:dce /out
  exec su-exec dce "$0" "$@"
fi

exec /opt/app/DiscordChatExporter.Cli "$@"
