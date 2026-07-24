using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IPO.Correspondence.Email
{
    public class Email
    {
        private readonly ICorrespondenceManagementService _correspondenceManagementService;
        private readonly ILogger _logger;

        public Email(ICorrespondenceManagementService correspondenceManagementService, ILogger logger)
        {
            _correspondenceManagementService = correspondenceManagementService;
            _logger = logger;
        }

        [Function("Email")]
        public async Task Run([ServiceBusTrigger("%Topic%", "%Subscription%", Connection = "ServiceBusConnectionString")] string mySbMsg)
        {
            await _correspondenceManagementService.ProcessNotificationMessageAsync(mySbMsg, NotificationType.Email);
            _logger.LogInformation($"Email-Correspondence ServiceBus topic trigger function processed message: {mySbMsg}");
        }
    }
}