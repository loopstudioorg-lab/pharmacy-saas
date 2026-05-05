namespace PMS.Core.Entities;

public class User : AuditableEntity
{
    public int PharmacyId { get; set; }
    public int? BranchId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsLocked { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public DateTime? LastPasswordChangeUtc { get; set; }
    public bool MustChangePassword { get; set; }
}
