using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
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

    public class FaqController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IFaqService _faqService;
        private readonly IBusinessBaseAsync<Faq> _faqRepository;

        public FaqController(IMapper mapper,
            IFaqService faqService,
            IBusinessBaseAsync<Faq> faqRepository,
            IHttpContextAccessor context,
            IConfiguration configuration) : base(faqService, context, configuration)
        {
            _mapper = mapper;
            _faqService = faqService;
            _faqRepository = faqRepository;
        }

        /// <summary>
        /// FAQ - METODO PARA LISTAR TODOS PAGINADO OU C/S FILTRO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<FaqViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromQuery] FaqFilterViewModel filterModel)
        {
            try
            {
                if (filterModel.HasFilter() == false)
                    return ReturnResponse(await _faqService.GetAll());
                else
                    return ReturnResponse(await _faqService.GetAll<FaqViewModel>(filterModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// FAQ - METODO DE DETALHES
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<FaqViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                var response = await _faqService.GetById(id);

                return ReturnResponse(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// FAQ - METODO PARA CADASTRO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///               "question": "string",
        ///               "response": "string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<FaqViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] FaqViewModel model)
        {
            try
            {
                model.TrimStringProperties();

                _service.SetModelState(ModelState);

                var response = await _faqService.Register(model);

                return ReturnResponse(response, DefaultMessages.Registered);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// FAQ - ATUALIZAR CAMPOS
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///               "question": "string",
        ///               "response": "string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<FaqViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PatchValues([FromRoute] string id, [FromBody] FaqViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _faqService.UpdatePatch(id, model);

                return ReturnResponse(response, DefaultMessages.Updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// FAQ - METODO DE REMOVER
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

                await _faqService.Delete(id);

                return ReturnResponse(null, DefaultMessages.Deleted);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// FAQ - METODO DE LISTAGEM COM DATATABLE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<FaqViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model)
        {
            var response = new DtResult<FaqViewModel>();
            try
            {
                var builder = Builders<Faq>.Filter;
                var conditions = new List<FilterDefinition<Faq>>() { builder.Where(x => x.Disabled == null) };

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var totalRecords = (int)await _faqRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = Util.MapSort<Faq>(model.Order, model.Columns, model.SortOrder);

                var result = await _faqRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalRecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _faqRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<FaqViewModel>>(result);
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
    }
}