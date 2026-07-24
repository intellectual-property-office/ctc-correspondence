using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using IPO.Common.Infrastructure;

namespace IPO.Correspondence.Models
{
	public static class NotificationMessageExtensions
    {
        public static IEnumerable<string> ToEmailAttachmentIdsList(this List<NotificationMessage> notificationMessages)
        {
            foreach (var notificationMessage in notificationMessages.Where(o => o.Type == NotificationType.Email && (o.IsFailed() || o.IsCompleted())))
            {
                yield return notificationMessage.ToEmailAttachmentId()!;
            }
        }

        public static string? ToEmailAttachmentId(this NotificationMessage notificationMessage)
        {
            try
            {
                var payLoad = IPOJsonSerialization.Deserialize<EmailContent>(notificationMessage?.PayLoad!);

                return payLoad?.FileId;
            }
            catch (Exception)
			{
                return null!;
            }
        }


        public static bool IsFailed(this NotificationMessage notificationMessage)
        {
            switch (notificationMessage.Status)
            {
                case NotificationStatus.Unknown:
                case NotificationStatus.Rejected:
                case NotificationStatus.UserError:
                case NotificationStatus.SystemFailure:
                case NotificationStatus.ValidationFailed:
                case NotificationStatus.VirusScanFailed:
                case NotificationStatus.Cancelled:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCompleted(this NotificationMessage notificationMessage)
        {
            switch (notificationMessage.Status)
            {
                case NotificationStatus.DeliveredConfirmed:
                case NotificationStatus.DeliveredUnconfirmed:
                    return true;
                default:
                    return false;
            }
        }
    }
}
