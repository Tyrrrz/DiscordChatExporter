using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

static string? TryGetMachineId()
{
    foreach (var path in new[] { "/etc/machine-id", "/var/lib/dbus/machine-id" })
    {
        try
        {
            var id = File.ReadAllText(path).Trim();
            if (!string.IsNullOrWhiteSpace(id))
                return id;
        }
        catch
        {
            // ignored
        }
    }

    try
    {
        return Environment.MachineName;
    }
    catch
    {
        return null;
    }
}

static string? DecryptToken(string? value, string encryptionSalt)
{
    if (string.IsNullOrWhiteSpace(value))
        return null;

    const string prefix = "enc:";
    if (!value.StartsWith(prefix, StringComparison.Ordinal))
        return value;

    try
    {
        var encryptedData = Convert.FromHexString(value[prefix.Length..]);
        var machineId = TryGetMachineId() ?? string.Empty;
        var key = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(machineId),
            Encoding.UTF8.GetBytes(encryptionSalt),
            600_000,
            HashAlgorithmName.SHA256,
            16
        );

        var tokenData = new byte[encryptedData.AsSpan(28).Length];
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(
            encryptedData.AsSpan(0, 12),
            encryptedData.AsSpan(28),
            encryptedData.AsSpan(12, 16),
            tokenData
        );

        return Encoding.UTF8.GetString(tokenData);
    }
    catch
    {
        return null;
    }
}

static string ResolveSettingsPath(string[] args)
{
    if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        return args[0];

    var envPath = Environment.GetEnvironmentVariable("DISCORDCHATEXPORTER_SETTINGS_PATH");
    if (!string.IsNullOrWhiteSpace(envPath))
    {
        if (envPath.EndsWith(Path.DirectorySeparatorChar) || Directory.Exists(envPath))
            return Path.Combine(envPath, "Settings.dat");

        return envPath;
    }

    throw new FileNotFoundException(
        "Settings path not provided. Pass Settings.dat path or set DISCORDCHATEXPORTER_SETTINGS_PATH."
    );
}

var settingsPath = ResolveSettingsPath(args);
if (!File.Exists(settingsPath))
    throw new FileNotFoundException($"Settings file not found: {settingsPath}");

using var document = JsonDocument.Parse(File.ReadAllText(settingsPath));
if (
    !document.RootElement.TryGetProperty("LastToken", out var lastTokenElement)
    || lastTokenElement.ValueKind != JsonValueKind.String
)
{
    Environment.Exit(2);
}

var salt =
    Environment.GetEnvironmentVariable("DCE_ENCRYPTION_SALT") ?? "HimalayanPinkSalt";
var token = DecryptToken(lastTokenElement.GetString(), salt);
if (string.IsNullOrWhiteSpace(token))
    Environment.Exit(3);

Console.Write(token);
