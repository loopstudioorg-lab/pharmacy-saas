using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Constants;
using PMS.Core.Dtos;
using PMS.Data;
using PMS.Security;
using PMS.Services;
using Xunit;

namespace PMS.Services.Tests;

public class SetupServiceTests : IDisposable
{
    private readonly PharmacyDbContext _db;
    private readonly SetupService _setup;
    private readonly string _tempDir;

    public SetupServiceTests()
    {
        var options = new DbContextOptionsBuilder<PharmacyDbContext>()
            .UseInMemoryDatabase($"PmsTests-{Guid.NewGuid():N}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new PharmacyDbContext(options);

        var clock = new SystemClock();
        var hasher = new BCryptPasswordHasher();
        var fingerprint = new DeviceFingerprintService();
        var enc = new AesEncryptionService(RandomNumberGenerator.GetBytes(32));
        var signer = new HmacLicenseSigner(RandomNumberGenerator.GetBytes(32));

        _tempDir = Path.Combine(Path.GetTempPath(), "pms-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        var licensePath = Path.Combine(_tempDir, "license.dat");

        var audit = new AuditService(_db, clock);
        var pharmacy = new PharmacyService(_db);
        var branch = new BranchService(_db);
        var machine = new MachineService(_db, fingerprint, clock);
        var user = new UserService(_db, hasher, clock);
        var modules = new ModuleSettingsService(_db, clock);
        var license = new LicenseService(_db, signer, enc, clock, licensePath);

        _setup = new SetupService(_db, pharmacy, branch, machine, user, license, modules, audit);
    }

    [Fact]
    public async Task IsSetupCompleted_is_false_on_fresh_db()
    {
        (await _setup.IsSetupCompletedAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task RunSetup_rejects_invalid_codes()
    {
        var result = await _setup.RunSetupAsync(new SetupRequestDto(
            "WRONG", "000000", "X", "Owner", "owner", "secret123",
            null, null, null, null));
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RunSetup_with_demo_codes_succeeds()
    {
        var result = await _setup.RunSetupAsync(new SetupRequestDto(
            AppConstants.DemoPharmacyCode,
            AppConstants.DemoSetupCode,
            "My Pharmacy",
            "Owner Name",
            "owner",
            "secret123",
            null, null, null, "Lahore"));

        result.Success.Should().BeTrue();
        result.PharmacyId.Should().NotBeNull();
        result.OwnerUserId.Should().NotBeNull();
        result.MachineId.Should().NotBeNull();
        result.TrialExpiryUtc.Should().NotBeNull();

        (await _setup.IsSetupCompletedAsync()).Should().BeTrue();
        (await _db.Branches.CountAsync()).Should().Be(1);
        (await _db.Roles.CountAsync()).Should().BeGreaterThan(1);
        (await _db.Licenses.CountAsync()).Should().Be(1);
        (await _db.AuditLogs.CountAsync(a => a.EventType == Core.Enums.AuditEventType.SetupCompleted))
            .Should().Be(1);
    }

    [Fact]
    public async Task RunSetup_twice_is_blocked()
    {
        var first = await _setup.RunSetupAsync(new SetupRequestDto(
            AppConstants.DemoPharmacyCode,
            AppConstants.DemoSetupCode,
            "First", "O", "owner", "secret123",
            null, null, null, null));
        first.Success.Should().BeTrue();

        var second = await _setup.RunSetupAsync(new SetupRequestDto(
            AppConstants.DemoPharmacyCode,
            AppConstants.DemoSetupCode,
            "Second", "O", "owner2", "secret123",
            null, null, null, null));
        second.Success.Should().BeFalse();
    }

    public void Dispose()
    {
        _db.Dispose();
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }
}
