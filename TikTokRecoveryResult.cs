namespace TikTokRecoverySdk;

public sealed class TikTokRecoveryResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? SessionId { get; init; }
    public string? RecoveryToken { get; init; }
    public string? State { get; init; }

    public static TikTokRecoveryResult Ok(string message = "", string? sessionId = null, string? recoveryToken = null, string? state = null) => new()
    {
        Success = true,
        Message = message,
        SessionId = sessionId,
        RecoveryToken = recoveryToken,
        State = state,
    };

    public static TikTokRecoveryResult Fail(string message) => new()
    {
        Success = false,
        Message = message,
    };
}
