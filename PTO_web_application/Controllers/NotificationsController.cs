using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;

namespace PTO_web_application.Controllers;

public class NotificationsController : Controller
{
    private readonly Supabase.Client _supabase;

    public NotificationsController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var idNumber = HttpContext.Session.GetString("sb-id-number");
        if (string.IsNullOrEmpty(idNumber))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userId = HttpContext.Session.GetString("sb-user-id");
        var model = new NotificationsViewModel();

        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                // Fetch notifications safely
                var response = await _supabase
                    .From<Models.Notification>()
                    .Where(n => n.UserId == Guid.Parse(userId))
                    .Order(n => n.DateSent, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                foreach (var notif in response.Models)
                {
                    model.Notifications.Add(new NotificationItem
                    {
                        Title = "System Notification",
                        Description = notif.Message,
                        DateText = notif.DateSent.ToString("dd MMM yyyy"),
                        ColorType = notif.IsRead ? "info" : "warning"
                    });
                }
            }
            catch (Exception ex)
            {
                // If the database fails, we just show the empty page instead of crashing
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
            }
        }

        return View(model);
    }
}