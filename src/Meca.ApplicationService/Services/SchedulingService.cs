using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Auxiliaries;
using Meca.Domain.ViewModels.Filters;
using Meca.Shared.ObjectValues;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Infra.Core3.MongoDb.Data.Modelos;
using UtilityFramework.Services.Core3.Interface;
using UtilityFramework.Services.Stripe.Core3;

namespace Meca.ApplicationService.Services
{
    public class SchedulingService : ApplicationServiceBase<Scheduling>, ISchedulingService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<Scheduling> _schedulingRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IBusinessBaseAsync<WorkshopAgenda> _workshopAgendaRepository;
        private readonly IBusinessBaseAsync<WorkshopServices> _workshopServicesRepository;
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IBusinessBaseAsync<UserAdministrator> _userAdministratorRepository;
        private readonly IBusinessBaseAsync<Vehicle> _vehicleRepository;
        private readonly IBusinessBaseAsync<AgendaAux> _agendaAuxRepository;
        private readonly IBusinessBaseAsync<Fees> _feesRepository;
        private readonly INotificationService _notificationService;
        private readonly ISchedulingHistoryService _schedulingHistoryService;
        private readonly ISenderMailService _senderMailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public SchedulingService(
            IMapper mapper,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            IBusinessBaseAsync<Workshop> workshopRepository,
            IBusinessBaseAsync<WorkshopAgenda> workshopAgendaRepository,
            IBusinessBaseAsync<WorkshopServices> workshopServicesRepository,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<UserAdministrator> userAdministratorRepository,
            IBusinessBaseAsync<Vehicle> vehicleRepository,
            IBusinessBaseAsync<AgendaAux> agendaAuxRepository,
            IBusinessBaseAsync<Fees> feesRepository,
            INotificationService notificationService,
            ISchedulingHistoryService schedulingHistoryService,
            ISenderMailService senderMailService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _schedulingRepository = schedulingRepository;
            _workshopRepository = workshopRepository;
            _workshopAgendaRepository = workshopAgendaRepository;
            _workshopServicesRepository = workshopServicesRepository;
            _profileRepository = profileRepository;
            _userAdministratorRepository = userAdministratorRepository;
            _vehicleRepository = vehicleRepository;
            _agendaAuxRepository = agendaAuxRepository;
            _notificationService = notificationService;
            _schedulingHistoryService = schedulingHistoryService;
            _senderMailService = senderMailService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _feesRepository = feesRepository;
        }

        /*CONSTRUTOR UTILIZADO POR TESTES DE UNIDADE*/
        public SchedulingService(
            IHostingEnvironment env,
            IMapper mapper,
            IConfiguration configuration,
            Acesso acesso,
            string testUnit,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            IBusinessBaseAsync<Fees> feesRepository)
        {
            _schedulingRepository = schedulingRepository;
            _mapper = mapper;
            _configuration = configuration;

            SetAccessTest(acesso);
            _feesRepository = feesRepository;
        }

        public async Task<List<SchedulingViewModel>> GetAll()
        {
            var listScheduling = await _schedulingRepository.FindAllAsync(Util.Sort<Scheduling>().Descending(nameof(Scheduling.Date)));
            return _mapper.Map<List<SchedulingViewModel>>(listScheduling);
        }

        public async Task<List<T>> GetAll<T>(SchedulingFilterViewModel filterView) where T : class
        {
            try
            {
                Console.WriteLine("[SCHEDULING_DEBUG] Iniciando GetAll com filtros");
                
                filterView.SetDefault();
                var builder = Builders<Scheduling>.Filter;
                var conditions = new List<FilterDefinition<Scheduling>>();

                // Filtro básico: apenas agendamentos não desabilitados
                conditions.Add(builder.Eq(x => x.DataBlocked, null));

                // Lógica para tratar quando profileId não é fornecido
                if ((int)_access.TypeToken == (int)TypeProfile.Profile && string.IsNullOrEmpty(filterView.ProfileId))
                {
                    Console.WriteLine($"[SCHEDULING_DEBUG] Usando userId do acesso: {_access.UserId}");
                    conditions.Add(builder.Eq(x => x.Profile.Id, _access.UserId));
                }

                // Filtro por profileId se fornecido
                if (!string.IsNullOrEmpty(filterView.ProfileId))
                {
                    Console.WriteLine($"[SCHEDULING_DEBUG] ProfileId fornecido: {filterView.ProfileId}");
                    conditions.Add(builder.Eq(x => x.Profile.Id, filterView.ProfileId));
                }

                // Filtro por workshopId se fornecido
                if (!string.IsNullOrEmpty(filterView.WorkshopId))
                {
                    Console.WriteLine($"[SCHEDULING_DEBUG] WorkshopId fornecido: {filterView.WorkshopId}");
                    conditions.Add(builder.Eq(x => x.Workshop.Id, filterView.WorkshopId));
                }

                // Filtro por data
                if (filterView.StartDate.HasValue)
                {
                    conditions.Add(builder.Gte(x => x.Date, filterView.StartDate.Value));
                }

                if (filterView.EndDate.HasValue)
                {
                    conditions.Add(builder.Lte(x => x.Date, filterView.EndDate.Value));
                }

                // Filtro por status
                if (filterView.Status != null && filterView.Status.Any())
                {
                    conditions.Add(builder.In(x => x.Status, filterView.Status));
                }

                // Filtro por tipo de token (workshop)
                if ((int)_access.TypeToken == (int)TypeProfile.Workshop)
                {
                    Console.WriteLine($"[SCHEDULING_DEBUG] Filtrando por workshop do acesso: {_access.UserId}");
                    conditions.Add(builder.Eq(x => x.Workshop.Id, _access.UserId));
                }

                var filter = conditions.Any() ? builder.And(conditions) : builder.Empty;

                Console.WriteLine($"[SCHEDULING_DEBUG] Aplicando filtro com {conditions.Count} condições");

                var listScheduling = await _schedulingRepository
                    .GetCollectionAsync()
                    .FindSync(filter, Util.FindOptions(filterView, Util.Sort<Scheduling>().Descending(x => x._id)))
                    .ToListAsync();

                Console.WriteLine($"[SCHEDULING_DEBUG] Encontrados {listScheduling.Count} agendamentos");

                return _mapper.Map<List<T>>(listScheduling);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCHEDULING_DEBUG] Erro em GetAll: {ex.Message}");
                Console.WriteLine($"[SCHEDULING_DEBUG] Stack trace: {ex.StackTrace}");
                
                // Tratamento específico para erro oldValue do MongoDB
                if (ex.Message.Contains("oldValue") || ex.InnerException?.Message?.Contains("oldValue") == true ||
                    ex.Message.Contains("Object reference not set") || ex.InnerException?.Message?.Contains("Object reference not set") == true)
                {
                    Console.WriteLine($"[SCHEDULING_DEBUG] Erro MongoDB detectado, tentando abordagem alternativa: {ex.Message}");
                    
                    // Tentar abordagem alternativa sem filtros complexos
                    try
                    {
                        var allSchedulings = await _schedulingRepository.FindAllAsync();
                        return _mapper.Map<List<T>>(allSchedulings);
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"[SCHEDULING_DEBUG] Erro na abordagem alternativa: {fallbackEx.Message}");
                        throw new Exception($"Erro em SchedulingService.GetAll: {ex.Message}", ex);
                    }
                }
                
                throw new Exception($"Erro em SchedulingService.GetAll: {ex.Message}", ex);
            }
        }

        public async Task<SchedulingViewModel> GetById(string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return null;
                }

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(id);

                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                var response = _mapper.Map<SchedulingViewModel>(schedulingEntity);

                var canShowStatus = new HashSet<SchedulingStatus>() { SchedulingStatus.BudgetApproved, SchedulingStatus.BudgetPartiallyApproved };

                if (canShowStatus.Contains(schedulingEntity.Status))
                {
                    var currentFee = await _feesRepository.FindOneByAsync(x => x.DataBlocked == null);

                    if (currentFee == null)
                    {
                        CreateNotification(DefaultMessages.FeeNotFound);
                        return null;
                    }
                    var workshopEntity = await _workshopRepository.FindByIdAsync(schedulingEntity.Workshop.Id);

                    if (workshopEntity == null)
                    {
                        CreateNotification(DefaultMessages.WorkshopNotFound);
                        return null;
                    }

                    response.DataTransfer = new()
                    {
                        PlatformCommission = currentFee.PlatformFee,
                        PlatformCommissionType = EStripeTypeValue.Percentage,
                        PartnerTransfer = new()
                        {
                            AccountId = workshopEntity.ExternalId
                        }
                    };
                }

                return response;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<SchedulingViewModel> Register(SchedulingViewModel model)
        {
            try
            {
                Console.WriteLine($"[Register] Iniciando registro de agendamento - Date: {model.Date}, WorkshopId: {model.Workshop?.Id}");
                
                if (ModelIsValid(model, false) == false)
                    return null;

                var profileEntity = await _profileRepository.FindByIdAsync(_access.UserId);
                if (profileEntity == null)
                {
                    CreateNotification(DefaultMessages.ProfileNotFound);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(model.Workshop.Id);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }
                
                Console.WriteLine($"[Register] Profile: {profileEntity.GetStringId()}, Workshop: {workshopEntity.GetStringId()}");

                var workshopAgendaEntity = await _workshopAgendaRepository.FindByAsync(x => x.Workshop.Id == workshopEntity.GetStringId());
                var workshopServicesEntity = await _workshopServicesRepository.FindByAsync(x => x.Workshop.Id == workshopEntity.GetStringId());

                Console.WriteLine($"[Register] WorkshopAgenda encontrada: {workshopAgendaEntity.Count()}");
                Console.WriteLine($"[Register] WorkshopServices encontrados: {workshopServicesEntity.Count()}");

                // Permitir agendamentos para Oficina se estiver com agenda e serviços configurados
                // Removida a validação de dados bancários para permitir agendamentos de teste
                if (!workshopAgendaEntity.Any() || !workshopServicesEntity.Any())
                {
                    CreateNotification("Essa Oficina não está habilitada para receber agendamentos. Verifique se a agenda e serviços estão configurados.");
                    return null;
                }

                var schedulingExist = await _schedulingRepository.FindOneByAsync(x => x.Date == model.Date && (int)x.Status <= (int)SchedulingStatus.Scheduled && x.Workshop.Id == model.Workshop.Id);
                if (schedulingExist != null)
                {
                    Console.WriteLine($"[Register] Agendamento já existe para esta data/hora");
                    CreateNotification(DefaultMessages.SchedulingInUse);
                    return null;
                }

                var minTimeSpan = await CalculateMinTimeScheduling(model.WorkshopServices);

                // Converte epoch para DateTime (UTC)
                DateTime utcDate = DateTimeOffset.FromUnixTimeSeconds(model.Date).UtcDateTime;

                // Define o fuso horário GMT-3
                TimeZoneInfo gmtMinus3 = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime schedulingDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, gmtMinus3);

                // Pegando data atual
                DateTime todayDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gmtMinus3);
                
                Console.WriteLine($"[Register] Data do agendamento: {schedulingDate:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"[Register] Data atual: {todayDate:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"[Register] MinTimeSpan: {minTimeSpan}");

                if (schedulingDate.Date == todayDate.Date && schedulingDate.TimeOfDay < todayDate.TimeOfDay.Add(minTimeSpan))
                {
                    Console.WriteLine($"[Register] Horário muito próximo - agendamento: {schedulingDate.TimeOfDay}, atual: {todayDate.TimeOfDay}, min: {minTimeSpan}");
                    CreateNotification("Horário inválido! Por favor, selecione um horário que respeite o horário mínimo para agendamento.");
                    return null;
                }

                if (schedulingDate.Date < todayDate.Date)
                {
                    Console.WriteLine($"[Register] Data no passado - agendamento: {schedulingDate.Date}, atual: {todayDate.Date}");
                    CreateNotification("Data inválida! Por favor, selecione uma data válida.");
                    return null;
                }

                var filterScheduling = new SchedulingAvailableFilterViewModel { Date = model.Date, WorkshopId = workshopEntity.GetStringId(), Services = model.WorkshopServices };
                var res = await GetAvailableScheduling(filterScheduling);
                var hourModelDate = schedulingDate.ToString("HH:mm");

                Console.WriteLine($"[Register] Validando horário: {hourModelDate}");
                Console.WriteLine($"[Register] WorkshopAgenda disponível: {res?.WorkshopAgenda?.Count ?? 0} horários");
                
                if (res != null && res.WorkshopAgenda != null)
                {
                    var availableHours = res.WorkshopAgenda.Where(x => x.Available).Select(x => x.Hour).ToList();
                    Console.WriteLine($"[Register] Horários disponíveis: {string.Join(", ", availableHours)}");
                    
                    if (!res.WorkshopAgenda.Any(x => x.Hour == hourModelDate && x.Available))
                    {
                        Console.WriteLine($"[Register] Horário {hourModelDate} não está disponível");
                        CreateNotification("Horário inválido! Por favor, selecione um horário dentro do horário de funcionamento da oficina.");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"[Register] Resposta ou WorkshopAgenda é null");
                    CreateNotification("Não foi possível verificar a disponibilidade de horários. Tente novamente.");
                    return null;
                }

                var schedulingEntity = _mapper.Map<Scheduling>(model);

                schedulingEntity.Profile = _mapper.Map<ProfileAux>(profileEntity);
                schedulingEntity.Workshop = _mapper.Map<WorkshopAux>(workshopEntity);

                var vehicleEntity = await _vehicleRepository.FindByIdAsync(model.Vehicle.Id);

                schedulingEntity.Vehicle = _mapper.Map<VehicleAux>(vehicleEntity);
                
                Console.WriteLine($"[Register] Agendamento criado com sucesso - ID: {schedulingEntity.GetStringId()}");

                string title = "Novo agendamento";
                StringBuilder message = new StringBuilder();
                message.AppendLine($"Você possui um novo agendamento.<br/>");
                message.AppendLine($"Data: {model.Date.MapUnixTime("dd/MM/yyyy HH:mm", "-")}<br/><br/>");
                message.AppendLine("<p>Informações do veículo abaixo<br/>");
                message.AppendLine($"<p>Placa: {vehicleEntity.Plate}<br/>");
                message.AppendLine($"<p>Fabricante: {vehicleEntity.Manufacturer}<br/>");
                message.AppendLine($"<p>Modelo: {vehicleEntity.Model}<br/>");
                message.AppendLine($"<p>Quilometragem: {vehicleEntity.Km}<br/>");
                message.AppendLine($"<p>Cor: {vehicleEntity.Color}<br/>");
                message.AppendLine($"<p>Ano: {vehicleEntity.Year}<br/>");
                message.AppendLine($"<p>Data da última revisão: {vehicleEntity.LastRevisionDate.MapUnixTime("dd/MM/yyyy", "-")}<br/>");
                await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);

                schedulingEntity = await _schedulingRepository.CreateReturnAsync(schedulingEntity);

                await RegisterSchedulingHistory(schedulingEntity);

                Console.WriteLine($"[Register] Agendamento salvo no banco com sucesso - ID: {schedulingEntity.GetStringId()}");
                
                return _mapper.Map<SchedulingViewModel>(schedulingEntity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Register] Erro no registro: {ex.Message}");
                Console.WriteLine($"[Register] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<SchedulingViewModel> UpdatePatch(string id, SchedulingViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                    return null;

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(id);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                schedulingEntity.SetIfDifferent(model, validOnly, _mapper);

                schedulingEntity = await _schedulingRepository.UpdateAsync(schedulingEntity);

                return _mapper.Map<SchedulingViewModel>(schedulingEntity);
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
                if (ObjectId.TryParse(id, out ObjectId _id) == false)
                {
                    CreateNotification(DefaultMessages.InvalidIdentifier);
                    return false;
                }

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(id);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return false;
                }

                if (_access.UserId != schedulingEntity.Profile.Id)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                if ((int)schedulingEntity.Status > (int)SchedulingStatus.AppointmentRefused)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                await _schedulingRepository.DeleteOneAsync(id);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SuggestNewTime(SuggestNewTimeViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return false;

                if (_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    CreateNotification(DefaultMessages.OnlyWorkshop);
                    return false;
                }

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return false;
                }

                if (schedulingEntity.Workshop.Id != _access.UserId)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                if (schedulingEntity.Status != SchedulingStatus.WaitingAppointment)
                {
                    CreateNotification("Você não pode sugerir outro horário para esse agendamento.");
                    return false;
                }

                schedulingEntity.SuggestedDate = model.Date;
                schedulingEntity.Status = SchedulingStatus.SuggestedTime;
                await _schedulingRepository.UpdateAsync(schedulingEntity);

                string title = "Sugestão de novo horário de agendamento";
                var message = new StringBuilder();
                message.AppendLine($"Temos sugestão de um novo horário para o seu agendamento de n°{schedulingEntity.GetStringId()}.<br/>");
                message.AppendLine($"Oficina: {schedulingEntity.Workshop.CompanyName}<br/>");
                message.AppendLine($"Horário sugerido: {schedulingEntity.SuggestedDate.MapUnixTime("dd/MM/yyyy HH:mm", "-")}<br/>");
                await SendProfileNotification(title, message, schedulingEntity.Profile.Id, schedulingEntity.Workshop);

                await RegisterSchedulingHistory(schedulingEntity);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SchedulingViewModel> ConfirmScheduling(ConfirmSchedulingViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(schedulingEntity.Workshop.Id);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                if (_access.TypeToken == (int)TypeProfile.Workshop)
                {
                    if (schedulingEntity.Workshop.Id != _access.UserId)
                    {
                        CreateNotification(DefaultMessages.NotPermission);
                        return null;
                    }

                    if (schedulingEntity.Status != SchedulingStatus.WaitingAppointment)
                    {
                        CreateNotification(DefaultMessages.NotPermission);
                        return null;
                    }

                    if (model.ConfirmSchedulingStatus == ConfirmStatus.Approved)
                    {
                        schedulingEntity.Status = SchedulingStatus.Scheduled;

                        string title = "Agendamento confirmado pela Oficina";
                        var message = new StringBuilder();

                        message.AppendLine($"Seu agendamento de n°{schedulingEntity.GetStringId()} foi confirmado com sucesso.<br/>");
                        message.AppendLine($"Oficina: {schedulingEntity.Workshop.CompanyName}<br/>");
                        message.AppendLine($"Data e Horário: {schedulingEntity.Date.MapUnixTime("dd/MM/yyyy HH:mm", "-")}<br/>");
                        message.AppendLine($"Endereço da Oficina: {workshopEntity.StreetAddress}, nº{workshopEntity.Number} - {workshopEntity.Neighborhood} - {workshopEntity.CityName} - {workshopEntity.StateUf}, {workshopEntity.ZipCode} - Complemento: {workshopEntity.Complement} <br/>");
                        await SendProfileNotification(title, message, schedulingEntity.Profile.Id, schedulingEntity.Workshop);
                    }

                    if (model.ConfirmSchedulingStatus == ConfirmStatus.Disapprove)
                    {
                        schedulingEntity.Status = SchedulingStatus.AppointmentRefused;

                        string title = "Agendamento recusado pela Oficina";
                        var message = new StringBuilder();

                        message.AppendLine($"Seu agendamento na data {schedulingEntity.Date.MapUnixTime("dd/MM/yyyy HH:mm", "-")}, com a oficina {schedulingEntity.Workshop.CompanyName}, foi recusado.<br/>");
                        message.AppendLine("Agende com outro estabelecimento pelo app.<br/>");
                        await SendProfileNotification(title, message, schedulingEntity.Profile.Id, schedulingEntity.Workshop);
                    }
                }

                if (_access.TypeToken == (int)TypeProfile.Profile)
                {
                    if (schedulingEntity.Profile.Id != _access.UserId)
                    {
                        CreateNotification(DefaultMessages.NotPermission);
                        return null;
                    }

                    if (schedulingEntity.Status != SchedulingStatus.SuggestedTime)
                    {
                        CreateNotification(DefaultMessages.NotPermission);
                        return null;
                    }

                    if (model.ConfirmSchedulingStatus == ConfirmStatus.Approved)
                    {
                        schedulingEntity.Status = SchedulingStatus.Scheduled;
                        schedulingEntity.Date = (long)schedulingEntity.SuggestedDate;

                        string title = "Agendamento confirmado pelo cliente";
                        var message = new StringBuilder();

                        message.AppendLine($"O agendamento de n° {schedulingEntity.GetStringId()} foi confirmado com sucesso pelo cliente {schedulingEntity.Profile.FullName}.<br/>");
                        message.AppendLine($"Data e Horário: {schedulingEntity.Date.MapUnixTime("dd/MM/yyyy HH:mm", "-")}<br/>");
                        await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);

                        string titleProfile = "Agendamento confirmado";
                        var messageProfile = new StringBuilder();

                        message.AppendLine($"O agendamento de n° {schedulingEntity.GetStringId()} foi confirmado com sucesso.<br/>");
                        message.AppendLine($"Data e Horário: {schedulingEntity.Date.MapUnixTime("dd/MM/yyyy HH:mm", "-")}<br/>");
                        message.AppendLine($"Endereço da Oficina: {workshopEntity.StreetAddress}, nº{workshopEntity.Number} - {workshopEntity.Neighborhood} - {workshopEntity.CityName} - {workshopEntity.StateUf}, {workshopEntity.ZipCode} - Complemento: {workshopEntity.Complement} <br/>");
                        await SendProfileNotification(titleProfile, messageProfile, schedulingEntity.Profile.Id, schedulingEntity.Workshop);
                    }

                    if (model.ConfirmSchedulingStatus == ConfirmStatus.Disapprove)
                    {
                        schedulingEntity.Status = SchedulingStatus.AppointmentRefused;

                        string title = "Agendamento recusado pelo cliente";
                        var message = new StringBuilder();

                        message.AppendLine($"O agendamento sugerido na data {schedulingEntity.SuggestedDate.MapUnixTime("dd/MM/yyyy HH:mm", "-")}, foi recusado pelo cliente {schedulingEntity.Profile.FullName}.<br/>");
                        await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);
                    }
                }

                schedulingEntity = await _schedulingRepository.UpdateAsync(schedulingEntity);

                await RegisterSchedulingHistory(schedulingEntity);

                return _mapper.Map<SchedulingViewModel>(schedulingEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SchedulingViewModel> SendBudget(SendBudgetViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                if (schedulingEntity.Status != SchedulingStatus.WaitingForBudget)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (schedulingEntity.Workshop.Id != _access.UserId)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                schedulingEntity.Status = SchedulingStatus.BudgetSent;

                await RegisterSchedulingHistory(schedulingEntity);

                schedulingEntity.BudgetServices = _mapper.Map<List<BudgetServicesAux>>(model.BudgetServices);
                schedulingEntity.Status = SchedulingStatus.WaitingForBudgetApproval;
                schedulingEntity.DiagnosticValue = model.DiagnosticValue;
                schedulingEntity.BudgetImages = model.BudgetImages;
                schedulingEntity.EstimatedTimeForCompletion = model.EstimatedTimeForCompletion;
                schedulingEntity.TotalValue = schedulingEntity.BudgetServices.Sum(x => x.Value) + model.DiagnosticValue;
                await _schedulingRepository.UpdateAsync(schedulingEntity);

                string title = "Orçamento recebido";
                var message = new StringBuilder();
                message.AppendLine($"A Oficina {schedulingEntity.Workshop.CompanyName}, te enviou um orçamento referente ao seu agendamento de n°{schedulingEntity.GetStringId()}<br/>");
                message.AppendLine($"Valor do diagnóstico: {schedulingEntity.DiagnosticValue?.ToString("C", CultureInfo.CurrentCulture)}<br/>");
                message.AppendLine($"Data estimada para conclusão: {schedulingEntity.EstimatedTimeForCompletion.MapUnixTime("dd/MM/yyyy HH:mm", "-")}<br/>");
                message.AppendLine($"Valor total: {schedulingEntity.TotalValue?.ToString("C", CultureInfo.CurrentCulture)}<br/>");
                await SendProfileNotification(title, message, schedulingEntity.Profile.Id, schedulingEntity.Workshop);

                await RegisterSchedulingHistory(schedulingEntity);

                return _mapper.Map<SchedulingViewModel>(schedulingEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SchedulingViewModel> ConfirmBudget(ConfirmBudgetViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                if (schedulingEntity.Status != SchedulingStatus.WaitingForBudgetApproval)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (_access.TypeToken != (int)TypeProfile.Profile)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (schedulingEntity.Profile.Id != _access.UserId)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (model.ConfirmBudgetStatus == ConfirmStatus.Disapprove)
                {
                    schedulingEntity.Status = SchedulingStatus.BudgetDisapprove;
                    schedulingEntity.ExcludedBudgetServices = schedulingEntity.BudgetServices;

                    string title = "Orçamento reprovado";
                    var message = new StringBuilder();

                    message.AppendLine($"O cliente {schedulingEntity.Profile.FullName}, do veículo de placa {schedulingEntity.Vehicle.Plate}, reprovou o orçamento referente ao agendamento de n°{schedulingEntity.GetStringId()}.<br/>");
                    await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);
                }

                if (model.ConfirmBudgetStatus == ConfirmStatus.Approved)
                {
                    if (model.BudgetServices == null || model.BudgetServices.Count == 0)
                    {
                        CreateNotification(DefaultMessages.BudgetServicesEmpty);
                        return null;
                    }

                    schedulingEntity.BudgetApprovalDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    schedulingEntity.PaymentStatus = PaymentStatus.Pending;

                    schedulingEntity.MaintainedBudgetServices = _mapper.Map<List<BudgetServicesAux>>(
                        schedulingEntity.BudgetServices.Where(ws => model.BudgetServices.Any(mws => mws.Title == ws.Title))
                    );

                    schedulingEntity.ExcludedBudgetServices = _mapper.Map<List<BudgetServicesAux>>(
                        schedulingEntity.BudgetServices.Where(ws => !model.BudgetServices.Any(mws => mws.Title == ws.Title))
                    );

                    schedulingEntity.TotalValue = schedulingEntity.MaintainedBudgetServices.Sum(x => x.Value) + schedulingEntity.DiagnosticValue;
                    schedulingEntity.Status = model.BudgetServices.Count == schedulingEntity.BudgetServices.Count ? SchedulingStatus.BudgetApproved : SchedulingStatus.BudgetPartiallyApproved;

                    await RegisterSchedulingHistory(schedulingEntity);

                    schedulingEntity.Status = SchedulingStatus.WaitingForPayment;

                    // Removido 13/05
                    // string title = "Orçamento aprovado";
                    // var message = new StringBuilder();

                    // message.AppendLine($"O cliente {schedulingEntity.Profile.FullName}, do veículo de placa {schedulingEntity.Vehicle.Plate}, aprovou o orçamento referente ao agendamento de n°{schedulingEntity.GetStringId()}.<br/>");
                    // await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);
                }

                await _schedulingRepository.UpdateAsync(schedulingEntity);

                await RegisterSchedulingHistory(schedulingEntity);

                return _mapper.Map<SchedulingViewModel>(schedulingEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SchedulingViewModel> ConfirmService(ConfirmServiceViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                if (_access.TypeToken != (int)TypeProfile.Profile)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                if (schedulingEntity.Status != SchedulingStatus.WaitingForServiceApproval)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (schedulingEntity.Profile.Id != _access.UserId)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                if (model.ConfirmServiceStatus == ConfirmStatus.Disapprove)
                {
                    schedulingEntity.ReasonDisapproval = model.ReasonDisapproval;
                    schedulingEntity.ImagesDisapproval = model.ImagesDisapproval;
                    schedulingEntity.Status = SchedulingStatus.ServiceReprovedByUser;

                    string title = "Serviço reprovado pelo cliente";
                    StringBuilder message = new StringBuilder();
                    message.AppendLine($"O Serviço foi reprovado pelo cliente, referente ao agendamento de n° {schedulingEntity.GetStringId()}.<br/>");
                    message.AppendLine($"Motivo do cliente: {model.ReasonDisapproval}.<br/>");
                    await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);
                }

                if (model.ConfirmServiceStatus == ConfirmStatus.Approved)
                {
                    schedulingEntity.Status = SchedulingStatus.ServiceApprovedByUser;

                    await RegisterSchedulingHistory(schedulingEntity);

                    schedulingEntity.Status = SchedulingStatus.ServiceFinished;

                    string title = "Serviço aprovado pelo cliente";
                    StringBuilder message = new StringBuilder();
                    message.AppendLine($"O Serviço foi aprovado com sucesso, referente ao agendamento de n° {schedulingEntity.GetStringId()}.<br/>");
                    await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);
                }

                await _schedulingRepository.UpdateAsync(schedulingEntity);

                await RegisterSchedulingHistory(schedulingEntity);

                return _mapper.Map<SchedulingViewModel>(schedulingEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SuggestFreeRepair(string schedulingId)
        {
            try
            {
                if (_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(schedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return false;
                }

                if (schedulingEntity.Workshop.Id != _access.UserId)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                schedulingEntity.FreeRepair = true;
                schedulingEntity.Status = SchedulingStatus.WaitingAppointment;
                schedulingEntity.AwaitFreeRepairScheduling = true;

                await _schedulingRepository.UpdateAsync(schedulingEntity);

                string title = "Reparação gratuita";
                StringBuilder message = new StringBuilder();
                message.AppendLine($"Pedimos desculpas pelo inconveniente, o estabelecimento {schedulingEntity.Workshop.CompanyName} gostaria de realizar o reparo gratuito do serviço realizado, referente ao agendamento de n° {schedulingEntity.GetStringId()}.<br/>");
                await SendProfileNotification(title, message, schedulingEntity.Profile.Id, schedulingEntity.Workshop);

                // Registrar histórico de agendamento
                await _schedulingHistoryService.Register(new SchedulingHistoryViewModel()
                {
                    StatusTitle = Util.GetStatusTitle(schedulingEntity.Status),
                    Status = schedulingEntity.Status,
                    Description = "A Oficina sugeriu reparo gratuito",
                    SchedulingId = schedulingEntity.GetStringId()
                });

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SchedulingViewModel> RegisterRepairAppointment(SchedulingViewModel model)
        {
            try
            {
                if (_access.TypeToken != (int)TypeProfile.Profile)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (string.IsNullOrEmpty(model.Id) == true)
                {
                    CreateNotification("Por favor, informe o ID do agendamento.");
                    return null;
                }

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.Id);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                if (schedulingEntity.Status != SchedulingStatus.WaitingAppointment && schedulingEntity.FreeRepair == false)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (schedulingEntity.Profile.Id != _access.UserId)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                var schedulingExist = await _schedulingRepository.FindOneByAsync(x => x.Date == model.Date && (int)x.Status <= (int)SchedulingStatus.Scheduled);
                if (schedulingExist != null)
                {
                    CreateNotification(DefaultMessages.SchedulingInUse);
                    return null;
                }

                var minTimeSpan = await CalculateMinTimeScheduling(_mapper.Map<List<WorkshopServicesAuxViewModel>>(schedulingEntity.MaintainedBudgetServices));

                // Converte epoch para DateTime (UTC)
                DateTime utcDate = DateTimeOffset.FromUnixTimeSeconds(model.Date).UtcDateTime;

                // Define o fuso horário GMT-3
                TimeZoneInfo gmtMinus3 = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime schedulingDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, gmtMinus3);

                // Pegando data atual
                DateTime todayDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gmtMinus3);

                if (schedulingDate.Date == todayDate.Date && schedulingDate.TimeOfDay < todayDate.TimeOfDay.Add(minTimeSpan))
                {
                    CreateNotification("Horário inválido! Por favor, selecione um horário que respeite o horário mínimo para agendamento.");
                    return null;
                }

                if (schedulingDate.Date < todayDate.Date)
                {
                    CreateNotification("Data inválida! Por favor, selecione uma data válida.");
                    return null;
                }

                schedulingEntity.Date = model.Date;
                schedulingEntity.AwaitFreeRepairScheduling = false;

                schedulingEntity = await _schedulingRepository.UpdateAsync(schedulingEntity);

                await RegisterSchedulingHistory(schedulingEntity);

                var vehicleEntity = await _vehicleRepository.FindByIdAsync(schedulingEntity.Vehicle.Id);

                string title = "Novo agendamento (reparo gratuito)";
                StringBuilder message = new StringBuilder();
                message.AppendLine($"Você possui um novo agendamento para reparo gratuito.<br/>");
                message.AppendLine($"Data: {todayDate.ToUnix().MapUnixTime("dd/MM/yyyy HH:mm", "-")}<br/><br/>");
                message.AppendLine("<p>Informações do veículo abaixo<br/>");
                message.AppendLine($"<p>Placa: {vehicleEntity.Plate}<br/>");
                message.AppendLine($"<p>Fabricante: {vehicleEntity.Manufacturer}<br/>");
                message.AppendLine($"<p>Modelo: {vehicleEntity.Model}<br/>");
                message.AppendLine($"<p>Quilometragem: {vehicleEntity.Km}<br/>");
                message.AppendLine($"<p>Cor: {vehicleEntity.Color}<br/>");
                message.AppendLine($"<p>Ano: {vehicleEntity.Year}<br/>");
                message.AppendLine($"<p>Data da última revisão: {vehicleEntity.LastRevisionDate}<br/>");
                await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);

                return _mapper.Map<SchedulingViewModel>(schedulingEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DisputeDisapprovedService(DisputeDisapprovedServiceViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return false;

                if (_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return false;
                }

                if (schedulingEntity.Workshop.Id != _access.UserId)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                if (schedulingEntity.Status != SchedulingStatus.ServiceReprovedByUser)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                schedulingEntity.Status = SchedulingStatus.WorkshopDispute;
                schedulingEntity.Dispute = model.Dispute;
                schedulingEntity.ImagesDispute = model.ImagesDispute;

                schedulingEntity = await _schedulingRepository.UpdateAsync(schedulingEntity);

                await RegisterSchedulingHistory(schedulingEntity);

                string title = "Contestação de serviço pela oficina";
                StringBuilder message = new StringBuilder();
                message.AppendLine($"A oficina {schedulingEntity.Workshop.CompanyName} contestou sua reprovação referente ao agendamento de n° {schedulingEntity.GetStringId()}<br/>");
                await SendProfileNotification(title, message, schedulingEntity.Profile.Id, schedulingEntity.Workshop);

                var sendPushViewModel = new SendPushViewModel()
                {
                    Title = "Contestação de serviço pela oficina",
                    Content = $"A oficina {schedulingEntity.Workshop.CompanyName} contestou a reprovação do serviço, pelo usuário {schedulingEntity.Profile.FullName} referente ao agendamento de n° {schedulingEntity.GetStringId()}",
                    TypeProfile = TypeProfile.UserAdministrator,
                    TargetId = [],
                };

                await _notificationService.SendAndRegisterNotification(sendPushViewModel, schedulingEntity.Workshop, schedulingEntity.Profile, schedulingEntity.GetStringId());

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SchedulingViewModel> ChangeSchedulingStatus(ChangeSchedulingStatusViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return null;

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return null;
                }

                if (_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                if (schedulingEntity.Workshop.Id != _access.UserId)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return null;
                }

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                string title = "";
                StringBuilder message = new StringBuilder();
                TypeProfile typeProfile = TypeProfile.Profile;

                switch (model.Status)
                {
                    case SchedulingStatus.DidNotAttend:
                        if (schedulingEntity.Status != SchedulingStatus.Scheduled)
                        {
                            CreateNotification(DefaultMessages.NotPermission);
                            return null;
                        }

                        schedulingEntity.Status = SchedulingStatus.DidNotAttend;

                        title = "Cliente não compareceu";
                        message.Clear();

                        message.AppendLine($"A oficina {schedulingEntity.Workshop.CompanyName} marcou que você não compareceu a oficina referente ao agendamento de n° {schedulingEntity.GetStringId()}<br/>");

                        break;
                    case SchedulingStatus.ScheduleCompleted:
                        if (schedulingEntity.Status != SchedulingStatus.Scheduled)
                        {
                            CreateNotification(DefaultMessages.NotPermission);
                            return null;
                        }

                        schedulingEntity.Status = SchedulingStatus.ScheduleCompleted;

                        await RegisterSchedulingHistory(schedulingEntity);

                        if (schedulingEntity.FreeRepair == true)
                        {
                            schedulingEntity.Status = SchedulingStatus.WaitingStart;
                        }
                        else
                        {
                            schedulingEntity.Status = SchedulingStatus.WaitingForBudget;

                            title = "Cliente aguardando orçamento";
                            message.Clear();
                            message.AppendLine($"O cliente {schedulingEntity.Profile.FullName}, do veículo de placa {schedulingEntity.Vehicle.Plate}, está aguardando o orçamento referente ao agendamento de n° {schedulingEntity.GetStringId()}<br/>");
                            typeProfile = TypeProfile.Workshop;
                        }

                        break;
                    case SchedulingStatus.WaitingForPart:
                        if (schedulingEntity.Status != SchedulingStatus.WaitingStart)
                        {
                            CreateNotification(DefaultMessages.NotPermission);
                            return null;
                        }

                        schedulingEntity.Status = SchedulingStatus.WaitingForPart;

                        title = "Oficina aguardando peças";
                        message.Clear();
                        message.AppendLine($"A oficina {schedulingEntity.Workshop.CompanyName} está aguardando peças para concluir o serviço do seu veículo referente ao agendamento de n° {schedulingEntity.GetStringId()}.<br/>");

                        break;
                    case SchedulingStatus.ServiceInProgress:
                        if (schedulingEntity.Status != SchedulingStatus.WaitingStart && schedulingEntity.Status != SchedulingStatus.WaitingForPart)
                        {
                            CreateNotification(DefaultMessages.NotPermission);
                            return null;
                        }

                        schedulingEntity.Status = SchedulingStatus.ServiceInProgress;
                        schedulingEntity.ServiceStartDate = now;

                        title = "Serviço em andamento";
                        message.Clear();
                        message.AppendLine($"O serviço referente ao agendamento de n° {schedulingEntity.GetStringId()}, está em andamento.<br/>");

                        break;
                    case SchedulingStatus.ServiceCompleted:
                        if ((int)schedulingEntity.Status < (int)SchedulingStatus.ServiceInProgress && (int)schedulingEntity.Status > (int)SchedulingStatus.WaitingForPart)
                        {
                            CreateNotification(DefaultMessages.NotPermission);
                            return null;
                        }

                        schedulingEntity.Status = SchedulingStatus.ServiceCompleted;

                        await RegisterSchedulingHistory(schedulingEntity);

                        schedulingEntity.Status = SchedulingStatus.WaitingForServiceApproval;
                        schedulingEntity.ServiceEndDate = now;

                        title = "Serviço concluído e aguardando aprovação";
                        message.Clear();
                        message.AppendLine($"O Serviço do seu veículo foi concluído e a Oficina {schedulingEntity.Workshop.CompanyName}, está aguardando a sua aprovação.<br/>");
                        message.AppendLine($"Data de conclusão: {now.MapUnixTime("dd/MM/yyyy HH:mm", "-")}.<br/>");

                        break;
                    default:
                        CreateNotification(DefaultMessages.NotPermission);
                        break;
                }

                schedulingEntity = await _schedulingRepository.UpdateAsync(schedulingEntity);

                await RegisterSchedulingHistory(schedulingEntity);

                if (typeProfile == TypeProfile.Profile)
                {
                    await SendProfileNotification(title, message, schedulingEntity.Profile.Id, schedulingEntity.Workshop);
                }
                else
                {
                    await SendWorkshopNotification(title, message, schedulingEntity.Workshop.Id, schedulingEntity.Profile);
                }

                return _mapper.Map<SchedulingViewModel>(schedulingEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ApproveOrReproveService(ApproveOrReproveServiceViewModel model)
        {
            try
            {
                if (ModelIsValid(model, false) == false)
                    return false;

                var schedulingEntity = await _schedulingRepository.FindByIdAsync(model.SchedulingId);
                if (schedulingEntity == null)
                {
                    CreateNotification(DefaultMessages.SchedulingNotFound);
                    return false;
                }

                if (_access.TypeToken != (int)TypeProfile.UserAdministrator)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                var userAdministratorEntity = await _userAdministratorRepository.FindByIdAsync(_access.UserId);
                if (userAdministratorEntity == null)
                {
                    CreateNotification(DefaultMessages.UserAdministratorNotFound);
                    return false;
                }

                if (schedulingEntity.Status != SchedulingStatus.WorkshopDispute)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                schedulingEntity.ServiceFinishedByAdmin = true;
                schedulingEntity.UserAdministrator = _mapper.Map<UserAdministratorAux>(userAdministratorEntity);

                if (model.ConfirmStatus == ConfirmStatus.Approved)
                {
                    if (model.BudgetServices.Count == 0)
                    {
                        CreateNotification("Para aprovar selecione ao menos um serviço.");
                        return false;
                    }

                    if (schedulingEntity.MaintainedBudgetServices.Count == model.BudgetServices.Count)
                    {
                        // TODO - PAGAMENTO
                        // O serviço será aprovado e o pagamento será liberado para o estabelecimento

                        schedulingEntity.Status = SchedulingStatus.ServiceApprovedByAdmin;
                        schedulingEntity.BudgetServicesApprovedByAdmin = schedulingEntity.MaintainedBudgetServices;

                        string title = "Serviço aprovado pelo administrador";
                        StringBuilder message = new StringBuilder();
                        message.AppendLine($"O serviço referente ao agendamento de n° {schedulingEntity.GetStringId()}, foi realizado corretamente e aprovado pelo Administrador Meca {userAdministratorEntity.Name}.<br/>");
                        message.AppendLine($"Para mais informações, entre em contato com o Administrador.<br/>");
                        await SendProfileNotification(title, message, schedulingEntity.Profile.Id, schedulingEntity.Workshop);

                        await RegisterSchedulingHistory(schedulingEntity);

                        schedulingEntity.Status = SchedulingStatus.ServiceFinished;
                    }
                    else
                    {
                        // TODO - PAGAMENTO
                        // O valor será liberado para o estabelecimento de acordo com os serviços aprovados. O restante do valor será reembolsado para o cliente de acordo com a forma de pagamento realizada

                        schedulingEntity.Status = SchedulingStatus.ServiceApprovedPartiallyByAdmin;
                        schedulingEntity.BudgetServicesApprovedByAdmin = _mapper.Map<List<BudgetServicesAux>>(model.BudgetServices);

                        var totalValueToWorkshop = schedulingEntity.BudgetServicesApprovedByAdmin.Sum(x => x.Value);
                        var totalRefundToProfile = schedulingEntity.TotalValue - totalValueToWorkshop;

                        var title = "Serviço aprovado parcialmente pelo administrador";

                        StringBuilder messageProfile = new StringBuilder();
                        messageProfile.AppendLine($"O serviço referente ao agendamento de n° {schedulingEntity.GetStringId()}, foi aprovado parcialmente pelo Administrador Meca {userAdministratorEntity.Name}.<br/>");
                        messageProfile.AppendLine($"Valor a ser reembolsado: {totalRefundToProfile?.ToString("C", CultureInfo.CurrentCulture)}.<br/>");
                        messageProfile.AppendLine($"Para mais informações, entre em contato com o Administrador.<br/>");
                        await SendProfileNotification(title, messageProfile, schedulingEntity.Profile.Id, schedulingEntity.Workshop);

                        StringBuilder messageWorkshop = new StringBuilder();
                        messageWorkshop.AppendLine($"O serviço referente ao agendamento de n° {schedulingEntity.GetStringId()}, foi aprovado parcialmente pelo Administrador Meca {userAdministratorEntity.Name}.<br/>");
                        messageWorkshop.AppendLine($"Valor a receber parcialmente: {totalValueToWorkshop.ToString("C", CultureInfo.CurrentCulture)}.<br/>");
                        messageWorkshop.AppendLine($"Para mais informações, entre em contato com o Administrador.<br/>");
                        await SendWorkshopNotification(title, messageWorkshop, schedulingEntity.Workshop.Id, schedulingEntity.Profile);

                        await RegisterSchedulingHistory(schedulingEntity);

                        schedulingEntity.Status = SchedulingStatus.ServiceFinished;
                    }
                }

                if (model.ConfirmStatus == ConfirmStatus.Disapprove)
                {
                    // TODO - PAGAMENTO
                    // o pagamento não será liberado para o estabelecimento e os valores pagos pelo cliente serão estornados de acordo com a forma de pagamento realizada

                    schedulingEntity.Status = SchedulingStatus.ServiceReprovedByAdmin;

                    var titleWorkshop = "Serviço reprovado pelo administrador";
                    StringBuilder messageWorkshop = new StringBuilder();
                    messageWorkshop.Clear();
                    messageWorkshop.AppendLine($"O serviço referente ao agendamento de n° {schedulingEntity.GetStringId()}, foi reprovado pelo Administrador Meca {userAdministratorEntity.Name}.<br/>");
                    messageWorkshop.AppendLine($"Para mais informações, entre em contato com o Administrador.<br/>");
                    await SendWorkshopNotification(titleWorkshop, messageWorkshop, schedulingEntity.Workshop.Id, schedulingEntity.Profile);

                    var titleProfile = "Pagamento estornado ao cliente";
                    StringBuilder messageProfile = new StringBuilder();
                    messageProfile.Clear();
                    messageProfile.AppendLine($"O pagamento do serviço referente ao agendamento de n° {schedulingEntity.GetStringId()}, foi estornado a você.<br/>");
                    await SendProfileNotification(titleProfile, messageProfile, schedulingEntity.Profile.Id, schedulingEntity.Workshop);

                    await RegisterSchedulingHistory(schedulingEntity);

                    schedulingEntity.Status = SchedulingStatus.ServiceFinished;
                }

                schedulingEntity = await _schedulingRepository.UpdateAsync(schedulingEntity);

                await RegisterSchedulingHistory(schedulingEntity);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RegisterSchedulingHistory(Scheduling schedulingEntity)
        {
            // Registrar histórico de agendamento
            await _schedulingHistoryService.Register(new SchedulingHistoryViewModel()
            {
                StatusTitle = Util.GetStatusTitle(schedulingEntity.Status),
                Status = schedulingEntity.Status,
                Description = schedulingEntity.Status.GetEnumMemberValue(),
                SchedulingId = schedulingEntity.GetStringId()
            });
        }

        public async Task SendProfileNotification(string title, StringBuilder message, string profileId, WorkshopAux workshop)
        {
            _ = Task.Run(async () =>
                            {
                                //  Enviar notificação (Email)
                                var profileEntity = await _profileRepository.FindByIdAsync(profileId);

                                var subject = title;
                                var dataBody = Util.GetTemplateVariables();

                                message.Insert(0, "<p>Olá <strong>{{ name }}</strong>,<br/>");
                                message.AppendLine(Util.GetEmailSignature());

                                dataBody.Add("{{ name }}", profileEntity.FullName);
                                dataBody.Add("{{ email }}", profileEntity.Email);
                                dataBody.Add("{{ title }}", title);
                                dataBody.Add("{{ message }}", message.ToString().ReplaceTag(dataBody));

                                var body = _senderMailService.GerateBody("custom", dataBody);

                                var unused = Task.Run(async () =>
                                {
                                    await _senderMailService.SendMessageEmailAsync(
                                        BaseConfig.ApplicationName,
                                        profileEntity.Email,
                                        body,
                                        subject);
                                });

                                //  Enviar notificação (Push)
                                var content = Regex.Replace(Regex.Replace(message.ToString().ReplaceTag(dataBody), "<.*?>", string.Empty), @"\r\n|\r|\n", " ");
                                var sendPushViewModel = new SendPushViewModel()
                                {
                                    Title = title,
                                    Content = content,
                                    TypeProfile = TypeProfile.Profile,
                                    TargetId = [profileEntity.GetStringId()],
                                };

                                await _notificationService.SendAndRegisterNotification(sendPushViewModel, workshop: workshop);
                            });
        }

        public async Task SendWorkshopNotification(string title, StringBuilder message, string workshopId, ProfileAux profile)
        {
            _ = Task.Run(async () =>
                            {
                                //  Enviar notificação (Email)
                                var workshopEntity = await _workshopRepository.FindByIdAsync(workshopId);

                                var subject = title;
                                var dataBody = Util.GetTemplateVariables();

                                message.Insert(0, "<p>Olá <strong>{{ name }}</strong>,<br/>");
                                message.AppendLine(Util.GetEmailSignature());

                                dataBody.Add("{{ name }}", workshopEntity.CompanyName);
                                dataBody.Add("{{ email }}", workshopEntity.Email);
                                dataBody.Add("{{ title }}", title);
                                dataBody.Add("{{ message }}", message.ToString().ReplaceTag(dataBody));

                                var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                                var body = _senderMailService.GerateBody("custom", dataBody);

                                var unused = Task.Run(async () =>
                                {
                                    await _senderMailService.SendMessageEmailAsync(
                                        BaseConfig.ApplicationName,
                                        workshopEntity.Email,
                                        body,
                                        subject);
                                });

                                //  Enviar notificação (Push)
                                var content = Regex.Replace(Regex.Replace(message.ToString().ReplaceTag(dataBody), "<.*?>", string.Empty), @"\r\n|\r|\n", " ");
                                var sendPushViewModel = new SendPushViewModel()
                                {
                                    Title = title,
                                    Content = content,
                                    TypeProfile = TypeProfile.Workshop,
                                    TargetId = [workshopEntity.GetStringId()],
                                };

                                await _notificationService.SendAndRegisterNotification(sendPushViewModel, profile: profile);
                            });
        }

        public async Task<CalendarAvailableViewModel> GetAvailableScheduling(SchedulingAvailableFilterViewModel filterModel)
        {
            try
            {
                Console.WriteLine($"[GetAvailableScheduling] Iniciando - WorkshopId: {filterModel?.WorkshopId}, Date: {filterModel?.Date}");
                
                var workshopId = "";

                if (ModelIsValid(filterModel, false) == false)
                    return null;

                if (filterModel.Date == 0)
                {
                    CreateNotification("Por favor, informe uma data.");
                    return null;
                }

                if (_access.TypeToken == (int)TypeProfile.Profile)
                {
                    if (string.IsNullOrEmpty(filterModel.WorkshopId))
                    {
                        CreateNotification("Por favor, informe o ID da Oficina.");
                        return null;
                    }

                    var workshopEntity = await _workshopRepository.FindByIdAsync(filterModel.WorkshopId);
                    if (workshopEntity == null)
                    {
                        CreateNotification(DefaultMessages.WorkshopNotFound);
                        return null;
                    }

                    workshopId = workshopEntity.GetStringId();

                    if (filterModel.Services.Any() == false)
                    {
                        CreateNotification("Por favor, informe os serviços que deseja realizar.");
                        return null;
                    }
                }
                else
                {
                    workshopId = _access.UserId;
                }

                // Converte epoch para DateTime (UTC)
                DateTime dateUtc = DateTimeOffset.FromUnixTimeSeconds(filterModel.Date).UtcDateTime;

                // Define o fuso horário GMT-3
                TimeZoneInfo gmtMinus3 = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime date = TimeZoneInfo.ConvertTimeFromUtc(dateUtc, gmtMinus3);
                
                Console.WriteLine($"[GetAvailableScheduling] Data convertida: {date:yyyy-MM-dd HH:mm:ss}, WorkshopId: {workshopId}");

                // Busca dados do banco
                var schedulingList = (List<Scheduling>)await _schedulingRepository.FindByAsync(x =>
                                        x.Workshop.Id == workshopId &&
                                        (int)x.Status >= (int)SchedulingStatus.Scheduled);

                if (_access.TypeToken == (int)TypeProfile.Profile)
                {
                    schedulingList = (List<Scheduling>)schedulingList.Where(x => DateTimeOffset.FromUnixTimeSeconds(x.Date).Date == date.Date).ToList();
                }
                else
                {
                    schedulingList = (List<Scheduling>)schedulingList.Where(x => DateTimeOffset.FromUnixTimeSeconds(x.Date).Date == date.Date).ToList();
                }

                var workshopAgendaEntity = await _workshopAgendaRepository.FindOneByAsync(x => x.Workshop.Id == workshopId);
                if (workshopAgendaEntity == null)
                {
                    Console.WriteLine($"[GetAvailableScheduling] Agenda não configurada para oficina: {workshopId}");
                    // Retornar agenda vazia em vez de null para evitar erro no app
                    return new CalendarAvailableViewModel()
                    {
                        Date = date.Date,
                        DayOfWeek = date.DayOfWeek,
                        Available = false,
                        Hours = new List<string>(),
                        WorkshopAgenda = new List<string>()
                    };
                }

                // Calcula o tempo mínimo de agendamento
                TimeSpan minTimeScheduling = await CalculateMinTimeScheduling(filterModel.Services);

                var calendarAvailable = new CalendarAvailableViewModel()
                {
                    Date = date.Date,
                    DayOfWeek = date.DayOfWeek
                };

                calendarAvailable = await UpdateCalendarWithWorkshopAgenda(calendarAvailable, workshopAgendaEntity);

                calendarAvailable = await ApplySchedulingFilters(calendarAvailable, schedulingList, minTimeScheduling, gmtMinus3, _access.TypeToken, workshopId);

                Console.WriteLine($"[GetAvailableScheduling] Resultado - Available: {calendarAvailable.Available}, Hours: {calendarAvailable.Hours.Count}");
                
                return calendarAvailable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAvailableScheduling] Erro: {ex.Message}");
                Console.WriteLine($"[GetAvailableScheduling] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private static async Task<TimeSpan> CalculateMinTimeScheduling(List<WorkshopServicesAuxViewModel> services)
        {
            if (services == null || services.Count == 0)
            {
                return TimeSpan.Zero; // Retorna 00:00 se não houver serviços
            }

            TimeSpan minTime = TimeSpan.Zero;

            foreach (var service in services)
            {
                double minTimes = service.MinTimeScheduling;
                int hours = (int)minTimes; // Parte inteira são as horas
                int minutes = (int)((minTimes - hours) * 60); // Parte decimal convertida para minutos
                TimeSpan currentMinTime = new TimeSpan(hours, minutes, 0);

                if (currentMinTime > minTime)
                {
                    minTime = currentMinTime;
                }
            }

            return minTime;
        }

        private async Task<CalendarAvailableViewModel> UpdateCalendarWithWorkshopAgenda(CalendarAvailableViewModel day, WorkshopAgenda workshopAgenda)
        {
            try
            {
                var agenda = GetAgendaForDay(workshopAgenda, day.DayOfWeek);
                if (agenda != null && agenda.Open && !string.IsNullOrEmpty(agenda.StartTime) && !string.IsNullOrEmpty(agenda.ClosingTime))
                {
                    day.Available = true;
                    day.Hours = GenerateSchedule(agenda.StartTime, agenda.ClosingTime, agenda.StartOfBreak, agenda.EndOfBreak);
                    Console.WriteLine($"[UpdateCalendarWithWorkshopAgenda] Agenda configurada para {day.DayOfWeek} - {day.Hours.Count} horários disponíveis");
                }
                else
                {
                    day.Available = false;
                    day.Hours.Clear();
                    day.WorkshopAgenda.Clear();
                    Console.WriteLine($"[UpdateCalendarWithWorkshopAgenda] Agenda não configurada para {day.DayOfWeek}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateCalendarWithWorkshopAgenda] Erro: {ex.Message}");
                day.Available = false;
                day.Hours.Clear();
                day.WorkshopAgenda.Clear();
            }

            return day;
        }

        private WorkshopAgendaAux GetAgendaForDay(WorkshopAgenda workshopAgenda, DayOfWeek dayOfWeek)
        {
            if (workshopAgenda == null)
                return null;

            return dayOfWeek switch
            {
                DayOfWeek.Sunday => workshopAgenda.Sunday,
                DayOfWeek.Monday => workshopAgenda.Monday,
                DayOfWeek.Tuesday => workshopAgenda.Tuesday,
                DayOfWeek.Wednesday => workshopAgenda.Wednesday,
                DayOfWeek.Thursday => workshopAgenda.Thursday,
                DayOfWeek.Friday => workshopAgenda.Friday,
                DayOfWeek.Saturday => workshopAgenda.Saturday,
                _ => null
            };
        }

        static List<string> GenerateSchedule(string startTime, string closingTime, string startOfBreak, string endOfBreak)
        {
            var schedule = new List<string>();

            if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(closingTime))
            {
                Console.WriteLine($"[GenerateSchedule] Horários vazios - startTime: '{startTime}', closingTime: '{closingTime}'");
                return schedule;
            }

            try
            {
                // Validar se os horários são válidos antes de fazer o parse
                if (!TimeSpan.TryParse(startTime, out _) || !TimeSpan.TryParse(closingTime, out _))
                {
                    Console.WriteLine($"[GenerateSchedule] Horários inválidos - startTime: '{startTime}', closingTime: '{closingTime}'");
                    return schedule;
                }
                
                // Converter strings de tempo para DateTime
                DateTime start = DateTime.Parse(startTime);
                DateTime close = DateTime.Parse(closingTime);
                
                // Verificar se há intervalo de pausa configurado
                bool hasBreak = !string.IsNullOrEmpty(startOfBreak) && !string.IsNullOrEmpty(endOfBreak);
                DateTime breakStart = DateTime.MinValue;
                DateTime breakEnd = DateTime.MinValue;
                
                if (hasBreak)
                {
                    // Validar se os horários de pausa são válidos
                    if (!TimeSpan.TryParse(startOfBreak, out _) || !TimeSpan.TryParse(endOfBreak, out _))
                    {
                        Console.WriteLine($"[GenerateSchedule] Horários de pausa inválidos - startOfBreak: '{startOfBreak}', endOfBreak: '{endOfBreak}'");
                        hasBreak = false; // Desabilitar pausa se os horários forem inválidos
                    }
                    else
                    {
                        breakStart = DateTime.Parse(startOfBreak);
                        breakEnd = DateTime.Parse(endOfBreak);
                    }
                }

                // Iterar de 30 em 30 minutos
                var time = 30;
                for (DateTime current = start; current < close; current = current.AddMinutes(time))
                {
                    // Ignorar o intervalo de pausa apenas se estiver configurado
                    if (hasBreak && current >= breakStart && current < breakEnd)
                    {
                        continue;
                    }

                    var timeString = current.ToString("HH:mm");
                    if (!string.IsNullOrEmpty(timeString))
                    {
                        schedule.Add(timeString);
                    }
                }
                
                Console.WriteLine($"[GenerateSchedule] Agenda gerada com {schedule.Count} horários");
            }
            catch (Exception ex)
            {
                // Log do erro para debug
                Console.WriteLine($"[GenerateSchedule] Erro ao gerar agenda: {ex.Message}");
                Console.WriteLine($"[GenerateSchedule] startTime: '{startTime}', closingTime: '{closingTime}', startOfBreak: '{startOfBreak}', endOfBreak: '{endOfBreak}'");
            }

            return schedule;
        }

        private async Task<CalendarAvailableViewModel> ApplySchedulingFilters(CalendarAvailableViewModel day, IEnumerable<Scheduling> schedulingList, TimeSpan minTimeScheduling, TimeZoneInfo timeZone, int typeToken, string workshopId = null)
        {
            try
            {
                Console.WriteLine($"[ApplySchedulingFilters] Iniciando - Date: {day.Date:yyyy-MM-dd}, Hours: {day.Hours.Count}, TypeToken: {typeToken}");
                
                // Verifica bloqueios extras do `AgendaAux`
                var agendaAuxList = await _agendaAuxRepository.FindByAsync(x => x.WorkshopId == workshopId);
            var blockedTimes = agendaAuxList
                .Select(a =>
                {
                    var agendaDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTimeOffset.FromUnixTimeSeconds(a.Date).UtcDateTime, timeZone);
                    return new
                    {
                        Date = agendaDateTime.Date,
                        Time = agendaDateTime.TimeOfDay
                    };
                })
                .Where(a => a.Date == day.Date.Date)
                .Select(a => a.Time)
                .ToHashSet(); // Melhora a busca

            var currentDate = DateTime.Now;

            if (typeToken == (int)TypeProfile.Profile)
            {
                if (day.Date == currentDate.Date)
                {
                    day.Hours = day.Hours
                                   .Where(hour => !string.IsNullOrEmpty(hour) && TimeSpan.TryParse(hour, out var hourSpan) && hourSpan >= currentDate.TimeOfDay.Add(minTimeScheduling))
                                   .ToList();
                }

                // Remove horários já agendados
                var scheduledHours = schedulingList
                    .Where(s => TimeZoneInfo
                        .ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(s.Date).UtcDateTime, timeZone)
                        .Date == day.Date)
                    .Select(s => TimeZoneInfo
                        .ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(s.Date).UtcDateTime, timeZone)
                        .TimeOfDay)
                    .ToHashSet(); // Melhora a performance na busca

                day.Hours = day.Hours
                    .Where(hour => !string.IsNullOrEmpty(hour) && 
                                   TimeSpan.TryParse(hour, out var hourSpan) && 
                                   !scheduledHours.Contains(hourSpan) && 
                                   !blockedTimes.Contains(hourSpan))
                    .ToList();

                if (day.Date < currentDate.Date)
                {
                    day.Hours.Clear();
                }
            }

            if (typeToken == (int)TypeProfile.Workshop)
            {
                // Criação da agenda inicial
                day.WorkshopAgenda = day.Hours
                    .Where(hour => !string.IsNullOrEmpty(hour))
                    .Select(hour => new WorkshopAgendaHoursViewModel
                    {
                        Hour = hour,
                        Available = day.Date == currentDate.Date && TimeSpan.TryParse(hour, out var hourSpan) && hourSpan >= currentDate.TimeOfDay ? true : day.Date > currentDate.Date ? true : false
                    })
                    .ToList();

                // Atualiza horários baseados nos agendamentos
                var scheduledWorkshops = schedulingList
                    .Select(s =>
                    {
                        var localDate = TimeZoneInfo.ConvertTimeFromUtc(
                            DateTimeOffset.FromUnixTimeSeconds(s.Date).UtcDateTime, timeZone);
                        return new
                        {
                            Date = localDate.Date,
                            Time = localDate.TimeOfDay,
                            Profile = s.Profile,
                            Vehicle = s.Vehicle
                        };
                    })
                    .Where(s => s.Date == day.Date.Date)
                    .ToList();

                for (var y = 0; y < day.WorkshopAgenda.Count; y++)
                {
                    if (TimeSpan.TryParse(day.WorkshopAgenda[y].Hour, out var hourSpan))
                    {
                        var match = scheduledWorkshops.FirstOrDefault(s => s.Time == hourSpan);
                        if (match != null)
                        {
                            day.WorkshopAgenda[y].Available = false;
                            day.WorkshopAgenda[y].Profile = _mapper.Map<ProfileAuxViewModel>(match.Profile);
                            day.WorkshopAgenda[y].Vehicle = _mapper.Map<VehicleAuxViewModel>(match.Vehicle);
                        }

                        if (blockedTimes.Contains(hourSpan))
                        {
                            day.WorkshopAgenda[y].Available = false;
                        }
                    }
                }

                // Manter os horários disponíveis na lista Hours
                day.Hours = day.WorkshopAgenda
                    .Where(x => x.Available)
                    .Select(x => x.Hour)
                    .ToList();
                
                Console.WriteLine($"[ApplySchedulingFilters] Finalizado - WorkshopAgenda: {day.WorkshopAgenda.Count} horários, Hours: {day.Hours.Count}");
            }

            return day;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApplySchedulingFilters] Erro: {ex.Message}");
                day.Available = false;
                day.Hours.Clear();
                day.WorkshopAgenda.Clear();
                return day;
            }
        }

        // Agenda completa
        // public async Task<List<CalendarAvailableViewModel>> GetAvailableScheduling(SchedulingAvailableFilterViewModel filterModel)
        // {
        //     try
        //     {
        //         var workshopId = "";

        //         if (ModelIsValid(filterModel, false) == false)
        //             return null;

        //         if (filterModel.Date == 0)
        //         {
        //             CreateNotification("Por favor, informe uma data.");
        //             return null;
        //         }

        //         if (_access.TypeToken == (int)TypeProfile.Profile)
        //         {
        //             if (string.IsNullOrEmpty(filterModel.WorkshopId))
        //             {
        //                 CreateNotification("Por favor, informe o ID da Oficina.");
        //                 return null;
        //             }

        //             var workshopEntity = await _workshopRepository.FindByIdAsync(filterModel.WorkshopId);
        //             if (workshopEntity == null)
        //             {
        //                 CreateNotification(DefaultMessages.WorkshopNotFound);
        //                 return null;
        //             }

        //             workshopId = workshopEntity.GetStringId();

        //             if (filterModel.Services.Any() == false)
        //             {
        //                 CreateNotification("Por favor, informe os serviços que deseja realizar.");
        //                 return null;
        //             }
        //         }
        //         else
        //         {
        //             workshopId = _access.UserId;
        //         }

        //         // Converte epoch para DateTime (UTC)
        //         DateTime currentDateUtc = DateTimeOffset.FromUnixTimeSeconds(filterModel.Date).UtcDateTime;

        //         // Define o fuso horário GMT-3
        //         TimeZoneInfo gmtMinus3 = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        //         DateTime currentDate = TimeZoneInfo.ConvertTimeFromUtc(currentDateUtc, gmtMinus3);

        //         // Obtém os dias relevantes para o calendário
        //         DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        //         DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        //         DateTime sevenDaysBefore = firstDayOfMonth.AddDays(-7);
        //         DateTime sevenDaysAfter = lastDayOfMonth.AddDays(7);

        //         // Monta a lista de dias
        //         var daysOfMonth = GenerateCalendarDays(sevenDaysBefore, sevenDaysAfter, currentDate);

        //         // Busca dados do banco
        //         var schedulingList = new List<Scheduling>();

        //         if (_access.TypeToken == (int)TypeProfile.Profile)
        //         {
        //             schedulingList = (List<Scheduling>)await _schedulingRepository.FindByAsync(x =>
        //                                 x.Workshop.Id == workshopId &&
        //                                 x.Status == SchedulingStatus.Scheduled &&
        //                                 x.Date >= currentDate.ToUnix() &&
        //                                 x.Date <= sevenDaysAfter.ToUnix());
        //         }
        //         else
        //         {
        //             schedulingList = (List<Scheduling>)await _schedulingRepository.FindByAsync(x =>
        //                                 x.Workshop.Id == workshopId &&
        //                                 (int)x.Status >= (int)SchedulingStatus.Scheduled);

        //             schedulingList = (List<Scheduling>)schedulingList.Where(x => DateTimeOffset.FromUnixTimeSeconds(x.Date).Date == currentDate.Date).ToList();
        //         }

        //         var workshopAgendaEntity = await _workshopAgendaRepository.FindOneByAsync(x => x.Workshop.Id == workshopId);

        //         // Calcula o tempo mínimo de agendamento
        //         TimeSpan minTimeScheduling = CalculateMinTimeScheduling(filterModel.Services);

        //         // Atualiza horários e disponibilidade
        //         UpdateCalendarWithWorkshopAgenda(daysOfMonth, workshopAgendaEntity);
        //         FilterUnavailableDays(daysOfMonth);

        //         // Ajusta os horários disponíveis de acordo com os agendamentos
        //         ApplySchedulingFilters(daysOfMonth, schedulingList, currentDate, minTimeScheduling, gmtMinus3, _access.TypeToken, workshopId);

        //         return daysOfMonth;
        //     }
        //     catch (Exception)
        //     {
        //         throw;
        //     }
        // }

        // private static List<CalendarAvailableViewModel> GenerateCalendarDays(DateTime startDate, DateTime endDate, DateTime comparisonDate)
        // {
        //     var days = new List<CalendarAvailableViewModel>();
        //     for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
        //     {
        //         days.Add(new CalendarAvailableViewModel
        //         {
        //             Date = currentDate,
        //             Available = currentDate >= comparisonDate.Date,
        //             DayOfWeek = currentDate.DayOfWeek,
        //             Hours = []
        //         });
        //     }
        //     return days;
        // }

        // private static TimeSpan CalculateMinTimeScheduling(List<WorkshopServicesAuxViewModel> services)
        // {
        //     TimeSpan minTime = TimeSpan.Parse("00:00");

        //     if (services == null)
        //     {
        //         return minTime;
        //     }

        //     for (var i = 0; i < services.Count; i++)
        //     {
        //         if (TimeSpan.TryParseExact(services[i].MinTimeScheduling, "hh\\:mm", null, out TimeSpan currentMinTime) &&
        //                            TimeSpan.TryParseExact(minTime.ToString(), "hh\\:mm", null, out TimeSpan minTimeSpan) &&
        //                            currentMinTime > minTimeSpan)
        //         {
        //             minTime = TimeSpan.Parse(services[i].MinTimeScheduling);
        //         }
        //     }

        //     return minTime;
        // }

        // private void UpdateCalendarWithWorkshopAgenda(List<CalendarAvailableViewModel> days, WorkshopAgenda workshopAgenda)
        // {
        //     for (var i = 0; i < days.Count; i++)
        //     {
        //         var agenda = GetAgendaForDay(workshopAgenda, days[i].DayOfWeek);
        //         if (agenda.Open)
        //         {
        //             days[i].Hours = GenerateSchedule(agenda.StartTime, agenda.ClosingTime, agenda.StartOfBreak, agenda.EndOfBreak);
        //         }
        //         else
        //         {
        //             days[i].Available = false;
        //             days[i].Hours.Clear();
        //         }
        //     }
        // }

        // private WorkshopAgendaAux GetAgendaForDay(WorkshopAgenda workshopAgenda, DayOfWeek dayOfWeek) =>
        //     dayOfWeek switch
        //     {
        //         DayOfWeek.Sunday => workshopAgenda.Sunday,
        //         DayOfWeek.Monday => workshopAgenda.Monday,
        //         DayOfWeek.Tuesday => workshopAgenda.Tuesday,
        //         DayOfWeek.Wednesday => workshopAgenda.Wednesday,
        //         DayOfWeek.Thursday => workshopAgenda.Thursday,
        //         DayOfWeek.Friday => workshopAgenda.Friday,
        //         DayOfWeek.Saturday => workshopAgenda.Saturday,
        //         _ => throw new ArgumentOutOfRangeException()
        //     };

        // private static void FilterUnavailableDays(List<CalendarAvailableViewModel> days)
        // {
        //     for (var i = 0; i < days.Count; i++)
        //     {
        //         if (days[i].Available == false)
        //         {
        //             days[i].Hours.Clear();
        //         }
        //     }
        // }

        // private async void ApplySchedulingFilters(List<CalendarAvailableViewModel> days, IEnumerable<Scheduling> schedulingList, DateTime currentDate, TimeSpan minTimeScheduling, TimeZoneInfo timeZone, int typeToken, string workshopId = null)
        // {
        //     // Verifica bloqueios extras do `AgendaAux`
        //     var agendaAuxList = await _agendaAuxRepository.FindByAsync(x => x.WorkshopId == workshopId);
        //     var blockedTimes = agendaAuxList
        //         .Select(a =>
        //         {
        //             var agendaDateTime = TimeZoneInfo.ConvertTimeFromUtc(
        //                 DateTimeOffset.FromUnixTimeSeconds(a.Date).UtcDateTime, timeZone);
        //             return new
        //             {
        //                 Date = agendaDateTime.Date,
        //                 Time = agendaDateTime.TimeOfDay
        //             };
        //         })
        //         .Where(a => a.Date == currentDate.Date)
        //         .Select(a => a.Time)
        //         .ToHashSet(); // Melhora a busca

        //     for (var i = 0; i < days.Count; i++)
        //     {
        //         if (typeToken == (int)TypeProfile.Profile)
        //         {
        //             // if (!days[i].Available)
        //             // {
        //             //     days[i].Hours.Clear();
        //             //     continue;
        //             // }

        //             // Filtra horários apenas se o dia for hoje
        //             if (days[i].Date.Date == currentDate.Date)
        //             {
        //                 days[i].Hours = days[i].Hours
        //                     .Where(hour => TimeSpan.Parse(hour) >= currentDate.TimeOfDay.Add(minTimeScheduling))
        //                     .ToList();
        //             }

        //             // Remove horários já agendados
        //             var scheduledHours = schedulingList
        //                 .Where(s => TimeZoneInfo
        //                     .ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(s.Date).UtcDateTime, timeZone)
        //                     .Date == days[i].Date.Date)
        //                 .Select(s => TimeZoneInfo
        //                     .ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(s.Date).UtcDateTime, timeZone)
        //                     .TimeOfDay)
        //                 .ToHashSet(); // Melhora a performance na busca

        //             days[i].Hours = days[i].Hours
        //                 .Where(hour => !scheduledHours.Contains(TimeSpan.Parse(hour)) && !blockedTimes.Contains(TimeSpan.Parse(hour)))
        //                 .ToList();
        //         }

        //         if (typeToken == (int)TypeProfile.Workshop)
        //         {
        //             // Criação da agenda inicial
        //             days[i].WorkshopAgenda = days[i].Hours
        //                 .Select(hour => new WorkshopAgendaHoursViewModel
        //                 {
        //                     Hour = hour,
        //                     Available = days[i].Date.Date != currentDate.Date || TimeSpan.Parse(hour) >= currentDate.TimeOfDay
        //                 })
        //                 .ToList();

        //             // Atualiza horários baseados nos agendamentos
        //             var scheduledWorkshops = schedulingList
        //                 .Select(s =>
        //                 {
        //                     var localDate = TimeZoneInfo.ConvertTimeFromUtc(
        //                         DateTimeOffset.FromUnixTimeSeconds(s.Date).UtcDateTime, timeZone);
        //                     return new
        //                     {
        //                         Date = localDate.Date,
        //                         Time = localDate.TimeOfDay,
        //                         Profile = s.Profile,
        //                         Vehicle = s.Vehicle
        //                     };
        //                 })
        //                 .Where(s => s.Date == days[i].Date.Date)
        //                 .ToList();

        //             for (var y = 0; y < days[i].WorkshopAgenda.Count; y++)
        //             {
        //                 var match = scheduledWorkshops.FirstOrDefault(s => s.Time == TimeSpan.Parse(days[i].WorkshopAgenda[y].Hour));
        //                 if (match != null)
        //                 {
        //                     days[i].WorkshopAgenda[y].Available = false;
        //                     days[i].WorkshopAgenda[y].Profile = _mapper.Map<ProfileAuxViewModel>(match.Profile);
        //                     days[i].WorkshopAgenda[y].Vehicle = _mapper.Map<VehicleAuxViewModel>(match.Vehicle);
        //                 }

        //                 if (blockedTimes.Contains(TimeSpan.Parse(days[i].WorkshopAgenda[y].Hour)))
        //                 {
        //                     days[i].WorkshopAgenda[y].Available = false;
        //                 }
        //             }

        //             days[i].Hours.Clear();
        //         }
        //     }
        // }

        // static List<string> GenerateSchedule(string startTime, string closingTime, string startOfBreak, string endOfBreak)
        // {
        //     var schedule = new List<string>();

        //     if (startTime == null)
        //     {
        //         return schedule;
        //     }

        //     // Converter strings de tempo para DateTime
        //     DateTime start = DateTime.Parse(startTime);
        //     DateTime close = DateTime.Parse(closingTime);
        //     DateTime breakStart = DateTime.Parse(startOfBreak);
        //     DateTime breakEnd = DateTime.Parse(endOfBreak);

        //     // Iterar de 1 em 1 hora
        //     var time = 1;
        //     for (DateTime current = start; current < close; current = current.AddHours(time))
        //     {
        //         // Ignorar o intervalo de pausa
        //         if (current >= breakStart && current < breakEnd)
        //         {
        //             continue;
        //         }

        //         schedule.Add(current.ToString("HH:mm"));
        //     }

        //     return schedule;
        // }
    }
}