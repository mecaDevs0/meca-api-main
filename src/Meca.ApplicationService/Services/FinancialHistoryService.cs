using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.Services;
using Meca.Domain.Services.Interface;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using Meca.Shared.ObjectValues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Stripe;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Core3.Interface;
using UtilityFramework.Services.Iugu.Core3;
using UtilityFramework.Services.Iugu.Core3.Interface;
using UtilityFramework.Services.Iugu.Core3.Models;
using UtilityFramework.Services.Stripe.Core3;
using UtilityFramework.Services.Stripe.Core3.Interfaces;
using UtilityFramework.Services.Stripe.Core3.Models;
using PaymentMethod = Meca.Data.Enum.PaymentMethod;

namespace Meca.ApplicationService.Services
{
    public class FinancialHistoryService : ApplicationServiceBase<FinancialHistory>, IFinancialHistoryService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<FinancialHistory> _financialHistoryRepository;
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IBusinessBaseAsync<CreditCard> _creditCardRepository;
        private readonly IBusinessBaseAsync<Scheduling> _schedulingRepository;
        private readonly IBusinessBaseAsync<Fees> _feesRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly ISchedulingHistoryService _schedulingHistoryService;
        private readonly ISchedulingService _schedulingService;
        private readonly IIuguMarketPlaceServices _iuguMarketPlaceServices;
        private readonly ISenderMailService _senderMailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<PaymentNotificationHub> _hubContext;
        private readonly IUtilService _utilService;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _env;
        private readonly bool _isSandBox;
        private readonly IStripePaymentIntentService _stripePaymentIntentService;

        public FinancialHistoryService(
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IHostingEnvironment env,
            IBusinessBaseAsync<FinancialHistory> financialHistoryRepository,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<Data.Entities.CreditCard> creditCardRepository,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            IBusinessBaseAsync<Fees> feesRepository,
            IBusinessBaseAsync<Workshop> workshopRepository,
            ISchedulingHistoryService schedulingHistoryService,
            ISchedulingService schedulingService,
            IIuguMarketPlaceServices iuguMarketPlaceServices,
            ISenderMailService senderMailService,
            IHubContext<PaymentNotificationHub> hubContext,
            IUtilService utilService,
            IStripePaymentIntentService stripePaymentIntentService)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _env = env;
            _isSandBox = Util.IsSandBox(env);
            _financialHistoryRepository = financialHistoryRepository;
            _profileRepository = profileRepository;
            _creditCardRepository = creditCardRepository;
            _schedulingRepository = schedulingRepository;
            _feesRepository = feesRepository;
            _workshopRepository = workshopRepository;
            _schedulingHistoryService = schedulingHistoryService;
            _schedulingService = schedulingService;
            _iuguMarketPlaceServices = iuguMarketPlaceServices;
            _senderMailService = senderMailService;
            _utilService = utilService;
            _hubContext = hubContext;
            _stripePaymentIntentService = stripePaymentIntentService;
        }

        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public FinancialHistoryService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            string testUnit)
        {

            _financialHistoryRepository = new BusinessBaseAsync<FinancialHistory>(env);

            var iuguService = new IuguService();

            _mapper = mapper;
            _configuration = configuration;
            _env = env;
            _utilService = new UtilService(iuguService, iuguService, new BusinessBaseAsync<Data.Entities.Transfer>(env), env);

            SetAccessTest(acesso);
        }

        public async Task<List<FinancialHistoryViewModel>> GetAll()
        {
            var listFinancialHistory = await _financialHistoryRepository.FindAllAsync(Util.Sort<FinancialHistory>().Descending(nameof(FinancialHistory.Created)));

            return _mapper.Map<List<FinancialHistoryViewModel>>(listFinancialHistory);
        }

        public async Task<List<T>> GetAll<T>(FinancialHistoryFilterViewModel filterView) where T : class
        {
            filterView.SetDefault();

            var builder = Builders<FinancialHistory>.Filter;
            var conditions = new List<FilterDefinition<FinancialHistory>>();

            if (filterView.DataBlocked != null)
            {
                switch (filterView.DataBlocked.GetValueOrDefault())
                {
                    case FilterActived.Actived:
                        conditions.Add(builder.Eq(x => x.DataBlocked, null));
                        break;
                    case FilterActived.Disabled:
                        conditions.Add(builder.Ne(x => x.DataBlocked, null));
                        break;
                }
            }

            if (filterView.PaymentStatus != null && filterView.PaymentStatus.Count > 0)
                conditions.Add(builder.In(x => x.PaymentStatus, filterView.PaymentStatus));

            if (string.IsNullOrEmpty(filterView.ProfileId) == false)
                conditions.Add(builder.Eq(x => x.Profile.Id, filterView.ProfileId));

            if (string.IsNullOrEmpty(filterView.WorkshopId) == false)
                conditions.Add(builder.Eq(x => x.Workshop.Id, filterView.WorkshopId));

            if (filterView.StartDate != null)
                conditions.Add(builder.Gte(x => x.Created, filterView.StartDate));

            if (filterView.EndDate != null)
                conditions.Add(builder.Lte(x => x.Created, filterView.EndDate));

            if (filterView.Id?.Count > 0)
                conditions.Add(builder.In(x => x._id, filterView.Id.Select(ObjectId.Parse).ToList()));

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            var listFinancialHistory = await _financialHistoryRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions(filterView, Util.Sort<FinancialHistory>().Descending(x => x.Created)))
            .ToListAsync();

            return _mapper.Map<List<T>>(listFinancialHistory);
        }

        public async Task<FinancialHistoryViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var financialHistoryEntity = await _financialHistoryRepository.FindByIdAsync(id);

                if (financialHistoryEntity == null)
                {
                    CreateNotification(DefaultMessages.FinancialHistoryNotFound);
                    return null;
                }
                return _mapper.Map<FinancialHistoryViewModel>(financialHistoryEntity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TransactionViewModel> Payment(PaymentViewModel model)
        {
            try
            {
                if (ModelIsValid(model) == false)
                    return null;

                if (_access.TypeToken != (int)TypeProfile.Profile)
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return null;
                }

                var profileEntity = await _profileRepository.FindByIdAsync(_access.UserId);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return null;
                }

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                if (schedulingEntity.Profile.Id != profileEntity.GetStringId())
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (schedulingEntity.Status != SchedulingStatus.WaitingForPayment && schedulingEntity.Status != SchedulingStatus.PaymentRejected)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                var paymentIntentDefail = await _stripePaymentIntentService.GetPaymentIntentDetailsAsync(model.InvoiceId);

                if (paymentIntentDefail.Success == false)
                {
                    CreateNotification(paymentIntentDefail.ErrorMessage);
                    return null;
                }

                if (paymentIntentDefail.Data.PaymentStatus != EStripePaymentStatus.Paid)
                {
                    CreateNotification(DefaultMessages.NotIdentifyedPayment);
                    return null;
                }

                var platformFeeEntity = await _feesRepository.FindOneByAsync(x => x.DataBlocked == null);

                var platformFee = platformFeeEntity?.PlatformFee ?? 0.0;
                var platformValue = Utilities.GetValueOfPercent(paymentIntentDefail.Data.GrossAmount, platformFee).NotAround();

                var paymentStatus = paymentIntentDefail.Data.PaymentStatus.MapPaymentStatus();

                double splitPercentage = 0.0025; // 0.25%
                double netAmount = paymentIntentDefail.Data.NetAmount - platformValue;
                double splitFee = netAmount * splitPercentage;

                var financialHistoryEntity = new FinancialHistory()
                {
                    Profile = _mapper.Map<ProfileAux>(profileEntity),
                    Workshop = _mapper.Map<WorkshopAux>(schedulingEntity.Workshop),
                    WorkshopServices = _mapper.Map<List<WorkshopServicesAux>>(schedulingEntity.WorkshopServices),
                    Vehicle = _mapper.Map<VehicleAux>(schedulingEntity.Vehicle),
                    SchedulingId = schedulingEntity.GetStringId(),
                    InvoiceId = model.InvoiceId,
                    PaymentStatus = paymentStatus,
                    PaymentMethod = paymentIntentDefail.Data.PaymentMethodType.MapPaymentMethod(),
                    Value = paymentIntentDefail.Data.GrossAmount,
                    PlatformFee = platformFee,
                    PlatformValue = platformValue,
                    GatewayValue = paymentIntentDefail.Data.ProcessingFee,
                    NetValue = paymentIntentDefail.Data.NetAmount,
                    MechanicalNetValue = netAmount - splitFee
                };

                var checkoutResponse = new TransactionViewModel();

                if (financialHistoryEntity.PaymentMethod == PaymentMethod.Pix)
                {
                    financialHistoryEntity.PixQrCode = checkoutResponse.PixQrCode;
                    financialHistoryEntity.PixQrCodeTxt = checkoutResponse.PixQrCodeTxt;
                }

                financialHistoryEntity = await _financialHistoryRepository.CreateReturnAsync(financialHistoryEntity);

                checkoutResponse.Message = DefaultMessages.TransactionSuccess;
                checkoutResponse.Value = paymentIntentDefail.Data.GrossAmount;

                string title = "Orçamento aprovado";
                StringBuilder messageBuilder = new();
                messageBuilder.AppendLine($"O cliente {schedulingEntity.Profile.FullName}, do veículo de placa {schedulingEntity.Vehicle.Plate}, aprovou o orçamento e efetuou o pagamento referente ao agendamento de n° {schedulingEntity.GetStringId()}<br/>");
                await _schedulingService.SendWorkshopNotification(title, messageBuilder, schedulingEntity.Workshop.Id, schedulingEntity.Profile);

                // Registrar histórico de agendamento
                _ = Task.Run(async () =>
                    {
                        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                        schedulingEntity.Status = SchedulingStatus.Paid;

                        // Pagamento aprovado
                        await _schedulingService.RegisterSchedulingHistory(schedulingEntity);

                        schedulingEntity.Status = SchedulingStatus.WaitingStart;
                        schedulingEntity.PaymentDate = now;
                        schedulingEntity.PaymentStatus = paymentStatus;

                        await _schedulingRepository.UpdateAsync(schedulingEntity);

                        // Aguardando inicio do serviço
                        await _schedulingService.RegisterSchedulingHistory(schedulingEntity);
                    }
                );

                return _mapper.Map<TransactionViewModel>(checkoutResponse);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ReleasePaymentService(string schedulingId)
        {
            var schedulingEntity = await _schedulingRepository.FindByIdAsync(schedulingId);

            if (schedulingEntity == null)
            {
                CreateNotification(DefaultMessages.SchedulingNotFound);
                return false;
            }

            var approvedStatus = new HashSet<SchedulingStatus>() { SchedulingStatus.ServiceApprovedByAdmin, SchedulingStatus.ServiceApprovedByUser, SchedulingStatus.ServiceApprovedPartiallyByAdmin };

            if (!approvedStatus.Contains(schedulingEntity.Status))
            {
                CreateNotification(DefaultMessages.SchedulingServiceNotApproved);
                return false;
            }

            var financialHistoryEntity = await _financialHistoryRepository.FindOneByAsync(x => x.SchedulingId == schedulingId);

            if (financialHistoryEntity == null)
            {
                CreateNotification(DefaultMessages.FinancialHistoryNotFound);
                return false;
            }

            if (string.IsNullOrEmpty(financialHistoryEntity.InvoiceId))
            {
                CreateNotification(DefaultMessages.InvoiceNotFound);
                return false;
            }

            var chargeInfo = await _stripePaymentIntentService.GetPaymentIntentDetailsAsync(financialHistoryEntity.InvoiceId);

            if (chargeInfo.Success == false)
            {
                CreateNotification(chargeInfo.ErrorMessage ?? DefaultMessages.InvoiceNotFound);
                return false;
            }

            if (chargeInfo.Data.FundsStatus != "available")
            {
                CreateNotification(DefaultMessages.InvoiceFundsNotAvailable);
                return false;
            }

            //TODO: IMPLEMENTAR SPLIT DOS VALORES

            return false;
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                if (_access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.OnlyAdministrator);
                    return false;
                }
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return false;
                }

                await _financialHistoryRepository.DeleteOneAsync(id);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> Canceled(string invoiceId)
        {
            try
            {
                var financialHistoryEntity = await _financialHistoryRepository.FindOneByAsync(x => x.InvoiceId == invoiceId);

                if (financialHistoryEntity == null)
                    return false;

                /*TODO APLICAR REGRAS EM CASO DE COBRAÇA CANCELADA*/

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> Expired(string invoiceId)
        {
            try
            {
                var financialHistoryEntity = await _financialHistoryRepository.FindOneByAsync(x => x.InvoiceId == invoiceId);

                if (financialHistoryEntity == null)
                    return false;

                /*TODO APLICAR REGRAS EM CASO DE COBRAÇA EXPIRADA*/

                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> ConfirmPayment(string invoiceId, PaymentStatus paymentStatus = PaymentStatus.Paid, IuguTriggerModel model = null)
        {
            try
            {
                var financialHistoryEntity = await _financialHistoryRepository.FindOneByAsync(x => x.InvoiceId == invoiceId);
                if (financialHistoryEntity == null)
                    return false;

#if !DEBUG
                if (financialHistoryEntity.ReleasedDate != null)
                    return false;
#endif

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                if (paymentStatus == PaymentStatus.Released)
                {
                    financialHistoryEntity.ReleasedDate = now;
                    financialHistoryEntity.PaymentStatus = paymentStatus;

                    await _financialHistoryRepository.UpdateAsync(financialHistoryEntity);

                    return true;
                }

                /*VALORES SEPARADOS EM IUGU, CLIENTE E MEGALEIOS*/
                var iuguValues = IuguUtility.CalculeFees(invoiceId, financialHistoryEntity.Value, 0, 0);
                iuguValues.ClientValue = (double)financialHistoryEntity.PlatformValue;
                iuguValues.NetValue = (iuguValues.NetValue - iuguValues.ClientValue).NotAround();

                financialHistoryEntity.PaymentDate = now;
                financialHistoryEntity.PaymentStatus = paymentStatus;
                financialHistoryEntity.NetValue = iuguValues.NetValue;
                financialHistoryEntity.ProcessingValue = iuguValues.TotalFeesValue;

                var supportEmail = Utilities.GetConfigurationRoot().GetSection("suportEmail").Get<List<string>>();
                var clientIuguSettigns = Utilities.GetConfigurationRoot(environment: _env).GetSection("IUGU:Client").Get<MegaClientIuguViewModel>();

                var workshopEntity = await _workshopRepository.FindByIdAsync(financialHistoryEntity.Workshop.Id);

                // TODO - PAGAMENTO
                // /*VALOR DA OFICINA*/
                // if (iuguValues.NetValue > 0 && workshopEntity != null)
                // {
                //     try
                //     {
                //         if (_isSandBox == false)
                //         {
                //             transfer = await _iuguMarketPlaceServices.RepasseValoresAsync(workshopEntity.LiveKey, workshopEntity.AccountKey, (decimal)iuguValues.NetValue, toWithdraw: false);
                //         }

                //         transfer = null;
                //     }
                //     catch (Exception)
                //     {
                //         _senderMailService.SendMessageEmail(BaseConfig.ApplicationName,
                //             supportEmail,
                //             $"Erro ao repassar valor da oficina id da fatura = {invoiceId} DATA:{JsonConvert.SerializeObject(model).JsonPrettify()}",
                //             "Erro de repasse");
                //     }
                // }

                // /*VALOR DA PLATAFORMA*/
                // if (_isSandBox == false && iuguValues.ClientValue > 0 && string.IsNullOrEmpty(clientIuguSettigns?.AccountKey) == false)
                // {
                //     try
                //     {
                //         await _iuguMarketPlaceServices.RepasseValoresAsync(clientIuguSettigns.LiveKey, clientIuguSettigns.AccountKey, (decimal)iuguValues.ClientValue, toWithdraw: false);
                //     }
                //     catch (Exception)
                //     {
                //         _senderMailService.SendMessageEmail(BaseConfig.ApplicationName,
                //                                             supportEmail,
                //                                             $"Erro ao repassar valor do plataforma id da fatura = {invoiceId} DATA:{JsonConvert.SerializeObject(model).JsonPrettify()}",
                //                                             "Erro de repasse");
                //     }
                // }

                await _financialHistoryRepository.UpdateAsync(financialHistoryEntity);


                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> PaymentFailed(string invoiceId)
        {
            try
            {
                var financialHistoryEntity = await _financialHistoryRepository.FindOneByAsync(x => x.InvoiceId == invoiceId);
                if (financialHistoryEntity == null)
                    return false;

                financialHistoryEntity.PaymentStatus = PaymentStatus.Declined;

                await _financialHistoryRepository.UpdateAsync(financialHistoryEntity);

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(financialHistoryEntity.SchedulingId);

                schedulingEntity.Status = SchedulingStatus.PaymentRejected;
                schedulingEntity.PaymentStatus = PaymentStatus.Declined;

                await _schedulingRepository.UpdateAsync(schedulingEntity);

                // Registrar histórico de agendamento
                await _schedulingService.RegisterSchedulingHistory(schedulingEntity);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}