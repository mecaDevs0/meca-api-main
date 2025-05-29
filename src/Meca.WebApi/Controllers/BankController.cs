using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Meca.Data.Entities;
using Meca.Domain.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiExplorerSettings(IgnoreApi = true)]

    public class BankController : MainController
    {
        private readonly IBusinessBaseAsync<Bank> _bankRepository;
        private readonly IMapper _mapper;

        public BankController(IBusinessBaseAsync<Bank> bankRepository,
                              IMapper mapper,
                              IHttpContextAccessor httpContext,
                              IConfiguration configuration) : base(null, httpContext, configuration)
        {
            _bankRepository = bankRepository;
            _mapper = mapper;
        }


        /// <summary>
        /// LISTA DE BANCOS DISPONIVEIS PARA CADASTRO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<BankViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var listBank = await _bankRepository.FindAllAsync(Builders<Bank>.Sort.Ascending(nameof(Bank.Name))).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<BankViewModel>>(listBank)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}