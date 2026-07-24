using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IPO.Correspondence.SMS
{
    public class Sms
    {
        private readonly ICorrespondenceManagementService _correspondenceManagementService;
        private readonly ILogger _logger;

        public Sms(ICorrespondenceManagementService correspondenceManagementService, ILogger logger)
        {
            _correspondenceManagementService = correspondenceManagementService;
            _logger = logger;
        }

        [Function("Sms")]
        public async Task Run([ServiceBusTrigger("%Topic%", "%Subscription%", Connection = "ServiceBusConnectionString")] string mySbMsg)
        {
            await _correspondenceManagementService.ProcessNotificationMessageAsync(mySbMsg, NotificationType.SMS);
            _logger.LogInformation($"SMS-Correspondence ServiceBus topic trigger function processed message: {mySbMsg}");
        }
    }
}