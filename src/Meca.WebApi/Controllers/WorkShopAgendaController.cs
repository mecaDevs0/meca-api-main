using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using Meca.ApplicationService.Interface;
using Meca.WebApi.Controllers;
using Meca.Domain.ViewModels;
using Meca.Domain;
using Meca.Data.Entities;
using System.Collections.Generic;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WorkshopAgendaController : MainController
    {

        private readonly IMapper _mapper;
        private readonly IWorkshopAgendaService _workshopAgendaService;

        public WorkshopAgendaController(
        IWorkshopAgendaService workshopAgendaService,
        IHttpContextAccessor context,
        IConfiguration configuration,
        IMapper mapper,
        IBusinessBaseAsync<Fees> feesRepository
        ) : base(workshopAgendaService, context, configuration)
        {
            _mapper = mapper;
            _workshopAgendaService = workshopAgendaService;
        }

        /// <summary>
        /// AGENDA DA OFICINA - METODO PARA OBTER AGENDA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<WorkshopAgendaViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            try
            {
                var response = await _workshopAgendaService.GetWorkshopAgenda(id);

                return ReturnResponse(response, "Sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDA DA OFICINA - REGISTRAR | ATUALIZAR CAMPOS
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///               {
        ///                 "Sunday": {
        ///                    "Open": false,
        ///                    "StartTime": "",
        ///                    "ClosingTime": "",
        ///                    "StartOfBreak": "",
        ///                    "EndOfBreak": ""
        ///                 },
        ///                 "Monday": {
        ///                    "Open": true,
        ///                    "StartTime": "08:00",
        ///                    "ClosingTime": "18:00",
        ///                    "StartOfBreak": "12:00",
        ///                    "EndOfBreak": "13:00"
        ///                 }
        ///               }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPatch]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PatchValues([FromBody] WorkshopAgendaViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _workshopAgendaService.RegisterOrUpdate(model);

                var message = string.IsNullOrEmpty(model.Id) ? DefaultMessages.Registered : DefaultMessages.Updated;

                return Ok(Utilities.ReturnSuccess(message, response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// AGENDA DA OFICINA - REMOVER HORÁRIOS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpDelete("Hour/{date}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] string date)
        {
            try
            {
                var response = await _workshopAgendaService.RemoveHour(date);

                return ReturnResponse(null, DefaultMessages.Deleted);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}