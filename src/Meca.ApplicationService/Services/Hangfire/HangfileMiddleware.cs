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
            string dataBaseName = null;

            if (useMongoDb)
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                dataBaseName = configuration.GetValue<string>("DatabaseName");

                // Add null check and fallback to DATABASE section if needed
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = configuration.GetSection("DATABASE:CONNECTIONSTRING").Value;
                    dataBaseName = configuration.GetSection("DATABASE:NAME").Value;
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is not configured. Please check appsettings.json");
                }

                try
                {
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("MongoDB connection string is null or empty");
                    }
                    
                    mongoUrlBuilder = new MongoUrlBuilder(connectionString);
                    mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());
                }
                catch (Exception ex)
                {
                    // If MongoUrlBuilder fails, try direct connection string
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("MongoDB connection string is null or empty", ex);
                    }
                    
                    mongoClient = new MongoClient(connectionString);
                    // Extract database name from connection string or use configured value
                    if (string.IsNullOrEmpty(dataBaseName))
                    {
                        var mongoUrl = MongoUrl.Create(connectionString);
                        dataBaseName = mongoUrl.DatabaseName ?? "meca-app-2025";
                    }
                    mongoUrlBuilder = new MongoUrlBuilder(connectionString);
                }

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
                    configuration.UseMongoStorage(mongoClient, dataBaseName, storageOptions);
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