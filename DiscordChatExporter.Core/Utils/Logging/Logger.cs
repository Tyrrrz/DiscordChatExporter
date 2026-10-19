using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace DiscordChatExporter.Core.Utils.Logging;

public static class Logger
{
    private static readonly Lock Lock = new();
    private static readonly AsyncLocal<string?> CurrentScope = new();
    private static readonly HashSet<string> Secrets = new(StringComparer.Ordinal);

    private static StreamWriter? _writer;
    private static bool _isSessionHeaderWritten;
    private static bool _hasWriterFailed;

    public static LogLevel? MinimumLevel { get; private set; }

    public static string? LogFilePath { get; private set; }

    public static bool IsEnabled(LogLevel level) =>
        MinimumLevel is not null && level >= MinimumLevel.Value;

    public static void Configure(LogLevel? minimumLevel)
    {
        using (Lock.EnterScope())
        {
            MinimumLevel = minimumLevel;

            if (minimumLevel is null)
            {
                CloseWriterCore();
            }
            else
            {
                _hasWriterFailed = false;
            }
        }
    }

    public static LogLevel? ResolveEnvironmentLogLevel()
    {
        var envValue = Environment.GetEnvironmentVariable("DISCORDCHATEXPORTER_LOG_LEVEL");
        if (
            !string.IsNullOrWhiteSpace(envValue)
            && Enum.TryParse<LogLevel>(envValue.Trim('"', '\'', ' '), true, out var parsedLevel)
        )
        {
            return parsedLevel;
        }

        return null;
    }

    public static string? ResolveEnvironmentLogPath() =>
        NormalizePathInput(Environment.GetEnvironmentVariable("DISCORDCHATEXPORTER_LOG_PATH"));

    private static string? NormalizePathInput(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var trimmed = path.Trim().Trim('"', '\'').Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public static void RegisterSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return;

        using (Lock.EnterScope())
        {
            Secrets.Add(secret.Trim());
        }
    }

    public static IDisposable BeginScope(string scope)
    {
        var previous = CurrentScope.Value;
        CurrentScope.Value = scope;
        return new ScopeResetter(previous);
    }

    public static void Log(
        LogLevel level,
        LogCategory category,
        string message,
        Exception? exception = null
    )
    {
        if (!IsEnabled(level))
            return;

        var timestamp = DateTimeOffset.Now.ToString(
            "yyyy-MM-dd HH:mm:ss.fff",
            CultureInfo.InvariantCulture
        );
        var levelTag = FormatLevel(level);
        var categoryTag = FormatCategory(category);
        var scope = CurrentScope.Value;

        var builder = new StringBuilder();
        builder
            .Append(timestamp)
            .Append(' ')
            .Append(levelTag)
            .Append(' ')
            .Append(categoryTag)
            .Append(' ');

        if (!string.IsNullOrWhiteSpace(scope))
            builder.Append('[').Append(scope).Append("] ");

        builder.Append(message);

        if (exception is not null)
        {
            builder.AppendLine();
            builder.Append(exception);
        }

        var line = Sanitize(builder.ToString());

#if DEBUG
        System.Diagnostics.Debug.WriteLine(line);
#endif

        WriteToFile(line);
    }

    public static void Trace(LogCategory category, string message) =>
        Log(LogLevel.Trace, category, message);

    public static void Debug(LogCategory category, string message) =>
        Log(LogLevel.Debug, category, message);

    public static void Info(LogCategory category, string message) =>
        Log(LogLevel.Info, category, message);

    public static void Warning(LogCategory category, string message, Exception? exception = null) =>
        Log(LogLevel.Warning, category, message, exception);

    public static void Error(LogCategory category, string message, Exception? exception = null) =>
        Log(LogLevel.Error, category, message, exception);

    public static void Flush()
    {
        using (Lock.EnterScope())
        {
            try
            {
                _writer?.Flush();
            }
            catch
            {
                // Ignore I/O errors during flush
            }
        }
    }

    private static void WriteToFile(string line)
    {
        using (Lock.EnterScope())
        {
            if (MinimumLevel is null || _hasWriterFailed)
                return;

            try
            {
                if (_writer is null)
                {
                    var filePath = ResolveLogFilePath();
                    var dirPath = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrWhiteSpace(dirPath))
                        Directory.CreateDirectory(dirPath);

                    var stream = new FileStream(
                        filePath,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    );

                    _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                    LogFilePath = filePath;

                    if (!_isSessionHeaderWritten)
                    {
                        _writer.WriteLine(
                            $"=== Session started at {DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)} (PID: {Environment.ProcessId}) ==="
                        );
                        _isSessionHeaderWritten = true;
                    }
                }

                _writer.WriteLine(line);
            }
            catch
            {
                _hasWriterFailed = true;
                CloseWriterCore();
            }
        }
    }

    private static void CloseWriterCore()
    {
        try
        {
            _writer?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }
        finally
        {
            _writer = null;
        }
    }

    private static string ResolveLogFilePath()
    {
        var defaultFileName =
            $"DiscordChatExporter-{DateTimeOffset.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.log";

        var customPath = ResolveEnvironmentLogPath();
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            var fullCustomPath = Path.GetFullPath(customPath);
            var isDirectory =
                Path.EndsInDirectorySeparator(customPath)
                || Directory.Exists(fullCustomPath)
                || string.IsNullOrWhiteSpace(Path.GetExtension(fullCustomPath));

            return isDirectory ? Path.Combine(fullCustomPath, defaultFileName) : fullCustomPath;
        }

        return Path.Combine(AppContext.BaseDirectory, "logs", defaultFileName);
    }

    private static string Sanitize(string input)
    {
        using (Lock.EnterScope())
        {
            if (Secrets.Count == 0)
                return input;

            var result = input;
            foreach (var secret in Secrets)
            {
                if (!string.IsNullOrEmpty(secret))
                    result = result.Replace(secret, "[REDACTED]", StringComparison.Ordinal);
            }

            return result;
        }
    }

    private static string FormatLevel(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => "[TRC]",
            LogLevel.Debug => "[DBG]",
            LogLevel.Info => "[INF]",
            LogLevel.Warning => "[WRN]",
            LogLevel.Error => "[ERR]",
            _ => "[UNK]",
        };

    private static string FormatCategory(LogCategory category) =>
        category switch
        {
            LogCategory.App => "[App    ]",
            LogCategory.Export => "[Export ]",
            LogCategory.Discord => "[Discord]",
            LogCategory.Http => "[HTTP   ]",
            LogCategory.Asset => "[Asset  ]",
            _ => "[Other  ]",
        };

    private sealed class ScopeResetter(string? previousScope) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            CurrentScope.Value = previousScope;
        }
    }
}
