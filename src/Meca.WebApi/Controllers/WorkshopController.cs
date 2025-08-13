using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meca.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WorkshopController : ControllerBase
    {
        private readonly IMongoDatabase _database;

        public WorkshopController(IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("[WORKSHOP_CTRL] Inicializando WorkshopController");
                Console.WriteLine("[WORKSHOP_CTRL] ==================================");
                
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine($"[WORKSHOP_CTRL] Connection String obtida: {!string.IsNullOrEmpty(connectionString)}");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("[WORKSHOP_CTRL] ERRO: Connection string está vazia");
                    throw new InvalidOperationException("Connection string não encontrada");
                }
                
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase("meca-app-2025");
                
                Console.WriteLine("[WORKSHOP_CTRL] Conexão com MongoDB estabelecida");
                Console.WriteLine("[WORKSHOP_CTRL] ==================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_CTRL] ERRO na inicialização: {ex.Message}");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_GET] ==================================");
                Console.WriteLine("[WORKSHOP_GET] Endpoint principal chamado");
                Console.WriteLine("[WORKSHOP_GET] ==================================");
                
                var collection = _database.GetCollection<dynamic>("Workshop");
                Console.WriteLine("[WORKSHOP_GET] Collection obtida com sucesso");
                
                // Contar documentos
                var totalCount = await collection.CountDocumentsAsync("{}");
                Console.WriteLine($"[WORKSHOP_GET] Total de documentos encontrados: {totalCount}");
                
                // Buscar documentos
                var workshops = await collection.Find("{}").Limit(30).ToListAsync();
                Console.WriteLine($"[WORKSHOP_GET] Encontradas {workshops.Count} oficinas");
                
                Console.WriteLine("[WORKSHOP_GET] Retornando resposta de sucesso");
                Console.WriteLine("[WORKSHOP_GET] ==================================");
                
                return Ok(new
                {
                    data = workshops,
                    erro = false,
                    message = "Sucesso",
                    count = workshops.Count,
                    totalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_GET] ==================================");
                Console.WriteLine($"[WORKSHOP_GET] ERRO: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_GET] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[WORKSHOP_GET] ==================================");
                
                return BadRequest(new
                {
                    data = (object)null,
                    erro = true,
                    message = "Erro interno",
                    messageEx = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("test")]
        public IActionResult Test()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_TEST] ==================================");
                Console.WriteLine("[WORKSHOP_TEST] Endpoint de teste chamado");
                Console.WriteLine("[WORKSHOP_TEST] ==================================");
                
                return Ok(new
                {
                    data = new List<object>(),
                    erro = false,
                    message = "Sucesso",
                    count = 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_TEST] Erro: {ex.Message}");
                return BadRequest(new
                {
                    data = (object)null,
                    erro = true,
                    message = "Erro interno",
                    messageEx = ex.Message
                });
            }
        }
    }
}
