using IPO.Common.Infrastructure;
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
        private readonly ILogger<Function> _logger;
        private readonly IMessagingClient _client;
        private readonly INotificationDbRepository _repository;

        public Function(IMessagingClient client, INotificationDbRepository notificationRepository, ILogger<Function> logger)
        {
            _client = client;
            _logger = logger;
            _repository = notificationRepository;
        }

        [NoAutomaticTrigger]
        [FunctionName("SendEmailNotificationsToOwners")]
        public async Task RunAsync()
        {
            _logger.LogInformation($"API-WebJob-Correspondence Timer trigger web job executed at: {DateTime.Now}");

            await _repository.NotifyOwnersForPendingNotificationsAsync(SendNotificationToOwnersAsync);
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
