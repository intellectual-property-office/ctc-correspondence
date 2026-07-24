using System.Collections.Generic;

namespace IPO.Correspondence.Models
{
    public static class NotificationTypeExtensions
    {
        public static KeyValuePair<string, object> ToMessageType(this NotificationType notificationType)
        {
            var msgType = new KeyValuePair<string, object>();

            switch (notificationType)
            {
                case NotificationType.Email:
                    msgType = new KeyValuePair<string, object>("MessageType", "Email");
                    break;
                case NotificationType.Letter:
                    msgType = new KeyValuePair<string, object>("MessageType", "Letter");
                    break;
                case NotificationType.PrecompiledLetter:
                    msgType = new KeyValuePair<string, object>("MessageType", "Letter");
                    break;
                case NotificationType.API:
                    msgType = new KeyValuePair<string, object>("MessageType", "API");
                    break;
                case NotificationType.SMS:
                    msgType = new KeyValuePair<string, object>("MessageType", "SMS");
                    break;
                default:
                    break;
            }

            return msgType;
        }
    }
}
