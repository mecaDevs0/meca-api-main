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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration; // Garante atribuição explícita

            BaseConfig.ApplicationName = ApplicationName =
            configuration.GetSection("ApplicationName").Get<string>() ?? Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0];
            EnableSwagger = configuration.GetSection("EnableSwagger").Get<bool>();
            EnableService = configuration.GetSection("EnableService").Get<bool>();

            /* CRIAR NO Settings json prop com array de cultures ["pt","pt-br"] */
            //var cultures = Utilities.GetConfigurationRoot().GetSection("TranslateLanguages").Get<List<string>>();
            //SupportedCultures = cultures.Select(x => new CultureInfo(x)).ToList();
        }
        public static string ApplicationName { get; set; }
        public static bool EnableSwagger { get; set; }
        public static bool EnableService { get; set; }

        /* PARA TRANSLATE*/
        //public static List<CultureInfo> SupportedCultures { get; set; } = new List<CultureInfo>();

        // This method gets Race by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /*TRANSLATE I18N*/
            //services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc(opt =>
            {
                opt.Filters.Add(typeof(CheckJson));
                opt.Filters.Add(typeof(PreventSpanFilter));
                opt.EnableEndpointRouting = false;
            })
            /*GLOBALIZAÇÃO*/
            //.AddDataAnnotationsLocalization(options =>
            //{
            //    options.DataAnnotationLocalizerProvider = (type, factory) =>
            //     factory.Create(typeof(SharedResource));
            //})
            .AddNewtonsoftJson();

            services.AddHealthChecks();

            //services.AddApplicationInsightsTelemetry(Configuration);

            //services.Configure<RequestLocalizationOptions>(options =>
            //{
            //    options.DefaultRequestCulture = new RequestCulture("pt");
            //    options.SupportedCultures = SupportedCultures;
            //    options.SupportedUICultures = SupportedCultures;
            //});

            /*USO DE SERVIÇOS HANGFIRE*/
            if (EnableService)
                services.AddHangfireMongoDb(Configuration);

            services.AddSignalR();

            /*ENABLE CORS*/
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

            /*CROP IMAGE*/
            services.AddImageResizer();

            /*ADD SWAGGER*/
            services.AddJwtSwagger(ApplicationName, enableSwaggerAuth: true);

            services.AddHttpContextAccessor();

            // services.AddScoped<IConfiguration>(Configuration);

            /*INJEÇÃO DE DEPENDENCIAS DE BANCO*/
            services.AddScoped(typeof(IBusinessBaseAsync<>), typeof(BusinessBaseAsync<>));


            // Injeção explícita dos serviços
            services.AddServicesInjection();
            services.AddAplicationServicesInjection();

            services.AddOptions();
        }

        // This method gets Race by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            loggerFactory.UseCustomLog(Configuration, $"Log/{BaseConfig.ApplicationName}-.txt");

            Utilities.SetHttpContext(serviceProvider.GetService<IHttpContextAccessor>());

            /* TRANSLATE API */
            //var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();

            //app.UseRequestLocalization(options.Value);

            /* PARA USO DO HANG FIRE ROTINAS*/
            if (EnableService)
                app.UseHangFire();

            /*CROP IMAGE*/
            app.UseImageResizer();

            // Configure rotas do SignalR e Webhook
            app.UseRouting();

            app.UseStaticFiles();

            var path = Path.Combine(Directory.GetCurrentDirectory(), @"Content");

            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = new PathString("/content")
            });

            app.UseCors("AllowAllOrigin");
            app.UseAuthorization();
            /*LOG BASICO*/
            app.UseRequestResponseLoggingLite();
            /*VERIFICA SE USUÁRIO ESTÁ COM ACESSO BLOQUEADO*/
            app.UseBlockMiddleware();
            /*RETORNO COM GZIP*/
            app.UseResponseCompression();
            /*JWT TOKEN*/
            app.UseJwtTokenApiAuth(Configuration);

            app.UseMvc();

            // Configure rotas do SignalR e Webhook
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<PaymentNotificationHub>("/PaymentNotificationHub").RequireCors("AllowAllOrigin");
                endpoints.MapControllers();
            });

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


            if (EnableService)
            {
                HangfileMiddleware.ClearJobs();

                var timeZoneBrazil = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

                RecurringJob.AddOrUpdate<IHangfireService>("NOTIFY_SCHEDULING", x => x.NotifyScheduling(null), Cron.Minutely(), timeZoneBrazil);

                RecurringJob.AddOrUpdate<IHangfireService>("REMOVE_AGENDA", x => x.RemoveAgenda(null), Cron.Daily(), timeZoneBrazil);

                /*EXAMPLE*/
                /*RODA TODA QUINTA ÀS 3 AM*/
                //RecurringJob.AddOrUpdate<IHangfireService>("NOME_DO_JOB", x => x.Metodo(),Cron.Weekly(DayOfWeek.Thursday, 3), timeZoneBrazil);
            }

            app.UseHealthChecks("/health", new HealthCheckOptions() { ResponseWriter = HealthCheckExtensions.WriteResponse });
        }
    }
}