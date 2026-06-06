# TikTok Hesap Kurtarma SDK

Bu proje, TikTok hesap kurtarma sürecini yönetmek için bir C# SDK ve örnek konsol uygulaması sağlar. Proje, kullanıcı doğrulaması, OTP gönderimi ve SQLite üzerinde kurtarma oturumlarını saklama özellikleri içerir.

## Özellikler

- Ad-soyad / kullanıcı adı / şifre / Gmail / telefon doğrulama akışı
- SQLite veritabanında `RecoverySessions` tablosu ile oturum kaydı
- Lisans anahtarı ile temel proje koruması
- Konsol tabanlı örnek uygulama

## Dosyalar

- `Program.cs` - uygulama giriş noktası ve doğrulama akışı
- `TikTokRecoveryClient.cs` - API çağrılarını yöneten SDK sınıfı
- `TikTokRecoveryDatabase.cs` - SQLite ile kurtarma oturumlarını saklayan sınıf
- `TikTokRecoveryOptions.cs` - API parametreleri ve ayarlar
- `TikTokRecoveryResult.cs` - API yanıtlarını normalize eden sonuç sınıfı
- `ProjectProtection.cs` - basit lisans doğrulaması
- `README.md` - proje dokümantasyonu
- `SDKforTiktok.sql` - SQLite tablo yapısı

## Kullanım

1. Projeyi açın.
2. `Program.cs` içindeki `BaseUrl` ve `ApiKey` değerlerini kendi API uç noktalarınızla güncelleyin.
3. Lisans anahtarını `TIKTOK_RECOVERY_LICENSE` ortam değişkeni olarak ayarlayın veya uygulama çalıştırılırken girin.

Örnek lisans anahtarı:

```
SDK-TIKTOK-2026-ACCESS
```

4. Aşağıdaki komutla projeyi çalıştırın:

```bash
dotnet run
```

## Veritabanı şeması

Veritabanı dosyası: `tiktok_recovery.db`

`SDKforTiktok.sql` içindeki şema oluşturma betiği ile veritabanı yapısını inceleyebilirsiniz.

## Lisans koruması

Uygulama, bir lisans anahtarı kontrolü içerir. Geçerli anahtar girilmeden program çalışmaya başlamaz.

## Geliştirme notları

- `TikTokRecoveryDatabase` SQLite tablosunda oturum geçmişini saklar.
- `RecoverySession` nesneleri SHA256 kullanarak şifreyi hashler.
- `ProjectProtection` basit bir lisans hash kontrolü uygular.
