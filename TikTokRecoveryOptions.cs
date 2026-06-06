namespace TikTokRecoverySdk;

public sealed class TikTokRecoveryOptions
{
    public string BaseUrl { get; init; } = "https://api.tiktok.com/recovery";
    public string ApiKey { get; init; } = string.Empty;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public string AuthHeaderName { get; init; } = "X-Api-Key";

    internal Uri BuildUri(string path)
    {
        var baseUri = BaseUrl.TrimEnd('/');
        return new Uri($"{baseUri}/{path.TrimStart('/')}", UriKind.Absolute);
    }
}
