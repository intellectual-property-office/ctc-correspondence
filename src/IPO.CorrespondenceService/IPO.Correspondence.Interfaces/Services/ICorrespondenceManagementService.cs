using IPO.Correspondence.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPO.Correspondence.Interfaces.Services
{
    public interface ICorrespondenceManagementService
    {
        // Database
        Task<IEnumerable<NotificationViewModel>> GetPendingNotificationsAsync(Guid ownerId, bool isInternalRead);
        Task<IEnumerable<NotificationViewModel>> GetAllNotificationsFromTimeStampAsync(Guid ownerId, DateTime from, DateTime to, bool isInternalRead);
        Task<NotificationViewModel> PeekNotificationAsync(Guid notificationId);

        // Messaging
        Task<Guid> SendNotificationMessageAsync(NotificationMessage notificationMessage);
        Task<Guid> PostNotificationAsync(Guid organisationId, NotificationModel correspondence);

        // File Operations
        Task<Guid> PostEmailWithFileNotificationAsync(PostEmailWithFileNotificationRequestModel model);
        Task<Guid> PostPrecompiledLetterRequestAsync(PrecompiledLetterRequestModel model);

        // Notify Client
        Task<TemplatePreviewResponse> GenerateTemplatePreviewAsync(TemplatePreviewRequest previewRequest);

        Task ProcessNotificationMessageAsync(string messagePayload, NotificationType notificationType);
        Task UpdateNotificationStatusesAsync();

    }
}
