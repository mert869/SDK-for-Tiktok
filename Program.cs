using System.Net.Http;
using TikTokRecoverySdk;

var licenseKey = Environment.GetEnvironmentVariable("TIKTOK_RECOVERY_LICENSE")?.Trim();
if (string.IsNullOrWhiteSpace(licenseKey))
{
    Console.Write("Lisans anahtarınız: ");
    licenseKey = Console.ReadLine()?.Trim() ?? string.Empty;
}

if (!ProjectProtection.ValidateLicenseKey(licenseKey))
{
    Console.WriteLine("Geçersiz lisans anahtarı. Uygulama sonlandırılıyor.");
    return;
}

Console.WriteLine("Lisans doğrulandı. TikTok hesabı kurtarma süreci başlıyor.");

var options = new TikTokRecoveryOptions
{
    BaseUrl = "https://api.tiktok.com/recovery",
    ApiKey = string.Empty // Gerçek bir API anahtarınız varsa buraya ekleyin.
};

using var httpClient = new HttpClient();
var client = new TikTokRecoveryClient(httpClient, options);
var database = new TikTokRecoveryDatabase("tiktok_recovery.db");

Console.WriteLine("TikTok Hesap Kurtarma ve Veritabanı Örneği");
Console.Write("Ad Soyad: ");
var fullName = Console.ReadLine()?.Trim() ?? string.Empty;
Console.Write("Kullanıcı Adı: ");
var username = Console.ReadLine()?.Trim() ?? string.Empty;
Console.Write("Şifre: ");
var password = Console.ReadLine()?.Trim() ?? string.Empty;
Console.Write("E-posta (Gmail): ");
var email = Console.ReadLine()?.Trim() ?? string.Empty;
Console.Write("Telefon Numarası: ");
var phoneNumber = Console.ReadLine()?.Trim() ?? string.Empty;

var verificationInfo = new TikTokRecoveryClient.UserVerificationInfo(fullName, username, password, email, phoneNumber);
var startResult = await client.StartVerificationAsync(verificationInfo);
if (!startResult.Success)
{
    Console.WriteLine($"Başarısız: {startResult.Message}");
    return;
}

var session = RecoverySession.CreateNew(
    startResult.SessionId ?? Guid.NewGuid().ToString(),
    verificationInfo.FullName,
    verificationInfo.Username,
    verificationInfo.Password,
    verificationInfo.Email,
    verificationInfo.PhoneNumber);

await database.SaveSessionAsync(session);
Console.WriteLine($"Doğrulama oturumu kaydedildi. SessionId: {session.SessionId}");
Console.Write("Doğrulama kodunu hangi kanala göndermek istiyorsunuz? (email / sms): ");
var deliveryMethod = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "email";

var codeResult = await client.RequestVerificationCodeAsync(session.SessionId, deliveryMethod);
if (!codeResult.Success)
{
    Console.WriteLine($"Doğrulama kodu isteği başarısız: {codeResult.Message}");
    await database.UpdateSessionStatusAsync(session.SessionId, "failed", codeResult.Message);
    return;
}

Console.WriteLine("Doğrulama kodu gönderildi. Lütfen gelen kodu girin.");
Console.Write("Doğrulama kodu: ");
var verificationCode = Console.ReadLine()?.Trim() ?? string.Empty;

await database.UpdateVerificationCodeAsync(session.SessionId, verificationCode);

var verifyResult = await client.VerifyCodeAsync(session.SessionId, verificationCode);
if (!verifyResult.Success)
{
    Console.WriteLine($"Doğrulama başarısız: {verifyResult.Message}");
    await database.UpdateSessionStatusAsync(session.SessionId, "failed", verifyResult.Message);
    return;
}

await database.UpdateSessionStatusAsync(session.SessionId, "verified", verifyResult.Message);
var storedSession = await database.GetSessionAsync(session.SessionId);

Console.WriteLine("Hesap bilgileri başarıyla doğrulandı.");
Console.WriteLine($"Durum: {verifyResult.Message}");
Console.WriteLine($"Veritabanındaki kayıt: SessionId={storedSession?.SessionId}, Status={storedSession?.Status}, UpdatedAt={storedSession?.UpdatedAt:O}");
