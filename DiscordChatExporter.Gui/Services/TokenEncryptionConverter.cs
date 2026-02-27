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

        byte[] data;
        try
        {
            data = Convert.FromHexString(value[Prefix.Length..]);
        }
        catch (FormatException)
        {
            return null;
        }

        // Layout: Nonce (12 bytes) | padLen (1 byte) | Tag (16 bytes) | Ciphertext
        if (data.Length < 29)
            return null;

        var padLen = data[12];
        if (padLen < 1 || padLen > MaxPaddingLength)
            return null;

        var nonce = data.AsSpan(0, 12);
        var tag = data.AsSpan(13, 16);
        var ciphertext = data.AsSpan(29);

        var decrypted = new byte[ciphertext.Length];
        try
        {
            using var aes = new AesGcm(Key.Value, 16);
            aes.Decrypt(nonce, ciphertext, tag, decrypted);
        }
        catch (CryptographicException)
        {
            return null;
        }

        if (padLen > decrypted.Length)
            return null;

        return Encoding.UTF8.GetString(decrypted.AsSpan(padLen));
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var nonce = RandomNumberGenerator.GetBytes(12);
        var padLen = RandomNumberGenerator.GetInt32(1, MaxPaddingLength + 1);
        var tokenBytes = Encoding.UTF8.GetBytes(value);

        // Random padding bytes vary the output length
        var padded = new byte[padLen + tokenBytes.Length];
        RandomNumberGenerator.Fill(padded.AsSpan(0, padLen));
        tokenBytes.CopyTo(padded, padLen);

        var ciphertext = new byte[padded.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(Key.Value, 16);
        aes.Encrypt(nonce, padded, ciphertext, tag);

        // Layout: Nonce (12 bytes) | padLen (1 byte) | Tag (16 bytes) | Ciphertext
        var data = new byte[29 + ciphertext.Length];
        nonce.CopyTo(data.AsSpan(0, 12));
        data[12] = (byte)padLen;
        tag.CopyTo(data.AsSpan(13, 16));
        ciphertext.CopyTo(data.AsSpan(29));

        writer.WriteStringValue(Prefix + Convert.ToHexStringLower(data));
    }
}
