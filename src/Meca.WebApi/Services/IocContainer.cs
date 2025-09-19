using System.Linq;
using System.Reflection;
using Meca.ApplicationService.Services.Hangfire;
using Meca.ApplicationService.Services.Hangfire.Interface;
using Meca.Domain.Services;
using Meca.Domain.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using UtilityFramework.Application.Core3;
using UtilityFramework.Services.Core3;
using UtilityFramework.Services.Core3.Interface;
using UtilityFramework.Services.Iugu.Core3;
using UtilityFramework.Services.Iugu.Core3.Interface;
using UtilityFramework.Services.Stripe.Core3;
using UtilityFramework.Services.Stripe.Core3.Interfaces;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using System;

namespace Meca.WebApi.Services
{
    public static class IocContainer
    {
        /// <summary>
        /// INJEÇÃO DE DEPENDENCIAS DE APLICATION SERVICES
        /// /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAplicationServicesInjection(this IServiceCollection services)
        {
            /*APLICATION SERVICES - INJEÇÃO AUTOMATICA DE DEPENDÊNCIAS*/
            var soluctionName = Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0];

            var assemblyName = Assembly
            .GetEntryAssembly()
            ?.GetReferencedAssemblies()
            .FirstOrDefault(x => x.Name.ContainsIgnoreCase($"{soluctionName}.ApplicationService"));

            if (assemblyName == null)
            {
                Console.WriteLine($"[IOC_DEBUG] ERRO: Não encontrou assembly {soluctionName}.ApplicationService");
                return services;
            }

            Console.WriteLine($"[IOC_DEBUG] Carregando assembly: {assemblyName.FullName}");

            var assembly = Assembly.Load(assemblyName);
            var types = assembly.GetTypes().ToList();
            var interfacesTypes = assembly.GetTypes().Where(x => x.GetTypeInfo().IsInterface).ToList();

            Console.WriteLine($"[IOC_DEBUG] Encontradas {interfacesTypes.Count} interfaces para registrar");
            Console.WriteLine($"[IOC_DEBUG] Encontrados {types.Count} tipos para registrar");

            for (var i = 0; i < interfacesTypes.Count; i++)
            {
                var className = interfacesTypes[i].Name.StartsWith("II") ? interfacesTypes[i].Name.Substring(1) : interfacesTypes[i].Name.TrimStart('I');
                var classType = types.Find(x => x.Name == className);
                if (classType != null)
                {
                    services.AddScoped(interfacesTypes[i], classType);
                    Console.WriteLine($"[IOC_DEBUG] Registrado: {interfacesTypes[i].Name} -> {classType.Name}");
                    
                    // Verificar especificamente o WorkshopService
                    if (interfacesTypes[i].Name == "IWorkshopService")
                    {
                        Console.WriteLine($"[IOC_DEBUG] WORKSHOP SERVICE REGISTRADO: {classType.Name}");
                    }
                }
                else
                {
                    Console.WriteLine($"[IOC_DEBUG] WARNING: Não encontrou implementação para {interfacesTypes[i].Name} (procurou por {className})");
                }
            }
            
            return services;
        }

        /// <summary>
        /// INJEÇÃO DE DEPENDENCIAS DE SERVIÇOS DE API
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServicesInjection(this IServiceCollection services)
        {
            Console.WriteLine($"[IOC_DEBUG] Iniciando registro de serviços de API");
            
            /*IUGU*/
            services.AddScoped(typeof(IIuguChargeServices), typeof(IuguService));
            services.AddScoped(typeof(IIuguMarketPlaceServices), typeof(IuguService));
            services.AddScoped(typeof(IIuguPaymentMethodService), typeof(IuguService));
            services.AddScoped(typeof(IIuguCustomerServices), typeof(IuguService));
            services.AddScoped(typeof(IIuguService), typeof(IuguService));
            Console.WriteLine($"[IOC_DEBUG] Serviços IUGU registrados");

            /*PAGBANK*/
            services.AddScoped(typeof(IPagBankService), typeof(PagBankService));
            services.AddScoped(typeof(PagBankPaymentService), typeof(PagBankPaymentService));
            Console.WriteLine($"[IOC_DEBUG] Serviços PagBank registrados");
            
            /*STRIPE - DESABILITADO PARA MIGRAÇÃO*/
            // services.AddScoped(typeof(IStripeCustomerService), typeof(StripeCustomerService));
            // services.AddScoped(typeof(IStripeMarketPlaceService), typeof(StripeMarketPlaceService));
            // services.AddScoped(typeof(IStripePaymentMethodService), typeof(StripePaymentMethodService));
            // services.AddScoped(typeof(IStripePaymentIntentService), typeof(StripePaymentIntentService));
            // services.AddScoped(typeof(IStripeTransferService), typeof(StripeTransferService));
            Console.WriteLine($"[IOC_DEBUG] Serviços Stripe desabilitados para migração");

            /* NOTIFICAÇÕES & EMAIL*/
            services.AddScoped(typeof(ISenderMailService), typeof(SendService));
            services.AddScoped(typeof(ISenderNotificationService), typeof(SendService));
            Console.WriteLine($"[IOC_DEBUG] Serviços de notificação e email registrados");

            /*FIREBASE*/
            services.AddScoped(typeof(IFirebaseServices), typeof(FirebaseServices));
            Console.WriteLine($"[IOC_DEBUG] Serviços Firebase registrados");

            /*UTILIDADES */
            services.AddScoped(typeof(IUtilService), typeof(UtilService));
            services.AddScoped(typeof(IHangfireService), typeof(HangfireService));
            services.AddScoped(typeof(IAgoraIOService), typeof(AgoraIOService));
            Console.WriteLine($"[IOC_DEBUG] Serviços de utilidades registrados");

            /*FINANCIAL HISTORY */
            services.AddScoped(typeof(IFinancialHistoryService), typeof(FinancialHistoryService));
            Console.WriteLine($"[IOC_DEBUG] Serviços FinancialHistory registrados");

            /*WORKSHOP SERVICES */
            services.AddScoped(typeof(IWorkshopServicesService), typeof(WorkshopServicesService));
            Console.WriteLine($"[IOC_DEBUG] Serviços WorkshopServices registrados");

            Console.WriteLine($"[IOC_DEBUG] Registro de serviços de API finalizado");
            return services;
        }
    }


}