using IPO.Common.Infrastructure;
using IPO.Common.Infrastructure.IPOAppInsightsLogger;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPO.Correspondence.APIWebJob
{
    public class Function
    {
        private readonly IIPOAppInsightsLogger _appInsightsLogger;
        private readonly IMessagingClient _client;
        private readonly INotificationDbRepository _repository;

        public Function(IMessagingClient client, INotificationDbRepository notificationRepository, IIPOAppInsightsLogger appInsightsLogger)
        {
            _client = client;
            _repository = notificationRepository;
            _appInsightsLogger = appInsightsLogger;
        }

        [NoAutomaticTrigger]
        [FunctionName("SendEmailNotificationsToOwners")]
        public async Task RunAsync()
        {
            _appInsightsLogger.ProcessingStarted();
            try
            {
                await _repository.NotifyOwnersForPendingNotificationsAsync(SendNotificationToOwnersAsync);
                _appInsightsLogger.ProcessingComplete();
            }
            catch (Exception ex)
            {
                _appInsightsLogger.Error(ex);
                throw;
            }
        }

        protected async virtual Task SendNotificationToOwnersAsync(IEnumerable<NotificationOwner> owners)
        {
            foreach (var owner in owners)
            {
                if (string.IsNullOrWhiteSpace(owner.Email) || string.IsNullOrWhiteSpace(owner.TemplateId))
                    continue;
                await _client.SendNotificationMessageAsync(CreateNotificationMessage(owner));
            }
        }

        protected virtual NotificationMessage CreateNotificationMessage(NotificationOwner owner)
        {
            var emailContent = new EmailContent
            {
                EmailAddress = owner.Email,
                TemplateId = owner.TemplateId
            };

            var notificationMessage = new NotificationMessage
            {
                CreatedOn = DateTime.UtcNow,
                Id = Guid.Empty,
                OrganisationId = owner.Id,
                Status = NotificationStatus.SendingToNotify,
                Type = NotificationType.Email,
                PayLoad = IPOJsonSerialization.Serialize(emailContent),
                Owner = new NotificationOwner { Id = _repository.SystemOwnerId }
            };
            return notificationMessage;
        }
    }
}