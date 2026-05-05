using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Entities;
using PMS.Data;

namespace PMS.Services;

public sealed class PharmacyService : IPharmacyService
{
    private readonly PharmacyDbContext _db;

    public PharmacyService(PharmacyDbContext db)
    {
        _db = db;
    }

    public Task<Pharmacy?> GetActivePharmacyAsync()
        => _db.Pharmacies.FirstOrDefaultAsync(p => p.IsActive);

    public async Task<int> CreateAsync(Pharmacy pharmacy)
    {
        _db.Pharmacies.Add(pharmacy);
        await _db.SaveChangesAsync();
        return pharmacy.Id;
    }
}

public sealed class BranchService : IBranchService
{
    private readonly PharmacyDbContext _db;

    public BranchService(PharmacyDbContext db)
    {
        _db = db;
    }

    public async Task<int> EnsureMainBranchAsync(int pharmacyId)
    {
        var existing = await _db.Branches
            .FirstOrDefaultAsync(b => b.PharmacyId == pharmacyId && b.IsActive);
        if (existing != null) return existing.Id;

        var branch = new Branch
        {
            PharmacyId = pharmacyId,
            Code = "MAIN",
            Name = "Main Branch",
            IsHeadOffice = true,
        };
        _db.Branches.Add(branch);
        await _db.SaveChangesAsync();
        return branch.Id;
    }

    public async Task<IReadOnlyList<Branch>> GetByPharmacyAsync(int pharmacyId)
        => await _db.Branches
            .Where(b => b.PharmacyId == pharmacyId && b.IsActive)
            .OrderBy(b => b.Id)
            .ToListAsync();
}
