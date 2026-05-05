using PMS.Core.Enums;

namespace PMS.Core.Entities;

public class LicenseInfo : AuditableEntity
{
    public int PharmacyId { get; set; }
    public int MachineId { get; set; }
    public LicenseTier Tier { get; set; } = LicenseTier.Basic;
    public LicenseStatus Status { get; set; } = LicenseStatus.Trial;
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryUtc { get; set; }
    public DateTime? LastValidatedUtc { get; set; }
    public long MonotonicCounter { get; set; }
    public string? Signature { get; set; }
    public string? ModulesJson { get; set; }
}
