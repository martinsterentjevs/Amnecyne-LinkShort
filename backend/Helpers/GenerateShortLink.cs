namespace Amnecyne.LinkShort.Helpers;

public class ShortLinkGenerator
{
    public static string GenerateShortUrl()
    {
        var randomBytes = new byte[6];
        Random.Shared.NextBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}