using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;

namespace IPO.Correspondence.BDDTests.Mocks
{
    public class MockMessagingClient : IMessagingClient
    {
        public Task SendNotificationMessageAsync(NotificationMessage notificationMessage)
        {
            return Task.CompletedTask;
        }
    }
}
