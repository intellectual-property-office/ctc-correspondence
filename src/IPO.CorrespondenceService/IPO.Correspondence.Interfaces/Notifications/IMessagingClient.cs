using System.Threading.Tasks;
using IPO.Correspondence.Models;

namespace IPO.Correspondence.Interfaces.Notifications
{
    public interface IMessagingClient
    {
        Task SendNotificationMessageAsync(NotificationMessage notificationMessage);
    }
}
