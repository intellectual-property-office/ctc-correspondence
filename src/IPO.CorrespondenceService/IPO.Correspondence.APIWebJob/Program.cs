using Azure.Messaging.ServiceBus;
using IPO.Correspondence.Data.Notifications;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Messaging;
using IPO.Correspondence.Services.Validation;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IPO.Configuration;

namespace IPO.Correspondence.APIWebJob
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorageQueues();
                b.AddTimers();

                b.Services.AddScoped(typeof(ILogger), typeof(Logger<Program>));
                b.Services.AddScoped<INotificationDbRepository, NotificationDbRepository>();
                b.Services.AddScoped<IMessagingClient, MessagingClient>();
                b.Services.AddScoped<ICorrespondenceServiceValidator, CorrespondenceServiceValidator>();

                AddServiceBusSender(b.Services);
                b.Services.AddDbContext<NotificationDbContext>((serviceProvider, options) =>
                {
                    var config = serviceProvider.GetService<IConfiguration>();
                    var connectionString = config!["NotificationDbConnection"];

                    options.UseSqlServer(connectionString!, x => x.MigrationsAssembly("IPO.Correspondence.Data"));
                });
            });

            builder.ConfigureLogging((context, b) =>
            {
                b.AddConsole();
            });

            builder.ConfigureAppConfiguration(configBuilder =>
            {
                // Add IPO Azure Configuration - only added when Azure App Config is set
                configBuilder.AddIPOAzureAppConfigWithManagedIdentity();

                // Add Template replacement configuration provider to replace templated values
                configBuilder.AddTemplateConfiguration();

            });

            var host = builder.Build();
            using (host)
            {
                var jobHost = host.Services.GetService<IJobHost>();

                await host.StartAsync();
                await jobHost!.CallAsync("SendEmailNotificationsToOwners");
                await host.StopAsync();
            }
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
    }
}
