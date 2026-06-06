using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TikTokRecoverySdk;

public sealed class TikTokRecoveryClient
{
    private readonly HttpClient _httpClient;
    private readonly TikTokRecoveryOptions _options;

    public TikTokRecoveryClient(HttpClient httpClient, TikTokRecoveryOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.Timeout = options.Timeout;

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add(options.AuthHeaderName, options.ApiKey);
        }
    }

    public sealed record UserVerificationInfo(string FullName, string Username, string Password, string Email, string PhoneNumber);

    public async Task<TikTokRecoveryResult> StartVerificationAsync(UserVerificationInfo userInfo, CancellationToken cancellationToken = default)
    {
        if (userInfo is null)
        {
            return TikTokRecoveryResult.Fail("Doğrulama bilgileri sağlanmalıdır.");
        }

        if (string.IsNullOrWhiteSpace(userInfo.FullName) || string.IsNullOrWhiteSpace(userInfo.Username) ||
            string.IsNullOrWhiteSpace(userInfo.Password) || string.IsNullOrWhiteSpace(userInfo.Email) ||
            string.IsNullOrWhiteSpace(userInfo.PhoneNumber))
        {
            return TikTokRecoveryResult.Fail("Ad soyad, kullanıcı adı, şifre, e-posta ve telefon numarası gereklidir.");
        }

        var request = new
        {
            fullName = userInfo.FullName,
            username = userInfo.Username,
            password = userInfo.Password,
            email = userInfo.Email,
            phoneNumber = userInfo.PhoneNumber,
        };

        return await SendRequestAsync("verification", HttpMethod.Post, request, cancellationToken);
    }

    public async Task<TikTokRecoveryResult> RequestVerificationCodeAsync(string sessionId, string deliveryMethod, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return TikTokRecoveryResult.Fail("Session ID sağlanmalıdır.");
        }

        if (string.IsNullOrWhiteSpace(deliveryMethod))
        {
            return TikTokRecoveryResult.Fail("Teslim yöntemi belirtilmelidir.");
        }

        var request = new { deliveryMethod };
        return await SendRequestAsync($"verification/{sessionId}/code", HttpMethod.Post, request, cancellationToken);
    }

    public async Task<TikTokRecoveryResult> VerifyCodeAsync(string sessionId, string verificationCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(verificationCode))
        {
            return TikTokRecoveryResult.Fail("Session ID ve doğrulama kodu gereklidir.");
        }

        var request = new { verificationCode };
        return await SendRequestAsync($"verification/{sessionId}/verify", HttpMethod.Post, request, cancellationToken);
    }

    public async Task<TikTokRecoveryResult> ResetPasswordAsync(string sessionId, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(newPassword))
        {
            return TikTokRecoveryResult.Fail("Session ID ve yeni şifre gereklidir.");
        }

        var request = new { newPassword };
        return await SendRequestAsync($"sessions/{sessionId}/password", HttpMethod.Post, request, cancellationToken);
    }

    public async Task<TikTokRecoveryResult> GetRecoveryStatusAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return TikTokRecoveryResult.Fail("Session ID sağlanmalıdır.");
        }

        return await SendRequestAsync($"sessions/{sessionId}/status", HttpMethod.Get, null, cancellationToken);
    }

    private async Task<TikTokRecoveryResult> SendRequestAsync(string path, HttpMethod method, object? content, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, _options.BuildUri(path));

        if (content is not null)
        {
            request.Content = JsonContent.Create(content);
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return TikTokRecoveryResult.Fail($"API çağrısı başarısız: {response.StatusCode}. {errorText}");
            }

            var payload = await response.Content.ReadFromJsonAsync<RecoveryApiResponse>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                return TikTokRecoveryResult.Fail("API yanıtı boş veya beklenmeyen biçimde geldi.");
            }

            return payload.Success
                ? TikTokRecoveryResult.Ok(payload.Message, payload.SessionId, payload.RecoveryToken, payload.State)
                : TikTokRecoveryResult.Fail(payload.Message);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return TikTokRecoveryResult.Fail("İşlem kullanıcı tarafından iptal edildi.");
        }
        catch (Exception ex)
        {
            return TikTokRecoveryResult.Fail($"Beklenmeyen hata: {ex.Message}");
        }
    }

    private sealed class RecoveryApiResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? SessionId { get; init; }
        public string? RecoveryToken { get; init; }
        public string? State { get; init; }
    }
}
