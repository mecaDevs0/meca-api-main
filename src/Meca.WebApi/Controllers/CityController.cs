using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Meca.Data.Entities;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using RestSharp;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiExplorerSettings(IgnoreApi = true)]

    public class CityController : MainController
    {
        private readonly IBusinessBaseAsync<City> _cityRepository;
        private readonly IBusinessBaseAsync<State> _stateRepository;
        private readonly IMapper _mapper;

        public CityController(IBusinessBaseAsync<City> cityRepository, IBusinessBaseAsync<State> stateRepository, IMapper mapper, IHttpContextAccessor httpContext, IConfiguration configuration) : base(null, httpContext, configuration)
        {
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _mapper = mapper;
        }




        /// <summary>
        /// LISTAR CIDADES POR ESTADOS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{stateId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<CityDefaultViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromRoute] string stateId)
        {
            try
            {
                var listCity = await _cityRepository.FindByAsync(x => x.StateId == stateId, Builders<Data.Entities.City>.Sort.Ascending(nameof(Data.Entities.City.Name))).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<CityDefaultViewModel>>(listCity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(responseList: true));
            }
        }

        /// <summary>
        /// LISTAR ESTADOS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("ListState")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<StateDefaultViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListState()
        {
            try
            {
                var listState = await _stateRepository.FindAllAsync(Builders<Data.Entities.State>.Sort.Ascending(nameof(Data.Entities.State.Name))).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<StateDefaultViewModel>>(listState)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// BUSCAR INFORMAÇÕES DE DETERMINADO CEP
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetInfoFromZipCode/{zipCode}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<InfoAddressViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInfoFromZipCode([FromRoute] string zipCode)
        {
            try
            {
                if (string.IsNullOrEmpty(zipCode))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.RequiredZipCode));

                var client = new RestClient($"http://viacep.com.br/ws/{zipCode.OnlyNumbers()}/json/");

                var request = new RestRequest(Method.GET);

                var infoZipCode = await client.ExecuteAsync<AddressInfoViewModel>(request);


                if (infoZipCode.StatusCode != HttpStatusCode.OK || infoZipCode.Data == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ZipCodeNotFoundOrOffline));

                if (infoZipCode.Data.Erro)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ZipCodeNotFound));

                var response = _mapper.Map<InfoAddressViewModel>(infoZipCode.Data);

                var builder = Builders<City>.Filter;
                var conditions = new List<FilterDefinition<City>>
                {
                    builder.Regex(x => x.Name, new BsonRegularExpression(infoZipCode.Data.Localidade, "i")),
                    builder.Where(x => x.StateUf == infoZipCode.Data.Uf)
                };

                var city = await _cityRepository.GetCollectionAsync().Find(builder.And(conditions)).FirstOrDefaultAsync().ConfigureAwait(false);

                if (city == null)
                    return Ok(Utilities.ReturnSuccess(data: response));

                response.CityId = city._id.ToString();
                response.CityName = city.Name;
                response.StateId = city.StateId;
                response.StateUf = infoZipCode.Data.Uf;
                response.StateName = city.StateName;
                response.Ibge = infoZipCode.Data.Ibge;
                response.Gia = infoZipCode.Data.Gia;

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

    }
}