using System;
using System.IO;
using System.Security.Cryptography;
using Obae.Models;


// https://www.milanjovanovic.tech/blog/implementing-aes-encryption-with-csharp
// Modified version with a hardcoded Encryption Key
// Threat model is low and is too much work to implement secure crypto storage uniquely for each platform
// Therefore just casually encrypt the session token so that the database value can't be read casually without 
// The application decrypting it during runtime 
// Sourcecode is also public and don't want to deal with hosting an encryption key
public class EncryptionHelper
{
    private const int KeySize = 256;
    private const int BlockSize = 128;
    
    public static EncryptionResult Encrypt(string plainText, byte[] encryptionKey)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = encryptionKey;
        aes.GenerateIV(); 

        byte[] encryptedData;

        using (var encryptor = aes.CreateEncryptor())
        using (var msEncrypt = new MemoryStream())
        {
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            encryptedData = msEncrypt.ToArray();
        }
        
        var result = EncryptionResult.CreateEncryptedData(encryptedData, aes.IV);

        return result;
    }
    
    public static string Decrypt(EncryptionResult encryptionResult, byte[] decryptionKey)
    {
        var (iv, encryptedData) = encryptionResult.GetIVAndEncryptedData();

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = decryptionKey;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(encryptedData);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        try
        {
            return srDecrypt.ReadToEnd();
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("Decryption failed", ex);
        }
    }
}