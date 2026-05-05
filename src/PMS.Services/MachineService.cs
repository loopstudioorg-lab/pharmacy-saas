using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Entities;
using PMS.Core.Enums;
using PMS.Data;

namespace PMS.Services;

public sealed class MachineService : IMachineService
{
    private readonly PharmacyDbContext _db;
    private readonly IDeviceFingerprintService _fingerprint;
    private readonly IClock _clock;

    public MachineService(PharmacyDbContext db, IDeviceFingerprintService fingerprint, IClock clock)
    {
        _db = db;
        _fingerprint = fingerprint;
        _clock = clock;
    }

    public Task<PharmacyMachine?> GetCurrentMachineAsync()
    {
        var fp = _fingerprint.Compute();
        return _db.Machines
            .Where(m => m.IsActive && m.FingerprintHash == fp.Hash)
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> RegisterCurrentMachineAsync(int pharmacyId, int? branchId)
    {
        var fp = _fingerprint.Compute();
        var existing = await _db.Machines
            .FirstOrDefaultAsync(m => m.PharmacyId == pharmacyId && m.FingerprintHash == fp.Hash);
        if (existing != null)
        {
            existing.Status = MachineStatus.Active;
            existing.IsActive = true;
            existing.UpdatedDate = _clock.UtcNow;
            await _db.SaveChangesAsync();
            return existing.Id;
        }

        var machine = new PharmacyMachine
        {
            PharmacyId = pharmacyId,
            BranchId = branchId,
            MachineCode = $"M-{fp.Hash[..8]}",
            MachineName = fp.MachineName,
            FingerprintHash = fp.Hash,
            OsVersion = fp.OsVersion,
            AppVersion = typeof(MachineService).Assembly.GetName().Version?.ToString(),
            RegisteredAtUtc = _clock.UtcNow,
            Status = MachineStatus.Active,
        };
        _db.Machines.Add(machine);
        await _db.SaveChangesAsync();
        return machine.Id;
    }

    public async Task<bool> IsCurrentMachineRegisteredAsync(int pharmacyId)
    {
        var fp = _fingerprint.Compute();
        return await _db.Machines.AnyAsync(m =>
            m.PharmacyId == pharmacyId &&
            m.FingerprintHash == fp.Hash &&
            m.IsActive &&
            m.Status == MachineStatus.Active);
    }

    public async Task UpdateLastLoginAsync(int machineId)
    {
        var m = await _db.Machines.FirstOrDefaultAsync(x => x.Id == machineId);
        if (m == null) return;
        m.LastLoginUtc = _clock.UtcNow;
        await _db.SaveChangesAsync();
    }
}
