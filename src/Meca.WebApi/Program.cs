using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Meca.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("[MECA_DEBUG] Program.Main iniciado");
            Console.WriteLine($"[MECA_DEBUG] Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            
            try
            {
                Console.WriteLine("[MECA_DEBUG] Criando HostBuilder...");
                var host = CreateHostBuilder(args).Build();
                Console.WriteLine("[MECA_DEBUG] HostBuilder criado com sucesso");
                
                Console.WriteLine("[MECA_DEBUG] Iniciando aplicação...");
                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MECA_DEBUG] ERRO na inicialização: {ex.Message}");
                Console.WriteLine($"[MECA_DEBUG] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    Console.WriteLine("[MECA_DEBUG] Configurando WebHostDefaults...");
                    webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                    Console.WriteLine("[MECA_DEBUG] WebHostDefaults configurado");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    Console.WriteLine("[MECA_DEBUG] Configurando serviços no Program.cs...");
                    
                    // Configurar Startup
                    var startup = new Startup(hostContext.Configuration);
                    startup.ConfigureServices(services);
                    
                    Console.WriteLine("[MECA_DEBUG] Serviços configurados no Program.cs");
                })
                .Configure((hostContext, app) =>
                {
                    Console.WriteLine("[MECA_DEBUG] Configurando aplicação no Program.cs...");
                    
                    // Configurar Startup
                    var startup = new Startup(hostContext.Configuration);
                    var env = hostContext.HostingEnvironment;
                    var loggerFactory = hostContext.Services.GetRequiredService<ILoggerFactory>();
                    var httpContextAccessor = hostContext.Services.GetRequiredService<IHttpContextAccessor>();
                    var serviceProvider = hostContext.Services;
                    
                    startup.Configure(app, env, loggerFactory, httpContextAccessor, serviceProvider);
                    
                    Console.WriteLine("[MECA_DEBUG] Aplicação configurada no Program.cs");
                });
    }
}
