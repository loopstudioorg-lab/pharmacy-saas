using System.Security.Cryptography;
using FluentAssertions;
using PMS.Security;
using Xunit;

namespace PMS.Security.Tests;

public class AesEncryptionTests
{
    private static AesEncryptionService NewService()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        return new AesEncryptionService(key);
    }

    [Fact]
    public void Roundtrip_string_succeeds()
    {
        var svc = NewService();
        var enc = svc.EncryptString("hello, world");
        svc.DecryptString(enc).Should().Be("hello, world");
    }

    [Fact]
    public void Different_calls_produce_different_ciphertext()
    {
        var svc = NewService();
        svc.EncryptString("same").Should().NotBe(svc.EncryptString("same"));
    }

    [Fact]
    public void Tampered_ciphertext_throws()
    {
        var svc = NewService();
        var enc = svc.EncryptString("payload");
        var bytes = System.Convert.FromBase64String(enc);
        bytes[^1] ^= 0x01;
        var tampered = System.Convert.ToBase64String(bytes);
        var act = () => svc.DecryptString(tampered);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Wrong_key_size_is_rejected()
    {
        var act = () => new AesEncryptionService(new byte[16]);
        act.Should().Throw<System.ArgumentException>();
    }
}
