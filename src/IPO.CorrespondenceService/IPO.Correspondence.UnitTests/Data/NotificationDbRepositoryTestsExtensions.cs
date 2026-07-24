using AutoFixture;
using IPO.Correspondence.Data.Notifications;
using IPO.Correspondence.Models;

namespace IPO.Correspondence.UnitTests.Data
{

    public static class NotificationDbRepositoryTestsExtensions
    {
        public static async Task<NotificationOwner> AddTestOwner(this NotificationDbContext notificationDbContext, Fixture fixture, Guid? id = null)
        {
            var owner = fixture.Create<NotificationOwner>();
            if (id != null)
                owner.Id = id.Value;
            await notificationDbContext.Owners.AddAsync(owner);
            await notificationDbContext.SaveChangesAsync();
            return owner;
        }

        public static async Task<List<NotificationMessage>> AddTestNotificationMessageSetForOwner(
            this NotificationDbContext notificationDbContext, 
            Fixture fixture, 
            NotificationOwner owner, 
            int numberOfMessages,
            Action<NotificationMessage> postInitialise = null!)
        {
            var messages = fixture.CreateMany<NotificationMessage>(numberOfMessages).ToList();
            messages.ForEach(o =>
            {
                o.Owner = owner;
                postInitialise?.Invoke(o);
            });

            await notificationDbContext.Messages.AddRangeAsync(messages);
            await notificationDbContext.SaveChangesAsync();
            return messages;
        }

        public static async Task<List<NotificationMessage>> AddTestNotificationMessageSetForOwnerWithTimeStamp(this NotificationDbContext notificationDbContext, Fixture fixture, NotificationOwner owner, int numberOfMessages, DateTime timeStamp)
        {
            var messages = fixture.CreateMany<NotificationMessage>(numberOfMessages).ToList();
            messages.ForEach(o =>
            {
                o.OrganisationId = owner.Id;
                o.Owner = owner;
                o.CreatedOn = timeStamp;
            });
            await notificationDbContext.Messages.AddRangeAsync(messages);
            await notificationDbContext.SaveChangesAsync();
            return messages;
        }
    }
}
