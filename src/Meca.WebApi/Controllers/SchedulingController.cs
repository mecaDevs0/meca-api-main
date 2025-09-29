using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class SchedulingController : MainController
    {
        private readonly IMapper _mapper;
        private readonly ISchedulingService _schedulingService;
        private readonly IBusinessBaseAsync<Scheduling> _schedulingRepository;

        public SchedulingController(IMapper mapper,
            ISchedulingService schedulingService,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            IHttpContextAccessor context,
            IConfiguration configuration) : base(schedulingService, context, configuration)
        {
            _mapper = mapper;
            _schedulingService = schedulingService;
            _schedulingRepository = schedulingRepository;
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA LISTAR TODOS PAGINADO OU C/S FILTRO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<SchedulingViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromQuery] SchedulingFilterViewModel filterModel)
        {
            try
            {
                if (filterModel.HasFilter() == false)
                    return ReturnResponse(await _schedulingService.GetAll());
                else
                    return ReturnResponse(await _schedulingService.GetAll<SchedulingViewModel>(filterModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO DE DETALHES
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                var response = await _schedulingService.GetById(id);

                return ReturnResponse(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA CADASTRO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///               "WorkshopServices": [
        ///                 {
        ///                     "id": "string",
        ///                     "name": "string"
        ///                 }
        ///               ],
        ///               "vehicle": {
        ///                     "id": "string",
        ///                     "plate": "string"
        ///                },
        ///               "observations": "string",
        ///               "date": 9559995,
        ///               "status": 0,
        ///               "Workshop": {
        ///                     "id": "string",
        ///                     "fullName": "string"
        ///                }
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] SchedulingViewModel model)
        {
            try
            {
                model.TrimStringProperties();

                _service.SetModelState(ModelState);

                var response = await _schedulingService.Register(model);

                return ReturnResponse(response, "Solicitação de agendamento enviada, aguarde o contato da oficina para confirmar agendamento");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        // /// <summary>
        // /// AGENDAMENTO - ATUALIZAR CAMPOS
        // /// </summary>
        // /// <remarks>
        // /// OBJ DE ENVIO
        // ///
        // ///         POST
        // ///             {
        // ///               "WorkshopServices": [
        // ///                 {
        // ///                     "id": "string",
        // ///                     "name": "string"
        // ///                 }
        // ///               ],
        // ///               "vehicle": {
        // ///                     "id": "string",
        // ///                     "plate": "string"
        // ///                },
        // ///               "observations": "string",
        // ///               "date": 9559995,
        // ///               "status": 0,
        // ///               "Workshop": {
        // ///                     "id": "string",
        // ///                     "fullName": "string"
        // ///                }
        // ///             }
        // /// </remarks>
        // /// <response code="200">Returns success</response>
        // /// <response code="400">Custom Error</response>
        // /// <response code="401">Unauthorize Error</response>
        // /// <response code="500">Exception Error</response>
        // /// <returns></returns>
        // [HttpPatch("{id}")]
        // [Produces("application/json")]
        // [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        // [ProducesResponseType(400)]
        // [ProducesResponseType(401)]
        // [ProducesResponseType(500)]
        // public async Task<IActionResult> PatchValues([FromRoute] string id, [FromBody] SchedulingViewModel model)
        // {
        //     try
        //     {
        //         model.TrimStringProperties();
        //         _service.SetModelState(ModelState);

        //         var response = await _schedulingService.UpdatePatch(id, model);

        //         return ReturnResponse(response, DefaultMessages.Updated);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.ReturnErro());
        //     }
        // }

        /// <summary>
        /// AGENDAMENTO - METODO DE REMOVER
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var _id) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                await _schedulingService.Delete(id);

                return ReturnResponse(null, DefaultMessages.Deleted);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO DE LISTAGEM COM DATATABLE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<SchedulingViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] SchedulingFilterViewModel filterView)
        {
            var response = new DtResult<SchedulingViewModel>();
            try
            {
                var builder = Builders<Scheduling>.Filter;
                var conditions = new List<FilterDefinition<Scheduling>>() { builder.Where(x => x.Disabled == null) };

                if (filterView.StartDate != null)
                    conditions.Add(builder.Gte(x => x.Created, filterView.StartDate));

                if (filterView.EndDate != null)
                    conditions.Add(builder.Lte(x => x.Created, filterView.EndDate));

                if (filterView.Status != null && filterView.Status.Any())
                    conditions.Add(builder.In(x => x.Status, filterView.Status));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var totalRecords = (int)await _schedulingRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = Util.MapSort<Scheduling>(model.Order, model.Columns, model.SortOrder);

                var result = await _schedulingRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalRecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _schedulingRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<SchedulingViewModel>>(result);
                response.Draw = model.Draw;
                response.RecordsFiltered = totalRecordsFiltered;
                response.RecordsTotal = totalRecords;

                return Ok(response);

            }
            catch (Exception ex)
            {
                response.Erro = true;
                response.MessageEx = $"{ex.InnerException} {ex.Message}".Trim();
                return Ok(response);
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA OBTER AGENDA (USUÁRIO/OFICINA)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("AvailableScheduling")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<CalendarAvailableViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAvailableScheduling([FromBody] SchedulingAvailableFilterViewModel filterModel)
        {
            try
            {
                return ReturnResponse(await _schedulingService.GetAvailableScheduling(filterModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA SUGERIR NOVO HORÁRIO DE AGENDAMENTO (OFICINA)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("SuggestNewTime")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SuggestNewTime([FromBody] SuggestNewTimeViewModel model)
        {
            try
            {
                return ReturnResponse(await _schedulingService.SuggestNewTime(model), "Horário sugerido enviado ao cliente.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA CONFIRMAR AGENDAMENTO (OFICINA/USUÁRIO)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ConfirmScheduling")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ConfirmScheduling([FromBody] ConfirmSchedulingViewModel model)
        {
            try
            {
                var response = await _schedulingService.ConfirmScheduling(model);
                return ReturnResponse(response, response.Status == SchedulingStatus.AppointmentRefused ? DefaultMessages.AppointmentRefused : DefaultMessages.AppointmentConfirmed);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA ALTERAR STATUS DO AGENDAMENTO (OFICINA)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ChangeSchedulingStatus")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ChangeSchedulingStatus([FromBody] ChangeSchedulingStatusViewModel model)
        {
            try
            {
                var response = await _schedulingService.ChangeSchedulingStatus(model);
                return ReturnResponse(response, DefaultMessages.Updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA ENVIAR ORÇAMENTO (OFICINA)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("SendBudget")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SendBudget([FromBody] SendBudgetViewModel model)
        {
            try
            {
                var response = await _schedulingService.SendBudget(model);
                return ReturnResponse(response, "Orçamento enviado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - BUSCAR AGENDAMENTOS POR OFICINA (MECA-APP-CLIENTE)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetByWorkshop/{workshopId}")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<SchedulingViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetByWorkshop([FromRoute] string workshopId)
        {
            try
            {
                Console.WriteLine($"[SCHEDULING_DEBUG] Buscando agendamentos para oficina: {workshopId}");
                
                if (string.IsNullOrEmpty(workshopId))
                {
                    return BadRequest(Utilities.ReturnErro("ID da oficina não informado"));
                }

                if (ObjectId.TryParse(workshopId, out var _id) == false)
                {
                    return BadRequest(Utilities.ReturnErro("ID da oficina inválido"));
                }

                var builder = Builders<Scheduling>.Filter;
                var filter = builder.Eq(x => x.Workshop.Id, workshopId);
                
                var schedulings = await _schedulingRepository.FindByAsync(filter, 
                    Builders<Scheduling>.Sort.Descending(nameof(Scheduling.Created)));
                
                Console.WriteLine($"[SCHEDULING_DEBUG] Encontrados {schedulings.Count()} agendamentos");
                
                var response = _mapper.Map<List<SchedulingViewModel>>(schedulings);
                
                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCHEDULING_ERROR] Erro ao buscar agendamentos: {ex.Message}");
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - BUSCAR HORÁRIOS DISPONÍVEIS POR OFICINA (MECA-APP-CLIENTE)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetAvailableTimes/{workshopId}")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<AvailableTimeViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAvailableTimes([FromRoute] string workshopId, [FromQuery] DateTime? date = null)
        {
            try
            {
                Console.WriteLine($"[SCHEDULING_DEBUG] Buscando horários disponíveis para oficina: {workshopId}, data: {date}");
                
                if (string.IsNullOrEmpty(workshopId))
                {
                    return BadRequest(Utilities.ReturnErro("ID da oficina não informado"));
                }

                if (ObjectId.TryParse(workshopId, out var _id) == false)
                {
                    return BadRequest(Utilities.ReturnErro("ID da oficina inválido"));
                }

                // Se não foi informada data, usar hoje
                var targetDate = date ?? DateTime.Today;
                
                // Gerar horários disponíveis (8h às 18h, de hora em hora)
                var availableTimes = new List<AvailableTimeViewModel>();
                
                for (int hour = 8; hour <= 18; hour++)
                {
                    var timeSlot = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, hour, 0, 0);
                    
                    availableTimes.Add(new AvailableTimeViewModel
                    {
                        Time = timeSlot,
                        Available = true,
                        FormattedTime = timeSlot.ToString("HH:mm")
                    });
                }
                
                Console.WriteLine($"[SCHEDULING_DEBUG] Gerados {availableTimes.Count} horários disponíveis");
                
                return Ok(Utilities.ReturnSuccess(data: availableTimes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCHEDULING_ERROR] Erro ao buscar horários disponíveis: {ex.Message}");
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA APROVAR/REPROVAR ORÇAMENTO (USUÁRIO)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ConfirmBudget")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ConfirmBudget([FromBody] ConfirmBudgetViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _schedulingService.ConfirmBudget(model);
                return ReturnResponse(response, response != null ? response.Status == SchedulingStatus.BudgetDisapprove ? "Orçamento reprovado." : "Orçamento aprovado com sucesso." : null);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA APROVAR/REPROVAR SERVIÇO (USUÁRIO)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ConfirmService")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ConfirmService([FromBody] ConfirmServiceViewModel model)
        {
            try
            {
                var response = await _schedulingService.ConfirmService(model);
                return ReturnResponse(response, response != null ? response.Status == SchedulingStatus.ServiceReprovedByUser ? "Serviço reprovado." : "Serviço aprovado com sucesso." : null);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA SUGERIR REPARO GRATUITO (OFICINA)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("SuggestFreeRepair")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SuggestFreeRepair(string schedulingId)
        {
            try
            {
                var response = await _schedulingService.SuggestFreeRepair(schedulingId);
                return ReturnResponse(response, DefaultMessages.Sended);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA REALIZAR AGENDAMENTO DE REPARO GRATUITO (USUÁRIO)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterRepairAppointment")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<SchedulingViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterRepairAppointment([FromBody] SchedulingViewModel model)
        {
            try
            {
                var response = await _schedulingService.RegisterRepairAppointment(model);

                return ReturnResponse(response, "Solicitação de agendamento enviada, aguarde o contato da oficina para confirmar agendamento");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA CONTESTAR REPROVAÇÃO DO USUÁRIO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("DisputeDisapprovedService")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DisputeDisapprovedService([FromBody] DisputeDisapprovedServiceViewModel model)
        {
            try
            {
                var response = await _schedulingService.DisputeDisapprovedService(model);

                return ReturnResponse(response, "Informações enviadas com sucesso, em breve a equipe da Meca analisará a contestação e entrará em contato o mais breve possível.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDAMENTO - METODO PARA APROVAR/APROVAR PARCIALMENTE/REPROVAR SERVIÇO (ADMIN)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ApproveOrReproveService")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ApproveOrReproveService([FromBody] ApproveOrReproveServiceViewModel model)
        {
            try
            {
                var response = await _schedulingService.ApproveOrReproveService(model);

                return ReturnResponse(response, DefaultMessages.Updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}