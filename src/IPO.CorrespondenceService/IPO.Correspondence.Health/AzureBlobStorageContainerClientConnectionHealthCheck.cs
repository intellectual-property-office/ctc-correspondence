using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IPO.Correspondence.Health
{
    public class AzureBlobStorageContainerClientConnectionHealthCheck : IHealthCheck
    {
        public const string HealthyMessage = "BLOB Store is Available";

        private readonly BlobContainerClient _containerClient;

        private static BlobClientOptions ClientOptions
        {
            get
            {
                BlobClientOptions blobClientOptions = new BlobClientOptions();
                blobClientOptions.Retry.MaxRetries = 0;
                return blobClientOptions;
            }
        }

        public AzureBlobStorageContainerClientConnectionHealthCheck(IConfiguration config, string connectionStringName, string containerName, Azure.Identity.DefaultAzureCredential? defaultAzureCredential = null)
        {
            string connectionString = config[connectionStringName]!;
            string container = config[containerName]!;

            var _serviceClient = defaultAzureCredential == null
                ? new BlobServiceClient(connectionString, ClientOptions)
                : new BlobServiceClient(new Uri(connectionString), defaultAzureCredential, ClientOptions);

            _containerClient = _serviceClient.GetBlobContainerClient(container);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var properties = await _containerClient.GetPropertiesAsync();

                return HealthCheckResult.Healthy(HealthyMessage);
            }
            catch (Exception ex)
            {
                return (context == null)
                    ? new HealthCheckResult(HealthStatus.Unhealthy, ex.Message, ex)
                    : new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex);
            }
        }
    }
}