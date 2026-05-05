using System.Security.Cryptography;
using System.Text;

namespace PMS.Security;

/// <summary>
/// Provides the per-install secrets used by AesEncryptionService and HmacLicenseSigner.
///
/// Phase 1 strategy: derive a stable 32-byte secret from a per-install GUID stored in the
/// pharmacy data folder. For Phase 6 we will replace this with DPAPI on Windows and
/// per-pharmacy keys signed by the admin server.
/// </summary>
public static class SecretsBootstrapper
{
    public static byte[] LoadOrCreateInstallSecret(string dataFolder)
    {
        Directory.CreateDirectory(dataFolder);
        var secretPath = Path.Combine(dataFolder, "install.secret");

        if (!File.Exists(secretPath))
        {
            var guid = Guid.NewGuid().ToString("N");
            File.WriteAllText(secretPath, guid, Encoding.UTF8);
            try
            {
                File.SetAttributes(secretPath, File.GetAttributes(secretPath) | FileAttributes.Hidden);
            }
            catch
            {
            }
        }

        var seed = File.ReadAllText(secretPath, Encoding.UTF8);
        return SHA256.HashData(Encoding.UTF8.GetBytes("pms-aes::" + seed));
    }

    public static byte[] LoadOrCreateHmacSecret(string dataFolder)
    {
        var seed = LoadOrCreateInstallSecret(dataFolder);
        return SHA256.HashData(Encoding.UTF8.GetBytes("pms-hmac::" + Convert.ToHexString(seed)));
    }
}
