namespace WebsiteBlocker.App.ViewModels;

public class ScheduleViewModel
{
    public bool Enabled { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }
}