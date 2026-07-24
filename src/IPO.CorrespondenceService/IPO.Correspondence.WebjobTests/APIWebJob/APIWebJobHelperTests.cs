using AutoFixture;
using IPO.Correspondence.APIWebJob;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace IPO.Correspondence.UnitTests.APIWebJob
{
    [TestClass]
    public class APIWebJobHelperTests : Function
    { 
        private readonly Fixture _fixture;
        public APIWebJobHelperTests(IMessagingClient client, INotificationDbRepository notificationRepository, ILogger<Function> logger) : base(client, notificationRepository, logger)
        { 
            _fixture = new Fixture();
        }

        public NotificationMessage TestCreateNotificationMessage(NotificationOwner owner)
        {
            return base.CreateNotificationMessage(owner);
        }

        public Task TestSendNotificationToOwnersAsync(IEnumerable<NotificationOwner> owners)
        {
            return base.SendNotificationToOwnersAsync(owners);
        }
    }
}
