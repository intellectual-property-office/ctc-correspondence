using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IPO.Correspondence.APIFunction
{
    public class APIFunction
    {
        private readonly ICorrespondenceManagementService _correspondenceManagementService;
        private readonly ILogger _logger;

        public APIFunction(ICorrespondenceManagementService correspondenceManagementService, ILogger logger)
        {
            _correspondenceManagementService = correspondenceManagementService;
            _logger = logger;
        }

        [Function("API")]
        public async Task RunAsync([ServiceBusTrigger("%Topic%", "%Subscription%", Connection = "ServiceBusConnectionString")] string mySbMsg)
        {
            await _correspondenceManagementService.ProcessNotificationMessageAsync(mySbMsg, NotificationType.API);
            _logger.LogInformation($"API-Correspondence ServiceBus topic trigger function processed message: {mySbMsg}");
        }
    }
}