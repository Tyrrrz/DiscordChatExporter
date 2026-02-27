using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordChatExporter.Gui.Services;

internal class TokenEncryptionConverter : JsonConverter<string?>
{
    private const string Prefix = "enc:";
    private const int MaxPaddingLength = 16;

    // Hardcoded key — sufficient to avoid plain-text storage; not a strong security guarantee
    private static readonly byte[] Key = "DCE-Token-Key-v1"u8.ToArray();

    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = reader.GetString();

        // No prefix means the token is stored as plain text (backward compatibility)
        if (value is null || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        try
        {
            var data = Convert.FromBase64String(value[Prefix.Length..]);
            using var aes = Aes.Create();
            aes.Key = Key;

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
        aes.Key = Key;
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
