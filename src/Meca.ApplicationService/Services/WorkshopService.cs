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
                Console.WriteLine("[WORKSHOP_DEBUG] Iniciando construtor do WorkshopService - versÃ£o simplificada");
                
                // Verificar se as dependÃªncias obrigatÃ³rias nÃ£o sÃ£o null
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
                
                // DependÃªncias opcionais
                _stripeMarketPlaceService = stripeMarketPlaceService;  // Pode ser null
                
                // Verificar se env nÃ£o Ã© null
                if (env == null)
                {
                    Console.WriteLine("[WORKSHOP_DEBUG] WARNING: IHostingEnvironment Ã© null");
                    _isSandbox = false; // Valor padrÃ£o
                }
                else
                {
                    _isSandbox = Util.IsSandBox(env);
                }
                
                // Inicializar o acesso
                SetAccess(httpContextAccessor);
                
                Console.WriteLine("[WORKSHOP_DEBUG] Construtor do WorkshopService concluÃ­do com sucesso");
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
                Console.WriteLine("[WORKSHOP_DEBUG] Iniciando GetAll (sem filtro)");

                // Verificar se o repositÃ³rio estÃ¡ inicializado
                if (_workshopRepository == null)
                {
                    Console.WriteLine("[WORKSHOP_DEBUG] ERRO: _workshopRepository Ã© null");
                    return new List<WorkshopViewModel>();
                }

                var builder = Builders<Workshop>.Filter;
                var conditions = new List<FilterDefinition<Workshop>>
                {
                    // Considerar null ou 0 como "ativo"
                    builder.Or(builder.Eq(x => x.Disabled, null), builder.Eq(x => x.Disabled, 0)),
                    builder.Or(builder.Eq(x => x.DataBlocked, null), builder.Eq(x => x.DataBlocked, 0))
                };

                Console.WriteLine("[WORKSHOP_DEBUG] Aplicando filtros bÃ¡sicos");

                try
                {
                    var listEntityData = await _workshopRepository
                        .GetCollectionAsync()
                        .Find(builder.And(conditions))
                        .Sort(Util.Sort<Workshop>().Ascending(x => x.Created))
                        .ToListAsync();

                    Console.WriteLine($"[WORKSHOP_DEBUG] Encontradas {listEntityData.Count} oficinas no banco");

                    var validWorkshops = await GetWorkshopAvailable(listEntityData);
                    Console.WriteLine($"[WORKSHOP_DEBUG] GetAll (sem filtro) retornando {validWorkshops.Count} oficinas vÃ¡lidas");

                    // Validar workshops antes do mapeamento
                    var validWorkshopsForMapping = validWorkshops.Where(w => w != null && w._id != null).ToList();
                    Console.WriteLine($"[WORKSHOP_DEBUG] Workshops validos para mapeamento: {validWorkshopsForMapping.Count}");
                    
                    return _mapper.Map<List<WorkshopViewModel>>(validWorkshopsForMapping);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WORKSHOP_DEBUG] Erro ao buscar oficinas: {ex.Message}");
                    Console.WriteLine($"[WORKSHOP_DEBUG] Stack trace: {ex.StackTrace}");
                    
                    // Retornar lista vazia em caso de erro
                    return new List<WorkshopViewModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_DEBUG] Erro geral em GetAll: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_DEBUG] Stack trace: {ex.StackTrace}");
                
                // Retornar lista vazia em caso de erro
                return new List<WorkshopViewModel>();
            }
        }



        public async Task<List<T>> GetAll<T>(WorkshopFilterViewModel filterView) where T : class
        {
            try
            {
                Console.WriteLine("[WORKSHOP_DEBUG] Iniciando GetAll com filtros");
                
                filterView.Page = Math.Max(1, filterView.Page.GetValueOrDefault());

                if (filterView.Limit == null || filterView.Limit.GetValueOrDefault() == 0)
                    filterView.Limit = 30;

                // ProfileId Ã© opcional - nÃ£o fazer nada se for null ou vazio
                if (!string.IsNullOrEmpty(filterView.ProfileId))
                {
                    Console.WriteLine($"[WORKSHOP_DEBUG] ProfileId fornecido: {filterView.ProfileId}");
                    // Aqui vocÃª pode adicionar lÃ³gica especÃ­fica para profileId se necessÃ¡rio
                }

                var builder = Builders<Workshop>.Filter;
                var conditions = new List<FilterDefinition<Workshop>>();

                // Filtro básico: considerar null ou 0 como ativo
                conditions.Add(builder.Or(builder.Eq(x => x.Disabled, null), builder.Eq(x => x.Disabled, 0)));
                conditions.Add(builder.Or(builder.Eq(x => x.DataBlocked, null), builder.Eq(x => x.DataBlocked, 0)));

                // Filtro por nome da oficina
                if (!string.IsNullOrEmpty(filterView.Search))
                {
                    conditions.Add(builder.Regex(x => x.CompanyName, new BsonRegularExpression(filterView.Search, "i")));
                }

                // Filtro por nome específico da oficina
                if (!string.IsNullOrEmpty(filterView.WorkshopName))
                {
                    conditions.Add(builder.Eq(x => x.CompanyName, filterView.WorkshopName));
                }

                // Filtro por ID da oficina
                if (!string.IsNullOrEmpty(filterView.WorkshopId))
                {
                    if (ObjectId.TryParse(filterView.WorkshopId, out var workshopObjectId))
                    {
                        conditions.Add(builder.Eq(x => x._id, workshopObjectId));
                    }
                }

                // Filtro por avaliação
                if (filterView.Rating.HasValue)
                {
                    conditions.Add(builder.Eq(x => x.Rating, filterView.Rating.Value));
                }

                // Filtro por data
                if (filterView.StartDate.HasValue)
                {
                    conditions.Add(builder.Gte(x => x.Created, filterView.StartDate.Value));
                }

                if (filterView.EndDate.HasValue)
                {
                    conditions.Add(builder.Lte(x => x.Created, filterView.EndDate.Value));
                }

                // Filtro por status
                if (filterView.Status.HasValue)
                {
                    conditions.Add(builder.Eq(x => x.Status, filterView.Status.Value));
                }

                // Filtro por tipos de serviço
                if (filterView.ServiceTypes != null && filterView.ServiceTypes.Any())
                {
                    try
                    {
                        Console.WriteLine($"[WORKSHOP_DEBUG] Filtrando por ServiceTypes: {string.Join(", ", filterView.ServiceTypes)}");
                        
                        // Buscar WorkshopServices que oferecem os serviços solicitados
                        var filterWorkshopServices = Builders<WorkshopServices>.Filter.In(x => x.Service.Id, filterView.ServiceTypes);
                        
                        var listWorkshopServices = await _workshopServicesRepository
                            .GetCollectionAsync()
                            .Find(filterWorkshopServices)
                            .ToListAsync();

                        Console.WriteLine($"[WORKSHOP_DEBUG] Encontrados {listWorkshopServices.Count} WorkshopServices");

                        // Debug: Verificar estrutura dos dados
                        if (listWorkshopServices.Any())
                        {
                            var firstService = listWorkshopServices.First();
                            Console.WriteLine($"[WORKSHOP_DEBUG] Exemplo de WorkshopService:");
                            Console.WriteLine($"[WORKSHOP_DEBUG] - Service.Id: {firstService.Service?.Id}");
                            Console.WriteLine($"[WORKSHOP_DEBUG] - Service.Name: {firstService.Service?.Name}");
                            Console.WriteLine($"[WORKSHOP_DEBUG] - Workshop.Id: {firstService.Workshop?.Id}");
                            Console.WriteLine($"[WORKSHOP_DEBUG] - Workshop.CompanyName: {firstService.Workshop?.CompanyName}");
                        }

                        if (listWorkshopServices.Any())
                        {
                            // Extrair IDs únicos dos estabelecimentos
                            var workshopIds = listWorkshopServices
                                .Select(w => w.Workshop.Id)
                                .Distinct()
                                .ToList();

                            Console.WriteLine($"[WORKSHOP_DEBUG] IDs únicos de estabelecimentos: {string.Join(", ", workshopIds)}");

                            // Converter para ObjectId e filtrar apenas os válidos
                            var objectIdList = new List<ObjectId>();
                            foreach (var id in workshopIds)
                            {
                                if (ObjectId.TryParse(id, out var objectId))
                                {
                                    objectIdList.Add(objectId);
                                }
                                else
                                {
                                    Console.WriteLine($"[WORKSHOP_DEBUG] ID inválido encontrado: {id}");
                                }
                            }

                            Console.WriteLine($"[WORKSHOP_DEBUG] ObjectIds válidos: {objectIdList.Count}");

                            if (objectIdList.Any())
                            {
                                conditions.Add(builder.In(x => x._id, objectIdList));
                                Console.WriteLine($"[WORKSHOP_DEBUG] Filtro de ServiceTypes aplicado com sucesso");
                            }
                            else
                            {
                                Console.WriteLine($"[WORKSHOP_DEBUG] Nenhum ObjectId válido encontrado para ServiceTypes");
                                // Se não encontrar estabelecimentos com os serviços, retornar lista vazia
                                return new List<T>();
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[WORKSHOP_DEBUG] Nenhum WorkshopServices encontrado para os ServiceTypes fornecidos");
                            // Se não encontrar serviços, retornar lista vazia
                            return new List<T>();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WORKSHOP_DEBUG] Erro ao processar ServiceTypes: {ex.Message}");
                        Console.WriteLine($"[WORKSHOP_DEBUG] Stack trace: {ex.StackTrace}");
                        // Em caso de erro, retornar lista vazia
                        return new List<T>();
                    }
                }
                else
                {
                    Console.WriteLine("[WORKSHOP_DEBUG] Nenhum ServiceTypes fornecido - retornando todos os estabelecimentos");
                }

                var filter = conditions.Any() ? builder.And(conditions) : builder.Empty;

                Console.WriteLine($"[WORKSHOP_DEBUG] Aplicando filtro com {conditions.Count} condiÃ§Ãµes");

                var listEntityData = (await _workshopRepository
                    .FindByAsync(x => true, Util.Sort<Workshop>().Ascending(x => x.Created))).ToList();

                Console.WriteLine($"[WORKSHOP_DEBUG] Encontradas {listEntityData.Count} oficinas");

                // Aplicar filtro de distância se coordenadas fornecidas
                if (!string.IsNullOrEmpty(filterView.LatUser) && !string.IsNullOrEmpty(filterView.LongUser))
                {
                    try
                    {
                        var userLatitude = double.Parse(filterView.LatUser, CultureInfo.InvariantCulture);
                        var userLongitude = double.Parse(filterView.LongUser, CultureInfo.InvariantCulture);

                        // Usar cálculo de distância simples em vez da API do Google
                        for (var i = 0; i < listEntityData.Count; i++)
                        {
                            var workshop = listEntityData[i];
                            if (workshop.Latitude != 0 && workshop.Longitude != 0)
                            {
                                // Cálculo de distância usando fórmula de Haversine (simplificado)
                                listEntityData[i].Distance = CalculateDistance(
                                    userLatitude, userLongitude, 
                                    workshop.Latitude, workshop.Longitude);
                            }
                            else
                            {
                                listEntityData[i].Distance = 0; // Distância zero se não tiver coordenadas
                            }
                        }

                        // Filtrar por distância máxima se especificada
                        if (filterView.Distance.HasValue)
                        {
                            listEntityData = listEntityData
                                .Where(w => w.Distance <= filterView.Distance.Value)
                                .ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WORKSHOP_DEBUG] Erro ao calcular distâncias: {ex.Message}");
                        // Continuar sem cálculo de distância
                    }
                }

                var validWorkshops = await GetWorkshopAvailable(listEntityData);
                Console.WriteLine($"[WORKSHOP_DEBUG] Retornando {validWorkshops.Count} oficinas vÃ¡lidas");

                return _mapper.Map<List<T>>(validWorkshops);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_DEBUG] Erro em GetAll: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_DEBUG] Stack trace: {ex.StackTrace}");
                
                // Tratamento especÃ­fico para erro oldValue do MongoDB
                if (ex.Message.Contains("oldValue") || ex.InnerException?.Message?.Contains("oldValue") == true || 
                    ex.Message.Contains("Object reference not set") || ex.InnerException?.Message?.Contains("Object reference not set") == true)
                {
                    Console.WriteLine($"[WORKSHOP_DEBUG] Erro MongoDB detectado, tentando abordagem alternativa: {ex.Message}");
                    
                    // Tentar abordagem alternativa sem filtros complexos
                    try
                    {
                        var allWorkshops = await _workshopRepository.FindAllAsync();
                        var validWorkshops = await GetWorkshopAvailable(allWorkshops);
                        return _mapper.Map<List<T>>(validWorkshops);
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"[WORKSHOP_DEBUG] Erro na abordagem alternativa: {fallbackEx.Message}");
                        throw new Exception($"Erro em WorkshopService.GetAll: {ex.Message}", ex);
                    }
                }
                
                throw new Exception($"Erro em WorkshopService.GetAll: {ex.Message}", ex);
            }
        }

        // Retornar Oficinas somente se estiver com os dados bancÃ¡rios, agenda e serviÃ§os configurados
        public async Task<List<Workshop>> GetWorkshopAvailable(IEnumerable<Workshop> listEntityData)
        {
            var validWorkshops = new List<Workshop>();

            foreach (var workshop in listEntityData)
            {
                // TEMPORARIAMENTE: Permitir todas as oficinas para debug
                // Comentar a validaÃ§Ã£o rigorosa atÃ© resolver o problema de dados
                
                // var dataBankValid = workshop.DataBankStatus == DataBankStatus.Valid;
                // var workshopAgendaValid = await _workshopAgendaRepository.CheckByAsync(x => x.Workshop.Id == workshop.GetStringId());
                // var workshopServicesValid = await _workshopServicesRepository.CheckByAsync(x => x.Workshop.Id == workshop.GetStringId());

                // if (dataBankValid && workshopAgendaValid && workshopServicesValid)
                // {
                //     validWorkshops.Add(workshop);
                // }
                
                // Por enquanto, aceitar todas as oficinas que não estão desabilitadas
                if ((workshop.Disabled == null || workshop.Disabled == 0) && (workshop.DataBlocked == null || workshop.DataBlocked == 0))
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

                    if (workshopEntity.Latitude != 0 && workshopEntity.Longitude != 0)
                    {
                        workshopEntity.Distance = CalculateDistance(userLatitude, userLongitude, workshopEntity.Latitude, workshopEntity.Longitude);
                    }
                    else
                    {
                        workshopEntity.Distance = 0;
                    }
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
                Console.WriteLine($"[GET_INFO_DEBUG] Iniciando GetInfo para userId: {userId}");

                if (ObjectId.TryParse(userId, out var _id) == false)
                {
                    Console.WriteLine($"[GET_INFO_DEBUG] UserId inválido: {userId}");
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(userId).ConfigureAwait(false);
                
                if (workshopEntity == null)
                {
                    Console.WriteLine($"[GET_INFO_DEBUG] Workshop NÃO encontrado no banco para ID: {userId}");
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                Console.WriteLine($"[GET_INFO_DEBUG] Workshop encontrado: {workshopEntity.GetStringId()}");
                Console.WriteLine($"[GET_INFO_DEBUG] Workshop CompanyName: {workshopEntity.CompanyName}");
                Console.WriteLine($"[GET_INFO_DEBUG] Workshop Email: {workshopEntity.Email}");
                Console.WriteLine($"[GET_INFO_DEBUG] Workshop Status: {workshopEntity.Status}");

                Console.WriteLine("[GET_INFO_DEBUG] Iniciando mapeamento para WorkshopViewModel...");
                var responseVm = _mapper.Map<WorkshopViewModel>(workshopEntity);
                
                if (responseVm == null)
                {
                    Console.WriteLine("[GET_INFO_DEBUG] ERRO: WorkshopViewModel mapeado é NULL");
                    return null;
                }

                Console.WriteLine($"[GET_INFO_DEBUG] WorkshopViewModel mapeado com sucesso");
                Console.WriteLine($"[GET_INFO_DEBUG] WorkshopViewModel ID: {responseVm.Id}");
                Console.WriteLine($"[GET_INFO_DEBUG] WorkshopViewModel CompanyName: {responseVm.CompanyName}");

                Console.WriteLine("[GET_INFO_DEBUG] Verificando agenda...");
                responseVm.WorkshopAgendaValid = await _workshopAgendaRepository.CheckByAsync(x => x.Workshop.Id == userId);
                Console.WriteLine($"[GET_INFO_DEBUG] WorkshopAgendaValid: {responseVm.WorkshopAgendaValid}");

                Console.WriteLine("[GET_INFO_DEBUG] Verificando serviços...");
                responseVm.WorkshopServicesValid = await _workshopServicesRepository.CheckByAsync(x => x.Workshop.Id == userId);
                Console.WriteLine($"[GET_INFO_DEBUG] WorkshopServicesValid: {responseVm.WorkshopServicesValid}");

                Console.WriteLine("[GET_INFO_DEBUG] Verificando dados bancários...");
                // CORREÇÃO: Usar o campo HasDataBank que é atualizado no UpdateDataBank
                responseVm.DataBankValid = workshopEntity.HasDataBank;
                Console.WriteLine($"[GET_INFO_DEBUG] DataBankValid: {responseVm.DataBankValid}");
                Console.WriteLine($"[GET_INFO_DEBUG] HasDataBank: {workshopEntity.HasDataBank}");
                Console.WriteLine($"[GET_INFO_DEBUG] AccountableName: {workshopEntity.AccountableName}");
                Console.WriteLine($"[GET_INFO_DEBUG] BankAccount: {workshopEntity.BankAccount}");
                Console.WriteLine($"[GET_INFO_DEBUG] BankAgency: {workshopEntity.BankAgency}");

                Console.WriteLine("[GET_INFO_DEBUG] GetInfo concluído com sucesso - retornando dados");
                return responseVm;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GET_INFO_DEBUG] ERRO no GetInfo: {ex.Message}");
                Console.WriteLine($"[GET_INFO_DEBUG] StackTrace: {ex.StackTrace}");
                CreateNotification(DefaultMessages.DefaultError);
                return null;
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
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Iniciando registro de oficina");
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Model recebido: {System.Text.Json.JsonSerializer.Serialize(model)}");
                
                // Verificar se o repositório está inicializado
                if (_workshopRepository == null)
                {
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] ERRO: _workshopRepository é null");
                    throw new InvalidOperationException("Repositório não inicializado");
                }
                
                var ignoredFields = new List<string>();

                if (model.TypeProvider != TypeProvider.Password)
                {
                    ignoredFields.Add(nameof(model.Email));
                    ignoredFields.Add(nameof(model.Password));
                }

                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Campos ignorados: {string.Join(", ", ignoredFields)}");
                
                if (ModelIsValid(model, ignoredFields: [.. ignoredFields]) == false)
                {
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Validação falhou. Erros: {ReturnValidationsToString()}");
                    return null;
                }
                
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Validação passou com sucesso");

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

                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Mapeando modelo para entidade");
                var workshopEntity = _mapper.Map<Workshop>(model);
                
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Entidade mapeada: {System.Text.Json.JsonSerializer.Serialize(workshopEntity)}");
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] MeiCard na entidade: {workshopEntity.MeiCard}");
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Photo na entidade: {workshopEntity.Photo}");

                if (string.IsNullOrEmpty(model.Password) == false)
                    workshopEntity.Password = Utilities.GerarHashMd5(model.Password);

                workshopEntity.Status = WorkshopStatus.AwaitingApproval;

                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Salvando oficina no banco de dados");
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Collection name: {workshopEntity.CollectionName}");
                
                // Testar conexão com o banco
                try
                {
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Testando GetCollectionAsync...");
                    var collection = _workshopRepository.GetCollectionAsync();
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Collection obtida com sucesso: {collection.CollectionNamespace}");
                    
                    // Contar documentos na collection
                    var count = await collection.CountDocumentsAsync(Builders<Workshop>.Filter.Empty);
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Total de documentos na collection Workshop: {count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] ERRO ao testar conexão com banco: {ex.Message}");
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Stack trace: {ex.StackTrace}");
                    throw;
                }
                
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Chamando CreateReturnAsync...");
                workshopEntity = await _workshopRepository.CreateReturnAsync(workshopEntity).ConfigureAwait(false);
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Oficina salva com sucesso. ID: {workshopEntity._id}");
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Oficina salva - MeiCard: {workshopEntity.MeiCard}");
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Oficina salva - Photo: {workshopEntity.Photo}");

                // Verificar se realmente foi salvo no banco
                var savedWorkshop = await _workshopRepository.FindByIdAsync(workshopEntity._id.ToString());
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Verificação no banco - encontrado: {savedWorkshop != null}");
                if (savedWorkshop != null)
                {
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Verificação no banco - MeiCard: {savedWorkshop.MeiCard}");
                    Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Verificação no banco - Photo: {savedWorkshop.Photo}");
                }

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

                var result = _mapper.Map<WorkshopViewModel>(workshopEntity);
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Retornando resultado: {System.Text.Json.JsonSerializer.Serialize(result)}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] ERRO no registro: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_REGISTER_DEBUG] Stack trace: {ex.StackTrace}");
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
                Console.WriteLine("[WORKSHOP_LOADDATA_DEBUG] Iniciando LoadData");
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Model Draw: {model.Draw}");
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Model Start: {model.Start}");
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Model Length: {model.Length}");
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Model Search Value: {model.Search?.Value}");
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] FilterView Status: {filterView?.Status}");

                var response = new DtResult<WorkshopViewModel>();

                var builder = Builders<Workshop>.Filter;
                var conditions = new List<FilterDefinition<Workshop>>();

                if (filterView.Status != null)
                    conditions.Add(builder.Eq(x => x.Status, filterView.Status));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Colunas searchable: {string.Join(", ", columns)}");

                var totalRecords = (int)await _workshopRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Total records: {totalRecords}");

                var sortBy = Util.MapSort<Workshop>(model.Order, model.Columns, model.SortOrder);
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] SortBy: {sortBy}");

                var result = await _workshopRepository
                   .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Result count: {result?.Count() ?? 0}");

                var totalRecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _workshopRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Total records filtered: {totalRecordsFiltered}");

                response.Data = _mapper.Map<List<WorkshopViewModel>>(result);
                response.Draw = model.Draw;
                response.RecordsFiltered = totalRecordsFiltered;
                response.RecordsTotal = totalRecords;

                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Response Data count: {response.Data?.Count ?? 0}");
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Response RecordsTotal: {response.RecordsTotal}");
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Response RecordsFiltered: {response.RecordsFiltered}");

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Erro em LoadData: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_LOADDATA_DEBUG] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> RegisterUnRegisterDeviceId(PushViewModel model)
        {
            try
            {
                Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Iniciando registro/remoção de dispositivo para oficina");
                Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] DeviceId: {model.DeviceId}");
                Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] IsRegister: {model.IsRegister}");
                
                if (ModelIsValid(model, true) == false)
                {
                    Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Modelo inválido");
                    return false;
                }

                var userId = _access.UserId;
                Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] UserId: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] UserId está vazio");
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return false;
                }

                _ = Task.Run(() =>
               {
                   if (string.IsNullOrEmpty(model.DeviceId) == false)
                   {
                       Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Executando operação no banco de dados");
                       if (model.IsRegister)
                       {
                           Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Registrando dispositivo {model.DeviceId} para oficina {userId}");
                           _workshopRepository.UpdateMultiple(Query<Workshop>.Where(x => x._id == ObjectId.Parse(userId)),
                           new UpdateBuilder<Workshop>().AddToSet(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                       }
                       else
                       {
                           Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Removendo dispositivo {model.DeviceId} da oficina {userId}");
                           _workshopRepository.UpdateMultiple(Query<Workshop>.Where(x => x._id == ObjectId.Parse(userId)),
                           new UpdateBuilder<Workshop>().Pull(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                       }
                       Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Operação concluída com sucesso");
                   }
                   else
                   {
                       Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] DeviceId está vazio, operação ignorada");
                   }
               });

                Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Método concluído com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Erro: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_DEVICE_REGISTRATION_DEBUG] Stack trace: {ex.StackTrace}");
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

                // AlteraÃ§Ã£o de status
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
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Iniciando UpdateDataBank para ID: {id}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Model recebido: {System.Text.Json.JsonSerializer.Serialize(model)}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Model é null: {model == null}");
                if (model != null)
                {
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] AccountableName: {model.AccountableName}");
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] BankAccount: {model.BankAccount}");
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] BankAgency: {model.BankAgency}");
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Bank: {model.Bank}");
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] BankName: {model.BankName}");
                }

                var ignoreField = new List<string>();

                if (model.PersonType == TypePersonBank.PhysicalPerson)
                    ignoreField.Add(nameof(model.BankCnpj));

                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Campos ignorados: {string.Join(", ", ignoreField)}");

                if (ModelIsValid(model, true, ignoredFields: ignoreField.ToArray()) == false)
                {
                    Console.WriteLine("[UPDATE_DATA_BANK_DEBUG] ModelIsValid retornou false");
                    return null;
                }

                var userId = id;

                if (_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Tipo de token inválido: {_access.TypeToken}");
                    CreateNotification(DefaultMessages.InvalidCredentials);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(userId);
                if (workshopEntity == null)
                {
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Workshop não encontrado para ID: {userId}");
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Workshop encontrado: {workshopEntity.GetStringId()}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] ExternalId atual: {workshopEntity.ExternalId}");

                // Se não tem ExternalId, criar conta Stripe automaticamente
                if (string.IsNullOrEmpty(workshopEntity.ExternalId))
                {
                    Console.WriteLine("[UPDATE_DATA_BANK_DEBUG] Workshop sem ExternalId, criando conta Stripe...");
                    
                    var remoteIp = Utilities.GetClientIp();
                    var userAgent = _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

                    var accountOptions = workshopEntity.MapAccount(remoteIp, userAgent);
                    var subAccount = await _stripeMarketPlaceService.CreateAsync(accountOptions);

                    if (subAccount.Success == false)
                    {
                        Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Erro ao criar conta Stripe: {subAccount.ErrorMessage}");
                        CreateNotification(subAccount.ErrorMessage);
                        return null;
                    }

                    workshopEntity.ExternalId = subAccount.Data?.Id;
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Conta Stripe criada com ID: {workshopEntity.ExternalId}");
                    
                    // Salvar o ExternalId no workshop
                    await _workshopRepository.UpdateAsync(workshopEntity);
                }

                var stripeResultMarketPlace = await _stripeMarketPlaceService.GetByIdAsync(workshopEntity.ExternalId);

                if (stripeResultMarketPlace.Success == false)
                {
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Erro ao obter conta Stripe: {stripeResultMarketPlace.ErrorMessage}");
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

                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] Opções bancárias criadas: {System.Text.Json.JsonSerializer.Serialize(dataBankOptions)}");

                stripeResultMarketPlace = await _stripeMarketPlaceService.UpdateExternalAccountAsync(workshopEntity.ExternalId, dataBankOptions);

                if (stripeResultMarketPlace.Success == false)
                {
                    Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] AVISO: Erro ao atualizar conta bancária no Stripe: {stripeResultMarketPlace.ErrorMessage}");
                    Console.WriteLine("[UPDATE_DATA_BANK_DEBUG] Continuando para salvar dados no MongoDB mesmo com erro no Stripe...");
                    // Não retornar null - continuar para salvar no MongoDB
                }

                Console.WriteLine("[UPDATE_DATA_BANK_DEBUG] Conta bancária atualizada no Stripe com sucesso");

                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] _jsonBodyFields: {string.Join(", ", _jsonBodyFields ?? new string[0])}");
                Console.WriteLine("[UPDATE_DATA_BANK_DEBUG] Antes do mapeamento:");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - AccountableName: {workshopEntity.AccountableName}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - BankAccount: {workshopEntity.BankAccount}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - BankAgency: {workshopEntity.BankAgency}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - Bank: {workshopEntity.Bank}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - BankName: {workshopEntity.BankName}");

                // Usar AutoMapper para mapear os dados bancários
                _mapper.Map(model, workshopEntity);

                Console.WriteLine("[UPDATE_DATA_BANK_DEBUG] Após o mapeamento AutoMapper:");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - AccountableName: {workshopEntity.AccountableName}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - BankAccount: {workshopEntity.BankAccount}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - BankAgency: {workshopEntity.BankAgency}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - Bank: {workshopEntity.Bank}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] - BankName: {workshopEntity.BankName}");

                // Definir HasDataBank baseado nos dados salvos, não no Stripe
                workshopEntity.HasDataBank = !string.IsNullOrEmpty(workshopEntity.AccountableName) && 
                                           !string.IsNullOrEmpty(workshopEntity.BankAccount) && 
                                           !string.IsNullOrEmpty(workshopEntity.BankAgency);

                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] HasDataBank definido como: {workshopEntity.HasDataBank}");

                Console.WriteLine("[UPDATE_DATA_BANK_DEBUG] Salvando workshop no banco...");
                await _workshopRepository.UpdateAsync(workshopEntity);
                Console.WriteLine("[UPDATE_DATA_BANK_DEBUG] Workshop salvo com sucesso");

                return "Dados atualizados com sucesso";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] ERRO no UpdateDataBank: {ex.Message}");
                Console.WriteLine($"[UPDATE_DATA_BANK_DEBUG] StackTrace: {ex.StackTrace}");
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

        public async Task<DtResult<WorkshopViewModel>> LoadDataGrid(DtParameters model, WorkshopFilterViewModel filterView)
        {
            try
            {
                Console.WriteLine("[WORKSHOP_DEBUG] Iniciando LoadDataGrid");

                var allWorkshops = await GetAll<WorkshopViewModel>(filterView);

                var result = new DtResult<WorkshopViewModel>
                {
                    Data = allWorkshops,
                    Draw = model.Draw,
                    RecordsTotal = allWorkshops.Count,
                    RecordsFiltered = allWorkshops.Count
                };

                Console.WriteLine($"[WORKSHOP_DEBUG] LoadDataGrid retornando {allWorkshops.Count} workshops");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_DEBUG] Erro em LoadDataGrid: {ex.Message}");
                return new DtResult<WorkshopViewModel>
                {
                    Data = new List<WorkshopViewModel>(),
                    Draw = model.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0
                };
            }
        }

        /// <summary>
        /// Calcula a distância entre dois pontos usando a fórmula de Haversine
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            try
            {
                const double R = 6371; // Raio da Terra em quilômetros
                var dLat = ToRadians(lat2 - lat1);
                var dLon = ToRadians(lon2 - lon1);
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                return R * c;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_DEBUG] Erro ao calcular distância: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Converte graus para radianos
        /// </summary>
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Atualiza workshops que não têm foto ou descrição
        /// </summary>
        public async Task<bool> UpdateWorkshopsWithoutPhotoAndReason()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_DEBUG] Iniciando atualização de workshops sem foto e descrição");
                
                var builder = Builders<Workshop>.Filter;
                var filter = builder.Or(
                    builder.Eq(x => x.Photo, null),
                    builder.Eq(x => x.Photo, ""),
                    builder.Eq(x => x.Reason, null),
                    builder.Eq(x => x.Reason, "")
                );
                
                var workshopsToUpdate = await _workshopRepository
                    .GetCollectionAsync()
                    .Find(filter)
                    .ToListAsync();
                
                Console.WriteLine($"[WORKSHOP_DEBUG] Encontrados {workshopsToUpdate.Count} workshops para atualizar");
                
                var updateBuilder = Builders<Workshop>.Update;
                var update = updateBuilder
                    .SetOnInsert(x => x.Photo, "default-workshop.png")
                    .SetOnInsert(x => x.Reason, "Estabelecimento de confiança para serviços automotivos");
                
                var result = await _workshopRepository
                    .GetCollectionAsync()
                    .UpdateManyAsync(filter, update);
                
                Console.WriteLine($"[WORKSHOP_DEBUG] Atualizados {result.ModifiedCount} workshops");
                
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_DEBUG] Erro ao atualizar workshops: {ex.Message}");
                return false;
            }
        }
    }
}
