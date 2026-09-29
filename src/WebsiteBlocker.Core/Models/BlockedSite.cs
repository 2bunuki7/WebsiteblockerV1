namespace WebsiteBlocker.Core.Models;

public class BlockedSite
{
    public string Domain { get; set; } = string.Empty;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}