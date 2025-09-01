using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Core3.Interface;
using Microsoft.Extensions.Hosting;
using UtilityFramework.Services.Iugu.Core3.Interface;
using Meca.Domain.ViewModels.Filters;
using Meca.Data.Enum;
using UtilityFramework.Application.Core3.JwtMiddleware;
using System.Security.Claims;
using System.IO;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class WorkshopController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IBusinessBaseAsync<Notification> _notificationRepository;
        private readonly ISenderMailService _senderMailService;
        private readonly IWorkshopService _workshopService;
        private readonly IIuguMarketPlaceServices _iuguMarketPlaceServices;
        private readonly bool _isSandbox;


        public WorkshopController(IMapper mapper,
            IBusinessBaseAsync<Workshop> workshopRepository,
            IBusinessBaseAsync<Notification> notificationRepository,
            ISenderMailService senderMailService,
            IWorkshopService workshopService,
            IHttpContextAccessor context,
            IIuguMarketPlaceServices iuguMarketPlaceServices,
            IHostingEnvironment env,
            IConfiguration configuration)
        {
            try
            {
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ===== INICIANDO CONSTRUTOR WORKSHOP CONTROLLER =====");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] mapper é null: {mapper == null}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] workshopRepository é null: {workshopRepository == null}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] notificationRepository é null: {notificationRepository == null}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] senderMailService é null: {senderMailService == null}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] workshopService é null: {workshopService == null}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] context é null: {context == null}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] iuguMarketPlaceServices é null: {iuguMarketPlaceServices == null}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] env é null: {env == null}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] configuration é null: {configuration == null}");
                
                // VALIDAÇÃO DEFINITIVA: Verificar se todas as dependências estão presentes
                if (mapper == null)
                {
                    Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO: mapper é null");
                    throw new ArgumentNullException(nameof(mapper), "AutoMapper não foi injetado corretamente");
                }
                
                if (workshopRepository == null)
                {
                    Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO: workshopRepository é null");
                    throw new ArgumentNullException(nameof(workshopRepository), "WorkshopRepository não foi injetado corretamente");
                }
                
                if (notificationRepository == null)
                {
                    Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO: notificationRepository é null");
                    throw new ArgumentNullException(nameof(notificationRepository), "NotificationRepository não foi injetado corretamente");
                }
                
                if (senderMailService == null)
                {
                    Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO: senderMailService é null");
                    throw new ArgumentNullException(nameof(senderMailService), "SenderMailService não foi injetado corretamente");
                }
                
                if (workshopService == null)
                {
                    Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO: workshopService é null");
                    throw new ArgumentNullException(nameof(workshopService), "WorkshopService não foi injetado corretamente");
                }
                
                if (iuguMarketPlaceServices == null)
                {
                    Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO: iuguMarketPlaceServices é null");
                    throw new ArgumentNullException(nameof(iuguMarketPlaceServices), "IuguMarketPlaceServices não foi injetado corretamente");
                }
                
                if (env == null)
                {
                    Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO: env é null");
                    throw new ArgumentNullException(nameof(env), "HostingEnvironment não foi injetado corretamente");
                }
                
                if (configuration == null)
                {
                    Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO: configuration é null");
                    throw new ArgumentNullException(nameof(configuration), "Configuration não foi injetado corretamente");
                }
                
                // ATRIBUIÇÃO DEFINITIVA: Atribuir as dependências
                _mapper = mapper;
                _workshopRepository = workshopRepository;
                _notificationRepository = notificationRepository;
                _senderMailService = senderMailService;
                _workshopService = workshopService;
                _iuguMarketPlaceServices = iuguMarketPlaceServices;
                _isSandbox = Util.IsSandBox(env);
                
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] Todas as dependências foram validadas e atribuídas com sucesso");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] _isSandbox: {_isSandbox}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ===== CONSTRUTOR WORKSHOP CONTROLLER FINALIZADO COM SUCESSO =====");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] ERRO CRÍTICO no construtor: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_CONSTRUCTOR_DEBUG] Stack trace: {ex.StackTrace}");
                throw;
            }
        }



        /// <summary>
        /// OFICINA - METODO PARA LISTAR TODOS PAGINADO OU C/S FILTRO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<WorkshopViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromQuery] WorkshopFilterViewModel filterModel)
        {
            try
            {
                Console.WriteLine("[WORKSHOP_CONTROLLER_DEBUG] Iniciando método Get");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] filterModel é null: {filterModel == null}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] _workshopService é null: {_workshopService == null}");
                
                // Teste simples: retornar dados mock primeiro
                var mockWorkshops = new List<WorkshopViewModel>
                {
                    new WorkshopViewModel
                    {
                        Id = "test123",
                        CompanyName = "Oficina Teste",
                        Address = "Rua Teste, 123",
                        Phone = "(11) 99999-9999",
                        Email = "teste@oficina.com"
                    }
                };
                
                if (filterModel.HasFilter() == false)
                    return Ok(Utilities.ReturnSuccess(data: await _workshopService.GetAll()));
                else
                    return Ok(Utilities.ReturnSuccess(data: await _workshopService.GetAll<WorkshopViewModel>(filterModel)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Erro no método Get: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Stack trace: {ex.StackTrace}");
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - ENDPOINT DE TESTE COM DADOS REAIS DO MONGODB
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("TestReal")]
        [Produces("application/json")]
        public async Task<IActionResult> TestReal()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_TEST_REAL_DEBUG] Iniciando endpoint de teste com dados reais");
                
                // Usar diretamente o repositório para buscar dados reais
                var workshops = await _workshopRepository.FindAllAsync();
                Console.WriteLine($"[WORKSHOP_TEST_REAL_DEBUG] Encontradas {workshops.Count} oficinas no banco");
                
                var workshopsList = workshops.Select(w => new
                {
                    Id = w._id?.ToString(),
                    CompanyName = w.CompanyName,
                    Address = w.Address,
                    Phone = w.Phone,
                    Email = w.Email,
                    Latitude = w.Latitude,
                    Longitude = w.Longitude,
                    Photo = w.Photo
                }).ToList();
                
                Console.WriteLine("[WORKSHOP_TEST_REAL_DEBUG] Retornando dados reais do MongoDB");
                return Ok(new { data = workshopsList, erro = false, message = "Sucesso" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_TEST_REAL_DEBUG] Erro no endpoint de teste: {ex.Message}");
                return BadRequest(new { data = (object)null, erro = true, message = ex.Message });
            }
        }

        /// <summary>
        /// OFICINA - ENDPOINT SIMPLES SEM DEPENDÊNCIAS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("Simple")]
        [Produces("application/json")]
        public IActionResult Simple()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_SIMPLE_DEBUG] Iniciando endpoint simples");
                
                // Retornar dados simples sem usar nenhum serviço
                var simpleData = new
                {
                    message = "Endpoint funcionando",
                    timestamp = DateTime.Now,
                    status = "OK"
                };
                
                Console.WriteLine("[WORKSHOP_SIMPLE_DEBUG] Retornando dados simples");
                return Ok(simpleData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_SIMPLE_DEBUG] Erro no endpoint simples: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// OFICINA - ENDPOINT DIRETO COM MONGODB
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("Direct")]
        [Produces("application/json")]
        public async Task<IActionResult> Direct()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_DIRECT_DEBUG] Iniciando endpoint direto");
                
                // Usar diretamente o repositório para buscar dados reais
                var workshops = await _workshopRepository.FindAllAsync();
                Console.WriteLine($"[WORKSHOP_DIRECT_DEBUG] Encontradas {workshops.Count} oficinas no banco");
                
                var workshopsList = workshops.Select(w => new
                {
                    Id = w._id?.ToString(),
                    CompanyName = w.CompanyName,
                    Address = w.Address,
                    Phone = w.Phone,
                    Email = w.Email,
                    Latitude = w.Latitude,
                    Longitude = w.Longitude,
                    Photo = w.Photo
                }).ToList();
                
                Console.WriteLine("[WORKSHOP_DIRECT_DEBUG] Retornando dados reais do MongoDB");
                return Ok(workshopsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_DIRECT_DEBUG] Erro no endpoint direto: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// OFICINA - ENDPOINT DE TESTE DE CONFIGURAÇÃO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("ConfigTest")]
        [Produces("application/json")]
        public IActionResult ConfigTest()
        {
            try
            {
                Console.WriteLine("[CONFIG_TEST_DEBUG] Iniciando teste de configuração");
                
                var currentDir = Directory.GetCurrentDirectory();
                Console.WriteLine($"[CONFIG_TEST_DEBUG] Diretório atual: {currentDir}");
                
                var appsettingsPath = Path.Combine(currentDir, "appsettings.Production.json");
                Console.WriteLine($"[CONFIG_TEST_DEBUG] Caminho do appsettings: {appsettingsPath}");
                
                var fileExists = File.Exists(appsettingsPath);
                Console.WriteLine($"[CONFIG_TEST_DEBUG] Arquivo existe: {fileExists}");
                
                if (fileExists)
                {
                    var content = File.ReadAllText(appsettingsPath);
                    var hasStripeKey = content.Contains("Stripe:SecretKey");
                    Console.WriteLine($"[CONFIG_TEST_DEBUG] Contém Stripe:SecretKey: {hasStripeKey}");
                }
                
                return Ok(new { 
                    currentDirectory = currentDir,
                    appsettingsPath = appsettingsPath,
                    fileExists = fileExists,
                    message = "Teste de configuração concluído"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONFIG_TEST_DEBUG] Erro no teste de configuração: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// OFICINA - OBTER DETALHES
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [HttpGet("Detail/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<WorkshopViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Detail([FromRoute] string id, string latUser, string longUser)
        {
            try
            {
                var response = await _workshopService.Detail(id, latUser, longUser);

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - OBTER INFORMAÇÕES
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetInfo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<WorkshopViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInfo()
        {
            try
            {
                Console.WriteLine("[WORKSHOP_CONTROLLER_DEBUG] Iniciando GetInfo no controller");
                
                var response = await _workshopService.GetInfo();
                
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Response do GetInfo: {(response == null ? "NULL" : "NOT NULL")}");
                if (response != null)
                {
                    Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Response ID: {response.Id}");
                    Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Response CompanyName: {response.CompanyName}");
                }

                // Verificar se o response é null e retornar erro apropriado
                if (response == null)
                {
                    Console.WriteLine("[WORKSHOP_CONTROLLER_DEBUG] Response é NULL - retornando erro");
                    return BadRequest(Utilities.ReturnErro("Workshop não encontrado ou dados inválidos"));
                }

                Console.WriteLine("[WORKSHOP_CONTROLLER_DEBUG] Retornando resposta com sucesso");
                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] ERRO no controller: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Stack trace: {ex.StackTrace}");
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - REMOVER
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
                var response = await _workshopService.Delete(id);

                return Ok(Utilities.ReturnSuccess(message: DefaultMessages.Deleted));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// OFICINA - REMOVER
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpDelete("Delete/Stripe/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteStripe([FromRoute] string id)
        {
            try
            {
                await _workshopService.DeleteStripe(id);

                return Ok(Utilities.ReturnSuccess(message: DefaultMessages.Deleted));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - REMOVER POR E-MAIL
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("DeleteByEmail")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteByEmail([FromQuery] string email)
        {
            try
            {
                var response = await _workshopService.DeleteByEmail(email);

                return Ok(Utilities.ReturnSuccess(message: DefaultMessages.Deleted));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - REGISTRAR
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///         {
        ///           "password": "string",
        ///           "providerId": "string", //opcional
        ///           "typeProvider": 0, //opcional
        ///           "fullName": "string",
        ///           "companyName": "string",
        ///           "login": "string",
        ///           "email": "contato@mecabr.com",
        ///           "photo": "string", /*nome do arquivo ex: 302183239.png*/
        ///           "cnpj": "18.771.876/0001-05",
        ///           "phone": "(11) 2020-2020",
        ///           "meiCard": "string", /*nome do arquivo ex: 302183239.png*/
        ///         }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [HttpPost("Register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<WorkshopViewModel>), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] WorkshopRegisterViewModel model)
        {
            try
            {
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Iniciando registro no controller");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Model recebido: {System.Text.Json.JsonSerializer.Serialize(model)}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] MeiCard no model: {model.MeiCard}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Photo no model: {model.Photo}");
                
                model.TrimStringProperties();


                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] ModelState.IsValid: {ModelState.IsValid}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] ModelState errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");

                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Chamando _workshopService.Register...");
                var response = await _workshopService.Register(model);
                
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Response do Register: {(response == null ? "NULL" : "NOT NULL")}");
                if (response != null)
                {
                    Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Response data: {System.Text.Json.JsonSerializer.Serialize(response)}");
                }

                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Retornando resposta...");
                var result = Ok(Utilities.ReturnSuccess(data: response, message: "Agradecemos pelas informações, nossa equipe efetuará uma análise e em breve você receberá um e-mail com a liberação de acesso à plataforma."));
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Resposta retornada: {result.GetType().Name}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] ERRO no controller: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Stack trace: {ex.StackTrace}");
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - LOGIN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "email":"string", //optional
        ///              "password":"string", //optional
        ///              "providerId":"string", //optional
        ///              "typeProvider":0 //Enum (optional)
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Token([FromBody] LoginViewModel model)
        {
            try
            {
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] Iniciando método Token");
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] Model recebido: {System.Text.Json.JsonSerializer.Serialize(model)}");
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] _workshopService é null: {_workshopService == null}");

                // VALIDAÇÃO DEFINITIVA: Verificar se o model não é null
                if (model == null)
                {
                    Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] ERRO: Model é null");
                    return BadRequest(Utilities.ReturnErro("Dados de login inválidos"));
                }

                // VALIDAÇÃO DEFINITIVA: Verificar se o WorkshopService está inicializado
                if (_workshopService == null)
                {
                    Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] ERRO CRÍTICO: _workshopService é null");
                    return StatusCode(500, Utilities.ReturnErro("Erro interno do servidor: WorkshopService não inicializado"));
                }

                // VALIDAÇÃO DEFINITIVA: Verificar se todas as dependências estão funcionando
                if (_mapper == null)
                {
                    Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] ERRO CRÍTICO: _mapper é null");
                    return StatusCode(500, Utilities.ReturnErro("Erro interno do servidor: AutoMapper não inicializado"));
                }

                // PROCESSAMENTO SEGURO: Limpar e validar dados de entrada
                model.TrimStringProperties();
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] Model após TrimStringProperties: {System.Text.Json.JsonSerializer.Serialize(model)}");

                // VALIDAÇÃO DEFINITIVA: Verificar se os dados obrigatórios estão presentes
                if (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.ProviderId))
                {
                    Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] ERRO: Email e ProviderId estão vazios");
                    return BadRequest(Utilities.ReturnErro("Email ou ProviderId é obrigatório"));
                }

                if (model.TypeProvider == TypeProvider.Password && string.IsNullOrEmpty(model.Password))
                {
                    Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] ERRO: Senha está vazia para login com senha");
                    return BadRequest(Utilities.ReturnErro("Senha é obrigatória para login com senha"));
                }

                // EXECUÇÃO DEFINITIVA: Chamar o WorkshopService com tratamento robusto
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] Chamando _workshopService.Token");
                var result = await _workshopService.Token(model);
                
                if (result != null)
                {
                    Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] Token gerado com sucesso via WorkshopService");
                    return Ok(Utilities.ReturnSuccess(data: result));
                }
                else
                {
                    Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] Falha na autenticação via WorkshopService");
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidLogin));
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] ERRO DE INJEÇÃO DE DEPENDÊNCIA: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] Stack trace: {ex.StackTrace}");
                return StatusCode(500, Utilities.ReturnErro("Erro interno do servidor: Dependência não inicializada"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] ERRO GERAL: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_TOKEN_DEBUG] Stack trace: {ex.StackTrace}");
                return StatusCode(500, Utilities.ReturnErro("Erro interno do servidor. Tente novamente."));
            }
        }

        /// <summary>
        /// OFICINA - BLOQUEAR / DESBLOQUEAR - ADMIN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "id": "string", // required
        ///              "block": true,
        ///              "reason": "" //motivo de bloquear a Oficina
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("BlockUnBlock")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [OnlyAdministrator]
        public async Task<IActionResult> BlockUnBlock([FromBody] BlockViewModel model)
        {
            try
            {
                model.TrimStringProperties();


                var response = await _workshopService.BlockUnBlock(model);

                return Ok(Utilities.ReturnSuccess(data: response != null, message: response?.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - ALTERAR SENHA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "currentPassword":"string",
        ///              "newPassword":"string",
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ChangePassword")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {
                model.TrimStringProperties();


                var response = await _workshopService.ChangePassword(model);

                return Ok(Utilities.ReturnSuccess(data: response, message: DefaultMessages.PasswordChanged));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - METODO DE LISTAGEM COM DATATABLE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<DtResult<WorkshopViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] WorkshopFilterViewModel filterView)
        {
            try
            {
                Console.WriteLine("[WORKSHOP_CONTROLLER_DEBUG] Iniciando LoadData no controller");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Model: {System.Text.Json.JsonSerializer.Serialize(model)}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] FilterView: {System.Text.Json.JsonSerializer.Serialize(filterView)}");

                // Usar LoadDataGrid em vez de LoadData para testar
                var response = await _workshopService.LoadDataGrid(model, filterView);

                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Response Data count: {response?.Data?.Count ?? 0}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Response RecordsTotal: {response?.RecordsTotal ?? 0}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Response RecordsFiltered: {response?.RecordsFiltered ?? 0}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Erro no controller: {ex.Message}");
                Console.WriteLine($"[WORKSHOP_CONTROLLER_DEBUG] Stack trace: {ex.StackTrace}");
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - REGISTRAR E REMOVER DEVICE ID ONESIGNAL OU FCM | CHAMAR APOS LOGIN E LOGOUT
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "deviceId":"string",
        ///              "isRegister":true  // true => registrar  | false => remover
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterUnRegisterDeviceId")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterUnRegisterDeviceIdAsync([FromBody] PushViewModel model)
        {
            try
            {
                model.TrimStringProperties();


                await _workshopService.RegisterUnRegisterDeviceId(model);

                return Ok(Utilities.ReturnSuccess());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - ATUALIZAR DADOS
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///         {
        ///           "password": "string",
        ///           "providerId": "string", //opcional
        ///           "typeProvider": 0, //opcional
        ///           "fullName": "string",
        ///           "companyName": "string",
        ///           "login": "string",
        ///           "email": "contato@mecabr.com",
        ///           "photo": "string", /*nome do arquivo ex: 302183239.png*/
        ///           "cnpj": "18.771.876/0001-05",
        ///           "phone": "(11) 2020-2020",
        ///           "meiCard": "string", /*nome do arquivo ex: 302183239.png*/
        ///         }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [HttpPost("Update")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<WorkshopViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdatePatch([FromRoute] string id, [FromBody] WorkshopRegisterViewModel model)
        {
            try
            {
                model.TrimStringProperties();


                var response = await _workshopService.UpdatePatch(id, model);

                return Ok(Utilities.ReturnSuccess(data: response, message: DefaultMessages.Updated));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - ESQUECI A SENHA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "email":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ForgotPassword([FromBody] LoginViewModel model)
        {
            try
            {
                model.TrimStringProperties();


                var response = await _workshopService.ForgotPassword(model);

                return Ok(Utilities.ReturnSuccess(data: response, message: DefaultMessages.VerifyYourEmail));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - VERIFICAR SE E-MAIL ESTÁ EM USO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///             POST
        ///             {
        ///                 "email":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("CheckEmail")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckEmail([FromBody] ValidationViewModel model)
        {
            try
            {
                model.TrimStringProperties();


                var response = await _workshopService.CheckEmail(model);

                return Ok(Utilities.ReturnSuccess(response ? DefaultMessages.Available : DefaultMessages.EmailInUse));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - VERIFICAR SE CNPJ ESTÁ EM USO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///             POST
        ///             {
        ///                 "cnpj":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("CheckCnpj")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckCnpj([FromBody] ValidationViewModel model)
        {
            try
            {
                model.TrimStringProperties();


                var response = await _workshopService.CheckCnpj(model);

                return Ok(Utilities.ReturnSuccess(response ? DefaultMessages.Available : DefaultMessages.CnpjInUse));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - VERIFICAR SE CAMPOS ESTÁ EM USO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///             POST
        ///             {
        ///                 "cnpj":"string",
        ///                 "email":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("CheckAll")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckAll([FromBody] ValidationViewModel model)
        {
            try
            {
                model.TrimStringProperties();


                var response = await _workshopService.CheckAll(model);

                return Ok(Utilities.ReturnSuccess(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - ATUALIZAR DADOS BANCÁRIOS |  Bank = enviar CODIGO DO BANCO | EX = 341
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPatch("UpdateDataBank/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateDataBank([FromRoute] string id, [FromBody] DataBankViewModel model)
        {
            Console.WriteLine($"[CONTROLLER_DEBUG] ===== UpdateDataBank INICIADO =====");
            Console.WriteLine($"[CONTROLLER_DEBUG] UpdateDataBank chamado - ID: {id}");
            Console.WriteLine($"[CONTROLLER_DEBUG] Request Path: {Request.Path}");
            Console.WriteLine($"[CONTROLLER_DEBUG] Request Method: {Request.Method}");
            Console.WriteLine($"[CONTROLLER_DEBUG] Request Headers: {string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
            Console.WriteLine($"[CONTROLLER_DEBUG] Request Body: {Request.Body}");
            Console.WriteLine($"[CONTROLLER_DEBUG] Request ContentType: {Request.ContentType}");
            Console.WriteLine($"[CONTROLLER_DEBUG] Request ContentLength: {Request.ContentLength}");
            
            try
            {
                Console.WriteLine($"[CONTROLLER_DEBUG] Model recebido: {System.Text.Json.JsonSerializer.Serialize(model)}");
                Console.WriteLine($"[CONTROLLER_DEBUG] Model é null: {model == null}");
                
                if (model == null)
                {
                    Console.WriteLine("[CONTROLLER_DEBUG] Model é null - retornando erro");
                    return BadRequest(Utilities.ReturnErro("Dados bancários inválidos"));
                }

                model.TrimStringProperties();
                Console.WriteLine($"[CONTROLLER_DEBUG] Model após TrimStringProperties: {System.Text.Json.JsonSerializer.Serialize(model)}");

                Console.WriteLine("[CONTROLLER_DEBUG] Chamando _workshopService.UpdateDataBank...");
                var response = await _workshopService.UpdateDataBank(model, id);
                Console.WriteLine($"[CONTROLLER_DEBUG] Response do UpdateDataBank: {response}");

                return Ok(Utilities.ReturnSuccess(data: response, message: DefaultMessages.Updated));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER_DEBUG] ERRO no UpdateDataBank: {ex.Message}");
                Console.WriteLine($"[CONTROLLER_DEBUG] StackTrace: {ex.StackTrace}");
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - OBTER DADOS BANCÁRIOS DA OFICINA LOGADO OU VIA ID
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetDataBank/{id}")]
        [HttpGet("GetDataBank")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(typeof(DataBankViewModel), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDataBank([FromRoute] string id)
        {
            try
            {
                var response = await _workshopService.GetDataBank(id);

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// OFICINA - ATUALIZAR WORKSHOPS SEM FOTO E DESCRIÇÃO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("UpdateWorkshopsWithoutPhotoAndReason")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateWorkshopsWithoutPhotoAndReason()
        {
            try
            {
                var result = await _workshopService.UpdateWorkshopsWithoutPhotoAndReason();

                if (result)
                {
                    return Ok(Utilities.ReturnSuccess("Workshops atualizados com sucesso"));
                }
                else
                {
                    return BadRequest(Utilities.ReturnErro("Erro ao atualizar workshops"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// TESTE - ENDPOINT PÚBLICO PARA VERIFICAR SE A API ESTÁ FUNCIONANDO
        /// </summary>
        [AllowAnonymous]
        [HttpGet("test")]
        [Produces("application/json")]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "API funcionando corretamente", 
                timestamp = DateTime.UtcNow,
                workshops = "Endpoint de teste criado com sucesso"
            });
        }

        /// <summary>
        /// TEMPORÁRIO - CONFIGURAR AGENDA E SERVIÇOS PARA OFICINA MC
        /// </summary>
        [AllowAnonymous]
        [HttpPost("setup-workshop-mc")]
        [Produces("application/json")]
        public async Task<IActionResult> SetupWorkshopMC()
        {
            try
            {
                var workshopId = "68a60a7a092c6cce3b52a96d"; // ID da Oficina MC
                
                // Verificar se a oficina existe
                var workshop = await _workshopRepository.FindByIdAsync(workshopId);
                if (workshop == null)
                {
                    return BadRequest(Utilities.ReturnErro("Oficina não encontrada"));
                }

                // Criar dados de agenda e serviços diretamente no MongoDB
                var agendaData = new
                {
                    Sunday = new { Open = false, StartTime = "", ClosingTime = "", StartOfBreak = "", EndOfBreak = "" },
                    Monday = new { Open = true, StartTime = "08:00", ClosingTime = "18:00", StartOfBreak = "12:00", EndOfBreak = "13:00" },
                    Tuesday = new { Open = true, StartTime = "08:00", ClosingTime = "18:00", StartOfBreak = "12:00", EndOfBreak = "13:00" },
                    Wednesday = new { Open = true, StartTime = "08:00", ClosingTime = "18:00", StartOfBreak = "12:00", EndOfBreak = "13:00" },
                    Thursday = new { Open = true, StartTime = "08:00", ClosingTime = "18:00", StartOfBreak = "12:00", EndOfBreak = "13:00" },
                    Friday = new { Open = true, StartTime = "08:00", ClosingTime = "18:00", StartOfBreak = "12:00", EndOfBreak = "13:00" },
                    Saturday = new { Open = false, StartTime = "", ClosingTime = "", StartOfBreak = "", EndOfBreak = "" },
                    Workshop = new { Id = workshopId, CompanyName = workshop.CompanyName, FullName = workshop.FullName },
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                var serviceData = new
                {
                    Service = new { Id = "682dcd131236edc26160c572", Name = "Mecânica geral" },
                    QuickService = false,
                    MinTimeScheduling = 1.0,
                    Description = "Serviços gerais de mecânica automotiva",
                    EstimatedTime = 2.0,
                    Photo = "mecanica-geral.png",
                    Workshop = new { Id = workshopId, CompanyName = workshop.CompanyName, FullName = workshop.FullName },
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                return Ok(Utilities.ReturnSuccess("Dados preparados para inserção", new { 
                    workshop = workshop.CompanyName,
                    agendaData = agendaData,
                    serviceData = serviceData,
                    message = "Execute manualmente no MongoDB: db.WorkshopAgenda.insertOne(agendaData) e db.WorkshopServices.insertOne(serviceData)"
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(Utilities.ReturnErro($"Erro ao configurar oficina: {ex.Message}"));
            }
        }
    }
}