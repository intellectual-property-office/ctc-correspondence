using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using IPO.Configuration;
using IPO.Correspondence.Data.Notifications;
using IPO.Correspondence.Gateways;
using IPO.Correspondence.GovNotify;
using IPO.Correspondence.Health;
using IPO.Correspondence.Interfaces;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Messaging;
using IPO.Correspondence.Services;
using IPO.Correspondence.Services.Validation;
using IPO.Correspondence.StatusUpdate;
using IPO.CTC.Common.Functions;
using IPO.ServiceRequest.Models.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

public class Program
{
    public static void Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddIPOAzureAppConfigWithManagedIdentity();
                config.AddTemplateConfiguration();

#if DEBUG
                var rootPath = AppContext.BaseDirectory;
                config.SetBasePath(rootPath);
                config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
#endif
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();

                services.AddScoped(typeof(ILogger), typeof(Logger<Program>));

                services.AddScoped<INotificationDbRepository, NotificationDbRepository>();
                services.AddIPOErrorAwareScoped<INotifyGateway, NotifyGateway>("E008");
                services.AddScoped<IFileStorageGateway, BlobStorageGateway>();
                services.AddScoped<IHealthChecker, HealthChecker>();

                var configuration = context.Configuration;
                var connectionString = configuration["NotificationDbConnection"];
                services.AddDbContext<NotificationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString, x => x.MigrationsAssembly("IPO.Correspondence.Data"));
                });

                services.Configure<Settings>(configuration);

#if DEBUG
                services.AddSingleton(x => new BlobServiceClient(configuration["AzureBlobStorageConnectionString"]));
#else
                var credential = new DefaultAzureCredential();
                var uri = new Uri(configuration["AzureBlobStorageConnectionString"]!.ToString());
                services.AddSingleton(x => new BlobServiceClient(uri, credential, new BlobClientOptions()));
#endif

                // Register PreliminaryFileValidator (required by CorrespondenceManagementService)
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

                // Register ServiceBusSender for MessagingClient
                services.AddSingleton<ServiceBusSender>(sp =>
                {
                    var cfg = sp.GetRequiredService<IConfiguration>();
#if DEBUG
                    var client = new ServiceBusClient(
                        cfg["ServiceBusConnectionString"],
                        new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
#else
                    var fqdn = cfg["ServiceBusConnectionString:fullyQualifiedNamespace"];
                    var sbCredential = new DefaultAzureCredential();
                    var client = new ServiceBusClient(fqdn, sbCredential);
#endif
                    return client.CreateSender(cfg["Topic"]);
                });

                services.AddScoped<IMessagingClient, MessagingClient>();
                services.AddScoped<ICorrespondenceServiceValidator, CorrespondenceServiceValidator>();
                services.AddScoped<ICorrespondenceManagementService, CorrespondenceManagementService>();

                services.AddSqlCheck(configuration)
#if DEBUG
                .AddBlobStoreCheck(configuration)
#else
                .AddBlobStoreContainerClientCheck(configuration, new DefaultAzureCredential())
#endif
                .AddServiceBusCheck(configuration)
                .AddGovNotifyCheck();
            })
            .Build();

        host.Run();
    }
}