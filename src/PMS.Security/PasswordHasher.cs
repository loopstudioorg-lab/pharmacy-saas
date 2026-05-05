using System.Security.Cryptography;
using PMS.Core.Abstractions;

namespace PMS.Security;

/// <summary>
/// BCrypt-backed password hashing. We additionally store a per-user salt that is mixed in
/// before BCrypt hashing - this defends against any single point of compromise of the BCrypt cost.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public (string Hash, string Salt) Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password must be provided.", nameof(password));
        }

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = BCrypt.Net.BCrypt.HashPassword(salt + password, WorkFactor);
        return (hash, salt);
    }

    public bool Verify(string password, string hash, string salt)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(salt + password, hash);
        }
        catch
        {
            return false;
        }
    }
}
