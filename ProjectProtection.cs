using System.Security.Cryptography;
using System.Text;

namespace TikTokRecoverySdk;

public static class ProjectProtection
{
    private const string ValidLicenseHash = "C2C6669DA7950CC1B1328147ED9878C4994AE2A1CB882473C80EC7409D0AC0E8";

    public static bool ValidateLicenseKey(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            return false;
        }

        var normalizedKey = licenseKey.Trim();
        var hashedKey = HashLicenseKey(normalizedKey);
        return string.Equals(hashedKey, ValidLicenseHash, StringComparison.OrdinalIgnoreCase);
    }

    public static string HashLicenseKey(string licenseKey)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(licenseKey);
        return Convert.ToHexString(sha256.ComputeHash(bytes));
    }
}
