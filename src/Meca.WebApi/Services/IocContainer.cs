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
                return services;

            var assembly = Assembly.Load(assemblyName);
            var types = assembly.GetTypes().ToList();
            var interfacesTypes = assembly.GetTypes().Where(x => x.GetTypeInfo().IsInterface).ToList();

            for (var i = 0; i < interfacesTypes.Count; i++)
            {
                var className = interfacesTypes[i].Name.StartsWith("II") ? interfacesTypes[i].Name.Substring(1) : interfacesTypes[i].Name.TrimStart('I');
                var classType = types.Find(x => x.Name == className);
                if (classType != null)
                    services.AddScoped(interfacesTypes[i], classType);
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
            /*IUGU*/
            services.AddScoped(typeof(IIuguChargeServices), typeof(IuguService));
            services.AddScoped(typeof(IIuguMarketPlaceServices), typeof(IuguService));
            services.AddScoped(typeof(IIuguPaymentMethodService), typeof(IuguService));
            services.AddScoped(typeof(IIuguCustomerServices), typeof(IuguService));
            services.AddScoped(typeof(IIuguService), typeof(IuguService));

            /*STRIPE*/
            services.AddScoped(typeof(IStripeCustomerService), typeof(StripeCustomerService));
            services.AddScoped(typeof(IStripeMarketPlaceService), typeof(StripeMarketPlaceService));
            services.AddScoped(typeof(IStripePaymentMethodService), typeof(StripePaymentMethodService));
            services.AddScoped(typeof(IStripePaymentIntentService), typeof(StripePaymentIntentService));
            services.AddScoped(typeof(IStripeTransferService), typeof(StripeTransferService));


            /* NOTIFICAÇÕES & EMAIL*/
            services.AddScoped(typeof(ISenderMailService), typeof(SendService));
            services.AddScoped(typeof(ISenderNotificationService), typeof(SendService));

            /*FIREBASE*/
            services.AddScoped(typeof(IFirebaseServices), typeof(FirebaseServices));

            /*UTILIDADES */
            services.AddScoped(typeof(IUtilService), typeof(UtilService));
            services.AddScoped(typeof(IHangfireService), typeof(HangfireService));
            services.AddScoped(typeof(IAgoraIOService), typeof(AgoraIOService));

            return services;
        }
    }
}