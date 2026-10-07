using AutoFixture;
using IPO.Common.Infrastructure.IPOAppInsightsLogger;
using IPO.Correspondence.APIWebJob;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPO.Correspondence.UnitTests.APIWebJob
{
    [TestClass]
    public class APIWebJobHelperTests : Function
    { 
        private readonly Fixture _fixture;
        public APIWebJobHelperTests(IMessagingClient client, INotificationDbRepository notificationRepository, IIPOAppInsightsLogger appInsightsLogger) : base(client, notificationRepository, appInsightsLogger)
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
