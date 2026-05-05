using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Constants;
using PMS.Core.Dtos;
using PMS.Core.Entities;
using PMS.Core.Enums;
using PMS.Data;

namespace PMS.Services;

public sealed class LicenseService : ILicenseService
{
    private readonly PharmacyDbContext _db;
    private readonly ILicenseSigner _signer;
    private readonly IEncryptionService _enc;
    private readonly IClock _clock;
    private readonly string _licenseFilePath;

    public LicenseService(
        PharmacyDbContext db,
        ILicenseSigner signer,
        IEncryptionService enc,
        IClock clock,
        string licenseFilePath)
    {
        _db = db;
        _signer = signer;
        _enc = enc;
        _clock = clock;
        _licenseFilePath = licenseFilePath;
    }

    public Task<LicenseInfo?> GetLicenseAsync()
        => _db.Licenses.OrderByDescending(l => l.Id).FirstOrDefaultAsync();

    public async Task<LicenseStateDto?> GetLicenseStateAsync()
    {
        var lic = await GetLicenseAsync();
        if (lic == null) return null;

        var now = _clock.UtcNow;
        var daysRemaining = (int)Math.Ceiling((lic.ExpiryUtc - now).TotalDays);
        var inGrace = daysRemaining < 0 && daysRemaining >= -AppConstants.OfflineGracePeriodDays;

        if (daysRemaining < -AppConstants.OfflineGracePeriodDays && lic.Status != LicenseStatus.Expired)
        {
            lic.Status = LicenseStatus.Expired;
            await _db.SaveChangesAsync();
        }
        else if (inGrace && lic.Status == LicenseStatus.Trial)
        {
            lic.Status = LicenseStatus.Grace;
            await _db.SaveChangesAsync();
        }

        return new LicenseStateDto(
            lic.Tier.ToString(),
            lic.Status.ToString(),
            lic.ExpiryUtc,
            Math.Max(0, daysRemaining),
            inGrace);
    }

    public async Task<int> CreateTrialAsync(int pharmacyId, int machineId, int trialDays)
    {
        var now = _clock.UtcNow;
        var expiry = now.AddDays(trialDays);

        var modules = new Dictionary<string, bool>
        {
            [ModuleKeys.Dashboard] = true,
            [ModuleKeys.SalesPos] = true,
            [ModuleKeys.Medicines] = true,
            [ModuleKeys.Purchase] = true,
            [ModuleKeys.Stock] = true,
            [ModuleKeys.Reports] = true,
            [ModuleKeys.Backup] = true,
        };
        var modulesJson = JsonSerializer.Serialize(modules);

        var payload = $"{pharmacyId}|{machineId}|{expiry:O}|{modulesJson}";
        var signature = _signer.Sign(payload);

        var lic = new LicenseInfo
        {
            PharmacyId = pharmacyId,
            MachineId = machineId,
            Tier = LicenseTier.Basic,
            Status = LicenseStatus.Trial,
            IssuedAtUtc = now,
            ExpiryUtc = expiry,
            LastValidatedUtc = now,
            MonotonicCounter = 1,
            ModulesJson = modulesJson,
            Signature = signature,
        };
        _db.Licenses.Add(lic);
        await _db.SaveChangesAsync();

        WriteLicenseFile(lic, payload);
        return lic.Id;
    }

    public async Task<bool> ValidateAsync()
    {
        var lic = await GetLicenseAsync();
        if (lic == null) return false;

        var payload = $"{lic.PharmacyId}|{lic.MachineId}|{lic.ExpiryUtc:O}|{lic.ModulesJson}";
        if (!_signer.Verify(payload, lic.Signature ?? string.Empty))
        {
            return false;
        }

        var now = _clock.UtcNow;
        if (lic.LastValidatedUtc.HasValue && now < lic.LastValidatedUtc.Value)
        {
            lic.Status = LicenseStatus.Suspended;
            await _db.SaveChangesAsync();
            return false;
        }

        lic.LastValidatedUtc = now;
        lic.MonotonicCounter += 1;
        await _db.SaveChangesAsync();
        return true;
    }

    private void WriteLicenseFile(LicenseInfo lic, string payload)
    {
        var dir = Path.GetDirectoryName(_licenseFilePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var bundle = new
        {
            lic.PharmacyId,
            lic.MachineId,
            Tier = (int)lic.Tier,
            Status = (int)lic.Status,
            lic.ExpiryUtc,
            lic.ModulesJson,
            lic.Signature,
            Payload = payload,
        };
        var json = JsonSerializer.Serialize(bundle);
        var encrypted = _enc.EncryptString(json);
        File.WriteAllText(_licenseFilePath, encrypted);
    }
}
