using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Meca.ApplicationService.Interface;
using Meca.Domain.ViewModels;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TestNotificationController : MainController
    {
        private readonly INotificationService _notificationService;

        public TestNotificationController(
            INotificationService notificationService,
            IHttpContextAccessor context,
            IConfiguration configuration) : base(notificationService, context, configuration)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// TESTE - ENVIAR NOTIFICAÇÃO DE TESTE
        /// </summary>
        /// <param name="model">Dados da notificação de teste</param>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("send-test")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SendTestNotification([FromBody] SendPushViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(Utilities.ReturnErro("Modelo de dados inválido"));
                }
                
                model.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _notificationService.SendAndRegisterNotification(model);

                if (response)
                {
                    return ReturnResponse(null, "Notificação de teste enviada com sucesso");
                }
                else
                {
                    return BadRequest(Utilities.ReturnErro("Erro ao enviar notificação de teste"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// TESTE - ENVIAR NOTIFICAÇÃO PARA TODOS OS DISPOSITIVOS
        /// </summary>
        /// <param name="title">Título da notificação</param>
        /// <param name="content">Conteúdo da notificação</param>
        /// <param name="typeProfile">Tipo de perfil (0=Admin, 1=Cliente, 2=Oficina)</param>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("send-to-all")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SendToAll([FromBody] TestNotificationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(Utilities.ReturnErro("Modelo de dados inválido"));
                }
                
                var model = new SendPushViewModel
                {
                    Title = request.Title,
                    Content = request.Content,
                    TypeProfile = request.TypeProfile,
                    TargetId = new List<string>() // Lista vazia = enviar para todos
                };

                var response = await _notificationService.SendAndRegisterNotification(model);

                if (response)
                {
                    return ReturnResponse(null, $"Notificação enviada para todos os {request.TypeProfile.ToString()}s");
                }
                else
                {
                    return BadRequest(Utilities.ReturnErro("Erro ao enviar notificação"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }

    public class TestNotificationRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public Meca.Data.Enum.TypeProfile TypeProfile { get; set; }
    }
}




