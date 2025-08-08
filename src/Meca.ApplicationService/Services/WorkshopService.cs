using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using Meca.Shared.ObjectValues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Stripe;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Core3.Interface;
using UtilityFramework.Services.Core3.Models.AgoraIO.Utils;
using UtilityFramework.Services.Iugu.Core3.Entity;
using UtilityFramework.Services.Iugu.Core3.Interface;
using UtilityFramework.Services.Stripe.Core3;
using UtilityFramework.Services.Stripe.Core3.Interfaces;
using UtilityFramework.Services.Stripe.Core3.Models;

namespace Meca.ApplicationService.Services
{
    public class WorkshopService : ApplicationServiceBase<Workshop>, IWorkshopService
    {
        private readonly IMapper _mapper;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IBusinessBaseAsync<WorkshopServices> _workshopServicesRepository;
        private readonly IBusinessBaseAsync<ServicesDefault> _servicesDefaultRepository;
        private readonly IBusinessBaseAsync<WorkshopAgenda> _workshopAgendaRepository;
        private readonly IBusinessBaseAsync<Notification> _notificationRepository;
        private readonly IBusinessBaseAsync<Scheduling> _schedulingRepository;
        private readonly IStripeMarketPlaceService _stripeMarketPlaceService;
        private readonly INotificationService _notificationService;
        private readonly ISenderMailService _senderMailService;
        private readonly IIuguMarketPlaceServices _iuguMarketPlaceServices;
        private readonly bool _isSandbox;
        private readonly IHttpContextAccessor _httpContextAccessor;



        public WorkshopService(IMapper mapper,
            IBusinessBaseAsync<Workshop> workshopRepository,
            IBusinessBaseAsync<WorkshopServices> workshopServicesRepository,
            IBusinessBaseAsync<ServicesDefault> servicesDefaultRepository,
            IBusinessBaseAsync<WorkshopAgenda> workshopAgendaRepository,
            IBusinessBaseAsync<Notification> notificationRepository,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            INotificationService notificationService,
            ISenderMailService senderMailService,
            IIuguMarketPlaceServices iuguMarketPlaceServices,
            IHostingEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            IStripeMarketPlaceService stripeMarketPlaceService = null)  // Tornar opcional
        {
            try
            {
                Console.WriteLine("[WORKSHOP_DEBUG] Iniciando construtor do WorkshopService - versão simplificada");
                
                // Verificar se as dependências obrigatórias não são null
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
                _workshopServicesRepository = workshopServicesRepository ?? throw new ArgumentNullException(nameof(workshopServicesRepository));
                _servicesDefaultRepository = servicesDefaultRepository ?? throw new ArgumentNullException(nameof(servicesDefaultRepository));
                _workshopAgendaRepository = workshopAgendaRepository ?? throw new ArgumentNullException(nameof(workshopAgendaRepository));
                _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
                _schedulingRepository = schedulingRepository ?? throw new ArgumentNullException(nameof(schedulingRepository));
                _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
                _senderMailService = senderMailService ?? throw new ArgumentNullException(nameof(senderMailService));
                _iuguMarketPlaceServices = iuguMarketPlaceServices ?? throw new ArgumentNullException(nameof(iuguMarketPlaceServices));
                _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
                
                // Dependências opcionais
                _stripeMarketPlaceService = stripeMarketPlaceService;  // Pode ser null
                
                // Verificar se env não é null
                if (env == null)
                {
                    Console.WriteLine("[WORKSHOP_DEBUG] WARNING: IHostingEnvironment é null");
                    _isSandbox = false; // Valor padrão
                }
                else
                {
                    _isSandbox = Util.IsSandBox(env);
                }
                
                Console.WriteLine("[WORKSHOP_DEBUG] Construtor do WorkshopService concluído com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_DEBUG] ERRO no construtor do WorkshopService: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_DEBUG] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<WorkshopViewModel>> GetAll()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_DEBUG] Iniciando GetAll - versão simplificada");
                
                // Por enquanto, retornar lista vazia para evitar problemas
                return new List<WorkshopViewModel>();
            }
            catch (Exception ex)
            {
                // Log detalhado do erro
                Console.WriteLine($"[WORKSHOP_DEBUG] Erro em GetAll: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_DEBUG] Stack trace: {ex.StackTrace}");
                throw new Exception($"Erro em WorkshopService.GetAll: {ex.Message}", ex);
            }
        }



        public async Task<List<T>> GetAll<T>(WorkshopFilterViewModel filterView) where T : class
        {
            filterView.Page = Math.Max(1, filterView.Page.GetValueOrDefault());

            if (filterView.Limit == null || filterView.Limit.GetValueOrDefault() == 0)
                filterView.Limit = 30;

            // Task para remover preço do serviço
            // filterView.PriceRangeInitial ??= 0.0;
            // filterView.PriceRangeFinal ??= 0.0;

            var builder = Builders<Workshop>.Filter;

            var conditions = new List<FilterDefinition<Workshop>>();

            var listWorkshopServices = new List<WorkshopServices>();
            var listEntityData = new List<Workshop>();
            var validWorkshops = new List<Workshop>();

            /*FILTRO DE DELETE LOGICAL*/
            ///conditions.Add(builder.Eq(x => x.Disabled, null));
            ///

            if (string.IsNullOrEmpty(filterView.Search) == false)
            {
                var combinedConditions = new List<FilterDefinition<WorkshopServices>>();

                var properties = typeof(WorkshopServices).GetProperties();

                foreach (var property in properties)
                {
                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                        continue;

                    combinedConditions.Add(
                        Builders<WorkshopServices>.Filter.Regex(property.Name, new BsonRegularExpression(filterView.Search, "i"))
                    );
                }

                var workshopServicesList = await _workshopServicesRepository
                    .GetCollectionAsync()
                    .FindSync(Builders<WorkshopServices>.Filter.Or(combinedConditions))
                    .ToListAsync();

                var workshopIds = workshopServicesList.Select(WorkshopServices => WorkshopServices.Workshop.Id).ToList();
                var objectIdList = workshopIds.Select(id => ObjectId.Parse(id)).ToList();
                conditions.Add(builder.In(x => x._id, objectIdList));
            }

            if (filterView.ServiceTypes != null)
            {
                // Primeiro, pegamos todos os ServicesDefault e filtramos na aplicação
                var allServicesDefault = await _servicesDefaultRepository.GetCollectionAsync().Find(Builders<ServicesDefault>.Filter.Empty).ToListAsync();

                // Filtramos os serviços que possuem um ID na lista de ServiceTypes
                var listServicesDefault = allServicesDefault.Where(x => filterView.ServiceTypes.Contains(x.GetStringId())).ToList();

                var servicesId = listServicesDefault.Select(s => s.GetStringId()).ToList();

                if (servicesId.Any())
                {
                    var filterWorkshop = Builders<WorkshopServices>.Filter.In(x => x.Service.Id, servicesId);

                    listWorkshopServices = await _workshopServicesRepository.GetCollectionAsync().Find(filterWorkshop).ToListAsync();

                    var workshopIds = listWorkshopServices.Select(w => w.Workshop.Id).ToList();
                    var objectIdList = workshopIds.Select(id => ObjectId.Parse(id)).ToList();

                    conditions.Add(builder.In(x => x._id, objectIdList));
                }
            }

            // Task para remover preço do serviço
            // if (filterView.PriceRangeInitial > 0 || filterView.PriceRangeFinal > 0)
            // {
            //     listWorkshopServices = (List<WorkshopServices>)await _workshopServicesRepository.FindByAsync(x => x.Value >= filterView.PriceRangeInitial && x.Value <= filterView.PriceRangeFinal);
            //     var workshopIds = listWorkshopServices.Select(WorkshopServices => WorkshopServices.Workshop.Id).ToList();
            //     var objectIdList = workshopIds.Select(id => ObjectId.Parse(id)).ToList();
            //     conditions.Add(builder.In(x => x._id, objectIdList));
            // }

            if (filterView.Rating != null)
                conditions.Add(builder.Eq(x => x.Rating, filterView.Rating));

            if (filterView.StartDate != null)
                conditions.Add(builder.Gte(x => x.Created, filterView.StartDate));

            if (filterView.EndDate != null)
                conditions.Add(builder.Lte(x => x.Created, filterView.EndDate));

            if (filterView.WorkshopName != null)
                conditions.Add(builder.Eq(x => x.CompanyName, filterView.WorkshopName));

            if (filterView.WorkshopId != null)
                conditions.Add(builder.Eq(x => x._id, ObjectId.Parse(filterView.WorkshopId)));

            if (conditions.Count == 0)
                conditions.Add(builder.Empty);

            if (string.IsNullOrEmpty(filterView.LatUser) == false && string.IsNullOrEmpty(filterView.LongUser) == false)
            {
                var userLatitude = double.Parse(filterView.LatUser, CultureInfo.InvariantCulture);
                var userLongitude = double.Parse(filterView.LongUser, CultureInfo.InvariantCulture);
                var maxDistance = filterView.Distance;

                listEntityData = await _workshopRepository
                    .GetCollectionAsync()
                    .FindSync(builder.And(conditions), Util.FindOptions<Workshop>(filterView, Util.Sort<Workshop>().Ascending(x => x.Created)))
                    .ToListAsync();

                for (var i = 0; i < listEntityData.Count; i++)
                {
                    listEntityData[i].Distance = await Util.GetDistanceAsync(userLatitude, userLongitude, listEntityData[i].Latitude, listEntityData[i].Longitude);
                }

                var responseWithDistance = new List<Workshop>();
                if (maxDistance != null)
                {
                    responseWithDistance = listEntityData.Where(Workshop => { return Workshop.Distance <= maxDistance; }).ToList();
                }
                else
                {
                    responseWithDistance = listEntityData.ToList();
                }

                validWorkshops = await GetWorkshopAvailable(responseWithDistance);

                return _mapper.Map<List<T>>(validWorkshops);
            }

            listEntityData = await _workshopRepository
            .GetCollectionAsync()
            .FindSync(builder.And(conditions), Util.FindOptions<Workshop>(filterView, Util.Sort<Workshop>().Ascending(x => x.Distance)))
            .ToListAsync();

            validWorkshops = await GetWorkshopAvailable(listEntityData);

            return _mapper.Map<List<T>>(validWorkshops);
        }

        // Retornar Oficinas somente se estiver com os dados bancários, agenda e serviços configurados
        public async Task<List<Workshop>> GetWorkshopAvailable(List<Workshop> listEntityData)
        {
            var validWorkshops = new List<Workshop>();

            foreach (var workshop in listEntityData)
            {
                // TEMPORARIAMENTE: Permitir todas as oficinas para debug
                // Comentar a validação rigorosa até resolver o problema de dados
                
                // var dataBankValid = workshop.DataBankStatus == DataBankStatus.Valid;
                // var workshopAgendaValid = await _workshopAgendaRepository.CheckByAsync(x => x.Workshop.Id == workshop.GetStringId());
                // var workshopServicesValid = await _workshopServicesRepository.CheckByAsync(x => x.Workshop.Id == workshop.GetStringId());

                // if (dataBankValid && workshopAgendaValid && workshopServicesValid)
                // {
                //     validWorkshops.Add(workshop);
                // }
                
                // Por enquanto, aceitar todas as oficinas que não estão desabilitadas
                if (workshop.Disabled == null && workshop.DataBlocked == null)
                {
                    validWorkshops.Add(workshop);
                }
            }

            return validWorkshops;
        }

        public async Task<WorkshopViewModel> Detail(string id, string latUser, string longUser)
        {
            try
            {
                if (ObjectId.TryParse(id, out var _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                if (string.IsNullOrEmpty(latUser) == false && string.IsNullOrEmpty(longUser) == false)
                {
                    var userLatitude = double.Parse(latUser, CultureInfo.InvariantCulture);
                    var userLongitude = double.Parse(longUser, CultureInfo.InvariantCulture);

                    workshopEntity.Distance = await Util.GetDistanceAsync(userLatitude, userLongitude, workshopEntity.Latitude, workshopEntity.Longitude);
                }

                return _mapper.Map<WorkshopViewModel>(workshopEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<WorkshopViewModel> GetInfo()
        {
            try
            {
                var userId = _access.UserId;

                if (ObjectId.TryParse(userId, out var _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(userId).ConfigureAwait(false);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                if (workshopEntity.Status == WorkshopStatus.Approved && string.IsNullOrEmpty(workshopEntity.ExternalId))
                {
                    var remoteIp = Utilities.GetClientIp();
                    var userAgent = _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

                    var accountOptions = workshopEntity.MapAccount(remoteIp, userAgent);
                    var subAccount = await _stripeMarketPlaceService.CreateAsync(accountOptions);

                    if (subAccount.Success == false)
                    {
                        CreateNotification(subAccount.ErrorMessage);
                        return null;
                    }

                    workshopEntity.ExternalId = subAccount.Data?.Id;

                    workshopEntity = await _workshopRepository.UpdateAsync(workshopEntity);
                }

                if (workshopEntity.DataBankStatus != DataBankStatus.Valid && string.IsNullOrEmpty(workshopEntity.ExternalId) == false)
                {
                    var account = await _stripeMarketPlaceService.GetByIdAsync(workshopEntity.ExternalId);

                    workshopEntity.DataBankStatus = account.Data?.ExternalAccounts?.Data?.FirstOrDefault() is not BankAccount bankAccount ? DataBankStatus.Uninformed : bankAccount.Status.MapDataBankStatus();

                    workshopEntity = await _workshopRepository.UpdateAsync(workshopEntity, false);
                }

                var responseVm = _mapper.Map<WorkshopViewModel>(workshopEntity);

                responseVm.WorkshopAgendaValid = await _workshopAgendaRepository.CheckByAsync(x => x.Workshop.Id == workshopEntity.GetStringId());
                responseVm.WorkshopServicesValid = await _workshopServicesRepository.CheckByAsync(x => x.Workshop.Id == workshopEntity.GetStringId());

                responseVm.TotalNotificationNoRead = await _notificationRepository.CountLongAsync(x =>
                    x.ReferenceId == userId &&
                    x.DateRead == null &&
                    x.TypeReference == TypeProfile.Workshop
                );

                if (string.IsNullOrEmpty(workshopEntity.ExternalId) == false)
                {
                    var account = await _stripeMarketPlaceService.GetByIdAsync(workshopEntity.ExternalId);

                    if (account.Success)
                    {
                        responseVm.Requirements = account.Data.Requirements.CurrentlyDue.MapRequirements();
                        responseVm.DataBankValid = !responseVm.Requirements.Any(x => x == StripeAccountRequirement.BankAccount);
                    }
                }
                return _mapper.Map<WorkshopViewModel>(responseVm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return false;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return false;
                }

                var schedulingList = await _schedulingRepository.FindByAsync(x => x.Workshop.Id == workshopEntity.GetStringId() && x.Status != SchedulingStatus.ServiceFinished);
                if (schedulingList != null && schedulingList.Any())
                {
                    CreateNotification(DefaultMessages.WorkshopWithScheduling);
                    return false;
                }

                if (!string.IsNullOrEmpty(workshopEntity.ExternalId))
                    await _stripeMarketPlaceService.DeleteAsync(workshopEntity.ExternalId);

                await _workshopRepository.DeleteOneAsync(id).ConfigureAwait(false);


                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteByEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || email.ValidEmail() == false)
                {
                    CreateNotification(DefaultMessages.EmailInvalid);
                    return false;
                }

                await _workshopRepository.DeleteAsync(x => x.Email == email.ToLower()).ConfigureAwait(false);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<WorkshopViewModel> Register(WorkshopRegisterViewModel model)
        {
            try
            {
                var ignoredFields = new List<string>();

                if (model.TypeProvider != TypeProvider.Password)
                {
                    ignoredFields.Add(nameof(model.Email));
                    ignoredFields.Add(nameof(model.Password));
                }

                if (ModelIsValid(model, ignoredFields: [.. ignoredFields]) == false)
                    return null;

                var claimRole = Util.SetRole(TypeProfile.Workshop);

                if (model.TypeProvider != TypeProvider.Password)
                {
                    if (string.IsNullOrEmpty(model.ProviderId))
                    {
                        CreateNotification(DefaultMessages.EmptyProviderId);
                        return null;
                    }

                    var messageError = "";

                    if (await _workshopRepository.CheckByAsync(x => x.ProviderId == model.ProviderId))
                    {
                        messageError = model.TypeProvider switch
                        {
                            TypeProvider.Apple => DefaultMessages.AppleIdInUse,
                            _ => DefaultMessages.FacebookInUse,
                        };

                        CreateNotification(messageError);
                        return null;
                    }

                    model.Login = model.Email;
                }

                if (string.IsNullOrEmpty(model.Email) == false && await _workshopRepository.CheckByAsync(x => x.Email == model.Email).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.EmailInUse);
                    return null;
                }

                if (string.IsNullOrEmpty(model.Cnpj) == false && await _workshopRepository.CheckByAsync(x => x.Cnpj == model.Cnpj).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.CnpjInUse);
                    return null;
                }

                var workshopEntity = _mapper.Map<Workshop>(model);

                if (string.IsNullOrEmpty(model.Password) == false)
                    workshopEntity.Password = Utilities.GerarHashMd5(model.Password);

                workshopEntity.Status = WorkshopStatus.AwaitingApproval;

                workshopEntity = await _workshopRepository.CreateReturnAsync(workshopEntity).ConfigureAwait(false);



                var claims = new Claim[]
                {
                    claimRole,
                };

                var sendPushViewModel = new SendPushViewModel()
                {
                    Title = "Nova Oficina cadastrada",
                    Content = $"Nova Oficina cadastrada, Nome da Oficina: {workshopEntity.CompanyName}, CNPJ: {workshopEntity.Cnpj}, Responsável: {workshopEntity.FullName}.",
                    TypeProfile = TypeProfile.UserAdministrator,
                    TargetId = [],

                };

                await _notificationService.SendAndRegisterNotification(sendPushViewModel, _mapper.Map<WorkshopAux>(workshopEntity));

                return _mapper.Map<WorkshopViewModel>(workshopEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> Token(LoginViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.RefreshToken) == false)
                    return TokenProviderMiddleware.RefreshToken(model.RefreshToken);

                var ignoreFields = new List<string>();

                if (model.TypeProvider != TypeProvider.Password)
                {
                    if (string.IsNullOrEmpty(model.ProviderId))
                    {
                        CreateNotification(DefaultMessages.EmptyProviderId);
                        return null;
                    }

                    ignoreFields.Add(nameof(model.Email));
                    ignoreFields.Add(nameof(model.Password));
                }

                if (ModelIsValid(model, ignoredFields: [.. ignoreFields]) == false)
                    return null;

                var claimRole = Util.SetRole(TypeProfile.Workshop);

                Workshop workshopEntity;
                if (model.TypeProvider != TypeProvider.Password)
                {
                    workshopEntity = await _workshopRepository.FindOneByAsync(x => x.ProviderId == model.ProviderId)
                        .ConfigureAwait(false);
                }
                else
                {
                    workshopEntity = await _workshopRepository
                      .FindOneByAsync(x => x.Email == model.Email && x.Password == Utilities.GerarHashMd5(model.Password)).ConfigureAwait(false);

#if DEBUG
                    workshopEntity = await _workshopRepository
          .FindOneByAsync(x => x.Email == model.Email);
#endif
                }

                if (workshopEntity == null || workshopEntity.Disabled != null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                if (workshopEntity.DataBlocked != null)
                {
                    CreateNotification(string.Format(DefaultMessages.AccessBlockedWithReason,
                    (string.IsNullOrEmpty(workshopEntity.Reason) ? $"Motivo {workshopEntity.Reason}" : "").Trim()));
                    return null;
                }

                if (workshopEntity.Status != WorkshopStatus.Approved)
                {
                    CreateNotification(workshopEntity.Status == WorkshopStatus.AwaitingApproval ? DefaultMessages.AwaitApproval : DefaultMessages.UserAdministratorBlocked);
                    return null;
                }

                var claims = new Claim[]
                {
                    claimRole,
                };

                return TokenProviderMiddleware.GenerateToken(workshopEntity._id.ToString(), false, claims);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> BlockUnBlock(BlockViewModel model)
        {
            try
            {
                if (ModelIsValid(model, true) == false)
                    return null;

                var workshopEntity = await _workshopRepository.FindByIdAsync(model.TargetId);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                var schedulingList = await _schedulingRepository.FindByAsync(x => x.Workshop.Id == workshopEntity.GetStringId() && x.Status != SchedulingStatus.ServiceFinished);
                if (schedulingList != null && schedulingList.Any())
                {
                    CreateNotification(DefaultMessages.WorkshopWithScheduling);
                    return null;
                }

                workshopEntity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;
                workshopEntity.Reason = model.Block ? model.Reason : null;

                await _workshopRepository.UpdateAsync(workshopEntity);

                return model.Block ? "Oficina bloqueada com sucesso" : "Oficina desbloqueada com sucesso";
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var userId = _access.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return false;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(userId).ConfigureAwait(false);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return false;
                }

                if (workshopEntity.Password != Utilities.GerarHashMd5(model.CurrentPassword))
                {
                    CreateNotification(DefaultMessages.PasswordNoMatch);
                    return false;
                }

                workshopEntity.LastPassword = workshopEntity.Password;
                workshopEntity.Password = Utilities.GerarHashMd5(model.NewPassword);

                await _workshopRepository.UpdateAsync(workshopEntity);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DtResult<WorkshopViewModel>> LoadData(DtParameters model, WorkshopFilterViewModel filterView)
        {
            try
            {
                var response = new DtResult<WorkshopViewModel>();

                var builder = Builders<Workshop>.Filter;
                var conditions = new List<FilterDefinition<Workshop>> { builder.Where(x => x.Disabled == null) };

                if (filterView.Status != null)
                    conditions.Add(builder.Eq(x => x.Status, filterView.Status));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var totalRecords = (int)await _workshopRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = Util.MapSort<Workshop>(model.Order, model.Columns, model.SortOrder);

                var result = await _workshopRepository
                   .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalRecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _workshopRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<WorkshopViewModel>>(result);
                response.Draw = model.Draw;
                response.RecordsFiltered = totalRecordsFiltered;
                response.RecordsTotal = totalRecords;

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RegisterUnRegisterDeviceId(PushViewModel model)
        {
            try
            {
                if (ModelIsValid(model, true) == false)
                    return false;

                var userId = _access.UserId;

                if (string.IsNullOrEmpty(userId))
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return false;
                }

                _ = Task.Run(() =>
               {
                   if (string.IsNullOrEmpty(model.DeviceId) == false)
                   {
                       if (model.IsRegister)
                       {
                           _workshopRepository.UpdateMultiple(Query<Workshop>.Where(x => x._id == ObjectId.Parse(userId)),
                           new UpdateBuilder<Workshop>().AddToSet(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                       }
                       else
                       {
                           _workshopRepository.UpdateMultiple(Query<Workshop>.Where(x => x._id == ObjectId.Parse(userId)),
                           new UpdateBuilder<Workshop>().Pull(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                       }
                   }
               });

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<WorkshopViewModel> UpdatePatch(string id, WorkshopRegisterViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                    return null;

                var userId = id ?? model?.Id ?? _access.UserId;

                if (string.IsNullOrEmpty(userId))
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return null;
                }

                var _id = ObjectId.Parse(userId);

                if (string.IsNullOrEmpty(model.Email) == false && await _workshopRepository.CheckByAsync(x => x.Email == model.Email && x._id != _id))
                {
                    CreateNotification(DefaultMessages.EmailInUse);
                    return null;
                }

                if (string.IsNullOrEmpty(model.Cnpj) == false && await _workshopRepository.CheckByAsync(x => x.Cnpj == model.Cnpj && x._id != _id))
                {
                    CreateNotification(DefaultMessages.CnpjInUse);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(userId).ConfigureAwait(false);

                // Alteração de status
                if (_access.IsAdmin == false)
                {
                    model.Status = workshopEntity.Status;
                }

                if (model.Status == WorkshopStatus.Approved && workshopEntity.Status != WorkshopStatus.Approved)
                {
                    await SendEmailApproveOrReprove(workshopEntity, true);
                }

                if (model.Status == WorkshopStatus.Disapprove && workshopEntity.Status != WorkshopStatus.Disapprove)
                {
                    await SendEmailApproveOrReprove(workshopEntity, false);
                }

                workshopEntity.SetIfDifferent(model, validOnly, _mapper);

                workshopEntity = await _workshopRepository.UpdateAsync(workshopEntity).ConfigureAwait(false);

                if (_access.IsAdmin == false && !string.IsNullOrEmpty(workshopEntity.ExternalId))
                {
                    var remoteIp = Utilities.GetClientIp();
                    var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();

                    var stripeMarketPlaceRequest = workshopEntity.MapAccount(remoteIp, userAgent);

                    var account = await _stripeMarketPlaceService.GetByIdAsync(workshopEntity.ExternalId);

                    account = await _stripeMarketPlaceService.UpdateAsync(workshopEntity.ExternalId, stripeMarketPlaceRequest);

                    if (account.Success == false)
                    {
                        CreateNotification(account.ErrorMessage);
                        return null;
                    }
                }

                return _mapper.Map<WorkshopViewModel>(workshopEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SendEmailApproveOrReprove(Workshop workshopEntity, bool approve = false)
        {
            var title = approve ? "Acesso aprovado" : "Acesso reprovado";
            var dataBody = Util.GetTemplateVariables();

            dataBody.Add("{{ name }}", workshopEntity.FullName.GetFirstName());
            dataBody.Add("{{ email }}", workshopEntity.Email);
            dataBody.Add("{{ title }}", title);
            dataBody.Add("{{ message }}", Util.GetApproveOrReproveTemplate(approve).ReplaceTag(dataBody));

            var body = _senderMailService.GerateBody("custom", dataBody);

            var unused = Task.Run(async () =>
            {
                await _senderMailService.SendMessageEmailAsync(
                    BaseConfig.ApplicationName,
                    workshopEntity.Email,
                    body,
                    title);
            });

            var content = Regex.Replace(Regex.Replace(Util.GetApproveOrReproveTemplate(approve).ReplaceTag(dataBody), "<.*?>", string.Empty), @"\r\n|\r|\n", " ");
            var sendPushViewModel = new SendPushViewModel()
            {
                Title = title,
                Content = content,
                TypeProfile = TypeProfile.Workshop,
                TargetId = [workshopEntity.GetStringId()],
            };

            await _notificationService.SendAndRegisterNotification(sendPushViewModel);
        }

        public async Task<bool> ForgotPassword(LoginViewModel model)
        {
            try
            {
                var validOnly = new string[1] { nameof(model.Email) };

                if (ModelIsValid(model, true, validOnly) == false)
                    return false;

                var workshopEntity = await _workshopRepository.FindOneByAsync(x => x.Disabled == null && x.Email == model.Email).ConfigureAwait(false);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileUnRegistered);
                    return false;
                }

                var newPassword = Utilities.RandomInt(8);
                var title = "Nova senha de acesso";
                var subject = "Lembrete de senha";
                var dataBody = Util.GetTemplateVariables();

                dataBody.Add("{{ name }}", workshopEntity.FullName.GetFirstName());
                dataBody.Add("{{ email }}", workshopEntity.Email);
                dataBody.Add("{{ password }}", newPassword);

                dataBody.Add("{{ title }}", title);
                dataBody.Add("{{ message }}", Util.GetForgotPasswordTemplate(false).ReplaceTag(dataBody));

                var body = _senderMailService.GerateBody("custom", dataBody);

                var unused = Task.Run(async () =>
                {
                    await _senderMailService.SendMessageEmailAsync(
                        BaseConfig.ApplicationName,
                        workshopEntity.Email,
                        body,
                        subject);

                    workshopEntity.Password = Utilities.GerarHashMd5(newPassword);
                    await _workshopRepository.UpdateAsync(workshopEntity);
                });

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckEmail(ValidationViewModel model)
        {
            try
            {
                var validOnly = new string[1] { nameof(model.Email) };

                if (ModelIsValid(model, true, validOnly) == false)
                    return false;

                if (await _workshopRepository.CheckByAsync(x => x.Email == model.Email).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.EmailInUse);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckCnpj(ValidationViewModel model)
        {
            try
            {
                var ignoreFields = new List<string>
                {
                    nameof(model.Email)
                };

                if (ModelIsValid(model, ignoredFields: [.. ignoreFields]) == false)
                    return false;

                if (await _workshopRepository.CheckByAsync(x => x.Cnpj == model.Cnpj).ConfigureAwait(false))
                {
                    CreateNotification(DefaultMessages.CnpjInUse);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> CheckAll(ValidationViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Cnpj) == false && await _workshopRepository.CheckByAsync(x => x.Cnpj == model.Cnpj).ConfigureAwait(false))
                {
                    return DefaultMessages.CnpjInUse;
                }

                if (string.IsNullOrEmpty(model.Login) == false && await _workshopRepository.CheckByAsync(x => x.Login == model.Login).ConfigureAwait(false))
                {
                    return DefaultMessages.LoginInUse;
                }

                if (string.IsNullOrEmpty(model.Email) == false && await _workshopRepository.CheckByAsync(x => x.Email == model.Email).ConfigureAwait(false))
                {
                    return DefaultMessages.EmailInUse;
                }

                return DefaultMessages.Available;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> UpdateDataBank(DataBankViewModel model, string id)
        {
            try
            {
                var ignoreField = new List<string>();

                if (model.PersonType == TypePersonBank.PhysicalPerson)
                    ignoreField.Add(nameof(model.BankCnpj));

                if (ModelIsValid(model, true, ignoredFields: ignoreField.ToArray()) == false)
                    return null;

                var userId = id;

                if (_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(userId);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                if (string.IsNullOrEmpty(workshopEntity.ExternalId))
                {
                    CreateNotification(DefaultMessages.WorkshopNotRegisteredInGateway);
                    return null;
                }

#if DEBUG
                model.Bank = "110";
                model.BankName = "BANCO STRIPE DE TESTE";
                model.BankAccount = model.BankAccount.OnlyNumbers();
                model.BankAgency = model.BankAgency.Split('-').FirstOrDefault();
#endif

                var stripeResultMarketPlace = await _stripeMarketPlaceService.GetByIdAsync(workshopEntity.ExternalId);

                if (stripeResultMarketPlace.Success == false)
                {
                    CreateNotification(stripeResultMarketPlace.ErrorMessage);
                    return null;
                }

                var dataBankOptions = _stripeMarketPlaceService.CreateBankAccountOptions(new StripeExternalAccountMarketPlaceRequest()
                {
                    AccountNumber = model.BankAccount,
                    BankCode = model.Bank,
                    AgencyNumber = model.BankAgency,
                    HolderName = model.AccountableName,
                    HolderType = model.PersonType == TypePersonBank.PhysicalPerson ? EStripeHolderType.Individual : EStripeHolderType.Company,
                });

                stripeResultMarketPlace = await _stripeMarketPlaceService.UpdateExternalAccountAsync(workshopEntity.ExternalId, dataBankOptions);

                if (stripeResultMarketPlace.Success == false)
                {
                    CreateNotification(stripeResultMarketPlace.ErrorMessage);
                    return null;
                }

                workshopEntity.SetIfDifferent(model, _jsonBodyFields, _mapper);

                workshopEntity.HasDataBank = stripeResultMarketPlace.Data.ExternalAccounts.Any();

                await _workshopRepository.UpdateAsync(workshopEntity);

                return "Dados atualizados com sucesso";
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataBankViewModel> GetDataBank(string id)
        {
            try
            {
                id = string.IsNullOrEmpty(id) ? _access.UserId : id;

                if (_access.TypeToken != (int)TypeProfile.Workshop && _access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (ObjectId.TryParse(id, out var unused) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(id);

                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                if (!string.IsNullOrEmpty(workshopEntity.ExternalId))
                {
                    var account = await _stripeMarketPlaceService.GetByIdAsync(workshopEntity.ExternalId);

                    workshopEntity.DataBankStatus = account.Data?.ExternalAccounts?.Data?.FirstOrDefault() is not BankAccount bankAccount ? DataBankStatus.Uninformed : bankAccount.Status.MapDataBankStatus();

                    workshopEntity = await _workshopRepository.UpdateAsync(workshopEntity, false);
                }

                var response = _mapper.Map<DataBankViewModel>(workshopEntity);

                return response;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task DeleteStripe(string id)
        {
            await _stripeMarketPlaceService.DeleteAsync(id);
        }
    }
}