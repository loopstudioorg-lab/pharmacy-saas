using Microsoft.EntityFrameworkCore;
using PMS.Core.Entities;

namespace PMS.Data;

/// <summary>
/// EF Core DbContext for admin / setup / CRUD paths. Hot paths (POS, reports) use Dapper via IDbConnectionFactory.
/// Schema is owned by the versioned SQL scripts in PMS.Database; we map to those tables (tbl_*).
/// </summary>
public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options)
    {
    }

    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<PharmacyMachine> Machines => Set<PharmacyMachine>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<LicenseInfo> Licenses => Set<LicenseInfo>();
    public DbSet<ModuleSetting> ModuleSettings => Set<ModuleSetting>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SyncQueueItem> SyncQueue => Set<SyncQueueItem>();
    public DbSet<SyncLog> SyncLogs => Set<SyncLog>();
    public DbSet<RecycleBinItem> RecycleBin => Set<RecycleBinItem>();
    public DbSet<SchemaVersion> SchemaVersions => Set<SchemaVersion>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Pharmacy>().ToTable("tbl_Pharmacies");
        b.Entity<Branch>().ToTable("tbl_Branches");
        b.Entity<PharmacyMachine>().ToTable("tbl_PharmacyMachines");
        b.Entity<User>().ToTable("tbl_Users");
        b.Entity<Role>().ToTable("tbl_Roles");
        b.Entity<Permission>().ToTable("tbl_Permissions");
        b.Entity<RolePermission>().ToTable("tbl_RolePermissions");
        b.Entity<LicenseInfo>().ToTable("tbl_LicenseInfo");
        b.Entity<ModuleSetting>().ToTable("tbl_ModuleSettings");
        b.Entity<FeatureFlag>().ToTable("tbl_FeatureFlags");
        b.Entity<AuditLog>().ToTable("tbl_AuditLogs");
        b.Entity<SyncQueueItem>().ToTable("tbl_SyncQueue");
        b.Entity<SyncLog>().ToTable("tbl_SyncLogs");
        b.Entity<RecycleBinItem>().ToTable("tbl_RecycleBin");
        b.Entity<SchemaVersion>().ToTable("tbl_SchemaVersion");

        b.Entity<Pharmacy>().HasIndex(p => p.PharmacyCode).IsUnique();
        b.Entity<Branch>().HasIndex(p => new { p.PharmacyId, p.Code }).IsUnique();
        b.Entity<User>().HasIndex(u => new { u.PharmacyId, u.Username }).IsUnique();
        b.Entity<Role>().HasIndex(r => new { r.PharmacyId, r.Key }).IsUnique();
        b.Entity<Permission>().HasIndex(p => new { p.ModuleKey, p.PermissionKey }).IsUnique();
        b.Entity<ModuleSetting>().HasIndex(p => new { p.PharmacyId, p.ModuleKey }).IsUnique();
        b.Entity<PharmacyMachine>().HasIndex(m => m.FingerprintHash);

        b.Entity<AuditLog>().Property(a => a.EventType).HasConversion<int>();
        b.Entity<PharmacyMachine>().Property(m => m.Status).HasConversion<int>();
        b.Entity<LicenseInfo>().Property(l => l.Status).HasConversion<int>();
        b.Entity<LicenseInfo>().Property(l => l.Tier).HasConversion<int>();
        b.Entity<SyncQueueItem>().Property(s => s.Status).HasConversion<int>();
    }
}
