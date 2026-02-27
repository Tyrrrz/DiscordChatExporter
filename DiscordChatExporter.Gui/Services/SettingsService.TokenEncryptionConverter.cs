using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordChatExporter.Gui.Utils.Extensions;

namespace DiscordChatExporter.Gui.Services;

public partial class SettingsService
{
    private class TokenEncryptionConverter : JsonConverter<string?>
    {
        private const string Prefix = "enc:";

        private static readonly Lazy<byte[]> Key = new(() =>
            Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(Environment.TryGetMachineId() ?? string.Empty),
                Encoding.UTF8.GetBytes(ThisAssembly.Project.EncryptionSalt),
                600_000,
                HashAlgorithmName.SHA256,
                16
            )
        );

        public override string? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var value = reader.GetString();

            // No prefix means the token is stored as plain text, which was the case for older
            // versions of the application. Load it as is and encrypt it on next save.
            if (
                string.IsNullOrWhiteSpace(value)
                || !value.StartsWith(Prefix, StringComparison.Ordinal)
            )
            {
                return value;
            }

            try
            {
                var encryptedData = Convert.FromHexString(value[Prefix.Length..]);
                var tokenData = new byte[encryptedData.AsSpan(28).Length];

                // Layout: nonce (12 bytes) | tag (16 bytes) | cipher
                using var aes = new AesGcm(Key.Value, 16);
                aes.Decrypt(
                    encryptedData.AsSpan(0, 12),
                    encryptedData.AsSpan(28),
                    encryptedData.AsSpan(12, 16),
                    tokenData
                );

                return Encoding.UTF8.GetString(tokenData);
            }
            catch (Exception ex)
                when (ex
                        is FormatException
                            or CryptographicException
                            or ArgumentException
                            or IndexOutOfRangeException
                )
            {
                return null;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            string? value,
            JsonSerializerOptions options
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                writer.WriteNullValue();
                return;
            }

            var tokenData = Encoding.UTF8.GetBytes(value);
            var encryptedData = new byte[28 + tokenData.Length];

            // Nonce
            RandomNumberGenerator.Fill(encryptedData.AsSpan(0, 12));

            // Layout: nonce (12 bytes) | tag (16 bytes) | cipher
            using var aes = new AesGcm(Key.Value, 16);
            aes.Encrypt(
                encryptedData.AsSpan(0, 12),
                tokenData,
                encryptedData.AsSpan(28),
                encryptedData.AsSpan(12, 16)
            );

            writer.WriteStringValue(Prefix + Convert.ToHexStringLower(encryptedData));
        }
    }
}
