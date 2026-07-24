using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Notify.Exceptions;

namespace IPO.Correspondence.Health
{
    public class GovNotifyHealthCheck : IHealthCheck
    {
        #region Fields and constructors

        private readonly INotifyGateway? _notifyGateway;
        //private readonly IServiceProvider _serviceProvider;
        private const string _templateId = "00000000-0000-0000-0000-000000000000";

        public GovNotifyHealthCheck(IServiceProvider serviceProvider)
        {
            //_serviceProvider = serviceProvider;

            _notifyGateway = serviceProvider.GetService<INotifyGateway>();
        }

        #endregion

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var previewRequest = new TemplatePreviewRequest
            {
                Content = new Dictionary<string, object> { { "templateId", _templateId }, }
            };

            try
            {
                if (_notifyGateway != null)
                {
                    var result = await _notifyGateway.GenerateTemplatePreviewAsync(previewRequest);
                    return new HealthCheckResult(status: HealthStatus.Healthy, string.Empty, exception: null);
                }

                return new HealthCheckResult(status: HealthStatus.Unhealthy, "Gateway is NULL");
            }
            catch (Exception ex)
            {
                // Currently there are issues with the format of the message 
                // returned from GovNotify
                // https://github.com/alphagov/notifications-net-client/issues/161
                // so check it is an error from Gov Notify with expected text fragments:

                if (
                    (ex.InnerException is not null &&
                    ex.InnerException is StatusCodeException se &&
                    se.Message.Contains("\"error\": \"NoResultFound\"") &&
                    se.Message.Contains("\"message\": \"No result found\""))
                    ||
                    (ex.InnerException is not null &&
                    ex.InnerException.InnerException is not null &&
                    ex.InnerException.InnerException is NotifyClientException notifyEx &&
                    notifyEx.Message.Contains("\"error\": \"NoResultFound\"") &&
                    notifyEx.Message.Contains("\"message\": \"No result found\""))
                    )
                {
                    return new HealthCheckResult(status: HealthStatus.Healthy, string.Empty, exception: null);
                }

                return new HealthCheckResult(status: HealthStatus.Unhealthy, ex.Message, exception: ex);
            }
        }
    }
}