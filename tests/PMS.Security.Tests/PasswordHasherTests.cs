using FluentAssertions;
using PMS.Security;
using Xunit;

namespace PMS.Security.Tests;

public class PasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_produces_distinct_salt_and_hash_each_call()
    {
        var (h1, s1) = _hasher.Hash("hunter2!");
        var (h2, s2) = _hasher.Hash("hunter2!");

        h1.Should().NotBe(h2);
        s1.Should().NotBe(s2);
    }

    [Fact]
    public void Verify_returns_true_for_correct_password()
    {
        var (hash, salt) = _hasher.Hash("CorrectHorseBattery!");
        _hasher.Verify("CorrectHorseBattery!", hash, salt).Should().BeTrue();
    }

    [Fact]
    public void Verify_returns_false_for_wrong_password()
    {
        var (hash, salt) = _hasher.Hash("CorrectHorseBattery!");
        _hasher.Verify("wrong-password", hash, salt).Should().BeFalse();
    }

    [Fact]
    public void Verify_returns_false_for_tampered_hash()
    {
        var (hash, salt) = _hasher.Hash("password123");
        _hasher.Verify("password123", hash + "x", salt).Should().BeFalse();
    }

    [Fact]
    public void Hash_throws_for_empty_password()
    {
        var act = () => _hasher.Hash("");
        act.Should().Throw<System.ArgumentException>();
    }
}
