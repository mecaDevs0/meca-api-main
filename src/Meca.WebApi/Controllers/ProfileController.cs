using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Filters;
using Meca.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Core3.Interface;
using Profile = Meca.Data.Entities.Profile;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ProfileController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IBusinessBaseAsync<Profile> _profileRepository;
        private readonly IBusinessBaseAsync<Notification> _notificationRepository;
        private readonly ISenderMailService _senderMailService;
        private readonly IProfileService _profileService;

        public ProfileController(IMapper mapper,
            IBusinessBaseAsync<Profile> profileRepository,
            IBusinessBaseAsync<Notification> notificationRepository,
            ISenderMailService senderMailService,
            IProfileService profileService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration) : base(profileService, httpContextAccessor, configuration)
        {
            _mapper = mapper;
            _profileRepository = profileRepository;
            _notificationRepository = notificationRepository;
            _senderMailService = senderMailService;
            _profileService = profileService;
        }

        /// <summary>
        /// USUÁRIO - METODO PARA LISTAR TODOS PAGINADO OU C/S FILTRO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<ProfileViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromQuery] ProfileFilterViewModel filterModel)
        {
            try
            {
                if (filterModel.HasFilter() == false)
                    return ReturnResponse(await _profileService.GetAll());
                else
                    return ReturnResponse(await _profileService.GetAll<ProfileViewModel>(filterModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - OBTER DETALHES
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [HttpGet("Detail/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<ProfileViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                var response = await _profileService.Detail(id);

                return ReturnResponse(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - INFORMAÇÕES DO USUÁRIOS LOGADO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetInfo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<ProfileViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInfo()
        {
            try
            {
                var response = await _profileService.GetInfo();

                return ReturnResponse(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - REMOVER
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
                var response = await _profileService.Delete(id);

                return ReturnResponse(null, DefaultMessages.Deleted);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - REMOVER POR E-MAIL
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
                var response = await _profileService.DeleteByEmail(email);

                return ReturnResponse(null, DefaultMessages.Deleted);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO DO SITE - ENVIAR E-MAIL SOLICITANDO AJUDA SOBRE O APP
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("SendEmailRequestingAppHelp")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SendEmailRequestingAppHelp([FromBody] SiteMecaFormViewModel model)
        {
            try
            {
                var response = await _profileService.SendEmailRequestingAppHelp(model);

                return ReturnResponse(null, DefaultMessages.Sended);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - REGISTRAR
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
        ///           "login": "string",
        ///           "email": "contato@mecabr.com",
        ///           "photo": "string", /*nome do arquivo ex: 302183239.png*/
        ///           "cpf": "364.818.768-69",
        ///           "phone": "(11) 2020-2020",
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
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] ProfileRegisterViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                
                // Comentado temporariamente para evitar erro de Object reference not set
                // if (_service != null)
                // {
                //     _service.SetModelState(ModelState);
                // }

                var response = await _profileService.Register(model);

                // Retornar diretamente sem usar ReturnResponse
                if (response != null)
                {
                    return Ok(Utilities.ReturnSuccess(data: response, message: DefaultMessages.Registered));
                }
                else
                {
                    return BadRequest(Utilities.ReturnErro("Erro ao registrar usuário."));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }

            // var claimRole = Util.SetRole(TypeProfile.Profile);
            // try
            // {
            //     var _jsonBodyFields = _httpContextAccessor.GetFieldsFromBody();
            //     model.TrimStringProperties();
            //     var ignoreValidation = new List<string>();

            //     if (model.TypeProvider != TypeProvider.Password)
            //     {
            //         ignoreValidation.Add(nameof(model.Email));
            //         ignoreValidation.Add(nameof(model.Password));
            //         ignoreValidation.Add(nameof(model.Phone));
            //     }

            //     var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());

            //     if (isInvalidState != null)
            //         return BadRequest(isInvalidState);

            //     if (model.TypeProvider != TypeProvider.Password)
            //     {
            //         if (string.IsNullOrEmpty(model.ProviderId))
            //             return BadRequest(Utilities.ReturnErro(DefaultMessages.EmptyProviderId));

            //         var messageErro = "";

            //         if (await _profileRepository.CheckByAsync(x => x.ProviderId == model.ProviderId))
            //         {
            //             switch (model.TypeProvider)
            //             {
            //                 case TypeProvider.Apple:
            //                     messageErro = DefaultMessages.AppleIdInUse;
            //                     break;
            //                 default:
            //                     messageErro = DefaultMessages.FacebookInUse;
            //                     break;
            //             }

            //             return BadRequest(Utilities.ReturnErro(messageErro));
            //         }
            //     }

            //     if (string.IsNullOrEmpty(model.Email) == false && await _profileRepository.CheckByAsync(x => x.Email == model.Email).ConfigureAwait(false))
            //         return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));

            //     if (string.IsNullOrEmpty(model.Cpf) == false && await _profileRepository.CheckByAsync(x => x.Cpf == model.Cpf).ConfigureAwait(false))
            //         return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfInUse));

            //     var profileEntity = _mapper.Map<Profile>(model);

            //     if (string.IsNullOrEmpty(model.Password) == false)
            //         profileEntity.Password = Utilities.GerarHashMd5(model.Password);

            //     var entityId = await _profileRepository.CreateAsync(profileEntity).ConfigureAwait(false);

            //     var claims = new Claim[]
            //     {
            //         claimRole,
            //     };

            //     return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(entityId, false, claims)));
            // }
            // catch (Exception ex)
            // {
            //     return BadRequest(ex.ReturnErro());
            // }
        }

        /// <summary>
        /// USUÁRIO - LOGIN
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
                model.TrimStringProperties();
                
                // Comentado temporariamente para evitar erro de Object reference not set
                // if (_service != null)
                // {
                //     _service.SetModelState(ModelState);
                // }

                var response = await _profileService.Token(model);

                // Retornar diretamente sem usar ReturnResponse
                if (response != null)
                {
                    return Ok(Utilities.ReturnSuccess(data: response));
                }
                else
                {
                    return BadRequest(Utilities.ReturnErro("Login e/ou senha inválidos."));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }

            // var claimRole = Util.SetRole(TypeProfile.Profile);

            // try
            // {
            //     model.TrimStringProperties();

            //     if (string.IsNullOrEmpty(model.RefreshToken) == false)
            //         return TokenProviderMiddleware.RefreshToken(model.RefreshToken);

            //     Profile profileEntity;
            //     if (model.TypeProvider != TypeProvider.Password)
            //     {
            //         profileEntity = await _profileRepository.FindOneByAsync(x => x.ProviderId == model.ProviderId)
            //             .ConfigureAwait(false);

            //         if (profileEntity == null || profileEntity.Disabled != null)
            //         {
            //             if (string.IsNullOrEmpty(model.Email) == false)
            //             {
            //                 profileEntity = await _profileRepository.FindOneByAsync(x => x.Email == model.Email).ConfigureAwait(false);

            //                 if (profileEntity != null)
            //                 {
            //                     profileEntity.ProviderId = model.ProviderId;
            //                     profileEntity.TypeProvider = model.TypeProvider;
            //                     await _profileRepository.UpdateAsync(profileEntity).ConfigureAwait(false);
            //                 }
            //                 else
            //                 {
            //                     return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound, new { IsRegister = true }));
            //                 }
            //             }
            //             else
            //             {
            //                 return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound, new { IsRegister = true }));
            //             }
            //         }
            //     }
            //     else
            //     {
            //         model.TrimStringProperties();
            //         var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Email), nameof(model.Password));

            //         if (isInvalidState != null)
            //             return BadRequest(isInvalidState);

            //         profileEntity = await _profileRepository
            //            .FindOneByAsync(x => x.Email == model.Email && x.Password == Utilities.GerarHashMd5(model.Password)).ConfigureAwait(false);
            //     }

            //     if (profileEntity == null || profileEntity.Disabled != null)
            //         return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidLogin));

            //     if (profileEntity.DataBlocked != null)
            //         return BadRequest(Utilities.ReturnErro(
            //             string.Format(DefaultMessages.AccessBlockedWithReason,
            //             (string.IsNullOrEmpty(profileEntity.Reason) ? $"Motivo {profileEntity.Reason}" : "").Trim()
            //             )));

            //     var claims = new Claim[]
            //     {
            //         claimRole,
            //     };

            //     return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(profileEntity.GetStringId(), false, claims)));
            // }
            // catch (Exception ex)
            // {
            //     return BadRequest(ex.ReturnErro());
            // }
        }

        /// <summary>
        /// USUÁRIO - BLOQUEAR / DESBLOQUEAR - ADMIN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "id": "string", // required
        ///              "block": true,
        ///              "reason": "" //motivo de bloquear o usuário
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

                var response = await _profileService.BlockUnBlock(model);

                return Ok(Utilities.ReturnSuccess(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - ALTERAR SENHA
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

                var response = await _profileService.ChangePassword(model);

                return ReturnResponse(response, DefaultMessages.PasswordChanged);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - METODO DE LISTAGEM COM DATATABLE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<ProfileViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model)
        {
            try
            {
                var response = await _profileService.LoadData(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - REGISTRAR E REMOVER DEVICE ID ONESIGNAL OU FCM | CHAMAR APOS LOGIN E LOGOUT
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
        public async Task<IActionResult> RegisterUnRegisterDeviceId([FromBody] PushViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                await _profileService.RegisterUnRegisterDeviceId(model);

                return Ok(Utilities.ReturnSuccess());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - ATUALIZAR DADOS
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///         {
        ///           "fullName": "string",
        ///           "login": "string",
        ///           "email": "contato@mecabr.com",
        ///           "photo": "string", /*nome do arquivo ex: 302183239.png*/
        ///           "cpf": "364.818.768-69",
        ///           "phone": "(11) 2020-2020",
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
        [ProducesResponseType(typeof(ReturnGenericViewModel<ProfileViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdatePatch([FromRoute] string id, [FromBody] ProfileRegisterViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _profileService.UpdatePatch(id, model);

                return ReturnResponse(response, DefaultMessages.Updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - ESQUECI A SENHA
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

                var response = await _profileService.ForgotPassword(model);

                return ReturnResponse(response, DefaultMessages.VerifyYourEmail);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - VERIFICAR SE E-MAIL ESTÁ EM USO
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

                var response = await _profileService.CheckEmail(model);

                return Ok(Utilities.ReturnSuccess(response ? DefaultMessages.Available : DefaultMessages.EmailInUse));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - VERIFICAR SE LOGIN ESTÁ EM USO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///             POST
        ///             {
        ///                 "login":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("CheckLogin")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckLogin([FromBody] ValidationViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _profileService.CheckLogin(model);

                return Ok(Utilities.ReturnSuccess(response ? DefaultMessages.Available : DefaultMessages.LoginInUse));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - VERIFICAR SE CPF ESTÁ EM USO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///             POST
        ///             {
        ///                 "cpf":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("CheckCpf")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckCpf([FromBody] ValidationViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _profileService.CheckCpf(model);

                return Ok(Utilities.ReturnSuccess(response ? DefaultMessages.Available : DefaultMessages.CpfInUse));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// USUÁRIO - VERIFICAR SE CAMPOS ESTÁ EM USO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///             POST
        ///             {
        ///                 "cpf":"string",
        ///                 "login":"string",
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

                var response = await _profileService.CheckAll(model);

                return Ok(Utilities.ReturnSuccess(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}