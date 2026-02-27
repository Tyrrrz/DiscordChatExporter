using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordChatExporter.Gui.Services;

internal partial class TokenEncryptionConverter : JsonConverter<string?>
{
    private const string Prefix = "enc:";
    private const int MaxPaddingLength = 16;

    // Key is derived from a machine-specific identifier so that a stolen settings file
    // cannot be easily decrypted on a different machine.
    private static readonly Lazy<byte[]> Key = new(DeriveKey);

    private static byte[] DeriveKey()
    {
        var machineId = GetMachineId();
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(machineId),
            Encoding.UTF8.GetBytes(EncryptionSalt),
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
                if (regKey?.GetValue("MachineGuid") is string guid && !string.IsNullOrWhiteSpace(guid))
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
                if (!string.IsNullOrWhiteSpace(id))
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

        // No prefix means the token is stored as plain text, which was
        // the case for older versions of the application.
        // Load it as is and encrypt it next time we save it.
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        try
        {
            var data = Convert.FromHexString(value[Prefix.Length..]);

            // Layout: Nonce (12 bytes) | padLen (1 byte) | Tag (16 bytes) | Ciphertext
            var padLen = data[12];
            var nonce = data.AsSpan(0, 12);
            var tag = data.AsSpan(13, 16);
            var ciphertext = data.AsSpan(29);

            var decrypted = new byte[ciphertext.Length];
            using var aes = new AesGcm(Key.Value, 16);
            aes.Decrypt(nonce, ciphertext, tag, decrypted);

            return Encoding.UTF8.GetString(decrypted.AsSpan(padLen));
        }
        catch (Exception ex)
            when (
                ex
                is FormatException
                    or CryptographicException
                    or ArgumentException
                    or IndexOutOfRangeException
            )
        {
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            writer.WriteNullValue();
            return;
        }

        var nonce = RandomNumberGenerator.GetBytes(12);
        var padding = RandomNumberGenerator.GetBytes(
            RandomNumberGenerator.GetInt32(1, MaxPaddingLength + 1)
        );
        var tokenBytes = Encoding.UTF8.GetBytes(value);

        // Assemble plaintext: padding + token
        var plaintext = new byte[padding.Length + tokenBytes.Length];
        padding.CopyTo(plaintext.AsSpan());
        tokenBytes.CopyTo(plaintext.AsSpan(padding.Length));

        // Layout: Nonce (12 bytes) | padLen (1 byte) | Tag (16 bytes) | Ciphertext
        var data = new byte[29 + plaintext.Length];
        nonce.CopyTo(data.AsSpan(0, 12));
        data[12] = (byte)padding.Length;

        using var aes = new AesGcm(Key.Value, 16);
        aes.Encrypt(nonce, plaintext, data.AsSpan(29), data.AsSpan(13, 16));

        writer.WriteStringValue(Prefix + Convert.ToHexStringLower(data));
    }
}
