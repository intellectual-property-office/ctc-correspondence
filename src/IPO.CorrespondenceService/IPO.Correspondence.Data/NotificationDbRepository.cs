using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IPO.Common.Infrastructure;

namespace IPO.Correspondence.Data.Notifications
{
    public class NotificationDbRepository : INotificationDbRepository
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger _logger;

        public Guid SystemOwnerId => Guid.Parse("00000000-0000-0000-0000-000000000001");

        public NotificationDbRepository(NotificationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationViewModel>> GetPendingNotificationsAsync(Guid ownerId, bool isInternalRead)
        {
            Expression<Func<NotificationMessage, bool>> filteringBasedOnConsumer =
                isInternalRead ? x => !x.InternalRead : x => !x.ExternalRead;

            var messages = await _context
                .Messages
                .Where(x => x.OrganisationId == ownerId)
                .Where(x => x.Owner!.Id != SystemOwnerId)
                .Where(filteringBasedOnConsumer)
                .OrderBy(x => x.CreatedOn)
                .Take(500)
                .ToListAsync();

            if (isInternalRead)
                messages.ForEach(x => x.InternalRead = true);
            else
                messages.ForEach(x => x.ExternalRead = true);

            var result = messages.Select(x => new NotificationViewModel(x, ownerId)).ToList();
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<NotificationViewModel>> GetAllNotificationsFromTimeStampAsync(Guid ownerId,
            DateTime from, DateTime to,
            bool isInternalRead)
        {
            var messages = await _context
                .Messages
                .Where(x => x.OrganisationId == ownerId)
                .Where(x => x.Owner!.Id != SystemOwnerId)
                .Where(x => x.CreatedOn >= from && x.CreatedOn <= to)
                .OrderBy(x => x.CreatedOn)
                .Take(1000)
                .ToListAsync();

            if (isInternalRead)
                messages.ForEach(x => x.InternalRead = true);
            else
                messages.ForEach(x => x.ExternalRead = true);

            var result = messages.Select(x => new NotificationViewModel(x, ownerId)).ToList();
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<NotificationViewModel> PeekNotificationAsync(Guid notificationId)
        {
            var message = await _context
                .Messages
                .Include(x => x.Owner)
                .FirstOrDefaultAsync(x => x.Id == notificationId);

            if (message == null)
            {
                var error = Error.GetError<NotificationDbRepository>();
                error.Description += $"Correspondence not found.";
                throw new StatusCodeException(error, "Issue with the Correspondence MicroService", null, 422);
            }

            var result = new NotificationViewModel(message);
            return result;
        }

        public async Task StoreAsync(NotificationMessage message, string externalId = null!)
        {
            await UpdateMessageOwnerAsync(message);
            var existingMessage = await _context.Messages.FindAsync(message.Id);

            if (existingMessage != null)
            {
                existingMessage.Owner = message.Owner;
                existingMessage.ExternalId = externalId;
                existingMessage.Errors = String.Empty;
                existingMessage.Status = message.Status;
            }
            else
            {
                message.ExternalId = externalId;
                _context.Messages.Add(message);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully processed message. Type:{MessageType}, correspondence id: {MessageId}, external id: {ExternalId}",
                message.Type.ToString(), message.Id, externalId);
        }

        public async Task<IEnumerable<NotificationMessage>> GetMessagesForStatusUpdateAsync()
        {
            var statusList = new[] { NotificationStatus.SendingToProvider, NotificationStatus.SendingToRecipient }
                .Cast<int>()
                .ToArray();

            var results = await _context
                .Messages
                .Where(m => m.Type != NotificationType.API)
                .Where(m => statusList.Contains((int)m.Status))
                .Take(500)
                .ToListAsync();

            return results;
        }

        public async Task NotifyOwnersForPendingNotificationsAsync(Func<IEnumerable<NotificationOwner>, Task> notifyChangesAsync)
        {
            var owners = await _context
                .Messages
                .Include(x => x.Owner)
                .Where(x => x.Type == NotificationType.API)
                .Where(x => !x.ExternalRead)
                .Select(x => x.Owner)
                .Distinct()
                .ToListAsync();

            await notifyChangesAsync(owners!);
        }

        protected virtual async Task UpdateMessageOwnerAsync(NotificationMessage message)
        {
            if (message.Owner == null)
            {
                return;
            }

            var dbOwner = await _context.Owners.FirstOrDefaultAsync(x => x.Id == message.Owner.Id);

            if (dbOwner == null)
            {
                return;
            }

            dbOwner.Email = message.Owner.Email;
            dbOwner.TemplateId = message.Owner.TemplateId;
            message.Owner = dbOwner;
        }
       
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}