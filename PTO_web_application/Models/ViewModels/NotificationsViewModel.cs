namespace PTO_web_application.Models.ViewModels;

public class NotificationsViewModel
{
    public List<NotificationItem> Notifications { get; set; } = new();
}

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DateText { get; set; } = string.Empty;
    public string ColorType { get; set; } = "info"; // warning, success, info
}