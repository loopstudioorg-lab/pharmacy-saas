using FluentAssertions;
using PMS.Security;
using Xunit;

namespace PMS.Security.Tests;

public class DeviceFingerprintTests
{
    [Fact]
    public void Compute_returns_a_stable_hash_across_calls()
    {
        var svc = new DeviceFingerprintService();
        var a = svc.Compute();
        var b = svc.Compute();

        a.Hash.Should().NotBeNullOrWhiteSpace();
        a.Hash.Should().Be(b.Hash);
    }

    [Fact]
    public void Hash_is_64_hex_chars_sha256()
    {
        var svc = new DeviceFingerprintService();
        var fp = svc.Compute();
        fp.Hash.Length.Should().Be(64);
    }
}
