using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meca.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WorkshopController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WorkshopController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Console.WriteLine("[WORKSHOP_CTRL] WorkshopController inicializado com sucesso");
        }

        /// <summary>
        /// OFICINA - METODO PARA LISTAR TODOS PAGINADO OU C/S FILTRO
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] WorkshopFilterViewModel filterModel)
        {
            try
            {
                Console.WriteLine("[WORKSHOP_GET] Endpoint principal chamado");
                
                // Verificar se o filtro é null
                if (filterModel == null)
                {
                    Console.WriteLine("[WORKSHOP_GET] FilterModel é null, criando novo");
                    filterModel = new WorkshopFilterViewModel();
                }
                
                Console.WriteLine($"[WORKSHOP_GET] Filtros - Search: '{filterModel.Search}', Rating: {filterModel.Rating}, Page: {filterModel.Page}, Limit: {filterModel.Limit}");
                
                // Obter connection string de forma mais robusta
                var connectionString = _configuration.GetConnectionString("DefaultConnection") ??
                                     _configuration.GetSection("DATABASE:ConnectionString").Value ??
                                     _configuration.GetSection("MongoDb:ConnectionString").Value;
                
                Console.WriteLine($"[WORKSHOP_GET] Connection String obtida: {!string.IsNullOrEmpty(connectionString)}");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("[WORKSHOP_GET] ERRO: Connection string não encontrada");
                    return BadRequest(new
                    {
                        data = (object)null,
                        erro = true,
                        message = "Configuração de banco de dados não encontrada",
                        messageEx = "Connection string não configurada"
                    });
                }
                
                // Conectar ao MongoDB
                var client = new MongoClient(connectionString);
                var databaseName = _configuration.GetSection("MongoDb:DatabaseName").Value ??
                                 _configuration.GetSection("DATABASE:Name").Value ??
                                 "meca-app-2025";
                
                var database = client.GetDatabase(databaseName);
                Console.WriteLine($"[WORKSHOP_GET] Conectado ao banco: {databaseName}");
                
                var collection = database.GetCollection<dynamic>("Workshop");
                Console.WriteLine("[WORKSHOP_GET] Collection obtida com sucesso");
                
                // Filtros básicos - usar string vazia em vez de null
                var filter = Builders<dynamic>.Filter.And(
                    Builders<dynamic>.Filter.Eq("Disabled", null),
                    Builders<dynamic>.Filter.Eq("DataBlocked", null)
                );

                Console.WriteLine("[WORKSHOP_GET] Filtros básicos aplicados");

                // Aplicar filtros adicionais se fornecidos
                if (!string.IsNullOrEmpty(filterModel.Search))
                {
                    Console.WriteLine($"[WORKSHOP_GET] Aplicando filtro de busca: {filterModel.Search}");
                    var searchFilter = Builders<dynamic>.Filter.Regex(
                        "CompanyName", 
                        new MongoDB.Bson.BsonRegularExpression(filterModel.Search, "i")
                    );
                    filter = Builders<dynamic>.Filter.And(filter, searchFilter);
                }

                if (filterModel.Rating.HasValue)
                {
                    Console.WriteLine($"[WORKSHOP_GET] Aplicando filtro de rating: {filterModel.Rating}");
                    var ratingFilter = Builders<dynamic>.Filter.Eq("Rating", filterModel.Rating.Value);
                    filter = Builders<dynamic>.Filter.And(filter, ratingFilter);
                }

                // Paginação
                var page = filterModel.Page ?? 1;
                var limit = filterModel.Limit ?? 30;
                var skip = (page - 1) * limit;

                Console.WriteLine($"[WORKSHOP_GET] Buscando oficinas - página: {page}, limite: {limit}, skip: {skip}");

                // Primeiro, contar documentos para debug
                var totalCount = await collection.CountDocumentsAsync(filter);
                Console.WriteLine($"[WORKSHOP_GET] Total de documentos encontrados: {totalCount}");

                var workshops = await collection
                    .Find(filter)
                    .Skip(skip)
                    .Limit(limit)
                    .ToListAsync();

                Console.WriteLine($"[WORKSHOP_GET] Encontradas {workshops.Count} oficinas após paginação");

                // Mapear os dados para o formato esperado pelo frontend
                var mappedWorkshops = workshops.Select(workshop => new
                {
                    id = workshop._id?.ToString(),
                    fullName = workshop.FullName,
                    companyName = workshop.CompanyName,
                    phone = workshop.Phone,
                    cnpj = workshop.Cnpj,
                    zipCode = workshop.ZipCode,
                    streetAddress = workshop.StreetAddress,
                    number = workshop.Number,
                    cityName = workshop.CityName,
                    cityId = workshop.CityId,
                    stateName = workshop.StateName,
                    stateUf = workshop.StateUf,
                    stateId = workshop.StateId,
                    neighborhood = workshop.Neighborhood,
                    complement = workshop.Complement,
                    latitude = workshop.Latitude,
                    longitude = workshop.Longitude,
                    photo = workshop.Photo,
                    meiCard = workshop.MeiCard,
                    email = workshop.Email,
                    password = workshop.Password,
                    rating = workshop.Rating != null ? (int)workshop.Rating : null,
                    distance = workshop.Distance != null ? (int)workshop.Distance : null,
                    reason = workshop.Reason
                }).ToList();

                Console.WriteLine($"[WORKSHOP_GET] {mappedWorkshops.Count} oficinas mapeadas para o frontend");

                return Ok(new
                {
                    data = mappedWorkshops,
                    erro = false,
                    message = "Sucesso",
                    count = mappedWorkshops.Count,
                    totalCount = totalCount,
                    page = page,
                    limit = limit
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_GET] ERRO: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_GET] Stack trace: {ex.StackTrace}");
                
                return BadRequest(new
                {
                    data = (object)null,
                    erro = true,
                    message = "Erro interno",
                    messageEx = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// OFICINA - TESTE SIMPLES
        /// </summary>
        [AllowAnonymous]
        [HttpGet("test")]
        public IActionResult Test()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_TEST] Endpoint de teste chamado");
                
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