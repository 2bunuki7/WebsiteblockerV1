using System;
using System.IO;
using System.Linq;
using System.Text;
using WebsiteBlocker.Core.Models;

namespace WebsiteBlocker.Core.Services;

public class BlockerService
{
    private const string StartMarker = "# WEBSITE_BLOCKER_START";
    private const string EndMarker = "# WEBSITE_BLOCKER_END";

    private readonly string _hostsFile;

    public BlockerService()
    {
        _hostsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            @"drivers\etc\hosts");
    }

    public void EnableBlocking(BlockerSettings settings)
    {
        DisableBlocking();

        if (settings.BlockedSites.Count == 0)
        {
            return;
        }

        var lines = File.Exists(_hostsFile)
            ? File.ReadAllLines(_hostsFile).ToList()
            : [];

        lines.Add(StartMarker);

        foreach (string domain in settings.BlockedSites)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                continue;
            }

            lines.Add($"127.0.0.1 {domain}");
            lines.Add($"127.0.0.1 www.{domain}");
        }

        lines.Add(EndMarker);

        File.WriteAllLines(
            _hostsFile,
            lines,
            new UTF8Encoding(false));

        FlushDns();
    }

    public void DisableBlocking()
    {
        if (!File.Exists(_hostsFile))
        {
            return;
        }

        var lines = File.ReadAllLines(_hostsFile).ToList();

        bool insideBlock = false;

        var cleanedLines = lines
            .Where(line =>
            {
                if (line.Trim() == StartMarker)
                {
                    insideBlock = true;
                    return false;
                }

                if (line.Trim() == EndMarker)
                {
                    insideBlock = false;
                    return false;
                }

                return !insideBlock;
            })
            .ToList();

        File.WriteAllLines(
            _hostsFile,
            cleanedLines,
            new UTF8Encoding(false));

        FlushDns();
    }

    private static void FlushDns()
    {
        try
        {
            using var process = new System.Diagnostics.Process();

            process.StartInfo.FileName = "ipconfig.exe";
            process.StartInfo.Arguments = "/flushdns";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();
        }
        catch
        {
            // DNS flushing failing should not crash the blocker.
        }
    }
}