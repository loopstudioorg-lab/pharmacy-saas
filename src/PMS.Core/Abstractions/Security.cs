namespace PMS.Core.Abstractions;

public interface IPasswordHasher
{
    (string Hash, string Salt) Hash(string password);
    bool Verify(string password, string hash, string salt);
}

public interface IEncryptionService
{
    string EncryptString(string plaintext);
    string DecryptString(string ciphertext);
    byte[] EncryptBytes(byte[] plaintext);
    byte[] DecryptBytes(byte[] ciphertext);
}

public interface IDeviceFingerprintService
{
    DeviceFingerprint Compute();
}

public sealed record DeviceFingerprint(
    string Hash,
    string MachineName,
    string? OsVersion,
    string? MotherboardSerial,
    string? CpuId,
    string? DiskSerial,
    string? MachineGuid,
    string? MacAddress);

public interface ILicenseSigner
{
    string Sign(string payload);
    bool Verify(string payload, string signature);
}

public interface IPermissionService
{
    Task<bool> CanAccessAsync(int userId, string moduleKey, string permissionKey);
    Task<bool> IsModuleEnabledAsync(int pharmacyId, string moduleKey);
}
