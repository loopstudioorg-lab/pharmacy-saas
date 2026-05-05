using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using PMS.Core.Abstractions;

namespace PMS.Security;

/// <summary>
/// Combined device fingerprint with tolerance (blueprint Section 6).
/// Reads multiple Windows hardware signals via WMI on Windows; falls back to a degraded
/// fingerprint on non-Windows hosts so dev tooling on Mac still works.
/// The hash is the SHA256 of the strongest available concatenation of signals.
/// </summary>
public sealed class DeviceFingerprintService : IDeviceFingerprintService
{
    public DeviceFingerprint Compute()
    {
        string? motherboard = null;
        string? cpuId = null;
        string? diskSerial = null;
        string? machineGuid = null;
        var osVersion = Environment.OSVersion.VersionString;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            motherboard = QueryWmi("SELECT SerialNumber FROM Win32_BaseBoard", "SerialNumber");
            cpuId = QueryWmi("SELECT ProcessorId FROM Win32_Processor", "ProcessorId");
            diskSerial = QueryWmi(
                "SELECT SerialNumber FROM Win32_PhysicalMedia WHERE Tag LIKE '%PHYSICALDRIVE0%'",
                "SerialNumber");
            machineGuid = QueryRegistryMachineGuid();
        }

        var mac = SafeMacAddress();
        var machineName = Environment.MachineName;

        var parts = new[]
        {
            motherboard,
            cpuId,
            diskSerial,
            machineGuid,
            mac,
            machineName,
        };

        var concatenated = string.Join("|", parts.Select(p => p ?? string.Empty));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(concatenated)));

        return new DeviceFingerprint(
            hash,
            machineName,
            osVersion,
            motherboard,
            cpuId,
            diskSerial,
            machineGuid,
            mac);
    }

    [SupportedOSPlatform("windows")]
    [SuppressMessage("Interoperability", "CA1416", Justification = "Guarded by runtime check.")]
    private static string? QueryWmi(string query, string property)
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(query);
            foreach (var obj in searcher.Get())
            {
                var value = obj[property]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }
        }
        catch
        {
        }
        return null;
    }

    [SupportedOSPlatform("windows")]
    [SuppressMessage("Interoperability", "CA1416", Justification = "Guarded by runtime check.")]
    private static string? QueryRegistryMachineGuid()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Cryptography");
            return key?.GetValue("MachineGuid")?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeMacAddress()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(n => n.GetPhysicalAddress().ToString())
                .FirstOrDefault(s => !string.IsNullOrEmpty(s));
        }
        catch
        {
            return null;
        }
    }
}
