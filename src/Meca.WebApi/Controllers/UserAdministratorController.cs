
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Entities.Auxiliaries;
using Meca.Data.Enum;
using Meca.Domain;
using Meca.Domain.ViewModels;
using Meca.Domain.ViewModels.Admin;
using Meca.Domain.ViewModels.Filters;
using Meca.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Core3.Interface;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]


    public class UserAdministratorController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IBusinessBaseAsync<UserAdministrator> _userAdministratorRepository;
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IBusinessBaseAsync<Scheduling> _schedulingRepository;
        private readonly IBusinessBaseAsync<Rating> _ratingRepository;
        private readonly IBusinessBaseAsync<Notification> _notificationRepository;
        private readonly IUserAdministratorService _userAdministratorService;
        private readonly IAccessLevelService _accessLevelService;
        private readonly ISenderMailService _senderMailService;


        public UserAdministratorController(
            IMapper mapper,
            IBusinessBaseAsync<UserAdministrator> userAdministratorRepository,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<Workshop> workshopRepository,
            IBusinessBaseAsync<Scheduling> schedulingRepository,
            IBusinessBaseAsync<Rating> ratingRepository,
            IBusinessBaseAsync<Notification> notificationRepository,
            ISenderMailService senderMailService,
            IUserAdministratorService userAdministratorService,
            IAccessLevelService accessLevelService,
            IHttpContextAccessor httpContext,
            IConfiguration configuration
        ) : base(userAdministratorService, httpContext, configuration)
        {
            _mapper = mapper;
            _userAdministratorRepository = userAdministratorRepository;
            _profileRepository = profileRepository;
            _workshopRepository = workshopRepository;
            _schedulingRepository = schedulingRepository;
            _ratingRepository = ratingRepository;
            _notificationRepository = notificationRepository;
            _senderMailService = senderMailService;
            _userAdministratorService = userAdministratorService;
            _accessLevelService = accessLevelService;
        }

        /// <summary>
        /// GET INFO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetInfo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> GetInfo()
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredentials));

                var userAdministratorEntity = await _userAdministratorRepository.FindByIdAsync(userId).ConfigureAwait(false);

                if (userAdministratorEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.UserAdministratorNotFound));

                var responseVm = _mapper.Map<UserAdministratorViewModel>(userAdministratorEntity);

                responseVm.TotalNotificationNoRead = await _notificationRepository.CountLongAsync(x =>
                    x.ReferenceId == userId &&
                    x.DateRead == null &&
                    x.TypeReference == TypeProfile.UserAdministrator
                );

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<UserAdministratorViewModel>(responseVm)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// GET INFO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [HttpGet("Detail/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<UserAdministratorViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [OnlyAdministrator]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredentials));

                var userAdministratorEntity = await _userAdministratorRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (userAdministratorEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.UserAdministratorNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<UserAdministratorViewModel>(userAdministratorEntity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// DASHBOARD
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("Dashboard")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DashboardViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredentials));

                var userAdministratorEntity = await _userAdministratorRepository.FindByIdAsync(userId).ConfigureAwait(false);

                if (userAdministratorEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.UserAdministratorNotFound));

                // Executa todas as contagens em paralelo para reduzir o tempo de espera
                var (totalClients, totalWorkshops, totalServices, totalRatings, totalServicesScheduled, totalOpenServices, totalServicesCompleted) = await GetDashboardDataAsync();

                var averageRatings = totalRatings.Any() ? totalRatings.Average(x => x.RatingAverage) : 0;

                var dashboard = new DashboardViewModel()
                {
                    TotalClients = totalClients,
                    TotalWorkshops = totalWorkshops,
                    TotalServices = totalServices,
                    TotalServicesScheduled = totalServicesScheduled,
                    TotalOpenServices = totalOpenServices,
                    TotalServicesCompleted = totalServicesCompleted,
                    TotalRatings = totalRatings.Count,
                    AverageRatings = averageRatings,
                };

                return ReturnResponse(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        // ----------------- Função auxiliar para otimização -----------------
        async Task<(long, long, long, List<Rating>, int, int, int)> GetDashboardDataAsync()
        {
            var totalClientsTask = _profileRepository.GetCollectionAsync().CountDocumentsAsync(Builders<Data.Entities.Profile>.Filter.Empty);
            var totalWorkshopsTask = _workshopRepository.GetCollectionAsync().CountDocumentsAsync(Builders<Workshop>.Filter.Empty);
            var totalServicesTask = _schedulingRepository.GetCollectionAsync().CountDocumentsAsync(Builders<Scheduling>.Filter.Empty);
            var totalRatingsTask = _ratingRepository.FindAllAsync();

            var totalServicesScheduledTask = _schedulingRepository.CountAsync(x => x.Status == SchedulingStatus.Scheduled);
            var totalServicesCompletedTask = _schedulingRepository.CountAsync(x => x.Status == SchedulingStatus.ServiceFinished);
            var totalOpenServicesTask = _schedulingRepository.CountAsync(x => x.Status != SchedulingStatus.Scheduled && x.Status != SchedulingStatus.ServiceFinished);

            // Aguarda todas as tarefas em paralelo
            await Task.WhenAll(totalClientsTask, totalWorkshopsTask, totalServicesTask, totalRatingsTask, totalServicesScheduledTask, totalOpenServicesTask, totalServicesCompletedTask);

            return (
                totalClientsTask.Result,
                totalWorkshopsTask.Result,
                totalServicesTask.Result,
                totalRatingsTask.Result.ToList(),
                totalServicesScheduledTask.Result,
                totalOpenServicesTask.Result,
                totalServicesCompletedTask.Result
            );
        }

        /// <summary>
        /// CADASTRO | UPDATE
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "name":"string",
        ///              "email":"string",
        ///              "password":"string"
        ///             }
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
        [ProducesResponseType(typeof(ReturnGenericViewModel<UserAdministratorViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] UserAdministratorViewModel model)
        {
            UserAdministrator userAdministratorEntity = null;
            var sendWellcome = false;
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();
                model.TrimStringProperties();

                if (string.IsNullOrEmpty(model.Id))
                {
                    var isInvalidState = ModelState.ValidModelState(nameof(model.Password));

                    if (isInvalidState != null)
                        return BadRequest(isInvalidState);

                    if (await _userAdministratorRepository.CheckByAsync(x => x.Email == model.Email))
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));

                    userAdministratorEntity = _mapper.Map<UserAdministrator>(model);

                    if (string.IsNullOrEmpty(model.Password))
                    {
                        model.Password = Utilities.RandomString(8);
                        sendWellcome = true;
                    }

                    userAdministratorEntity.Password = Utilities.GerarHashMd5(model.Password);

                    userAdministratorEntity = await _userAdministratorRepository.CreateReturnAsync(userAdministratorEntity);

                    if (sendWellcome)
                    {
                        var _ = Task.Run(async () =>
                        {
                            var dataBody = Util.GetTemplateVariables();

                            var title = "Bem vindo";

                            dataBody.Add("{{ email }}", userAdministratorEntity.Email);
                            dataBody.Add("{{ password }}", model.Password);
                            dataBody.Add("{{ name }}", userAdministratorEntity.Name);

                            dataBody.Add("{{ title }}", title);
                            dataBody.Add("{{ message }}", Util.GetWellcomeTemplate(true).ReplaceTag(dataBody));

                            var body = _senderMailService.GerateBody("custom", dataBody);

                            await _senderMailService.SendMessageEmailAsync(
                                BaseConfig.ApplicationName,
                                userAdministratorEntity.Email,
                                body,
                                title
                            );

                        });
                    }
                }
                else
                {
                    var isInvalidState = ModelState.ValidModelStateOnlyFields(validOnly);

                    if (isInvalidState != null)
                        return BadRequest(isInvalidState);

                    userAdministratorEntity = await _userAdministratorRepository.FindByIdAsync(model.Id);

                    if (userAdministratorEntity == null)
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.UserAdministratorNotFound));

                    if (await _userAdministratorRepository.CheckByAsync(x => x.Email == model.Email &&
                        x._id != userAdministratorEntity._id))
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));

                    userAdministratorEntity.SetIfDifferent(model, validOnly);

                    if (Util.CheckHasField(validOnly, nameof(model.AccessLevel)))
                    {
                        userAdministratorEntity.AccessLevel = _mapper.Map<BaseReferenceAux>(model.AccessLevel);
                    }

                    if (string.IsNullOrEmpty(model.Password) == false)
                        userAdministratorEntity.Password = Utilities.GerarHashMd5(model.Password);

                    userAdministratorEntity = await _userAdministratorRepository.UpdateAsync(userAdministratorEntity);

                }

                var message = string.IsNullOrEmpty(model.Id) ? DefaultMessages.Registered : DefaultMessages.Updated;

                return Ok(Utilities.ReturnSuccess(message, _mapper.Map<UserAdministratorViewModel>(userAdministratorEntity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// UPDATE COM ID NA URL OU TOKEN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "name":"string",
        ///              "email":"string",
        ///              "password":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Update")]
        [HttpPatch("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdatePath([FromRoute] string id, [FromBody] UserAdministratorViewModel model)
        {
            try
            {
                var userId = id ?? model?.Id ?? Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredentials));

                var validOnly = _httpContextAccessor.GetFieldsFromBody();
                model.TrimStringProperties();

                var isInvalidState = ModelState.ValidModelStateOnlyFields(validOnly);

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var userAdministratorEntity = await _userAdministratorRepository.FindByIdAsync(userId);

                if (userAdministratorEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.UserAdministratorNotFound));

                if (await _userAdministratorRepository.CheckByAsync(x => x.Email == model.Email &&
                    x._id != userAdministratorEntity._id))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));

                userAdministratorEntity.SetIfDifferent(model, validOnly);

                if (Util.CheckHasField(validOnly, nameof(model.AccessLevel)))
                {
                    userAdministratorEntity.AccessLevel = _mapper.Map<BaseReferenceAux>(model.AccessLevel);
                }

                if (Util.CheckHasField(validOnly, nameof(model.Password)))
                    userAdministratorEntity.Password = Utilities.GerarHashMd5(model.Password);

                userAdministratorEntity = await _userAdministratorRepository.UpdateAsync(userAdministratorEntity);

                var message = string.IsNullOrEmpty(model.Id) ? DefaultMessages.Registered : DefaultMessages.Updated;

                return Ok(Utilities.ReturnSuccess(message, _mapper.Map<UserAdministratorViewModel>(userAdministratorEntity)));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// METODO DE LOGIN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "email":"string",
        ///              "password":"string",
        ///              "refreshToken":"string" /*para renovar token*/
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
        public async Task<IActionResult> Token([FromBody] LoginAdminViewModel model)
        {
            var claim = Util.SetRole(TypeProfile.UserAdministrator);

            try
            {
                model.TrimStringProperties();

                if (string.IsNullOrEmpty(model.RefreshToken) == false)
                    return TokenProviderMiddleware.RefreshToken(model.RefreshToken);

                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var userAdministratorEntity = await _userAdministratorRepository.FindOneByAsync(x =>
                   x.Email == model.Email && x.Password == Utilities.GerarHashMd5(model.Password)).ConfigureAwait(false);

                if (userAdministratorEntity == null)
                {
                    var rootEmail = "contato@mecabr.com";
                    var rootPassword = "megaleios";

                    if (Equals(model.Email, rootEmail) &&
                        Equals(model.Password, rootPassword) &&
                        await _userAdministratorRepository.CountAsync(x => x.Disabled == null) == 0)
                    {
                        var accessLevelServiceDefaultEntity = await _accessLevelService.RegisterDefault();

                        userAdministratorEntity = new UserAdministrator()
                        {
                            Email = rootEmail,
                            Password = Utilities.GerarHashMd5(rootPassword),
                            Name = "Meca",
                            AccessLevel = new BaseReferenceAux()
                            {
                                Id = accessLevelServiceDefaultEntity.Id,
                                Name = accessLevelServiceDefaultEntity.Name
                            },
                            IsDefault = true
                        };
                        userAdministratorEntity = await _userAdministratorRepository.CreateReturnAsync(userAdministratorEntity);
                    }
                }

                if (userAdministratorEntity == null)
                    return BadRequest(Utilities.ReturnErro("Login e/ou senha inválidos."));

                if (userAdministratorEntity.DataBlocked != null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.UserAdministratorBlocked));

                await _accessLevelService.RefreshMenu();

                return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(userAdministratorEntity.GetStringId(), false, claim)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// ESQUECI A SENHA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///             POST
        ///                 {
        ///                  "email":"string"
        ///                 }
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
        public async Task<IActionResult> ForgotPassword([FromBody] LoginAdminViewModel model)
        {
            try
            {
                model?.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Email));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var userAdministratorEntity = await _userAdministratorRepository.FindOneByAsync(x => x.Email == model.Email);

                if (userAdministratorEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileUnRegistered));

                var dataBody = Util.GetTemplateVariables();

                var newPassword = Utilities.RandomString(8);

                var title = "Lembrete de senha";

                dataBody.Add("{{ name }}", userAdministratorEntity.Name.GetFirstName());
                dataBody.Add("{{ email }}", userAdministratorEntity.Email);
                dataBody.Add("{{ password }}", newPassword);

                dataBody.Add("{{ title }}", title);
                dataBody.Add("{{ message }}", Util.GetForgotPasswordTemplate().ReplaceTag(dataBody));


                var body = _senderMailService.GerateBody("custom", dataBody);

                // Salvar a nova senha primeiro
                userAdministratorEntity.Password = Utilities.GerarHashMd5(newPassword);
                await _userAdministratorRepository.UpdateAsync(userAdministratorEntity);

                // Enviar email de forma assíncrona
                var unused = Task.Run(async () =>
                {
                    await _senderMailService.SendMessageEmailAsync(
                        $"{BaseConfig.ApplicationName}-Dashboad",
                        userAdministratorEntity.Email,
                        body,
                        title);
                });

                return Ok(Utilities.ReturnSuccess(DefaultMessages.VerifyYourEmail));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// ALTERAR PASSWORD
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
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro());

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var userAdministratorEntity = await _userAdministratorRepository.FindByIdAsync(userId).ConfigureAwait(false);

                if (userAdministratorEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.UserAdministratorNotFound));

                if (userAdministratorEntity.Password != Utilities.GerarHashMd5(model.CurrentPassword))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.PasswordNoMatch));

                userAdministratorEntity.Password = Utilities.GerarHashMd5(model.NewPassword);

                _userAdministratorRepository.Update(userAdministratorEntity);

                return Ok(Utilities.ReturnSuccess(DefaultMessages.PasswordChanged));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REMOVER UM PERFIL DE ACESSO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///              "id":"string"
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [HttpPost("Delete")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [OnlyAdministrator]
        public async Task<IActionResult> Delete([FromRoute] string id, [FromBody] UserAdministratorViewModel model)
        {
            try
            {
                model ??= new UserAdministratorViewModel() { Id = id };
                model?.TrimStringProperties();


                if (string.IsNullOrEmpty(model.Id))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                await _userAdministratorRepository.DeleteOneAsync(model.Id).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(DefaultMessages.Deleted));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// Bloquear / Desbloquear
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///                 "targetId":"string"
        ///                 "block":true
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("BlockUnblock")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [OnlyAdministrator]
        public async Task<IActionResult> BlockUnblock([FromBody] BlockViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var userAdministratorEntity =
                    await _userAdministratorRepository.FindByIdAsync(model.TargetId).ConfigureAwait(false);

                if (userAdministratorEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.UserAdministratorNotFound));

                userAdministratorEntity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;

                await _userAdministratorRepository.UpdateAsync(userAdministratorEntity).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(model.Block ? "Bloqueado com sucesso" : "Desbloqueado com sucesso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// METODO DE LISTAGEM COM DATATABLE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<UserAdministratorViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, UserAdministratorFilterViewModel filterView)
        {
            var response = new DtResult<UserAdministratorViewModel>();
            try
            {
                var builder = Builders<UserAdministrator>.Filter;
                var conditions = new List<FilterDefinition<UserAdministrator>>
            {
                builder.Where(x => x.Disabled == null)
            };

                if (string.IsNullOrEmpty(filterView.AccessLevelId) == false)
                {
                    conditions.Add(builder.Eq(x => x.AccessLevel.Id, filterView.AccessLevelId));
                }

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var totalRecords = (int)await _userAdministratorRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = Util.MapSort<UserAdministrator>(model.Order, model.Columns, model.SortOrder);

                var result = await _userAdministratorRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalRecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _userAdministratorRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<UserAdministratorViewModel>>(result);
                response.Draw = model.Draw;
                response.RecordsFiltered = totalRecordsFiltered;
                response.RecordsTotal = totalRecords;

                return Ok(response);

            }
            catch (Exception ex)
            {
                response.Erro = true;
                response.MessageEx = $"{ex.InnerException} {ex.Message}".Trim();
                return Ok(response);
            }
        }
    }
}