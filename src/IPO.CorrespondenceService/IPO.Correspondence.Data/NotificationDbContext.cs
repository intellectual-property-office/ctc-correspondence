using IPO.Correspondence.Models;
using Microsoft.EntityFrameworkCore;

namespace IPO.Correspondence.Data.Notifications
{
    public class NotificationDbContext : DbContext, INotificationDbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<NotificationMessage> Messages { get; set; }
        public virtual DbSet<NotificationOwner> Owners { get; set; }
    }
}
