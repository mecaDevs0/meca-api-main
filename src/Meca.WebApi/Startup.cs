using System;
using System.IO;
using System.Reflection;
using Hangfire;
using Meca.ApplicationService.Interface;
using Meca.ApplicationService.Services;
using Meca.ApplicationService.Services.Hangfire.Interface;
using Meca.ApplicationService.Services.HangFire;
using Meca.Domain;
using Meca.Domain.AutoMapper;
using Meca.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;

            Console.WriteLine($"[MECA_DEBUG] Startup iniciado");
            Console.WriteLine($"[MECA_DEBUG] Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            Console.WriteLine($"[MECA_DEBUG] Configuration providers:");
            
            if (Configuration is IConfigurationRoot configRoot)
            {
                foreach (var provider in configRoot.Providers)
                {
                    Console.WriteLine($"[MECA_DEBUG] Provider: {provider.GetType().Name}");
                }
            }
            
            Console.WriteLine($"[MECA_DEBUG] DATABASE:ConnectionString = {Configuration["DATABASE:CONNECTIONSTRING"]}");
            Console.WriteLine($"[MECA_DEBUG] DATABASE:NAME = {Configuration["DATABASE:NAME"]}");
            Console.WriteLine($"[MECA_DEBUG] ConnectionStrings:DefaultConnection = {Configuration["ConnectionStrings:DefaultConnection"]}");

            BaseConfig.ApplicationName = ApplicationName =
            configuration.GetSection("ApplicationName").Get<string>() ?? Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0];
            EnableSwagger = configuration.GetSection("EnableSwagger").Get<bool>();
            EnableService = configuration.GetSection("EnableService").Get<bool>();
        }
        public static string ApplicationName { get; set; }
        public static bool EnableSwagger { get; set; }
        public static bool EnableService { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                // DEBUG: Log the connection string to verify it's correct
var connectionString = Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[MECA_DEBUG] Connection String Sendo Usada: {connectionString}");

                // DEBUG: Test DATABASE section binding
var databaseSection = Configuration.GetSection("DATABASE");
Console.WriteLine($"[MECA_DEBUG] DATABASE Section exists: {databaseSection.Exists()}");
Console.WriteLine($"[MECA_DEBUG] DATABASE:ConnectionString = {databaseSection["ConnectionString"]}");
Console.WriteLine($"[MECA_DEBUG] DATABASE:Name = {databaseSection["Name"]}");

                // DEBUG: Test MongoDB section
var mongoSection = Configuration.GetSection("MongoDb");
Console.WriteLine($"[MECA_DEBUG] MongoDb Section exists: {mongoSection.Exists()}");
Console.WriteLine($"[MECA_DEBUG] MongoDb:ConnectionString = {mongoSection["ConnectionString"]}");
Console.WriteLine($"[MECA_DEBUG] MongoDb:DatabaseName = {mongoSection["DatabaseName"]}");

                services.AddControllers(opt =>
                {
                    opt.Filters.Add(typeof(CheckJson));
                    opt.Filters.Add(typeof(PreventSpanFilter));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MECA_DEBUG] ERRO DURANTE ConfigureServices: {ex.Message}");
                Console.WriteLine($"[MECA_DEBUG] STACK TRACE: {ex.StackTrace}");
                throw;
            }

            services.AddHealthChecks();

            if (EnableService)
            {
                try
                {
                    services.AddHangfireMongoDb(Configuration);
                    Console.WriteLine("[MECA_DEBUG] Hangfire configurado com sucesso");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MECA_DEBUG] Erro ao configurar Hangfire: {ex.Message}");
                    // Continue without Hangfire if it fails
                }
            }
            else
            {
                Console.WriteLine("[MECA_DEBUG] Hangfire desabilitado");
            }

            services.AddSignalR();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigin",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .Build());
            });

            services.AddAutoMapper(c =>
            {
                c.AddProfile<DomainToViewModelMappingProfile>();
                c.AddProfile<ViewModelToDomainMappingProfile>();
            }, typeof(Startup));

            services.AddImageResizer();

            services.AddJwtSwagger(ApplicationName, enableSwaggerAuth: true);

            services.AddHttpContextAccessor();

            services.AddScoped(typeof(IBusinessBaseAsync<>), typeof(BusinessBaseAsync<>));

            Console.WriteLine("[MECA_DEBUG] Registrando serviços...");
            services.AddServicesInjection();
            Console.WriteLine("[MECA_DEBUG] Serviços registrados com sucesso");
            
            Console.WriteLine("[MECA_DEBUG] Registrando serviços de aplicação...");
            services.AddAplicationServicesInjection();
            Console.WriteLine("[MECA_DEBUG] Serviços de aplicação registrados com sucesso");

            services.AddOptions();

            // Removido: AddUtilityFramework não existe
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            loggerFactory.UseCustomLog((IConfigurationRoot)Configuration, $"Log/{BaseConfig.ApplicationName}-.txt");

            if (EnableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"../swagger/v1/swagger.json".Trim(), $"API {ApplicationName} {env.EnvironmentName}");
                    c.EnableFilter();
                    c.EnableDeepLinking();
                });
            }

            app.UseRouting();

            app.UseCors("AllowAllOrigin");

            app.UseJwtTokenApiAuth((IConfigurationRoot)Configuration);
            app.UseAuthentication();
            app.UseAuthorization();

            if (EnableService)
                app.UseHangFire();

            app.UseImageResizer();

            app.UseStaticFiles();
            var path = Path.Combine(Directory.GetCurrentDirectory(), @"Content");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = new PathString("/content")
            });

            app.UseRequestResponseLoggingLite();
            app.UseBlockMiddleware();
            app.UseResponseCompression();

            // Removido: UseUtilityFramework não existe

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<PaymentNotificationHub>("/PaymentNotificationHub").RequireCors("AllowAllOrigin");
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions() { ResponseWriter = HealthCheckExtensions.WriteResponse });
            });

            if (EnableService)
            {
                HangfileMiddleware.ClearJobs();

                var timeZoneBrazil = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

                RecurringJob.AddOrUpdate<IHangfireService>("NOTIFY_SCHEDULING", x => x.NotifyScheduling(null), Cron.Minutely(), timeZoneBrazil);

                RecurringJob.AddOrUpdate<IHangfireService>("REMOVE_AGENDA", x => x.RemoveAgenda(null), Cron.Daily(), timeZoneBrazil);
            }
        }
    }
}
