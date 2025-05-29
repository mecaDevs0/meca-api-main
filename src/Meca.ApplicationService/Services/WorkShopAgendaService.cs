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
        }

        public async Task<WorkshopAgendaViewModel> GetWorkshopAgenda(string id = null)
        {
            try
            {
                if ((int)_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    if (string.IsNullOrEmpty(id) == true)
                    {
                        CreateNotification(DefaultMessages.InvalidIdentifier);
                        return null;
                    }
                }
                else
                {
                    id = _access.UserId;
                }

                var workshopEntity = await _workshopRepository.FindByIdAsync(id);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                var workshopAgendaEntity = await _workshopAgendaRepository.FindOneByAsync(x => x.Workshop.Id == workshopEntity.GetStringId());

                return _mapper.Map<WorkshopAgendaViewModel>(workshopAgendaEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<WorkshopAgendaViewModel> RegisterOrUpdate(WorkshopAgendaViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                if (ModelIsValid(model, true, validOnly) == false)
                    return null;

                WorkshopAgenda workshopAgendaEntity = null;

                var workshopEntity = await _workshopRepository.FindByIdAsync(_access.UserId);
                if (workshopEntity == null)
                {
                    CreateNotification(DefaultMessages.WorkshopNotFound);
                    return null;
                }

                if (string.IsNullOrEmpty(model.Id))
                {
                    workshopAgendaEntity = _mapper.Map<WorkshopAgenda>(model);
                    workshopAgendaEntity.Workshop = _mapper.Map<WorkshopAux>(workshopEntity);
                    workshopAgendaEntity = await _workshopAgendaRepository.CreateReturnAsync(workshopAgendaEntity);
                }
                else
                {
                    workshopAgendaEntity = await _workshopAgendaRepository.FindByIdAsync(model.Id);

                    if (workshopAgendaEntity == null)
                    {
                        CreateNotification(DefaultMessages.WorkshopAgendaNotFound);
                        return null;
                    }

                    workshopAgendaEntity.SetIfDifferent(model, validOnly, _mapper);
                    workshopAgendaEntity = await _workshopAgendaRepository.UpdateAsync(workshopAgendaEntity);
                }

                return _mapper.Map<WorkshopAgendaViewModel>(workshopAgendaEntity);

            }
            catch (Exception)
            {
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

                if ((int)_access.TypeToken != (int)TypeProfile.Workshop)
                {
                    CreateNotification(DefaultMessages.NotPermission);
                    return false;
                }

                var userId = _access.UserId;

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