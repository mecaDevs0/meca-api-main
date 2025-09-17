using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Arteris.Domain.ViewModels;
using Arteris.Domain.ViewModels.HereApi;
using AutoMapper;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using MimeTypes.Core;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Data.Modelos;
using UtilityFramework.Services.Iugu.Core3.Entity;
using UtilityFramework.Services.Iugu.Core3.Models;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using UtilityFramework.Services.Stripe.Core3.Models;
using UtilityFramework.Services.Stripe.Core3;
using UtilityFramework.Services.Stripe.Core3.Interfaces;
using Meca.Data.Entities;
using Stripe;

namespace Meca.Domain
{
    public static class Util
    {
        private static IStringLocalizer _localizer { get; set; }

        public static string GetCustomUrl(int index)
            => BaseConfig.CustomUrls[index].TrimEnd('/');

        public static Dictionary<string, string> GetTemplateVariables()
        {

            var dataBody = new Dictionary<string, string>();
            try
            {
                dataBody.Add("__bg-cardbody__", "#F2F3F3");
                dataBody.Add("__bg-cardtitle__", "#00C977");
                dataBody.Add("__bg-cardfooter__", "#00C977");
                dataBody.Add("__cl-body__", "#000000");
                dataBody.Add("{{ baseUrl }}", $"{GetCustomUrl(0)}/content/images");
                dataBody.Add("{{ contact }}", Utilities.GetConfigurationRoot().GetSection("contactEmail").Get<string>());
                dataBody.Add("{{ appName }}", BaseConfig.ApplicationName);
            }
            catch (Exception)
            {

                //unused
            }

            return dataBody;

        }

        public static string GetEmailSignature()
        {
            try
            {
                var message = new StringBuilder();
                message.AppendLine("<div class=\"divider\"></div>");
                message.AppendLine("<p style=\"margin: 0; font-size: 16px; color: #4a5568;\">Atenciosamente,</p>");
                message.AppendLine("<p style=\"margin: 0; font-weight: 600; color: #2d3748;\">Equipe {{ appName }}</p>");

                return message.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetTemplateVerificationDataBank(string status)
        {
            try
            {
                var message = new StringBuilder();
                message.AppendLine("<p>Olá <strong>{{ name }}</strong>,</p>");

                switch (status)
                {
                    case "accepted":
                        message.AppendLine("<p>✅ <strong>Ótimas notícias!</strong> Seus dados bancários foram <strong>validados com sucesso</strong>.</p>");
                        message.AppendLine("<p>Agora você já pode receber suas transações com cartão de crédito em nossa plataforma.</p>");
                        message.AppendLine("<div style=\"background: #f0f9ff; border: 1px solid #0ea5e9; border-radius: 12px; padding: 20px; margin: 20px 0;\">");
                        message.AppendLine("<p style=\"margin: 0; color: #0c4a6e;\"><strong>Status:</strong> Dados bancários aprovados ✅</p>");
                        message.AppendLine("</div>");
                        message.AppendLine("<p>Você pode começar a receber pagamentos imediatamente!</p>");
                        break;
                    default:
                        message.AppendLine("<p>❌ <strong>Atenção!</strong> Seus dados bancários foram <strong>rejeitados</strong>.</p>");
                        message.AppendLine("<p>Infelizmente, os dados bancários informados não puderam ser validados.</p>");
                        message.AppendLine("<div style=\"background: #fef2f2; border: 1px solid #ef4444; border-radius: 12px; padding: 20px; margin: 20px 0;\">");
                        message.AppendLine("<p style=\"margin: 0; color: #991b1b;\"><strong>Status:</strong> Dados bancários rejeitados ❌</p>");
                        message.AppendLine("</div>");
                        message.AppendLine("<p>Por favor, verifique os dados informados e atualize-os em sua conta para continuar recebendo pagamentos.</p>");
                        message.AppendLine("<div style=\"text-align: center; margin: 30px 0;\">");
                        message.AppendLine("<a href=\"#\" class=\"cta-button\">Atualizar Dados Bancários</a>");
                        message.AppendLine("</div>");
                        break;
                }

                message.AppendLine(GetEmailSignature());

                return message.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetWellcomeTemplate(bool dashboard = false)
        {
            try
            {
                var isDashboard = dashboard ? " - Dashboard" : "";

                var message = new StringBuilder();
                message.AppendLine("<p>Olá <strong>{{ name }}</strong>,</p>");
                message.AppendLine("<p>Seja muito bem-vindo ao <strong>{{ appName }}</strong>" + isDashboard + "! 🎉</p>");
                message.AppendLine("<p>Sua conta foi criada com sucesso e você já pode acessar nossa plataforma. Suas credenciais de acesso estão abaixo:</p>");
                message.AppendLine("<div class=\"credentials\">");
                message.AppendLine("<div class=\"credential-item\">");
                message.AppendLine("<span class=\"credential-label\">E-mail:</span>");
                message.AppendLine("<span class=\"credential-value\">{{ email }}</span>");
                message.AppendLine("</div>");
                message.AppendLine("<div class=\"credential-item\">");
                message.AppendLine("<span class=\"credential-label\">Senha:</span>");
                message.AppendLine("<span class=\"credential-value\">{{ password }}</span>");
                message.AppendLine("</div>");
                message.AppendLine("</div>");
                message.AppendLine("<p><strong>🔐 Importante:</strong> Por segurança, recomendamos que você altere sua senha no primeiro acesso.</p>");
                message.AppendLine("<p>Estamos felizes em tê-lo conosco! Se precisar de ajuda, não hesite em entrar em contato.</p>");
                message.AppendLine(GetEmailSignature());

                return message.ToString();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetForgotPasswordTemplate(bool dashboard = false)
        {
            try
            {
                var isDashboard = dashboard ? " - Dashboard" : "";

                var message = new StringBuilder();
                message.AppendLine("<p>Olá <strong>{{ name }}</strong>,</p>");
                message.AppendLine("<p>Recebemos sua solicitação de recuperação de senha para o <strong>{{ appName }}</strong>" + isDashboard + ". Sua nova senha foi gerada com sucesso e está pronta para uso!</p>");
                message.AppendLine("<div class=\"credentials\">");
                message.AppendLine("<div class=\"credential-item\">");
                message.AppendLine("<span class=\"credential-label\">E-mail:</span>");
                message.AppendLine("<span class=\"credential-value\">{{ email }}</span>");
                message.AppendLine("</div>");
                message.AppendLine("<div class=\"credential-item\">");
                message.AppendLine("<span class=\"credential-label\">Nova Senha:</span>");
                message.AppendLine("<span class=\"credential-value\">{{ password }}</span>");
                message.AppendLine("</div>");
                message.AppendLine("</div>");
                message.AppendLine("<p><strong>💡 Dica de Segurança:</strong> Para sua proteção, recomendamos que você altere esta senha no próximo acesso ao sistema.</p>");
                message.AppendLine("<p>Se você não solicitou esta recuperação de senha, entre em contato conosco imediatamente.</p>");
                message.AppendLine(GetEmailSignature());

                return message.ToString();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetApproveOrReproveTemplate(bool approve = false)
        {
            try
            {
                var message = new StringBuilder();
                message.AppendLine("<p>Olá <strong>{{ name }}</strong>,</p>");
                
                if (approve)
                {
                    message.AppendLine($"<p>🎉 <strong>Parabéns!</strong> Seja bem-vindo ao <strong>{BaseConfig.ApplicationName}</strong>!</p>");
                    message.AppendLine("<p>Seu cadastro foi <strong>aprovado</strong> e você está habilitado para realizar serviços em nossa plataforma.</p>");
                    message.AppendLine("<p>Acesse o sistema e comece a utilizar agora mesmo! Estamos ansiosos para trabalhar com você.</p>");
                    message.AppendLine("<div style=\"text-align: center; margin: 30px 0;\">");
                    message.AppendLine("<a href=\"#\" class=\"cta-button\">Acessar Plataforma</a>");
                    message.AppendLine("</div>");
                }
                else
                {
                    message.AppendLine($"<p>Infelizmente, seu cadastro no <strong>{BaseConfig.ApplicationName}</strong> foi <strong>reprovado</strong>.</p>");
                    message.AppendLine("<p>Você não poderá realizar serviços em nossa plataforma no momento.</p>");
                    message.AppendLine("<p>Caso tenha dúvidas sobre esta decisão ou queira mais informações, entre em contato com nosso suporte.</p>");
                    message.AppendLine("<div style=\"text-align: center; margin: 30px 0;\">");
                    message.AppendLine("<a href=\"mailto:{{ contact }}\" class=\"cta-button\">Entrar em Contato</a>");
                    message.AppendLine("</div>");
                }

                message.AppendLine(GetEmailSignature());

                return message.ToString();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetErrorMessage(this IuguChargeResponse checkout)
        {
            string message = null;
            if (string.IsNullOrEmpty(checkout.MsgLR) == false)
                message = checkout.MsgLR;
            else if (string.IsNullOrEmpty(checkout.MessageError) == false)
                message = checkout.MessageError;
            else if (string.IsNullOrEmpty(checkout.Message) == false)
                message = checkout.Message;
            else
                message = "Não foi possível efetuar a cobrança, verifique os dados do cartão ou saldo";

            return message;
        }

        public static string GetExtension(this IFormFile file, bool fromMimeType = true)
        {
            var extension = Path.GetExtension(file.FileName);

            if (fromMimeType)
            {
                try
                {
                    extension = MimeTypeMap.GetExtension(file.ContentType);
                }
                catch (Exception) {/*UNUSED*/}
            }
            return (extension.Count(x => x == '.') == 0 ? $".{extension}" : extension).ToLower();
        }

        public static dynamic GetSettingsPush()
        {

            dynamic settings = new JObject();

            settings.ios_badgeType = "Increase";
            settings.ios_badgeCount = 1;
            //settings.android_channel_id = ""; /*solicitar para equipe mobile*/

            return settings;

        }

        public static dynamic GetPayloadPush(RouteNotification route = RouteNotification.System)
        {

            dynamic payload = new JObject();

            payload.route = (int)route;

            return payload;
        }

        public static bool IsSandBox(IHostingEnvironment env = null)
        {
            if (env == null && Utilities.HttpContextAccessor?.HttpContext?.RequestServices != null)
                env = Utilities.HttpContextAccessor.HttpContext.RequestServices.GetService(typeof(IHostingEnvironment)) as IHostingEnvironment;

            if (env == null)
                return true;

            return Utilities.GetConfigurationRoot(environment: env).GetSection("IUGU:SANDBOX").Get<bool>();
        }

        public static SortDefinitionBuilder<T> Sort<T>() where T : ModelBase
         => Builders<T>.Sort;

        public static SortDefinition<T> MapSort<T>(DtOrder[] order, DtColumn[] columns, string sortColum) where T : ModelBase
        {
            try
            {
                var listSort = new List<SortDefinition<T>>();

                for (int i = 0; i < order.Length; i++)
                {
                    switch (order[i].Dir)
                    {
                        case DtOrderDir.Desc:
                            listSort.Add(Sort<T>().Descending(columns[order[i].Column]?.Name ?? sortColum));
                            break;
                        default:
                            listSort.Add(Sort<T>().Ascending(columns[order[i].Column]?.Name ?? sortColum));
                            break;
                    }
                }
                return Sort<T>().Combine(listSort);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static SortDefinition<T> CustomSort<T>(this SortByCustom sort, string field) where T : ModelBase
         => sort == SortByCustom.Asc ? Util.Sort<T>().Ascending(field) : Util.Sort<T>().Descending(field);


        public static Claim SetRole(TypeProfile typeProfile)
         => new Claim(ClaimTypes.Role, ((int)typeProfile).ToString());

        public static FindOptions<T> FindOptions<T>(BaseFilterViewModel filterView, SortDefinition<T> sortBy = null)
        {
            var findOptions = new FindOptions<T>() { Collation = new Collation("en", strength: CollationStrength.Primary) };

            if (sortBy != null)
                findOptions.Sort = sortBy;

            if (filterView != null && filterView.Limit != 0)
            {
                findOptions.Skip = filterView.SkipDocuments();
                findOptions.Limit = filterView.Limit;
            }

            return findOptions;
        }

        public static bool HasFilter<T>(this T filterView) where T : class
            => filterView.GetType().GetProperties().Count(p => p.GetValue(filterView) != null) > 0;

        public static bool CheckHasField(this string[] jsonBody, string fieldName)
            => jsonBody != null && jsonBody.Count(x => x == fieldName) > 0;

        public static TypeProfile GetRole(this HttpRequest request)
        {

            var role = request.GetClaimFromToken(ClaimTypes.Role);

            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException(nameof(ClaimTypes.Role), "Tipo de usuário não identificado");

            return (TypeProfile)Enum.Parse(typeof(TypeProfile), role);
        }
        public static List<SelectItemEnumViewModel> GetMembersOfEnum<T>()
        {
            try
            {
                if (typeof(T).GetTypeInfo().IsEnum == false)
                    throw new ArgumentException("Type must be an enum");

                return Enum.GetValues(typeof(T))
                    .Cast<T>()
                    .Select(x => new SelectItemEnumViewModel()
                    {
                        Value = (int)(object)x,
                        Name = x.ToString(),
                    }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static Language GetCurrentLocale(this HttpRequest request)
        {
            try
            {
                if (request.Headers.Keys.Count(x => x == "Accept-Language") > 0)
                    return (Language)Enum.Parse(typeof(Language), request.Headers.GetHeaderValue("Accept-Language"), true);
            }
            catch (Exception)
            {
                /**/
            }
            return Language.En;
        }

        public static List<NotificationAux> MapNotificationAux<T>(IMapper _mapper, T dataEntity)
        {
            var response = new List<NotificationAux>();

            if (dataEntity != null)
                response.Add(_mapper.Map<NotificationAux>(dataEntity));

            return response;
        }


        private static void SetLocalizer()
        {
            if (_localizer == null)
            {
                var factory = Utilities.HttpContextAccessor.HttpContext.RequestServices.GetService(typeof(IStringLocalizerFactory)) as IStringLocalizerFactory;
                var type = typeof(SharedResource);
                _localizer = factory.Create(type);
            }
        }

        public static string GetTranslate(this string key)
        {
            if (_localizer == null)
                SetLocalizer();

            return _localizer[key]?.ToString();
        }

        /// <summary>
        /// OBTER DADOS DE SUBCONTA IUGU EM Config.json
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static MegaClientIuguViewModel GetIuguSubAccount(string field = "Client", IHostingEnvironment env = null)
        {
            if (env == null)
                env = Utilities.HttpContextAccessor.HttpContext.RequestServices.GetService(typeof(IHostingEnvironment)) as IHostingEnvironment;

            return Utilities.GetConfigurationRoot(environment: env).GetSection($"IUGU:{field}").Get<MegaClientIuguViewModel>();
        }

        /// <summary>
        /// MAPEAR DADOS DE ENDEREÇO PARA IUGU (AddressModel)
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static AddressModel SetAddress<T>(T data) where T : new()
        {
            var address = new AddressModel();

            try
            {
                address.ZipCode = Utilities.GetValueByProperty(data, "ZipCode") as string;
                address.Street = Utilities.GetValueByProperty(data, "StreetAddress") as string;
                address.Number = Utilities.GetValueByProperty(data, "Number") as string;
                address.Complement = Utilities.GetValueByProperty(data, "Complement") as string;
                address.District = Utilities.GetValueByProperty(data, "Neighborhood") as string;
                address.State = Utilities.GetValueByProperty(data, "StateUf") as string;
                address.City = Utilities.GetValueByProperty(data, "CityName") as string;
                address.Country = "Brasil";
            }
            catch (Exception)
            {

                throw;
            }

            return address;
        }

        public static double MapResponseCalculateKM(string content)
        {
            CalculateKmResponseViewModel response = null;
            try
            {
                var data = JsonConvert.DeserializeObject<HereApiResponseViewModel>(content);

                if (data.Routes.Count > 0)
                {
                    var routeInfo = data.Routes[0].Sections[0];

                    response = new CalculateKmResponseViewModel()
                    {
                        Travel = new TravelViewModel()
                        {
                            Distance = routeInfo.Summary.Length
                        }
                    };
                }

                return response != null ? (response.Travel.Distance / 1000) : 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<double> GetDistanceAsync(double originLat, double originLng, double destLat, double destLng)
        {
            HttpClient httpClient = new HttpClient();
            var apiKey = Utilities.GetConfigurationRoot().GetSection("googleApi:apiKey").Get<string>();
            string url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                     $"?origins={originLat.ToString(CultureInfo.InvariantCulture)},{originLng.ToString(CultureInfo.InvariantCulture)}" +
                     $"&destinations={destLat.ToString(CultureInfo.InvariantCulture)},{destLng.ToString(CultureInfo.InvariantCulture)}" +
                     $"&units=metric&key={apiKey}";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return 0.0;

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(jsonResponse);

            // Verifica se a resposta contém dados válidos
            var status = data["rows"]?[0]?["elements"]?[0]?["status"]?.ToString();
            if (status != "OK")
                return 0.0;

            // Obtém a distância em metros e converte para KM
            double distanceMeters = data["rows"][0]["elements"][0]["distance"]["value"].ToObject<double>();
            return distanceMeters / 1000.0;
        }

        public static string GetDisplayName(Type viewModelType, string propertyName)
        {
            // Obtém a propriedade da view model
            var property = viewModelType.GetProperty(propertyName);

            if (property != null)
            {
                // Obtém o atributo [Display(Name = "...")] da propriedade
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();

                // Se o atributo existir, retorna o valor de Name
                if (displayAttribute != null)
                {
                    return displayAttribute.Name;
                }
            }

            // Se não encontrar o atributo, retorna o nome da propriedade
            return propertyName;
        }

        public static SchedulingStatusTitle GetStatusTitle(SchedulingStatus schedulingStatus)
        {
            var status = (int)schedulingStatus;
            var statusTitle = SchedulingStatusTitle.Scheduling;

            if (status >= (int)SchedulingStatus.WaitingForBudget && status <= (int)SchedulingStatus.BudgetDisapprove)
            {
                statusTitle = SchedulingStatusTitle.Budget;
            }

            if (status >= (int)SchedulingStatus.WaitingForPayment && status <= (int)SchedulingStatus.PaymentRejected)
            {
                statusTitle = SchedulingStatusTitle.Payment;
            }

            if (status >= (int)SchedulingStatus.WaitingStart && status <= (int)SchedulingStatus.ServiceCompleted)
            {
                statusTitle = SchedulingStatusTitle.Service;
            }

            if (status >= (int)SchedulingStatus.WaitingForServiceApproval && status <= (int)SchedulingStatus.ServiceReprovedByAdmin)
            {
                statusTitle = SchedulingStatusTitle.Approval;
            }

            if (status == (int)SchedulingStatus.ServiceFinished)
            {
                statusTitle = SchedulingStatusTitle.Completed;
            }

            return statusTitle;
        }


        public static StripeCustomerRequest MapStripeCustomerRequest(this Data.Entities.Profile profileEntity)
        {
            return new StripeCustomerRequest()
            {
                CustomerId = string.IsNullOrEmpty(profileEntity.ExternalId) ? null : profileEntity.ExternalId,
                CpfCnpj = profileEntity.Cpf,
                Email = profileEntity.Email,
                FullName = profileEntity.FullName,
                Phone = profileEntity.Phone,
                Address = new StripeAddress()
                {
                    ZipCode = profileEntity.ZipCode,
                    Street = profileEntity.StreetAddress,
                    Number = profileEntity.Number,
                    Complement = profileEntity.Complement,
                    City = profileEntity.CityName,
                    State = profileEntity.StateName,
                    Neighborhood = profileEntity.Neighborhood,
                }
            };
        }

        public static StripeMarketPlaceRequest MapAccount(this Workshop workshopEntity,
                                                          string remoteIP,
                                                          string userAgent)
        {

            var response = new StripeMarketPlaceRequest()
            {
                Email = workshopEntity.Email,
                BusinessName = workshopEntity.CompanyName,
                AcceptTerms = true,
                BusinessType = EStripeBusinessType.Company,
                RemoteIp = remoteIP,
                UserAgent = userAgent,
                SupportEmail = workshopEntity.Email,
                PayoutSchedule = EStripePayoutSchedule.Daily,
                ProductDescription = "Serviços de manutenção e reparo de veículos automotivos",
            };

            // if (workshopEntity.PersonType == TypePersonBank.PhysicalPerson)
            // {
            //     response.Individual = new StripeIndividualRequest()
            //     {
            //         FirstName = workshopEntity.FullName.GetFirstName(),
            //         LastName = workshopEntity.FullName.GetLastName(),
            //         Email = workshopEntity.Email,
            //         Phone = workshopEntity.Phone,
            //         Cpf = workshopEntity.AccountableCpf,
            //         RequiredDocument = false,
            //         Address = new StripeAddress()
            //         {
            //             ZipCode = workshopEntity.ZipCode,
            //             Street = workshopEntity.StreetAddress,
            //             Number = workshopEntity.Number,
            //             Complement = workshopEntity.Complement,
            //             City = workshopEntity.CityName,
            //             State = workshopEntity.StateName,
            //             Neighborhood = workshopEntity.Neighborhood,
            //         },
            //     };
            // }

            response.Company = new StripeCompanyRequest()
            {
                LegalName = workshopEntity.CompanyName,
                Phone = workshopEntity.Phone.MapPhone(),
                Cnpj = workshopEntity.Cnpj,
                RequiredDocument = false,
                Address = new StripeAddress()
                {
                    ZipCode = workshopEntity.ZipCode,
                    Street = workshopEntity.StreetAddress,
                    Number = workshopEntity.Number,
                    Complement = workshopEntity.Complement,
                    City = workshopEntity.CityName,
                    State = workshopEntity.StateName,
                    Neighborhood = workshopEntity.Neighborhood,
                },
                Representative = new StripeIndividualRequest()
                {
                    FirstName = workshopEntity.FullName.GetFirstName(),
                    LastName = workshopEntity.FullName.GetLastName(),
                    Email = workshopEntity.Email,
                    Phone = workshopEntity.Phone.MapPhone(),
                    Cpf = workshopEntity.Cpf,
                    RequiredDocument = false,
                    Address = new StripeAddress()
                    {
                        ZipCode = workshopEntity.ZipCode,
                        Street = workshopEntity.StreetAddress,
                        Number = workshopEntity.Number,
                        Complement = workshopEntity.Complement,
                        City = workshopEntity.CityName,
                        State = workshopEntity.StateName,
                        Neighborhood = workshopEntity.Neighborhood,
                    },
                    BirthDate = workshopEntity.BirthDate,
                    DocumentFront = workshopEntity.FileDocument?.Split('/')?.LastOrDefault()?.ResolveFilePath()
                },
                DocumentFront = workshopEntity.MeiCard?.Split('/')?.LastOrDefault()?.ResolveFilePath()
            };

            return response;
        }
        public async static Task<AccountUpdateOptions> MapUpdateAccount(this Workshop workshopEntity, IStripeMarketPlaceService stripeService)
        {

            var updateOptions = new AccountUpdateOptions()
            {

                Email = workshopEntity.Email,
                BusinessProfile = new AccountBusinessProfileOptions()
                {
                    Name = workshopEntity.CompanyName ?? workshopEntity.FullName,
                    SupportEmail = workshopEntity.Email,
                    SupportPhone = workshopEntity.Phone
                }
            };

            var options = new AccountCreateOptions();

            if (workshopEntity.PersonType == TypePersonBank.LegalPerson)
            {
                await stripeService.ConfigureCompanyOptions(options, new StripeCompanyRequest()
                {
                    LegalName = workshopEntity.CompanyName,
                    Phone = workshopEntity.Phone.MapPhone(),
                    Cnpj = workshopEntity.AccountableCpf,
                    RequiredDocument = false,
                    Address = new StripeAddress()
                    {
                        ZipCode = workshopEntity.ZipCode,
                        Street = workshopEntity.StreetAddress,
                        Number = workshopEntity.Number,
                        Complement = workshopEntity.Complement,
                        City = workshopEntity.CityName,
                        State = workshopEntity.StateName,
                        Neighborhood = workshopEntity.Neighborhood,
                    },
                    Representative = new StripeIndividualRequest()
                    {
                        FirstName = workshopEntity.FullName.GetFirstName(),
                        LastName = workshopEntity.FullName.GetLastName(),
                        Email = workshopEntity.Email,
                        Phone = workshopEntity.Phone.MapPhone(),
                        Cpf = workshopEntity.AccountableCpf,
                        RequiredDocument = false,
                        Address = new StripeAddress()
                        {
                            ZipCode = workshopEntity.ZipCode,
                            Street = workshopEntity.StreetAddress,
                            Number = workshopEntity.Number,
                            Complement = workshopEntity.Complement,
                            City = workshopEntity.CityName,
                            State = workshopEntity.StateName,
                            Neighborhood = workshopEntity.Neighborhood,
                        },
                    },
                    DocumentFront = workshopEntity.MeiCard
                });

                updateOptions.Company = options.Company;

                var person = await stripeService.ConfigurePersonOptions(new StripeIndividualRequest()
                {
                    FirstName = workshopEntity.FullName.GetFirstName(),
                    LastName = workshopEntity.FullName.GetLastName(),
                    Email = workshopEntity.Email,
                    Phone = workshopEntity.Phone.MapPhone(),
                    Cpf = workshopEntity.AccountableCpf,
                    RequiredDocument = false,
                    Address = new StripeAddress()
                    {
                        ZipCode = workshopEntity.ZipCode,
                        Street = workshopEntity.StreetAddress,
                        Number = workshopEntity.Number,
                        Complement = workshopEntity.Complement,
                        City = workshopEntity.CityName,
                        Neighborhood = workshopEntity.Neighborhood,
                        State = workshopEntity.StateName,
                    }
                });
            }
            else
            {
                updateOptions.Individual = new AccountIndividualOptions()
                {
                    FirstName = workshopEntity.FullName.GetFirstName(),
                    LastName = workshopEntity.FullName.GetLastName(),
                    Email = workshopEntity.Email,
                    Phone = workshopEntity.Phone,
                    PoliticalExposure = "none",
                    Relationship = new()
                    {
                        Director = true,
                        Executive = true,
                        Owner = true,
                        PercentOwnership = 100,
                        Title = "CEO"
                    },
                    Address = new AddressOptions()
                    {
                        PostalCode = workshopEntity.ZipCode,
                        Line1 = $"{workshopEntity.StreetAddress}, {workshopEntity.Number}",
                        Line2 = workshopEntity.Complement,
                        City = workshopEntity.CityName,
                        State = workshopEntity.StateName,
                        Country = "br"
                    }
                };
            }
            return updateOptions;
        }

        public static Data.Enum.PaymentMethod MapPaymentMethod(this string paymentMethod)
        {
            return paymentMethod switch
            {
                "pix" => Data.Enum.PaymentMethod.Pix,
                _ => Data.Enum.PaymentMethod.CreditCard,
            };
        }

        public static PaymentStatus MapPaymentStatus(this EStripePaymentStatus paymentStatus)
        {
            return paymentStatus switch
            {
                EStripePaymentStatus.Pending => PaymentStatus.Pending,
                EStripePaymentStatus.PreAuthorized => PaymentStatus.Pending,
                EStripePaymentStatus.Paid => PaymentStatus.Paid,
                EStripePaymentStatus.Refunded => PaymentStatus.Refund,
                EStripePaymentStatus.RefundedPartailly => PaymentStatus.Refund,
                EStripePaymentStatus.Disputed => PaymentStatus.Refund,
                EStripePaymentStatus.Cancelled => PaymentStatus.Canceled,
                EStripePaymentStatus.Rejected => PaymentStatus.Declined,
                _ => PaymentStatus.Pending
            };

        }

        public static List<string> Filter(this List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains("person_"))
                {
                    list[i] = list[i].Replace(list[i].Split('.').First(), "individual");
                }
            }
            return list;
        }

        public static DataBankStatus MapDataBankStatus(this string stripeStatus)
        {
            return stripeStatus switch
            {
                "new" => DataBankStatus.Valid,
                "validated" => DataBankStatus.Valid,
                "verified" => DataBankStatus.Valid,
                "check_failed" => DataBankStatus.Invalid,
                "verification_failed" => DataBankStatus.Invalid,
                "errored" => DataBankStatus.Invalid,
                _ => DataBankStatus.Uninformed
            };
        }
    }
}