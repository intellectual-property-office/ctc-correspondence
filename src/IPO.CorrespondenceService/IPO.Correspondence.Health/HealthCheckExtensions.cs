using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using IPO.CTC.Common.Functions;
using IPO.CTC.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IPO.Correspondence.Health
{
    /// <summary>
    /// Common functions to add standard correspondence health checks to correspondence service components
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Adds a standard SQL server check for a correspondence service component
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance being extended.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to read for the health checks configuration</param>
        /// <returns>The <see cref="IServiceCollection"/> instance</returns>
        /// <remarks>
        /// The following property is expected within the <paramref name="configuration"/>
        /// 
        /// <list type="bullet">
        /// <item><description>
        /// NotificationDbConnection
        /// </description></item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddSqlCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var name = "Correspondence Database Health Check";
            var connectionProperty = "NotificationDbConnection";

            services.AddTypeActivatedCheck<SQLHealthCheck>(
                name,
                configuration,
                connectionProperty);

            return services;
        }

        /// <summary>
        /// Adds a standard BLOB Store check for a correspondence service component
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance being extended.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to read for the health checks configuration</param>
        /// <returns>The <see cref="IServiceCollection"/> instance</returns>
        /// <remarks>
        /// The following property is expected within the <paramref name="configuration"/>
        /// 
        /// <list type="bullet">
        /// <item><description>
        /// AzureBlobStorageConnectionString
        /// </description></item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddBlobStoreCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var name = "Correspondence BLOB Store Health Check";
            var connectionProperty = "AzureBlobStorageConnectionString";

            services.AddTypeActivatedCheck<AzureBlobStorageConnectionHealthCheck>(
                name,
                configuration,
                connectionProperty);

            return services;
        }

        public static IServiceCollection AddBlobStoreContainerClientCheck(this IServiceCollection services, IConfiguration configuration, DefaultAzureCredential? defaultAzureCredential = null)
        {
            var name = "Correspondence BLOB Store Health Check";
            var connectionProperty = "AzureBlobStorageConnectionString";
            var containerProperty = "ContainerName";

            services.AddTypeActivatedCheck<AzureBlobStorageContainerClientConnectionHealthCheck>(
                name,
                configuration,
                connectionProperty,
                containerProperty,
                defaultAzureCredential!);

            return services;
        }

        /// <summary>
        /// Adds a standard Service Bus check for a correspondence service component
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance being extended.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to read for the health checks configuration</param>
        /// <returns>The <see cref="IServiceCollection"/> instance</returns>
        /// <remarks>
        /// The following properties are expected within the <paramref name="configuration"/>
        /// 
        /// <list type="bullet">
        /// <item><description>
        /// ServiceBusConnectionString
        /// </description></item>
        /// <item><description>
        /// Topic
        /// </description></item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddServiceBusCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var name = "Correspondence Service Bus Health Check";
            var topicProperty = "Topic";

#if DEBUG
            var connectionProperty = "ServiceBusConnectionString";

            services.AddTypeActivatedCheck<ServiceBusTopicHealthCheck>(
                name,
                configuration,
                connectionProperty,
                topicProperty);
#else
            var credential = new DefaultAzureCredential();
            var connectionProperty = "ServiceBusConnectionString:fullyQualifiedNamespace";

            services.AddTypeActivatedCheck<ServiceBusTopicHealthCheck>(
                name,
                configuration,
                connectionProperty,
                topicProperty,
                credential);
#endif

            return services;
        }

        public static IServiceCollection AddGovNotifyCheck(this IServiceCollection services)
        {
            services.AddHealthChecks().AddCheck<GovNotifyHealthCheck>("Gov Notify Check");

            return services;
        }

        public static IServiceCollection AddFunctionServiceCheck(this IServiceCollection services, IConfiguration configuration, string functionUrlProperty)
        {
            const string apimKeyProperty = "FunctionServiceSubKey";
            const string versionProperty = "Version";

            var address = configuration[functionUrlProperty];

#if !DEBUG
            // Function app configuration does not currently include the protocol
            if (!address!.StartsWith("https://"))
            {
                address = $"https://{address}";
            }
#endif

            var uri = new Uri(address!);

            var client = new HttpClient
            {
                BaseAddress = new Uri(address!.Replace(uri.PathAndQuery, string.Empty))//uri
            };

            var version = GetMajorVersion(configuration[versionProperty]!);

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration[apimKeyProperty]);
            client.DefaultRequestHeaders.Add("Accept-Version", version);

            services.AddHealthChecks().AddCheck(
                name: functionUrlProperty,
                instance: new IpoApimServiceReadyCheck(client, uri.LocalPath),
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { HealthTags.Ready }
            );

            return services;
        }

        private static string GetMajorVersion(string versionString)
        {
            // The only version available within the devops configuration is this API's version
            // For ready checking the function apps via APIM just the major version number
            // is required for the Accept-Version header 

            if (string.IsNullOrEmpty(versionString))
            {
                return string.Empty;
            }

            var cleaned = new string(versionString.Where(e => char.IsDigit(e) || e == '.').ToArray());

            var version = string.IsNullOrWhiteSpace(cleaned)
                ? new Version("0.0")
                : cleaned.Contains(".")
                    ? new Version(cleaned)
                    : new Version($"{cleaned}.0");

            return version.Major.ToString();
        }
    }
}