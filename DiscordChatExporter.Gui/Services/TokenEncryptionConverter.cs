using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordChatExporter.Gui.Services;

internal class TokenEncryptionConverter : JsonConverter<string?>
{
    private const string Prefix = "enc:";
    private const int MaxPaddingLength = 16;

    // Key is derived from a machine-specific identifier so that a stolen Settings.dat
    // cannot be decrypted on a different machine.
    private static readonly Lazy<byte[]> Key = new(DeriveKey);

    private static byte[] DeriveKey()
    {
        var machineId = GetMachineId();
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(machineId),
            "DCE-Token-Salt"u8.ToArray(),
            iterations: 10_000,
            HashAlgorithmName.SHA256,
            outputLength: 16
        );
    }

    private static string GetMachineId()
    {
        // Windows: stable GUID written during OS installation
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography"
                );
                if (regKey?.GetValue("MachineGuid") is string guid && guid.Length > 0)
                    return guid;
            }
            catch { }
        }

        // Linux: /etc/machine-id (set once by systemd at first boot)
        foreach (var path in new[] { "/etc/machine-id", "/var/lib/dbus/machine-id" })
        {
            try
            {
                var id = File.ReadAllText(path).Trim();
                if (id.Length > 0)
                    return id;
            }
            catch { }
        }

        // Last-resort fallback (e.g. macOS without /etc/machine-id)
        return Environment.MachineName;
    }

    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = reader.GetString();

        // No prefix means the token is stored as plain text (backward compatibility)
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        try
        {
            var data = Convert.FromBase64String(value[Prefix.Length..]);
            using var aes = Aes.Create();
            aes.Key = Key.Value;

            // Layout: IV (16 bytes) | padLen (1 byte) | ciphertext
            var decrypted = aes.DecryptCbc(data[17..], data[..16]);
            return Encoding.UTF8.GetString(decrypted[data[16]..]);
        }
        catch
        {
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        using var aes = Aes.Create();
        aes.Key = Key.Value;
        aes.GenerateIV(); // Random IV ensures non-deterministic output

        // Random padding bytes vary the output length
        var padLen = RandomNumberGenerator.GetInt32(1, MaxPaddingLength + 1);
        var tokenBytes = Encoding.UTF8.GetBytes(value);
        var padded = new byte[padLen + tokenBytes.Length];
        RandomNumberGenerator.Fill(padded[..padLen]);
        tokenBytes.CopyTo(padded, padLen);

        var ciphertext = aes.EncryptCbc(padded, aes.IV);

        // Layout: IV (16 bytes) | padLen (1 byte) | ciphertext
        var data = new byte[17 + ciphertext.Length];
        aes.IV.CopyTo(data.AsSpan(0, 16));
        data[16] = (byte)padLen;
        ciphertext.CopyTo(data.AsSpan(17));

        writer.WriteStringValue(Prefix + Convert.ToBase64String(data));
    }
}
