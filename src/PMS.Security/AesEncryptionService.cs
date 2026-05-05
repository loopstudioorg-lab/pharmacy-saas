using System.Security.Cryptography;
using System.Text;
using PMS.Core.Abstractions;

namespace PMS.Security;

/// <summary>
/// AES-256-GCM encryption used for the local license file and any other small at-rest secret.
/// The key is derived from a per-install secret stored in DPAPI on Windows;
/// for cross-platform code paths we accept a key bytes constructor argument.
/// </summary>
public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public AesEncryptionService(byte[] key)
    {
        if (key.Length != 32)
        {
            throw new ArgumentException("Key must be 32 bytes (AES-256).", nameof(key));
        }
        _key = key;
    }

    public string EncryptString(string plaintext)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        return Convert.ToBase64String(EncryptBytes(bytes));
    }

    public string DecryptString(string ciphertext)
    {
        var bytes = Convert.FromBase64String(ciphertext);
        return Encoding.UTF8.GetString(DecryptBytes(bytes));
    }

    public byte[] EncryptBytes(byte[] plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var result = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);
        return result;
    }

    public byte[] DecryptBytes(byte[] ciphertext)
    {
        if (ciphertext.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Ciphertext too short.");
        }

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var encrypted = new byte[ciphertext.Length - NonceSize - TagSize];

        Buffer.BlockCopy(ciphertext, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(ciphertext, NonceSize + TagSize, encrypted, 0, encrypted.Length);

        var plain = new byte[encrypted.Length];
        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, encrypted, tag, plain);
        return plain;
    }
}
