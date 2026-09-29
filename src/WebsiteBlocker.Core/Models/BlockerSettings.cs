namespace WebsiteBlocker.Core.Models;

public class BlockerSettings
{
    public bool Enabled { get; set; }

    public List<string> BlockedSites { get; set; } = new();

    public bool BlockSubdomains { get; set; } = true;

    public bool EnableLogging { get; set; } = true;
}