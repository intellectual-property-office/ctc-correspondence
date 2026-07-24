using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using IPO.Common.API;
using IPO.Correspondence.Data.Notifications;
using IPO.Correspondence.Gateways;
using IPO.Correspondence.GovNotify;
using IPO.Correspondence.Interfaces;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Messaging;
using IPO.Correspondence.Services.Validation;
using IPO.CTC.Common.Functions;
using IPO.ServiceRequest.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IPO.Correspondence.Services.DependencyInjection
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class FunctionServiceCollectionExtensions
    {
        public static IServiceCollection AddCorrespondenceFunctionServices(this IServiceCollection services,
            IConfiguration configuration, Type loggerCategoryType)
        {
            services.AddScoped<ILogger>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return factory.CreateLogger(loggerCategoryType.FullName!);
            });

            services.AddScoped<INotificationDbRepository, NotificationDbRepository>();
            IPOStartupExtensions.AddIPOErrorAwareScoped<INotifyGateway, NotifyGateway>(services, "E008");
            services.AddSingleton<IFileStorageGateway, BlobStorageGateway>();
            services.Configure<Settings>(configuration);
            services.AddScoped<IHealthChecker, HealthChecker>();

            services.AddScoped<ICorrespondenceServiceValidator, CorrespondenceServiceValidator>();
            AddServiceBusSender(services);

            services.AddScoped<IMessagingClient, MessagingClient>();
            services.AddScoped<IPreliminaryFileValidator>(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var accepted = (cfg["AcceptedFileExtensions"] ?? string.Empty)
                    .ToUpperInvariant()
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToList();

                var sizeLimit = cfg.GetValue<int>("SizeLimit");

                return new PreliminaryFileValidator
                {
                    AcceptedFileExtensions = accepted,
                    SizeLimit = sizeLimit
                };
            });

            services.AddScoped<ICorrespondenceManagementService, CorrespondenceManagementService>();
            AddBlobServiceClient(services, configuration);

            return services;
        }

        private static void AddServiceBusSender(IServiceCollection services)
        {
            services.AddSingleton<ServiceBusSender>(x =>
            {
                var config = x.GetService<IConfiguration>();
#if DEBUG
                ServiceBusClient serviceBusClient = new(config!["ServiceBusConnectionString"],
                         new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
#else
                var credential = new Azure.Identity.DefaultAzureCredential();
                var serviceBusConnectionsConnectionString = config!["ServiceBusConnectionString:fullyQualifiedNamespace"];
                var serviceBusClient = new ServiceBusClient(serviceBusConnectionsConnectionString, credential);
#endif 
                return serviceBusClient.CreateSender(config["Topic"]);
            });
        }

        private static void AddBlobServiceClient(IServiceCollection services, IConfiguration configuration)
        {
#if DEBUG
            services.AddSingleton(x => new BlobServiceClient(configuration["AzureBlobStorageConnectionString"]));
#else
                var credential = new Azure.Identity.DefaultAzureCredential();
                var uri = new Uri(configuration["AzureBlobStorageConnectionString"]!.ToString());
                services.AddSingleton(x => new BlobServiceClient(uri, credential, new BlobClientOptions()));
#endif
        }
    }
}