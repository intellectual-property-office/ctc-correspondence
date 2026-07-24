using IPO.Correspondence.API;
using IPO.Correspondence.BDDTests.Mocks;
using IPO.Correspondence.Interfaces;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IPO.Correspondence.BDDTests.Helpers
{
    public class TestStartup : Startup
    {
        public static Common.API.Version? Version;

        public TestStartup(IConfiguration configuration) : base(configuration)
        {
            Version = _helper.Version;
        }
        protected override void AddServices(IServiceCollection services)
        {
            services.AddScoped<ICorrespondenceManagementService, MockCorrespondenceManagementService>();
            services.AddScoped<INotificationDbRepository, MockNotificationDbRepository>();
            services.AddScoped<IMessagingClient, MockMessagingClient>();
            services.AddScoped<IFileStorageGateway, MockFileStorageGateway>();
            services.AddScoped<INotifyGateway, MockNotifyGateway>();
            services.AddScoped<IPreliminaryFileValidator, MockPreliminaryFileValidator>();

        }
        protected override void ConfigureSwagger(IServiceCollection services) { }
        protected override void AddNotificationsDatabase(IServiceCollection services, string errorCode) { }
        protected override void MigrateDatabase(IApplicationBuilder app) { }
        protected override void AddServiceBusSender(IServiceCollection services) { }
        protected override void AddSwaggerExamplesForType<T>(IServiceCollection services) { }
        protected override void AddHealthChecks(IServiceCollection services) { }
        public static TestServer GetTestServer()
        {
            var hostBuilder = new HostBuilder()
               .ConfigureWebHost(webHost =>
               {
                   webHost
                      .UseTestServer()
                      .UseStartup<TestStartup>();
               });
            var host = hostBuilder.Start();
            return host.GetTestServer();
        }
    }
}