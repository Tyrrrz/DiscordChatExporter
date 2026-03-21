using System;
using System.IO;

namespace DiscordChatExporter.Gui;

public partial class StartOptions
{
    public required string SettingsPath { get; init; }

    public required bool IsAutoUpdateDisabled { get; init; }
}

public partial class StartOptions
{
    public static StartOptions Current { get; } =
        new()
        {
            SettingsPath =
                Environment.GetEnvironmentVariable("DISCORDCHATEXPORTER_SETTINGS_PATH") is { } path
                && !string.IsNullOrWhiteSpace(path)
                    ? Path.EndsInDirectorySeparator(path) || Directory.Exists(path)
                        ? Path.Combine(path, "Settings.dat")
                        : path
                    : Path.Combine(AppContext.BaseDirectory, "Settings.dat"),
            IsAutoUpdateDisabled =
                Environment.GetEnvironmentVariable("DISCORDCHATEXPORTER_DISABLE_UPDATE") is { } v
                && (v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase)),
        };
}
