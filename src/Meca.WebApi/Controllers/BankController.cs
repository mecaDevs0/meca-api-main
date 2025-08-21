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
                Console.WriteLine("[BANK_DEBUG] Iniciando busca de bancos");
                Console.WriteLine($"[BANK_DEBUG] Repository type: {_bankRepository.GetType().Name}");
                
                var listBank = await _bankRepository.FindAllAsync(Builders<Bank>.Sort.Ascending(nameof(Bank.Name))).ConfigureAwait(false);
                
                Console.WriteLine($"[BANK_DEBUG] Bancos encontrados: {listBank?.Count ?? 0}");
                if (listBank != null && listBank.Count > 0)
                {
                    Console.WriteLine($"[BANK_DEBUG] Primeiro banco: {listBank[0].Name} - {listBank[0].Code}");
                }
                
                var mappedData = _mapper.Map<List<BankViewModel>>(listBank);
                Console.WriteLine($"[BANK_DEBUG] Dados mapeados: {mappedData?.Count ?? 0}");

                return Ok(Utilities.ReturnSuccess(data: mappedData));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BANK_DEBUG] Erro: {ex.Message}");
                Console.WriteLine($"[BANK_DEBUG] Stack trace: {ex.StackTrace}");
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}