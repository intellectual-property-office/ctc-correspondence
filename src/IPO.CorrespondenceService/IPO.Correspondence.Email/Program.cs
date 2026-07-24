using Azure.Identity;
using IPO.Configuration;
using IPO.Correspondence.Data.Notifications;
using IPO.Correspondence.Health;
using IPO.Correspondence.Services.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();

                services.AddCorrespondenceFunctionServices(context.Configuration, typeof(Program));

                var configuration = context.Configuration;
                var connectionString = configuration["NotificationDbConnection"];
                services.AddDbContext<NotificationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString, x => x.MigrationsAssembly("IPO.Correspondence.Data"));
                });

                services.AddSqlCheck(configuration)
                        .AddServiceBusCheck(configuration)
                        .AddGovNotifyCheck();
            })
            .Build();

        host.Run();
    }
}