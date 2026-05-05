using PMS.Core.Enums;

namespace PMS.Core.Entities;

public class PharmacyMachine : AuditableEntity
{
    public int PharmacyId { get; set; }
    public int? BranchId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string FingerprintHash { get; set; } = string.Empty;
    public string? OsVersion { get; set; }
    public string? AppVersion { get; set; }
    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginUtc { get; set; }
    public DateTime? LastSyncUtc { get; set; }
    public MachineStatus Status { get; set; } = MachineStatus.Active;
}
