using System.Net;
using IPO.CTC.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IPO.Correspondence.Health
{
    public class IpoApimServiceReadyCheck : IHealthCheck
    {
        public const string HealthyMessage = "Service is available";

        public const string UnhealthyMessage = "Service is not available";

        private readonly HttpClient _client;

        public readonly string ReadyPath = $"health/{HealthTags.Ready}";

        private string _routeStarts = string.Empty;

        public IpoApimServiceReadyCheck(HttpClient client, string routeStarts = "")
        {
            _client = client;

            _routeStarts = routeStarts;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = string.Join("/", new[] { _routeStarts, ReadyPath });
            var response = await _client.GetAsync(route, cancellationToken);

            return response.StatusCode == HttpStatusCode.OK
                ? HealthCheckResult.Healthy(HealthyMessage)
                : HealthCheckResult.Unhealthy(UnhealthyMessage);
        }
    }
}