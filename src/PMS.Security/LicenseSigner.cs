using System.Security.Cryptography;
using System.Text;
using PMS.Core.Abstractions;

namespace PMS.Security;

/// <summary>
/// HMAC-SHA256 license signer used for offline renewal codes and license-file integrity.
/// In production the symmetric key is replaced by an RSA key pair where the super admin
/// holds the private key; client only verifies. For Phase 1 we use HMAC for simplicity.
/// </summary>
public sealed class HmacLicenseSigner : ILicenseSigner
{
    private readonly byte[] _key;

    public HmacLicenseSigner(byte[] key)
    {
        if (key.Length < 32)
        {
            throw new ArgumentException("HMAC key must be at least 32 bytes.", nameof(key));
        }
        _key = key;
    }

    public string Sign(string payload)
    {
        using var hmac = new HMACSHA256(_key);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(bytes);
    }

    public bool Verify(string payload, string signature)
    {
        try
        {
            var expected = Sign(payload);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature));
        }
        catch
        {
            return false;
        }
    }
}
