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
using System.Linq;

namespace Meca.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration)
        {
            Console.WriteLine("[MECA_DEBUG] === INICIANDO CONSTRUTOR DO STARTUP ===");
            
            try
            {
                Console.WriteLine("[MECA_DEBUG] 1. Configuração recebida...");
                Configuration = configuration;
                Console.WriteLine("[MECA_DEBUG] 2. Configuration atribuído com sucesso");

                Console.WriteLine("[MECA_DEBUG] 3. Verificando providers de configuração...");
                var configRoot = configuration as IConfigurationRoot;
                if (configRoot != null)
                {
                    Console.WriteLine($"[MECA_DEBUG] Providers encontrados: {configRoot.Providers.Count()}");
                    foreach (var provider in configRoot.Providers)
                    {
                        Console.WriteLine($"[MECA_DEBUG] Provider: {provider.GetType().Name}");
                    }
                }
                else
                {
                    Console.WriteLine("[MECA_DEBUG] Configuration não é IConfigurationRoot");
                }

                Console.WriteLine("[MECA_DEBUG] 4. Verificando variáveis de ambiente...");
                Console.WriteLine($"[MECA_DEBUG] ASPNETCORE_ENVIRONMENT = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
                Console.WriteLine($"[MECA_DEBUG] DOTNET_ENVIRONMENT = {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}");

                Console.WriteLine("[MECA_DEBUG] 5. Verificando configurações...");
                Console.WriteLine($"[MECA_DEBUG] ConnectionStrings:DefaultConnection = {Configuration["ConnectionStrings:DefaultConnection"]}");

                Console.WriteLine("[MECA_DEBUG] 6. Configurando BaseConfig...");
                BaseConfig.ApplicationName = ApplicationName =
                configuration.GetSection("ApplicationName").Get<string>() ?? Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0];
                Console.WriteLine($"[MECA_DEBUG] ApplicationName = {ApplicationName}");

                Console.WriteLine("[MECA_DEBUG] 7. Configurando EnableSwagger...");
                EnableSwagger = configuration.GetSection("EnableSwagger").Get<bool>();
                Console.WriteLine($"[MECA_DEBUG] EnableSwagger = {EnableSwagger}");

                Console.WriteLine("[MECA_DEBUG] 8. Configurando EnableService...");
                EnableService = configuration.GetSection("EnableService").Get<bool>();
                Console.WriteLine($"[MECA_DEBUG] EnableService = {EnableService}");

                Console.WriteLine("[MECA_DEBUG] === CONSTRUTOR DO STARTUP CONCLUÍDO COM SUCESSO ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MECA_DEBUG] ERRO NO CONSTRUTOR DO STARTUP: {ex.Message}");
                Console.WriteLine($"[MECA_DEBUG] STACK TRACE: {ex.StackTrace}");
                throw;
            }
        }
        public static string ApplicationName { get; set; }
        public static bool EnableSwagger { get; set; }
        public static bool EnableService { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("[MECA_DEBUG] === INICIANDO ConfigureServices ===");
            
            try
            {
                Console.WriteLine("[MECA_DEBUG] 1. Verificando configuração...");
                
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

                Console.WriteLine("[MECA_DEBUG] 2. Configurando Controllers...");
                
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
                
                Console.WriteLine("[MECA_DEBUG] 3. Controllers configurados com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MECA_DEBUG] ERRO DURANTE ConfigureServices: {ex.Message}");
                Console.WriteLine($"[MECA_DEBUG] STACK TRACE: {ex.StackTrace}");
                throw;
            }

            Console.WriteLine("[MECA_DEBUG] 4. Adicionando HealthChecks...");
            services.AddHealthChecks();

            if (EnableService)
            {
                try
                {
                    Console.WriteLine("[MECA_DEBUG] 5. Configurando Hangfire...");
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

            Console.WriteLine("[MECA_DEBUG] 6. Adicionando SignalR...");
            services.AddSignalR();

            Console.WriteLine("[MECA_DEBUG] 7. Configurando CORS...");
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigin",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .Build());
            });

            Console.WriteLine("[MECA_DEBUG] 8. Configurando AutoMapper...");
            services.AddAutoMapper(c =>
            {
                c.AddProfile<DomainToViewModelMappingProfile>();
                c.AddProfile<ViewModelToDomainMappingProfile>();
            }, typeof(Startup));

            Console.WriteLine("[MECA_DEBUG] 9. Adicionando ImageResizer...");
            services.AddImageResizer();

            Console.WriteLine("[MECA_DEBUG] 10. Configurando JWT Swagger...");
            services.AddJwtSwagger(ApplicationName, enableSwaggerAuth: true);

            Console.WriteLine("[MECA_DEBUG] 11. Adicionando HttpContextAccessor...");
            services.AddHttpContextAccessor();

            Console.WriteLine("[MECA_DEBUG] 12. Registrando BusinessBaseAsync...");
            services.AddScoped(typeof(IBusinessBaseAsync<>), typeof(BusinessBaseAsync<>));

            Console.WriteLine("[MECA_DEBUG] 13. Registrando serviços...");
            services.AddServicesInjection();
            Console.WriteLine("[MECA_DEBUG] Serviços registrados com sucesso");
            
            Console.WriteLine("[MECA_DEBUG] 14. Registrando serviços de aplicação...");
            services.AddAplicationServicesInjection();
            Console.WriteLine("[MECA_DEBUG] Serviços de aplicação registrados com sucesso");

            Console.WriteLine("[MECA_DEBUG] 15. Adicionando Options...");
            services.AddOptions();

            Console.WriteLine("[MECA_DEBUG] === ConfigureServices CONCLUÍDO COM SUCESSO ===");
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
