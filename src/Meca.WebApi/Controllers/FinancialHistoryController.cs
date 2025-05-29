using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Export;
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
    public class FinancialHistoryController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IBusinessBaseAsync<FinancialHistory> _financialHistoryRepository;
        private readonly IFinancialHistoryService _financialHistoryService;

        public FinancialHistoryController(
        IFinancialHistoryService financialHistoryService,
        IHttpContextAccessor context,
        IConfiguration configuration,
        IMapper mapper,
        IBusinessBaseAsync<FinancialHistory> financialHistoryRepository
        ) : base(financialHistoryService, context, configuration)
        {
            _mapper = mapper;
            _financialHistoryRepository = financialHistoryRepository;
            _financialHistoryService = financialHistoryService;
        }

        /// <summary>
        /// HISTÓRICO FINANCEIRO - METODO PARA LISTAR TODOS PAGINADO OU C/S FILTRO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<FinancialHistoryViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromQuery] FinancialHistoryFilterViewModel filterModel)
        {
            try
            {
                if (filterModel.HasFilter() == false)
                    return ReturnResponse(await _financialHistoryService.GetAll());
                else
                    return ReturnResponse(await _financialHistoryService.GetAll<FinancialHistoryViewModel>(filterModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// HISTÓRICO FINANCEIRO - METODO DE DETALHES
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<FinancialHistoryViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                var response = await _financialHistoryService.GetById(id);

                return ReturnResponse(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// HISTÓRICO FINANCEIRO - REGISTRAR PAGAMENTO DO SERVIÇO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///
        ///         POST
        ///             {
        ///               "paymentMethod": 0,
        ///               "creditCardId": "string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Payment")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<TransactionViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Payment([FromBody] PaymentViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _financialHistoryService.Payment(model);

                return ReturnResponse(response, response?.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// HISTÓRICO FINANCEIRO - METODO DE REMOVER
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

                await _financialHistoryService.Delete(id);

                return ReturnResponse(null, DefaultMessages.Deleted);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// HISTÓRICO FINANCEIRO - METODO DE LISTAGEM COM DATATABLE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<FinancialHistoryViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] FinancialHistoryFilterViewModel filterView)
        {
            var response = new DtResult<FinancialHistoryViewModel>();
            try
            {
                var builder = Builders<FinancialHistory>.Filter;
                var conditions = new List<FilterDefinition<FinancialHistory>>() { builder.Where(x => x.Disabled == null) };

                if (filterView.PaymentStatus != null && filterView.PaymentStatus.Count > 0)
                    conditions.Add(builder.In(x => x.PaymentStatus, filterView.PaymentStatus));

                if (string.IsNullOrEmpty(filterView.ProfileId) == false)
                    conditions.Add(builder.Eq(x => x.Profile.Id, filterView.ProfileId));

                if (filterView.StartDate != null)
                    conditions.Add(builder.Gte(x => x.Created, filterView.StartDate));

                if (filterView.EndDate != null)
                    conditions.Add(builder.Lte(x => x.Created, filterView.EndDate));

                if (filterView.Id?.Count > 0)
                    conditions.Add(builder.In(x => x._id, filterView.Id.Select(ObjectId.Parse).ToList()));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var totalRecords = (int)await _financialHistoryRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = Util.MapSort<FinancialHistory>(model.Order, model.Columns, model.SortOrder);

                var result = await _financialHistoryRepository
                   .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalRecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _financialHistoryRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<FinancialHistoryViewModel>>(result);
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
        /// HISTÓRICO FINANCEIRO - METODO DE EXPORTAÇÃO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Export")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<FinancialHistoryExportViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Export([FromForm] DtParameters model, [FromForm] FinancialHistoryFilterViewModel filterView)
        {
            try
            {
                var builder = Builders<FinancialHistory>.Filter;
                var conditions = new List<FilterDefinition<FinancialHistory>>
                {
                    builder.Where(x => x.Disabled == null)
                };

                if (filterView.PaymentStatus != null && filterView.PaymentStatus.Count > 0)
                    conditions.Add(builder.In(x => x.PaymentStatus, filterView.PaymentStatus));

                if (string.IsNullOrEmpty(filterView.ProfileId) == false)
                    conditions.Add(builder.Eq(x => x.Profile.Id, filterView.ProfileId));

                if (filterView.StartDate != null)
                    conditions.Add(builder.Gte(x => x.Created, filterView.StartDate));

                if (filterView.EndDate != null)
                    conditions.Add(builder.Lte(x => x.Created, filterView.EndDate));

                if (filterView.Id?.Count > 0)
                    conditions.Add(builder.In(x => x._id, filterView.Id.Select(ObjectId.Parse).ToList()));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var totalRecords = (int)await _financialHistoryRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = Util.MapSort<FinancialHistory>(model.Order, model.Columns, model.SortOrder);

                var result = await _financialHistoryRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, 0, 0, conditions, columns);

                var listViewModel = _mapper.Map<List<FinancialHistoryExportViewModel>>(result);

                var fileName = "historico_financieiro.xlsx";
                var path = Path.Combine($"{Directory.GetCurrentDirectory()}/Content", @"ExportFiles");

                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);

                Utilities.ExportToExcel(listViewModel, path, "Histórico Financeiro", fileName: fileName.Split('.')[0]);

                if (System.IO.File.Exists(fullPathFile) == false)
                    return BadRequest(Utilities.ReturnErro("Ocorreu um erro fazer download do arquivo"));

                var fileBytes = System.IO.File.ReadAllBytes(fullPathFile);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}