using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IPO.Correspondence.Letter
{
    public class Letter
    {
        private readonly ICorrespondenceManagementService _correspondenceManagementService;
        private readonly ILogger _logger;

        public Letter(ICorrespondenceManagementService correspondenceManagementService, ILogger logger)
        {
            _correspondenceManagementService = correspondenceManagementService;
            _logger = logger;
        }

        [Function("Letter")]
        public async Task Run([ServiceBusTrigger("%Topic%", "%Subscription%", Connection = "ServiceBusConnectionString")] string mySbMsg)
        {
            await _correspondenceManagementService.ProcessNotificationMessageAsync(mySbMsg, NotificationType.Letter);
            _logger.LogInformation($"Letter-Correspondence ServiceBus topic trigger function processed message: {mySbMsg}");
        }
    }
}