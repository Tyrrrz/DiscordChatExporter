#!/usr/bin/env sh

# If we are root, ensure the files in /out are writable
# by the dce user and restart the process as the dce user
if [ "$(id -u)" = '0' ]; then
  chown -R dce:dce /out
  exec su-exec dce "$0" "$@"
fi

exec /opt/app/DiscordChatExporter.Cli "$@"
