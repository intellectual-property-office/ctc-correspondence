#if !DEBUG
using Azure.Identity;
#endif
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using IPO.Common.API;
using IPO.Common.Infrastructure;
using IPO.Correspondence.API.Models;
using IPO.Correspondence.Data.Notifications;
using IPO.Correspondence.Gateways;
using IPO.Correspondence.GovNotify;
using IPO.Correspondence.Health;
using IPO.Correspondence.Interfaces;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Messaging;
using IPO.Correspondence.Models;
using IPO.Correspondence.Services;
using IPO.Correspondence.Services.Validation;
using IPO.CTC.HealthChecks;
using IPO.ServiceRequest.Models.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Linq;

namespace IPO.Correspondence.API
{
    public class Startup
    {
        internal readonly IPOStartupHelper _helper;
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _helper = new IPOStartupHelper("IPO.Correspondence.API", "version");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _helper.AddIPOServicesConfiguration(services,
                setupControllers: x => x.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true,
                mvcBuilderAction: x => x.AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new IPOJsonObjectConverter());
                    options.JsonSerializerOptions.Converters.Add(new IPOJsonEnumConverter());
                }));


            ConfigureSwagger(services);
            services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            AddNotificationsDatabase(services, "E002");
            AddServices(services);
            AddServiceBusSender(services);
            services.AddHttpContextAccessor();
            services.Configure<Settings>(_configuration);
            AddHealthChecks(services);
        }

        protected virtual void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c => c.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(IFormFile)
                   , () => new OpenApiSchema()
                   {
                       Type = "file",
                       Format = "binary"
                   }));

            services.AddSwaggerGen(config =>
            {
                config.ExampleFilters();
                config.DescribeAllParametersInCamelCase();
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Correspondence Microservice",
                    Version = "v1"
                });

                config.EnableAnnotations();
            });

            AddSwaggerExamplesForType<TemplatePreviewRequestExamples>(services);
            AddSwaggerExamplesForType<TemplatePreviewResponseExamples>(services);
            AddSwaggerExamplesForType<NotificationModelExamples>(services);
        }

        protected virtual void AddServices(IServiceCollection services)
        {
            services.AddIPOErrorAwareScoped<INotificationDbRepository, NotificationDbRepository>("E003");
            services.AddIPOErrorAwareScoped<IMessagingClient, MessagingClient>("E004");
            services.AddIPOErrorAwareScoped<IFileStorageGateway, BlobStorageGateway>("E005");
            services.AddIPOErrorAwareScoped<ICorrespondenceServiceValidator, CorrespondenceServiceValidator>("E007");
            services.AddIPOErrorAwareScoped<INotifyGateway, NotifyGateway>("E008");
            services.AddIPOErrorAwareScoped<ICorrespondenceManagementService, CorrespondenceManagementService>("E009");

            var fileChecks = new PreliminaryFileValidator
            {
                AcceptedFileExtensions = _configuration["AcceptedFileExtensions"]!.ToUpperInvariant().Split(',').ToList(),
                SizeLimit = _configuration.GetValue<int>("SizeLimit")
            };
            services.AddIPOErrorAwareScoped<IPreliminaryFileValidator>(x => fileChecks, Error.Create<PreliminaryFileValidator>("E006"));


            services.AddSingleton(x => new BlobServiceClient(_configuration["AzureBlobStorageConnectionString"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            MigrateDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRewriter(new RewriteOptions().Add(RewriteRules.RewriteAlwaysOn));

            _helper.UseIPOConfigurations(app, env);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        protected virtual void AddHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks().AddTypeActivatedCheck<SQLHealthCheck>(
                name: "Correspondence Database Health Check",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { HealthTags.Ready },
                args: new object[]
                {
                    _configuration,
                    "NotificationDbConnection"
                }
             );

#if DEBUG
            // Adding this blob health check type, on the assumption that local development is working with
            // a local blob storage emulator
            services.AddHealthChecks().AddTypeActivatedCheck<AzureBlobStorageConnectionHealthCheck>(
                name: "Correspondence BLOB Store Health Check",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { HealthTags.Ready },
                args: new object[]
                {
                    _configuration,
                    "AzureBlobStorageConnectionString"
                }
            );
#else
            services.AddHealthChecks().AddTypeActivatedCheck<AzureBlobStorageContainerClientConnectionHealthCheck>(            
                name: "Correspondence BLOB Store Health Check",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { HealthTags.Ready },
                args: new object[]
                {
                    _configuration,
                    "AzureBlobStorageConnectionString",
                    "ContainerName"
                }
            );
#endif

            services.AddHealthChecks().AddCheck<GovNotifyHealthCheck>(
                name: "Correspondence Gov Notify Check",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { HealthTags.Ready }
            );

            services
                .AddFunctionServiceCheck(_configuration, "ApiFunctionService")
                .AddFunctionServiceCheck(_configuration, "EmailService")
                .AddFunctionServiceCheck(_configuration, "LetterFunctionService")
                .AddFunctionServiceCheck(_configuration, "SmsFunctionService")
                .AddFunctionServiceCheck(_configuration, "StatusUpdateFunctionService");

#if DEBUG
            services.AddHealthChecks().AddTypeActivatedCheck<ServiceBusTopicHealthCheck>(
               name: "Correspondence Service Bus Health Check",
               failureStatus: HealthStatus.Unhealthy,
               tags: new[] { HealthTags.Ready },
               args: new object[]
               {
                   _configuration,
                  "ServiceBusConnectionString",
                   "Topic"
               }
            );
#else
            var credential = new DefaultAzureCredential();

            services.AddHealthChecks().AddTypeActivatedCheck<ServiceBusTopicHealthCheck>(
               name: "Correspondence Service Bus Health Check",
               failureStatus: HealthStatus.Unhealthy,
               tags: new[] { HealthTags.Ready },
               args: new object[]
               {
                   _configuration,
                  "ServiceBusConnectionString:fullyQualifiedNamespace",
                   "Topic",
                   credential
               }
            );
#endif
        }

        protected virtual void AddNotificationsDatabase(IServiceCollection services, string errorCode)
        {
            var dbConnection = _configuration["NotificationDbConnection"];
            services.AddIPOErrorAwareDbContext<INotificationDbContext, NotificationDbContext>(errorCode, options =>
                options.UseSqlServer(dbConnection!));
        }

        protected virtual void MigrateDatabase(IApplicationBuilder app)
        {
            var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<NotificationDbContext>();
            dbContext!.Database.Migrate();
        }

        protected virtual void AddServiceBusSender(IServiceCollection services)
        {
            services.AddSingleton<ServiceBusSender>(x =>
            {
#if DEBUG
                ServiceBusClient serviceBusClient = new(_configuration["ServiceBusConnectionString"],
                         new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
#else
                var credential = new Azure.Identity.DefaultAzureCredential();
                var serviceBusConnectionsConnectionString = _configuration["ServiceBusConnectionString:fullyQualifiedNamespace"];
                var serviceBusClient = new ServiceBusClient(serviceBusConnectionsConnectionString, credential);
#endif
                return serviceBusClient.CreateSender(_configuration["Topic"]);
            });
        }

        protected virtual void AddSwaggerExamplesForType<T>(IServiceCollection services)
        {
            services.AddSwaggerExamplesFromAssemblyOf<T>();
        }
    }
}