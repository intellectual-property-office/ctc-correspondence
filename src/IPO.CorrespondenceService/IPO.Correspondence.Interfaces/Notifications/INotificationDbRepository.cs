using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPO.Correspondence.Models;

namespace IPO.Correspondence.Interfaces.Notifications
{
    public interface INotificationDbRepository
    {
        Guid SystemOwnerId { get; }
        Task NotifyOwnersForPendingNotificationsAsync(Func<IEnumerable<NotificationOwner>, Task> notifyChangesAsync);

        Task StoreAsync(NotificationMessage message, string externalId = null!);
        Task<NotificationViewModel> PeekNotificationAsync(Guid notificationId);
        Task<IEnumerable<NotificationViewModel>> GetPendingNotificationsAsync(Guid ownerId, bool isInternalRead);
        Task<IEnumerable<NotificationViewModel>> GetAllNotificationsFromTimeStampAsync(Guid ownerId, DateTime from, DateTime to, bool isInternalRead);
        Task<IEnumerable<NotificationMessage>> GetMessagesForStatusUpdateAsync();
        Task SaveChangesAsync();
    }
}