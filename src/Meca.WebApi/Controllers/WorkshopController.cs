using System;
using System.Collections.Generic;
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

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class WorkshopController : MainController
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
            IConfiguration configuration) : base(workshopService, context, configuration)
        {
            _mapper = mapper;
            _workshopRepository = workshopRepository;
            _notificationRepository = notificationRepository;
            _senderMailService = senderMailService;
            _workshopService = workshopService;
            _iuguMarketPlaceServices = iuguMarketPlaceServices;
            _isSandbox = Util.IsSandBox(env);
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
                if (filterModel.HasFilter() == false)
                    return ReturnResponse(await _workshopService.GetAll());
                else
                    return ReturnResponse(await _workshopService.GetAll<WorkshopViewModel>(filterModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
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

                return ReturnResponse(response);
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
                var response = await _workshopService.GetInfo();

                return ReturnResponse(response);
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

                return ReturnResponse(null, DefaultMessages.Deleted);
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

                return ReturnResponse(null, DefaultMessages.Deleted);
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

                return ReturnResponse(null, DefaultMessages.Deleted);
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
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _workshopService.Register(model);

                return ReturnResponse(response, "Agradecemos pelas informações, nossa equipe efetuará uma análise e em breve você receberá um e-mail com a liberação de acesso à plataforma.");
            }
            catch (Exception ex)
            {
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
            // try
            // {
            //     model.TrimStringProperties();
            //     _service.SetModelState(ModelState);

            //     var response = await _workshopService.Token(model);

            //     return ReturnResponse(response);
            // }
            // catch (Exception ex)
            // {
            //     return BadRequest(ex.ReturnErro());
            // }

            var claimRole = Util.SetRole(TypeProfile.Workshop);

            try
            {
                model.TrimStringProperties();

                if (string.IsNullOrEmpty(model.RefreshToken) == false)
                    return TokenProviderMiddleware.RefreshToken(model.RefreshToken);

                Workshop workshopEntity;
                if (model.TypeProvider != TypeProvider.Password)
                {
                    workshopEntity = await _workshopRepository.FindOneByAsync(x => x.ProviderId == model.ProviderId)
                        .ConfigureAwait(false);

                    if (workshopEntity == null || workshopEntity.Disabled != null)
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound, new { IsRegister = true }));
                }
                else
                {
                    model.TrimStringProperties();
                    var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Email), nameof(model.Password));

                    if (isInvalidState != null)
                        return BadRequest(isInvalidState);

                    workshopEntity = await _workshopRepository
                       .FindOneByAsync(x => x.Email == model.Email && x.Password == Utilities.GerarHashMd5(model.Password)).ConfigureAwait(false);

#if DEBUG
                    workshopEntity = await _workshopRepository
          .FindOneByAsync(x => x.Email == model.Email);
#endif
                }

                if (workshopEntity == null || workshopEntity.Disabled != null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidLogin));

                if (workshopEntity.DataBlocked != null)
                    return BadRequest(Utilities.ReturnErro(
                        string.Format(DefaultMessages.AccessBlockedWithReason,
                        (string.IsNullOrEmpty(workshopEntity.Reason) ? $"Motivo {workshopEntity.Reason}" : "").Trim()
                        )));

                if (workshopEntity.Status != WorkshopStatus.Approved)
                    return BadRequest(Utilities.ReturnErro(workshopEntity.Status == WorkshopStatus.AwaitingApproval ? DefaultMessages.AwaitApproval : DefaultMessages.UserAdministratorBlocked));

                var claims = new Claim[]
                {
                    claimRole,
                };

                return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(workshopEntity.GetStringId(), false, claims)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
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
                _service.SetModelState(ModelState);

                var response = await _workshopService.BlockUnBlock(model);

                return ReturnResponse(response != null, response);
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
                _service.SetModelState(ModelState);

                var response = await _workshopService.ChangePassword(model);

                return ReturnResponse(response, DefaultMessages.PasswordChanged);
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
                var response = await _workshopService.LoadData(model, filterView);

                return Ok(response);
            }
            catch (Exception ex)
            {
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
                _service.SetModelState(ModelState);

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
                _service.SetModelState(ModelState);

                var response = await _workshopService.UpdatePatch(id, model);

                return ReturnResponse(response, DefaultMessages.Updated);
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
                _service.SetModelState(ModelState);

                var response = await _workshopService.ForgotPassword(model);

                return ReturnResponse(response, DefaultMessages.VerifyYourEmail);
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
                _service.SetModelState(ModelState);

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
                _service.SetModelState(ModelState);

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
                _service.SetModelState(ModelState);

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
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _workshopService.UpdateDataBank(model, id);

                return ReturnResponse(response, DefaultMessages.Updated);
            }
            catch (Exception ex)
            {
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

                return ReturnResponse(response);
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
                    return BadRequest(Utilities.ReturnError("Erro ao atualizar workshops"));
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
    }
}