using System;
using System.Collections.Generic;
using System.Linq;
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
                
                var listBank = await _bankRepository.FindAllAsync(Builders<Bank>.Sort.Ascending(nameof(Bank.Name))).ConfigureAwait(false);
                
                Console.WriteLine($"[BANK_DEBUG] Encontrados {listBank.Count()} bancos no banco de dados");

                // Remover duplicatas por nome, mantendo apenas o primeiro (código principal)
                var uniqueBanks = listBank
                    .GroupBy(b => b.Name)
                    .Select(g => g.First())
                    .OrderBy(b => b.Name)
                    .ToList();
                
                Console.WriteLine($"[BANK_DEBUG] {uniqueBanks.Count} bancos únicos após remoção de duplicatas");

                // Mapeamento manual para evitar problemas com AutoMapper
                var bankViewModels = uniqueBanks.Select(b => new BankViewModel
                {
                    Id = b._id?.ToString() ?? "",
                    Code = b.Code ?? "",
                    Name = b.Name ?? ""
                }).ToList();

                Console.WriteLine($"[BANK_DEBUG] Mapeamento concluído com sucesso");
                
                return Ok(Utilities.ReturnSuccess(data: bankViewModels));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BANK_ERROR] Erro ao buscar bancos: {ex.Message}");
                Console.WriteLine($"[BANK_ERROR] Stack trace: {ex.StackTrace}");
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}