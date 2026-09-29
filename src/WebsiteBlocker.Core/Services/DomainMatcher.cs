namespace WebsiteBlocker.Core.Services;

public static class DomainMatcher
{
    public static string NormalizeDomain(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string domain = input.Trim().ToLowerInvariant();

        domain = domain
            .Replace("https://", "")
            .Replace("http://", "");

        domain = domain.Split('/')[0];

        domain = domain.Split('?')[0];

        domain = domain.Split('#')[0];

        if (domain.StartsWith("www."))
        {
            domain = domain[4..];
        }

        return domain.Trim('.');
    }

    public static bool Matches(
        string requestedDomain,
        string blockedDomain)
    {
        requestedDomain = NormalizeDomain(requestedDomain);
        blockedDomain = NormalizeDomain(blockedDomain);

        if (requestedDomain == blockedDomain)
            return true;

        return requestedDomain.EndsWith(
            "." + blockedDomain,
            StringComparison.OrdinalIgnoreCase);
    }
}