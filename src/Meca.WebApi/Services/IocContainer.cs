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
            services.AddSingleton(typeof(IIuguChargeServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguMarketPlaceServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguPaymentMethodService), typeof(IuguService));
            services.AddSingleton(typeof(IIuguCustomerServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguService), typeof(IuguService));

            /*STRIPE*/
            services.AddSingleton(typeof(IStripeCustomerService), typeof(StripeCustomerService));
            services.AddSingleton(typeof(IStripeMarketPlaceService), typeof(StripeMarketPlaceService));
            services.AddSingleton(typeof(IStripePaymentMethodService), typeof(StripePaymentMethodService));
            services.AddSingleton(typeof(IStripePaymentIntentService), typeof(StripePaymentIntentService));
            services.AddSingleton(typeof(IStripeTransferService), typeof(StripeTransferService));



            /* NOTIFICAÇÕES & EMAIL*/
            services.AddSingleton(typeof(ISenderMailService), typeof(SendService));
            services.AddSingleton(typeof(ISenderNotificationService), typeof(SendService));

            /*FIREBASE*/
            services.AddSingleton(typeof(IFirebaseServices), typeof(FirebaseServices));

            /*UTILIDADES */
            services.AddSingleton(typeof(IUtilService), typeof(UtilService));
            services.AddSingleton(typeof(IHangfireService), typeof(HangfireService));
            services.AddSingleton(typeof(IAgoraIOService), typeof(AgoraIOService));

            return services;
        }
    }
}