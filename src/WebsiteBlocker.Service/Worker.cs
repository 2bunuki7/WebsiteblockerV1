using Microsoft.Extensions.Hosting;
using WebsiteBlocker.Core.Services;

namespace WebsiteBlocker.Service;

public class Worker : BackgroundService
{
    private readonly ConfigurationService _configurationService;
    private readonly BlockerService _blockerService;
    private readonly Logger _logger;

    public Worker()
    {
        _configurationService = new ConfigurationService();
        _blockerService = new BlockerService();
        _logger = new Logger();
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.Log("Website Blocker service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var settings = _configurationService.Load();

                if (settings.Enabled)
                {
                    _blockerService.EnableBlocking(settings);
                }
                else
                {
                    _blockerService.DisableBlocking();
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error: {ex.Message}");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }

        _logger.Log("Website Blocker service stopped.");
    }
}