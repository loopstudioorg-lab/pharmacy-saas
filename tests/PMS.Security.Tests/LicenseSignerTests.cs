using System.Security.Cryptography;
using FluentAssertions;
using PMS.Security;
using Xunit;

namespace PMS.Security.Tests;

public class LicenseSignerTests
{
    [Fact]
    public void Sign_then_verify_returns_true()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var signer = new HmacLicenseSigner(key);
        var payload = "1|2|2026-12-31T23:59:59Z|{}";

        var sig = signer.Sign(payload);
        signer.Verify(payload, sig).Should().BeTrue();
    }

    [Fact]
    public void Verify_rejects_tampered_payload()
    {
        var signer = new HmacLicenseSigner(RandomNumberGenerator.GetBytes(32));
        var sig = signer.Sign("payload-A");
        signer.Verify("payload-B", sig).Should().BeFalse();
    }

    [Fact]
    public void Different_keys_cannot_verify_each_others_signatures()
    {
        var s1 = new HmacLicenseSigner(RandomNumberGenerator.GetBytes(32));
        var s2 = new HmacLicenseSigner(RandomNumberGenerator.GetBytes(32));

        var sig = s1.Sign("payload");
        s2.Verify("payload", sig).Should().BeFalse();
    }
}
