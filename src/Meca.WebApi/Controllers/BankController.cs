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
                Console.WriteLine($"[BANK_DEBUG] Repository type: {_bankRepository.GetType().Name}");
                
                // Teste de conexão direta
                var testEntity = new Bank { Name = "Teste", Code = "001" };
                Console.WriteLine($"[BANK_DEBUG] Collection name: {testEntity.CollectionName}");
                
                // Teste sem filtro primeiro
                var allBanks = await _bankRepository.FindAllAsync().ConfigureAwait(false);
                var allBankList = allBanks?.ToList() ?? new List<Bank>();
                Console.WriteLine($"[BANK_DEBUG] Todos os bancos (sem filtro): {allBankList.Count}");
                
                // Teste com filtro
                var listBank = await _bankRepository.FindAllAsync(Builders<Bank>.Sort.Ascending(nameof(Bank.Name))).ConfigureAwait(false);
                var bankList = listBank?.ToList() ?? new List<Bank>();
                
                Console.WriteLine($"[BANK_DEBUG] Bancos encontrados (com filtro): {bankList.Count}");
                if (bankList.Count > 0)
                {
                    Console.WriteLine($"[BANK_DEBUG] Primeiro banco: {bankList[0].Name} - {bankList[0].Code}");
                }
                else
                {
                    Console.WriteLine("[BANK_DEBUG] Nenhum banco encontrado na coleção");
                }
                
                var mappedData = _mapper.Map<List<BankViewModel>>(bankList);
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

        /// <summary>
        /// TESTE DIRETO - CONEXÃO MONGODB
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestDirectConnection()
        {
            try
            {
                Console.WriteLine("[BANK_TEST] Iniciando teste de conexão direta");
                
                // Teste direto com MongoDB
                var connectionString = "mongodb+srv://pedrosantana:qsmEphWv3dQ2wSGk@cluster0.ccsupmg.mongodb.net/meca-app-2025?retryWrites=true&w=majority";
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("meca-app-2025");
                var collection = database.GetCollection<Bank>("Bank");
                
                Console.WriteLine($"[BANK_TEST] Collection name: Bank");
                Console.WriteLine($"[BANK_TEST] Database name: meca-app-2025");
                
                var count = await collection.CountDocumentsAsync(Builders<Bank>.Filter.Empty);
                Console.WriteLine($"[BANK_TEST] Total de documentos na coleção: {count}");
                
                var banks = await collection.Find(Builders<Bank>.Filter.Empty).Limit(5).ToListAsync();
                Console.WriteLine($"[BANK_TEST] Bancos encontrados (diretamente): {banks.Count}");
                
                if (banks.Count > 0)
                {
                    Console.WriteLine($"[BANK_TEST] Primeiro banco: {banks[0].Name} - {banks[0].Code}");
                }
                
                return Ok(new { 
                    message = "Teste de conexão direta", 
                    count = count, 
                    banks = banks.Select(b => new { b.Name, b.Code }).ToList() 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BANK_TEST] Erro: {ex.Message}");
                Console.WriteLine($"[BANK_TEST] Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}