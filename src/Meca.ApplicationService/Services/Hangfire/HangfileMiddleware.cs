using System;
using Hangfire;
using Hangfire.Console;
using Hangfire.MemoryStorage;
using Hangfire.Mongo;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;


namespace Meca.ApplicationService.Services.HangFire
{
    public static class HangfileMiddleware
    {
        public static IServiceCollection AddHangfireMongoDb(this IServiceCollection services, IConfiguration configuration, bool useMongoDb = true, string prefix = "Hangfire", int attempts = 3, bool checkConnection = false)
        {
            MongoUrlBuilder mongoUrlBuilder = null;
            MongoClient mongoClient = null;
            MongoStorageOptions storageOptions = null;

            if (useMongoDb)
            {
                var remoteDatabase = configuration.GetSection("DATABASE:REMOTE").Get<string>();
                var dataBaseName = configuration.GetSection("DATABASE:NAME").Get<string>();

                mongoUrlBuilder = new MongoUrlBuilder($"mongodb://{remoteDatabase}/{dataBaseName}");
                mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

                var migrationOptions = new MongoMigrationOptions
                {
                    Strategy = MongoMigrationStrategy.Drop,
                    BackupStrategy = MongoBackupStrategy.None
                };

                storageOptions = new MongoStorageOptions
                {
                    MigrationOptions = migrationOptions,
                    Prefix = prefix,
                    CheckConnection = checkConnection
                };

            }

            GlobalJobFilters.Filters.Add(new ProlongExpirationTimeAttribute());
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
            {
                Attempts = attempts,
                OnAttemptsExceeded = AttemptsExceededAction.Delete
            });

            // Add Hangfire services. Hangfire.AspNetCore nuget required
            services.AddHangfire(configuration =>
            {

                configuration.UseConsole();
                if (useMongoDb)
                    configuration.UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, storageOptions);
                else
                    configuration.UseMemoryStorage();
            });

            return services;
        }

        public static IApplicationBuilder UseHangFire(this IApplicationBuilder app)
        {
            app.UseHangfireServer();
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new []
                {
                    new MyAuthorizationFilter(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>())
                }
            });
            return app;
        }

        public static void ClearJobs()
        {
            try
            {
                using var connection = JobStorage.Current.GetConnection();
                var jobs = connection.GetRecurringJobs();
                if (jobs != null)
                {
                    for (int i = 0; i < jobs.Count; i++)
                    {
                        RecurringJob.RemoveIfExists(jobs[i].Id);
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

    }


}