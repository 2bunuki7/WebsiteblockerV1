using System.Text.Json;
using WebsiteBlocker.Core.Models;

namespace WebsiteBlocker.Core.Services;

public class ConfigurationService
{
    private readonly string _dataDirectory;
    private readonly string _settingsFile;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public ConfigurationService()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WebsiteBlocker");

        _settingsFile = Path.Combine(
            _dataDirectory,
            "settings.json");

        Directory.CreateDirectory(_dataDirectory);
    }

    public BlockerSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFile))
            {
                return new BlockerSettings();
            }

            string json = File.ReadAllText(_settingsFile);

            return JsonSerializer.Deserialize<BlockerSettings>(
                       json,
                       _jsonOptions)
                   ?? new BlockerSettings();
        }
        catch
        {
            return new BlockerSettings();
        }
    }

    public void Save(BlockerSettings settings)
    {
        Directory.CreateDirectory(_dataDirectory);

        string json = JsonSerializer.Serialize(
            settings,
            _jsonOptions);

        File.WriteAllText(
            _settingsFile,
            json);
    }
}