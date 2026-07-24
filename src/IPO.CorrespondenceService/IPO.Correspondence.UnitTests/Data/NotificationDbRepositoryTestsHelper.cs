
using IPO.Correspondence.Data.Notifications;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPO.Correspondence.UnitTests.Data
{
    [TestClass]
    [TestCategory("NotificationDbRepositoryTests")]
    public class NotificationDbRepositoryTestsHelper : NotificationDbRepository
    {
        
        public NotificationDbRepositoryTestsHelper(NotificationDbContext context, ILogger logger) 
            : base(context, logger)
        {
        }

        public Task TestUpdateMessageOwnerAsync(NotificationMessage message)
        {
            return base.UpdateMessageOwnerAsync(message);
        }
    }
}
