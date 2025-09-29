using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.ApplicationService.Services;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using UtilityFramework.Application.Core3;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.ApplicationService.Services
{

    public class WorkshopAgendaService : ApplicationServiceBase<WorkshopAgenda>, IWorkshopAgendaService
    {
        private IConfiguration _configuration;
        private readonly IBusinessBaseAsync<WorkshopAgenda> _workshopAgendaRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IBusinessBaseAsync<AgendaAux> _agendaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public WorkshopAgendaService(
           IMapper mapper,
           IBusinessBaseAsync<WorkshopAgenda> WorkshopAgendaRepository,
           IBusinessBaseAsync<Workshop> workshopRepository,
           IBusinessBaseAsync<AgendaAux> agendaRepository,
           IHttpContextAccessor httpContextAccessor,
           IConfiguration configuration)
        {
            _mapper = mapper;
            _workshopAgendaRepository = WorkshopAgendaRepository;
            _workshopRepository = workshopRepository;
            _agendaRepository = agendaRepository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            
            // Inicializar o acesso
            _access = SetAccess(httpContextAccessor);
        }

        public async Task<WorkshopAgendaViewModel> GetWorkshopAgenda(string id = null)
        {
            try
            {
                // Garantir que o acesso seja inicializado
                if (_access == null)
                {
                    Console.WriteLine("[GetWorkshopAgenda] _access é null, tentando redefinir");
                    if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null)
                    {
                        _access = SetAccess(_httpContextAccessor);
                        Console.WriteLine($"[GetWorkshopAgenda] _access redefinido: UserId={_access?.UserId}, TypeToken={_access?.TypeToken}");
                    }
                    else
                    {
                        Console.WriteLine("[GetWorkshopAgenda] HttpContext é null, criando acesso padrão");
                        _access = new Acesso(id, (int)TypeProfile.Workshop);
                    }
                }
                
                if (_access == null)
                {
                    Console.WriteLine("[GetWorkshopAgenda] _access ainda é null, usando ID fornecido");
                    _access = new Acesso(id, (int)TypeProfile.Workshop);
                }

                Console.WriteLine($"[GetWorkshopAgenda] DEBUG: _access.TypeToken = {_access?.TypeToken}, id recebido = {id}");

                // Verificar se o tipo de token é válido
                if (_access?.TypeToken == null)
                {
                    Console.WriteLine("[GetWorkshopAgenda] ERRO: TypeToken é null");
                    CreateNotification("Tipo de token inválido");
                    return new WorkshopAgendaViewModel();
                }

                if ((int)_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    if (string.IsNullOrEmpty(id) == true)
                    {
                        CreateNotification(DefaultMessages.InvalidIdentifier);
                        return new WorkshopAgendaViewModel();
                    }
                }
                else
                {
                    id = _access?.UserId;
                    Console.WriteLine($"[GetWorkshopAgenda] DEBUG: Usando workshopId do token: {id}");
                    
                    // Verificar se o UserId do token é válido
                    if (string.IsNullOrEmpty(id))
                    {
                        Console.WriteLine("[GetWorkshopAgenda] ERRO: UserId do token é null ou vazio");
                        CreateNotification("ID da oficina não encontrado no token");
                        return new WorkshopAgendaViewModel();
                    }
                }

                if (string.IsNullOrEmpty(id))
                {
                    Console.WriteLine("[GetWorkshopAgenda] ERRO: id é null ou vazio");
                    CreateNotification("ID da oficina não informado");
                    return new WorkshopAgendaViewModel();
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(id);
                if (workshopEntity == null)
                {
                    Console.WriteLine($"[GetWorkshopAgenda] ERRO: Workshop não encontrado para id: {id}");
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return new WorkshopAgendaViewModel();
                }

                Console.WriteLine($"[GetWorkshopAgenda] DEBUG: Workshop encontrado: {workshopEntity.GetStringId()}");

                WorkshopAgenda workshopAgendaEntity = null;
                try
                {
                    workshopAgendaEntity = await _workshopAgendaRepository.FindOneByAsync(x => x.Workshop.Id == workshopEntity._id.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetWorkshopAgenda] ERRO ao buscar agenda: {ex.Message}");
                    // Continuar com agenda padrão em caso de erro
                }

                if (workshopAgendaEntity == null)
                {
                    Console.WriteLine("[GetWorkshopAgenda] DEBUG: Agenda não encontrada, retornando agenda padrão");
                    // Retornar agenda padrão se não existir
                    return new WorkshopAgendaViewModel
                    {
                        Monday = new WorkshopAgendaAuxViewModel { Open = false },
                        Tuesday = new WorkshopAgendaAuxViewModel { Open = false },
                        Wednesday = new WorkshopAgendaAuxViewModel { Open = false },
                        Thursday = new WorkshopAgendaAuxViewModel { Open = false },
                        Friday = new WorkshopAgendaAuxViewModel { Open = false },
                        Saturday = new WorkshopAgendaAuxViewModel { Open = false },
                        Sunday = new WorkshopAgendaAuxViewModel { Open = false }
                    };
                }

                Console.WriteLine("[GetWorkshopAgenda] DEBUG: Agenda encontrada, mapeando para ViewModel");
                try
                {
                    return _mapper.Map<WorkshopAgendaViewModel>(workshopAgendaEntity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetWorkshopAgenda] ERRO no mapeamento: {ex.Message}");
                    // Retornar agenda padrão em caso de erro no mapeamento
                    return new WorkshopAgendaViewModel
                    {
                        Monday = new WorkshopAgendaAuxViewModel { Open = false },
                        Tuesday = new WorkshopAgendaAuxViewModel { Open = false },
                        Wednesday = new WorkshopAgendaAuxViewModel { Open = false },
                        Thursday = new WorkshopAgendaAuxViewModel { Open = false },
                        Friday = new WorkshopAgendaAuxViewModel { Open = false },
                        Saturday = new WorkshopAgendaAuxViewModel { Open = false },
                        Sunday = new WorkshopAgendaAuxViewModel { Open = false }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetWorkshopAgenda] ERRO: {ex.Message}");
                Console.WriteLine($"[GetWorkshopAgenda] StackTrace: {ex.StackTrace}");
                CreateNotification($"Erro ao obter agenda: {ex.Message}");
                return new WorkshopAgendaViewModel();
            }
        }

        public async Task<WorkshopAgendaViewModel> RegisterOrUpdate(WorkshopAgendaViewModel model)
        {
            try
            {
                // Garantir que o acesso seja inicializado
                if (_access == null)
                {
                    _access = SetAccess(_httpContextAccessor);
                }
                
                if (_access == null)
                {
                    Console.WriteLine("[RegisterOrUpdate] ERRO: _access é null após SetAccess");
                    CreateNotification("Acesso não autorizado");
                    return null;
                }

                Console.WriteLine($"[RegisterOrUpdate] DEBUG: _access.UserId = {_access?.UserId}, model.Id = {model?.Id}");

                if (model == null)
                {
                    Console.WriteLine("[RegisterOrUpdate] ERRO: model é null");
                    CreateNotification("Dados da agenda não informados");
                    return null;
                }

                var validOnly = _httpContextAccessor?.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                {
                    Console.WriteLine("[RegisterOrUpdate] ERRO: Modelo inválido");
                    return null;
                }

                WorkshopAgenda workshopAgendaEntity = null;

                if (string.IsNullOrEmpty(_access?.UserId))
                {
                    Console.WriteLine("[RegisterOrUpdate] ERRO: _access.UserId é null ou vazio");
                    CreateNotification("ID do usuário não encontrado no token");
                    return null;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(_access.UserId);
                if (workshopEntity == null)
                {
                    Console.WriteLine($"[RegisterOrUpdate] ERRO: Workshop não encontrado para userId: {_access?.UserId}");
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                Console.WriteLine($"[RegisterOrUpdate] DEBUG: Workshop encontrado: {workshopEntity.GetStringId()}");

                if (string.IsNullOrEmpty(model.Id))
                {
                    Console.WriteLine("[RegisterOrUpdate] DEBUG: Criando nova agenda");
                    workshopAgendaEntity = _mapper.Map<WorkshopAgenda>(model);
                    
                    // Verificar se o mapeamento foi bem-sucedido
                    if (workshopAgendaEntity == null)
                    {
                        Console.WriteLine("[RegisterOrUpdate] ERRO: Falha ao mapear WorkshopAgenda");
                        CreateNotification("Erro ao processar dados da agenda");
                        return null;
                    }
                    
                    // Mapear o workshop de forma segura
                    var workshopAux = _mapper.Map<WorkshopAux>(workshopEntity);
                    if (workshopAux == null)
                    {
                        Console.WriteLine("[RegisterOrUpdate] ERRO: Falha ao mapear WorkshopAux");
                        CreateNotification("Erro ao processar dados da oficina");
                        return null;
                    }
                    
                    workshopAgendaEntity.Workshop = workshopAux;
                    workshopAgendaEntity = await _workshopAgendaRepository.CreateReturnAsync(workshopAgendaEntity);
                }
                else
                {
                    Console.WriteLine($"[RegisterOrUpdate] DEBUG: Atualizando agenda existente: {model.Id}");
                    workshopAgendaEntity = await _workshopAgendaRepository.FindByIdAsync(model.Id);

                    if (workshopAgendaEntity == null)
                    {
                        Console.WriteLine($"[RegisterOrUpdate] ERRO: Agenda não encontrada para id: {model.Id}");
                        CreateNotification(DefaultMessages.WorkshopAgendaNotFound);
                        return null;
                    }

                    workshopAgendaEntity.SetIfDifferent(model, validOnly, _mapper);
                    workshopAgendaEntity = await _workshopAgendaRepository.UpdateAsync(workshopAgendaEntity);
                }

                Console.WriteLine("[RegisterOrUpdate] DEBUG: Operação concluída com sucesso");
                return _mapper.Map<WorkshopAgendaViewModel>(workshopAgendaEntity);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterOrUpdate] ERRO: {ex.Message}");
                Console.WriteLine($"[RegisterOrUpdate] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> RemoveHour(string date)
        {
            try
            {
                if (string.IsNullOrEmpty(date) == true)
                {
                    CreateNotification("Por favor, informe a data e horário que deseja remover.");
                    return false;
                }

                if (_access?.TypeToken == null || (int)_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                var userId = _access?.UserId;

                var agendaList = await _agendaRepository.FindByAsync(x => x.WorkshopId == userId);
                var hasSame = agendaList.Where(x => x.Date == long.Parse(date)).ToList();

                if (hasSame.Any())
                {
                    CreateNotification("Esse horário já foi removido.");
                    return false;
                }

                var agendaAux = new AgendaAux()
                {
                    WorkshopId = userId,
                    Date = long.Parse(date)
                };

                await _agendaRepository.CreateReturnAsync(agendaAux);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}