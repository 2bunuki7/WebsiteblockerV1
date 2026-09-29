using System;
using System.Windows;
using System.Windows.Media;
using WebsiteBlocker.Core.Models;
using WebsiteBlocker.Core.Services;

namespace WebsiteBlocker.App;

public partial class MainWindow : Window
{
    private readonly BlockerService _blockerService;
    private readonly ConfigurationService _configurationService;

    private bool _blockerEnabled;

    public MainWindow()
    {
        InitializeComponent();

        _blockerService = new BlockerService();
        _configurationService = new ConfigurationService();

        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            var settings = _configurationService.Load();

            WebsiteListBox.ItemsSource = settings.BlockedSites;

            _blockerEnabled = settings.Enabled;

            UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load settings:\n\n{ex.Message}",
                "Website Blocker",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddWebsite_Click(object sender, RoutedEventArgs e)
    {
        string website = WebsiteTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(website))
        {
            MessageBox.Show(
                "Enter a website first.",
                "Website Blocker",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        string domain = DomainMatcher.NormalizeDomain(website);

        if (string.IsNullOrWhiteSpace(domain))
        {
            MessageBox.Show(
                "That doesn't look like a valid domain.",
                "Website Blocker",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var settings = _configurationService.Load();

        if (settings.BlockedSites.Contains(
                domain,
                StringComparer.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                "That website is already blocked.",
                "Website Blocker",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        settings.BlockedSites.Add(domain);

        _configurationService.Save(settings);

        RefreshWebsiteList(settings);

        WebsiteTextBox.Clear();

        if (_blockerEnabled)
        {
            ApplyBlocking(settings);
        }
    }

    private void RemoveWebsite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button)
        {
            return;
        }

        if (button.DataContext is not string domain)
        {
            return;
        }

        var settings = _configurationService.Load();

        settings.BlockedSites.Remove(domain);

        _configurationService.Save(settings);

        RefreshWebsiteList(settings);

        if (_blockerEnabled)
        {
            ApplyBlocking(settings);
        }
    }

    private void ToggleBlocker_Click(object sender, RoutedEventArgs e)
    {
        var settings = _configurationService.Load();

        _blockerEnabled = !_blockerEnabled;

        settings.Enabled = _blockerEnabled;

        _configurationService.Save(settings);

        ApplyBlocking(settings);

        UpdateStatus();
    }

    private void ApplyBlocking(BlockerSettings settings)
    {
        try
        {
            if (settings.Enabled)
            {
                _blockerService.EnableBlocking(settings);
            }
            else
            {
                _blockerService.DisableBlocking();
            }
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(
                "Administrator privileges are required to modify the Windows hosts file.\n\n" +
                "Run Website Blocker as Administrator.",
                "Administrator Required",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            _blockerEnabled = false;

            settings.Enabled = false;

            _configurationService.Save(settings);

            UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred:\n\n{ex.Message}",
                "Website Blocker",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            _blockerEnabled = false;

            settings.Enabled = false;

            _configurationService.Save(settings);

            UpdateStatus();
        }
    }

    private void RefreshWebsiteList(BlockerSettings settings)
    {
        WebsiteListBox.ItemsSource = null;
        WebsiteListBox.ItemsSource = settings.BlockedSites;
    }

    private void UpdateStatus()
    {
        if (_blockerEnabled)
        {
            StatusIndicator.Fill = Brushes.LimeGreen;

            StatusText.Text = "Blocker ON";

            ToggleButton.Content = "Disable Blocker";
        }
        else
        {
            StatusIndicator.Fill = Brushes.Gray;

            StatusText.Text = "Blocker OFF";

            ToggleButton.Content = "Enable Blocker";
        }
    }
}