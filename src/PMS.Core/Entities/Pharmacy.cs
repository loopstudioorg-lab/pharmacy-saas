namespace PMS.Core.Entities;

public class Pharmacy : AuditableEntity
{
    public string PharmacyCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "Pakistan";
    public string? TaxNumber { get; set; }
    public string? LogoPath { get; set; }
    public Guid PharmacyGuid { get; set; } = Guid.NewGuid();
}
