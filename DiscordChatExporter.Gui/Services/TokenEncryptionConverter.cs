using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordChatExporter.Gui.Utils.Extensions;

namespace DiscordChatExporter.Gui.Services;

internal class TokenEncryptionConverter : JsonConverter<string?>
{
    private const string Prefix = "enc:";

    private static readonly Lazy<byte[]> Key = new(() =>
        Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(Environment.TryGetMachineId() ?? string.Empty),
            Encoding.UTF8.GetBytes(ThisAssembly.Project.EncryptionSalt),
            iterations: 10_000,
            HashAlgorithmName.SHA256,
            outputLength: 16
        )
    );

    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = reader.GetString();

        // No prefix means the token is stored as plain text, which was
        // the case for older versions of the application.
        // Load it as is and encrypt it on next save.
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        try
        {
            var data = Convert.FromHexString(value[Prefix.Length..]);

            // Layout: nonce (12 bytes) | paddingLength (1 byte) | tag (16 bytes) | cipher
            var nonce = data.AsSpan(0, 12);
            var paddingLength = data[12];
            var tag = data.AsSpan(13, 16);
            var cipher = data.AsSpan(29);

            var decrypted = new byte[cipher.Length];
            using var aes = new AesGcm(Key.Value, 16);
            aes.Decrypt(nonce, cipher, tag, decrypted);

            return Encoding.UTF8.GetString(decrypted.AsSpan(paddingLength));
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

        var paddingLength = RandomNumberGenerator.GetInt32(1, 17);
        var tokenData = Encoding.UTF8.GetBytes(value);

        // Layout: nonce (12 bytes) | paddingLength (1 byte) | tag (16 bytes) | cipher (paddingLength + tokenData.Length)
        var data = new byte[29 + paddingLength + tokenData.Length];
        RandomNumberGenerator.Fill(data.AsSpan(0, 12)); // nonce
        data[12] = (byte)paddingLength;
        var cipherSource = data.AsSpan(29);
        RandomNumberGenerator.Fill(cipherSource[..paddingLength]); // random padding
        tokenData.CopyTo(cipherSource[paddingLength..]); // token

        using var aes = new AesGcm(Key.Value, 16);
        aes.Encrypt(data.AsSpan(0, 12), cipherSource, cipherSource, data.AsSpan(13, 16));

        writer.WriteStringValue(Prefix + Convert.ToHexStringLower(data));
    }
}

