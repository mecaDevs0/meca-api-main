using System;
using System.IO;
using System.Reflection;
using System.Text;
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
using Microsoft.IdentityModel.Tokens;
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
            Configuration = configuration;
            
            // DEBUG: Verificar se a configuração está sendo lida corretamente
            Console.WriteLine($"[DEBUG_STARTUP] Iniciando Startup");
            
            var connectionString = configuration.GetSection("DATABASE:CONNECTIONSTRING").Value;
            Console.WriteLine($"[DEBUG_STARTUP] Connection String: {connectionString}");
            
            var jwtKey = configuration.GetSection("Jwt:SecretKey").Value;
            Console.WriteLine($"[DEBUG_STARTUP] JWT Key: {jwtKey}");
            
            var databaseName = configuration.GetSection("DATABASE:NAME").Value;
            Console.WriteLine($"[DEBUG_STARTUP] Database Name: {databaseName}");
            
            BaseConfig.ApplicationName = ApplicationName =
                configuration.GetSection("ApplicationName").Get<string>() ?? Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0];
            
            Console.WriteLine($"[DEBUG_STARTUP] Application Name: {ApplicationName}");
            
            EnableSwagger = configuration.GetSection("EnableSwagger").Get<bool>();
            EnableService = configuration.GetSection("EnableService").Get<bool>();
            
            Console.WriteLine($"[DEBUG_STARTUP] EnableSwagger: {EnableSwagger}");
            Console.WriteLine($"[DEBUG_STARTUP] EnableService: {EnableService}");
            Console.WriteLine($"[DEBUG_STARTUP] Startup finalizado");
        }
        
        public static string ApplicationName { get; set; }
        public static bool EnableSwagger { get; set; }
        public static bool EnableService { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine($"[DEBUG_STARTUP] Iniciando ConfigureServices");
            
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigin", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            services.AddResponseCompression();

            services.AddControllers()
                .AddNewtonsoftJson();

            services.AddSignalR();

            services.AddHealthChecks();

            services.AddAutoMapper(c =>
            {
                c.AddProfile<DomainToViewModelMappingProfile>();
                c.AddProfile<ViewModelToDomainMappingProfile>();
            }, typeof(Startup));

            services.AddImageResizer();

            services.AddJwtSwagger(ApplicationName, enableSwaggerAuth: true);

            services.AddHttpContextAccessor();

            services.AddScoped(typeof(IBusinessBaseAsync<>), typeof(BusinessBaseAsync<>));

            Console.WriteLine($"[DEBUG_STARTUP] Registrando serviços de injeção");
            services.AddServicesInjection();
            
            Console.WriteLine($"[DEBUG_STARTUP] Registrando serviços de aplicação");
            services.AddAplicationServicesInjection();

            // CORREÇÃO: Adicionar configuração do Hangfire
            if (EnableService)
            {
                Console.WriteLine($"[DEBUG_STARTUP] Configurando Hangfire");
                services.AddHangfireMongoDb(Configuration);
            }

            services.AddOptions();
            
            Console.WriteLine($"[DEBUG_STARTUP] ConfigureServices finalizado");
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

            // TESTE: Middleware personalizado para interceptar requisições PATCH
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "PATCH")
                {
                    Console.WriteLine($"[PATCH_INTERCEPTOR] ===== PATCH INTERCEPTADO =====");
                    Console.WriteLine($"[PATCH_INTERCEPTOR] Path: {context.Request.Path}");
                    Console.WriteLine($"[PATCH_INTERCEPTOR] Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
                    Console.WriteLine($"[PATCH_INTERCEPTOR] ContentType: {context.Request.ContentType}");
                    Console.WriteLine($"[PATCH_INTERCEPTOR] ContentLength: {context.Request.ContentLength}");
                }
                
                await next();
                
                if (context.Request.Method == "PATCH")
                {
                    Console.WriteLine($"[PATCH_INTERCEPTOR] ===== PATCH FINALIZADO =====");
                    Console.WriteLine($"[PATCH_INTERCEPTOR] StatusCode: {context.Response.StatusCode}");
                }
            });
            
            app.UseRequestResponseLoggingLite();
            app.UseBlockMiddleware();
            app.UseResponseCompression();

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
