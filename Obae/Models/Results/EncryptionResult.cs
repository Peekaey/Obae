using System;

namespace Obae.Models;

// Modified from
// https://www.milanjovanovic.tech/blog/implementing-aes-encryption-with-csharp
public class EncryptionResult
{
    // The IV is prepended to the encrypted data
    public string EncryptedData { get; set; }

    public static EncryptionResult CreateEncryptedData(byte[] data, byte[] iv)
    {
        // Combine IV and encrypted data
        var combined = new byte[iv.Length + data.Length];
        Array.Copy(iv, 0, combined, 0, iv.Length);
        Array.Copy(data, 0, combined, iv.Length, data.Length);

        return new EncryptionResult
        {
            EncryptedData = Convert.ToBase64String(combined)
        };
    }

    public (byte[] iv, byte[] encryptedData) GetIVAndEncryptedData()
    {
        var combined = Convert.FromBase64String(EncryptedData);

        // Extract IV and data
        var iv = new byte[16]; // AES block size is 16 bytes (128 / 8)
        var encryptedData = new byte[combined.Length - 16];

        Array.Copy(combined, 0, iv, 0, 16);
        Array.Copy(combined, 16, encryptedData, 0, encryptedData.Length);

        return (iv, encryptedData);
    }
}