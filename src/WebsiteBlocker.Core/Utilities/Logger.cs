using System;
using System.IO;

namespace WebsiteBlocker.Core.Services;

public class Logger
{
    private readonly string _logFile;

    public Logger()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WebsiteBlocker",
            "Logs");

        Directory.CreateDirectory(folder);

        _logFile = Path.Combine(folder, "blocker.log");
    }

    public void Log(string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

        File.AppendAllText(
            _logFile,
            line + Environment.NewLine);
    }
}