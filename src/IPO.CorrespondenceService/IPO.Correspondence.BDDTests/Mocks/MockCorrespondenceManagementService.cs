using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;

namespace IPO.Correspondence.BDDTests.Mocks
{
    public class MockCorrespondenceManagementService : ICorrespondenceManagementService
    {
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

        public Task<NotificationViewModel> PeekNotificationAsync(Guid notificationId)
        {
            return Task.FromResult(new NotificationViewModel(new NotificationMessage()));
        }

        public Task<Guid> SendNotificationMessageAsync(NotificationMessage notificationMessage)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task<Guid> PostNotificationAsync(Guid organisationId, NotificationModel correspondence)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task<Guid> PostEmailWithFileNotificationAsync(PostEmailWithFileNotificationRequestModel model)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task<Guid> PostPrecompiledLetterRequestAsync(PrecompiledLetterRequestModel model)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task<TemplatePreviewResponse> GenerateTemplatePreviewAsync(TemplatePreviewRequest previewRequest)
        {
            return Task.FromResult(new TemplatePreviewResponse("", "", 1, "", ""));
        }

        public Task ProcessNotificationMessageAsync(string messagePayload, NotificationType notificationType)
        {
            return Task.CompletedTask;
        }

        public Task UpdateNotificationStatusesAsync()
        {
            return Task.CompletedTask;
        }
    }
}