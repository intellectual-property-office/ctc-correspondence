using Azure.Messaging.ServiceBus;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;
using System.Text;
using System.Threading.Tasks;

namespace IPO.Correspondence.Messaging
{
    public class MessagingClient : IMessagingClient
    {
        private ServiceBusSender _serviceBusSender;
        private readonly ICorrespondenceServiceValidator _correspondenceServiceValidator;

        public MessagingClient(ServiceBusSender serviceBusSender, ICorrespondenceServiceValidator correspondenceServiceValidator)
        {
            _serviceBusSender = serviceBusSender;
            _correspondenceServiceValidator = correspondenceServiceValidator;
        }

        public async Task SendNotificationMessageAsync(NotificationMessage notificationMessage)
        {
            var messageBody = IPOJsonSerialization.Serialize(notificationMessage);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

            if (notificationMessage.APIKeyType != null)
            {
                _correspondenceServiceValidator.VerifySpecifiedSendType(notificationMessage.APIKeyType.Value);
            }

            message.ApplicationProperties.Add(notificationMessage.Type.ToMessageType());

            await _serviceBusSender.SendMessageAsync(message);
        }
    }
}
