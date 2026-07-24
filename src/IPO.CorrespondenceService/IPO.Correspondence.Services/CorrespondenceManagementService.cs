using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Logging;

namespace IPO.Correspondence.Services
{
    public class CorrespondenceManagementService : ICorrespondenceManagementService
    {
        private readonly INotificationDbRepository _repository;
        private readonly IMessagingClient _messagingClient;
        private readonly IFileStorageGateway _fileStorageGateway;
        private readonly IPreliminaryFileValidator _fileValidator;
        private readonly INotifyGateway _notifyGateway;
        private readonly ILogger _logger;

        public CorrespondenceManagementService(
           INotificationDbRepository repository,
           IMessagingClient messagingClient,
           IFileStorageGateway fileStorageGateway,
           IPreliminaryFileValidator fileValidator,
           INotifyGateway notifyGateway,
           ILogger logger)
        {
            _repository = repository;
            _messagingClient = messagingClient;
            _fileStorageGateway = fileStorageGateway;
            _fileValidator = fileValidator;
            _notifyGateway = notifyGateway;
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationViewModel>> GetPendingNotificationsAsync(Guid ownerId, bool isInternalRead)
            => await _repository.GetPendingNotificationsAsync(ownerId, isInternalRead);

        public async Task<IEnumerable<NotificationViewModel>> GetAllNotificationsFromTimeStampAsync(Guid ownerId, DateTime from, DateTime to, bool isInternalRead)
            => await _repository.GetAllNotificationsFromTimeStampAsync(ownerId, from, to, isInternalRead);

        public async Task<NotificationViewModel> PeekNotificationAsync(Guid notificationId)
            => await _repository.PeekNotificationAsync(notificationId);

        public async Task<Guid> SendNotificationMessageAsync(NotificationMessage notificationMessage)
        {
            await _messagingClient.SendNotificationMessageAsync(notificationMessage);
            return notificationMessage.Id;
        }

        public async Task<Guid> PostNotificationAsync(Guid organisationId, NotificationModel correspondence)
        {
            var notificationMessage = new NotificationMessage
            {
                CreatedOn = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                OrganisationId = organisationId,
                Status = NotificationStatus.SendingToNotify,
                Type = correspondence.Channel!.Value,
                APIKeyType = correspondence.SendType ?? null,
                PayLoad = IPOJsonSerialization.Serialize(correspondence.Content)
            };

            await _messagingClient.SendNotificationMessageAsync(notificationMessage);
            return notificationMessage.Id;
        }

        public async Task<Guid> PostEmailWithFileNotificationAsync(PostEmailWithFileNotificationRequestModel model)
        {
            _fileValidator.Validate(model.File!);

            using var memoryStream = new MemoryStream();
            await model.File!.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var fileId = await _fileStorageGateway.UploadAttachmentAsync(memoryStream);

            model.Content!.Add(nameof(EmailContent.FileName), model.File.FileName);
            model.Content.Add(nameof(EmailContent.FileId), fileId);

            return await PostNotificationAsync(
                model.organisationId,
                new NotificationModel
                {
                    Channel = NotificationType.Email,
                    SendType = model.SendType,
                    Content = model.Content,
                });
        }

        public async Task<Guid> PostPrecompiledLetterRequestAsync(PrecompiledLetterRequestModel model)
        {
            _fileValidator.ValidatePrecompiledLetter(model.File!);

            using var memoryStream = new MemoryStream();
            await model.File!.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var fileId = await _fileStorageGateway.UploadAttachmentAsync(memoryStream);

            var content = new Dictionary<string, object?>
            {
                { nameof(PrecompiledLetterContent.FileName), model.File.FileName },
                { nameof(PrecompiledLetterContent.FileId), fileId },
                { nameof(PrecompiledLetterContent.ClientReference), Guid.NewGuid() },
                { nameof(PrecompiledLetterContent.Postage), model.Postage != null
                    ? Enum.GetName(typeof(PrecompiledLetterPostage), model.Postage)
                    : Enum.GetName(typeof(PrecompiledLetterPostage), PrecompiledLetterPostage.SecondClass) }
            };

            return await PostNotificationAsync(
                model.organisationId,
                new NotificationModel
                {
                    Channel = NotificationType.PrecompiledLetter,
                    SendType = model.SendType,
                    Content = content!
                });
        }

        public async Task<TemplatePreviewResponse> GenerateTemplatePreviewAsync(TemplatePreviewRequest previewRequest)
            => await _notifyGateway.GenerateTemplatePreviewAsync(previewRequest);

        public async Task ProcessNotificationMessageAsync(string message, NotificationType notificationType)
        {
            if (String.IsNullOrWhiteSpace(message))
            {
                var error = Error.Validation;
                error.Description += $". Message payload cannot be null or empty: {message}";
                throw new StatusCodeException(error, "", null, 422);
            }

            switch (notificationType)
            {
                case NotificationType.API:
                    await ProcessApiNotificationAsync(message!);
                    break;

                case NotificationType.SMS:
                    await ProcessSmsNotificationAsync(message!);
                    break;

                case NotificationType.Email:
                    await ProcessEmailNotificationAsync(message!);
                    break;

                case NotificationType.Letter:
                case NotificationType.PrecompiledLetter:
                    await ProcessLetterNotificationAsync(message!);
                    break;
            }
        }

        public async Task UpdateNotificationStatusesAsync()
        {
            var messages = await _repository.GetMessagesForStatusUpdateAsync();

            if (!messages.Any())
            {
                return;
            }

            await _notifyGateway.CheckStatusUpdatesAsync(messages);
            await _repository.SaveChangesAsync();

            var attachmentIds = messages.ToList().ToEmailAttachmentIdsList();
            if (attachmentIds.Any())
            {
                await _fileStorageGateway.DeleteAttachmentsAsync(attachmentIds);
            }
        }

        private async Task ProcessApiNotificationAsync(string mySbMsg)
        {
            var message = IPOJsonSerialization.Deserialize<NotificationMessage>(mySbMsg);
            var content = IPOJsonSerialization.Deserialize<ApiContent>(message!.PayLoad!);

            message.Owner = new NotificationOwner { Email = content!.Email!, Id = message.OrganisationId, TemplateId = content.TemplateId };
            message.Status = NotificationStatus.DeliveredConfirmed;

            await _repository.StoreAsync(message, null!);
        }

        private async Task ProcessSmsNotificationAsync(string mySbMsg)
        {
            var message = IPOJsonSerialization.Deserialize<NotificationMessage>(mySbMsg);
            var smsContent = IPOJsonSerialization.Deserialize<SmsContent>(message!.PayLoad!);
            var sendType = message.APIKeyType.ToString();

            message.Status = NotificationStatus.SendingToProvider;

            try
            {
                var externalId = await _notifyGateway.SendSmsAsync(smsContent!, sendType!);
                await _repository.StoreAsync(message, externalId);

                _logger.LogInformation("Successfully processed SMS message. Correspondence id: {MessageId}, external id: {ExternalId}",
                    message.Id, externalId);
            }
            catch (Exception ex)
            {
                var errorStatus = SetErrorStatus(ex);
                var errorMessage = SetErrorMessage(ex);

                message.Status = errorStatus;
                message.Errors = errorMessage;

                await _repository.StoreAsync(message);

                _logger.LogError("An error occurred processing SMS message. Correspondence (id:{MessageId})\nDetails: {ExceptionMessage}",
                    message.Id, ex.Message);
            }
        }

        private async Task ProcessEmailNotificationAsync(string mySbMsg)
        {
            var message = IPOJsonSerialization.Deserialize<NotificationMessage>(mySbMsg);
            var emailContent = IPOJsonSerialization.Deserialize<EmailContent>(message!.PayLoad!);
            var sendType = message.APIKeyType.ToString();

            byte[] fileBytes = null!;
            if (!string.IsNullOrWhiteSpace(emailContent!.FileId))
            {
                emailContent.Personalisation ??= new Dictionary<string, object>();
                var fileStream = await _fileStorageGateway.GetFileStreamAsync(emailContent.FileId);
                fileBytes = fileStream.ToArray();
            }

            message.Status = NotificationStatus.SendingToProvider;

            try
            {
                var externalId = await _notifyGateway.SendEmailAsync(emailContent, fileBytes, sendType!);
                await _repository.StoreAsync(message, externalId);

                _logger.LogInformation("Successfully processed Email message. Correspondence id: {MessageId}, external id: {ExternalId}",
                    message.Id, externalId);
            }
            catch (Exception ex)
            {
                var errorStatus = SetErrorStatus(ex);
                var errorMessage = SetErrorMessage(ex);

                message.Status = errorStatus;
                message.Errors = errorMessage;

                await _repository.StoreAsync(message);

                _logger.LogError("An error occurred processing Email message. Correspondence (id:{MessageId})\nDetails: {ExceptionMessage}",
                    message.Id, ex.Message);
            }
        }

        private async Task ProcessLetterNotificationAsync(string mySbMsg)
        {
            string sendType = string.Empty;
            var externalId = string.Empty;
            var message = IPOJsonSerialization.Deserialize<NotificationMessage>(mySbMsg);

            sendType = message!.APIKeyType!.ToString()!;
            message.Status = NotificationStatus.SendingToRecipient;

            try
            {
                switch (message.Type)
                {
                    case NotificationType.Letter:
                        var letterContent = IPOJsonSerialization.Deserialize<LetterContent>(message!.PayLoad!);

                        externalId = await _notifyGateway.SendLetterAsync(letterContent!, sendType);
                        await _repository.StoreAsync(message, externalId);
                        _logger.LogInformation("Successfully processed Letter message. Correspondence id: {MessageId}, external id: {ExternalId}",
                            message.Id, externalId);
                        break;

                    case NotificationType.PrecompiledLetter:
                        var precompiledLetterContent = IPOJsonSerialization.Deserialize<PrecompiledLetterContent>(message!.PayLoad!);
                        var fileStream = await _fileStorageGateway.GetFileStreamAsync(precompiledLetterContent!.FileId!);
                        byte[] pdfContents = fileStream.ToArray();

                        externalId = await _notifyGateway.SendPrecompiledLetterAsync(precompiledLetterContent, pdfContents, sendType);
                        await _repository.StoreAsync(message, externalId);
                        _logger.LogInformation("Successfully processed PrecompiledLetter message. Correspondence id: {MessageId}, external id: {ExternalId}",
                            message.Id, externalId);
                        break;
                }
            }
            catch (Exception ex)
            {
                var errorStatus = SetErrorStatus(ex);
                var errorMessage = SetErrorMessage(ex);

                message.Status = errorStatus;
                message.Errors = errorMessage;

                await _repository.StoreAsync(message);

                _logger.LogError("An error occurred processing Letter message. Correspondence (id:{MessageId})\nDetails: {ExceptionMessage}",
                    message.Id, ex.Message);
            }
        }

        private string SetErrorMessage(Exception ex)
        {
            var errorMessage = string.Empty;

            if (ex.Source == "GovukNotify")
            {
                errorMessage = ex.Message;
            }
            else if (ex.InnerException != null && ex.InnerException.Source == "GovukNotify")
            {
                errorMessage = ex.InnerException.Message;
            }

            return errorMessage;
        }

        private NotificationStatus SetErrorStatus(Exception e)
        {
            var errorStatus = NotificationStatus.Unknown;

            if (e.Source == "GovukNotify")
            {
                errorStatus = e.Message switch
                {
                    string a when a.Contains("Status code 400") || a.Contains("Status code 404") => NotificationStatus.UserError,
                    string b when b.Contains("Status code 403") || b.Contains("Status code 429") => NotificationStatus.Rejected,
                    string c when c.Contains("Status code 500") => NotificationStatus.SystemFailure,
                    _ => NotificationStatus.Unknown
                };
            }
            else if (e.InnerException != null && e.InnerException.Source == "GovukNotify")
            {
                errorStatus = e.InnerException.Message switch
                {
                    string a when a.Contains("Status code 400") || a.Contains("Status code 404") => NotificationStatus.UserError,
                    string b when b.Contains("Status code 403") || b.Contains("Status code 429") => NotificationStatus.Rejected,
                    string c when c.Contains("Status code 500") => NotificationStatus.SystemFailure,
                    _ => NotificationStatus.Unknown
                };
            }

            return errorStatus;
        }
    }
}