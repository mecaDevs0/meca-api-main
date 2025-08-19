using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                    Console.WriteLine("[MECA_DEBUG] WebHostDefaults configurado");
                });
    }
}
