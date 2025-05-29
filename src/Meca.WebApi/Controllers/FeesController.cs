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

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FeesController : MainController
    {

        private readonly IMapper _mapper;
        private readonly IFeesService _feesService;

        public FeesController(
        IFeesService feesService,
        IHttpContextAccessor context,
        IConfiguration configuration,
        IMapper mapper,
        IBusinessBaseAsync<Fees> feesRepository
        ) : base(feesService, context, configuration)
        {
            _mapper = mapper;
            _feesService = feesService;
        }

        /// <summary>
        /// TAXAS DA PLATAFORMA - METODO PARA OBTER AS TAXAS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<FeesViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var response = await _feesService.GetFees();

                return ReturnResponse(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// TAXAS DA PLATAFORMA - REGISTRAR | ATUALIZAR CAMPOS
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "platformFee": 0.99
        ///             }
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
        public async Task<IActionResult> PatchValues([FromBody] FeesViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _feesService.RegisterOrUpdate(model);

                var message = string.IsNullOrEmpty(model.Id) ? DefaultMessages.Registered : DefaultMessages.Updated;

                return Ok(Utilities.ReturnSuccess(message, response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}