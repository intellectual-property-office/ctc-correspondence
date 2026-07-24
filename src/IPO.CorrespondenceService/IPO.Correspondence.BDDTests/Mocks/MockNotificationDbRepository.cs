using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;

namespace IPO.Correspondence.BDDTests.Mocks
{
    public class MockNotificationDbRepository : INotificationDbRepository
    {
        public Guid SystemOwnerId => Guid.Parse("00000000-0000-0000-0000-000000000001");

        public Task NotifyOwnersForPendingNotificationsAsync(Func<IEnumerable<NotificationOwner>, Task> notifyChangesAsync)
        {
            return Task.CompletedTask;
        }

        public Task StoreAsync(NotificationMessage message, string externalId = null!)
        {
            return Task.CompletedTask;
        }

        public Task<NotificationViewModel> PeekNotificationAsync(Guid notificationId)
        {
            return Task.FromResult(new NotificationViewModel(new NotificationMessage()));
        }

        public Task<IEnumerable<NotificationViewModel>> GetPendingNotificationsAsync(Guid ownerId, bool isInternalRead)
        {
            return Task.FromResult<IEnumerable<NotificationViewModel>>(
                new List<NotificationViewModel> { new NotificationViewModel(new NotificationMessage(), ownerId) });
        }

        public Task<IEnumerable<NotificationViewModel>> GetAllNotificationsFromTimeStampAsync(Guid ownerId, DateTime from, DateTime to, bool isInternalRead)
        {
            return Task.FromResult<IEnumerable<NotificationViewModel>>(
                new List<NotificationViewModel> { new NotificationViewModel(new NotificationMessage(), ownerId) });
        }

        public Task<IEnumerable<NotificationMessage>> GetMessagesForStatusUpdateAsync()
        {
            return Task.FromResult<IEnumerable<NotificationMessage>>(
                new List<NotificationMessage> { new NotificationMessage() });
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}