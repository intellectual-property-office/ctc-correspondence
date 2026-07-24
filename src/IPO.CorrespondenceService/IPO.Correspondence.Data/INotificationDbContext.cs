using IPO.Correspondence.Models;
using Microsoft.EntityFrameworkCore;

namespace IPO.Correspondence.Data.Notifications
{
    public interface INotificationDbContext
    {
        DbSet<NotificationMessage> Messages { get; set; }
        DbSet<NotificationOwner> Owners { get; set; }
    }
}