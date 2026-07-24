using IPO.Correspondence.Interfaces.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IPO.Correspondence.StatusUpdate
{
    public class StatusUpdate
    {
        private readonly ICorrespondenceManagementService _correspondenceManagementService;
        private readonly ILogger _logger;

        public StatusUpdate(ICorrespondenceManagementService correspondenceManagementService, ILogger logger)
        {
            _correspondenceManagementService = correspondenceManagementService;
            _logger = logger;
        }

        [Function("StatusUpdate")]
        public async Task Run([TimerTrigger("%CronPattern%")] TimerInfo myTimer)
        {
            _logger.LogInformation($"StatusUpdate-Correspondence executed on timer at {DateTime.Now}");
            await _correspondenceManagementService.UpdateNotificationStatusesAsync();
        }
    }
}