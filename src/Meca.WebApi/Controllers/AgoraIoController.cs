using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Services.Core3.Interface;
using UtilityFramework.Services.Core3.Models.AgoraIO;
using UtilityFramework.Services.Core3.Models.AgoraIO.Media;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AgoraIoController : MainController
    {
        private readonly IAgoraIOService _agoraIOService;

        public AgoraIoController(IAgoraIOService agoraIOService)
        {
            _agoraIOService = agoraIOService;
        }

        /// <summary>
        /// AGORAIO - METODO PARA GERAR TOKEN CHANNEL
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///               "channel": "string",
        ///               "uid": 0
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<AuthenticateResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public ActionResult<AuthenticateResponse> GenerateToken([FromBody] AuthenticateRequest model)
        {

            try
            {
                var token = _agoraIOService.GenerateToken(model.Channel, model.Uid, model.ExpiredTs,
                [
                    Privileges.kJoinChannel,
                    Privileges.kPublishAudioStream,
                    Privileges.kPublishVideoStream,
                    Privileges.kPublishDataStream,
                    Privileges.kRtmLogin
                ]);

                return Ok(Utilities.ReturnSuccess(data: new AuthenticateResponse()
                {
                    Channel = model.Channel,
                    Uid = model.Uid,
                    Token = token
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }


        }

        /// <summary>
        /// AGORAIO - METODO PARA OBTER O RESOURCE ID
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///               "cname": "string",
        ///               "uid": "string"
        ///               "clientRequest": {
        ///                 "resourceExpiredHour": 0 // Exemplo 24 horas
        ///               }
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Acquire")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> AcquireResourceId([FromBody] AcquireRequestViewModel model)
        {
            try
            {
                var response = await _agoraIOService.AcquireResourceId(model);

                return ReturnResponse(response);
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// AGORAIO - METODO PARA INICIAR A GRAVAÇÃO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///                "cname": "string",
        ///                "uid": "string"  // mesmo utilizado no método acquire.
        ///                "clientRequest": {
        ///                     "token":""
        ///                 }
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("StartRecording")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> StartRecording([FromQuery] string resourceId, [FromBody] AcquireRequestViewModel model)
        {

            try
            {
                var response = await _agoraIOService.StartRecording(resourceId, model);

                return ReturnResponse(response);

            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// AGORAIO - METODO PARA SALVAR A GRAVAÇÃO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///
        ///         POST
        ///             {
        ///                "cname": "string",
        ///                "uid": "string", // mesmo utilizado no método acquire.
        ///                "clientRequest": {
        ///                    "async_stop": true
        ///                }
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("StopRecording")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> StopRecording([FromQuery] string resourceId, [FromQuery] string sid, [FromBody] StopRequestViewModel model)
        {

            try
            {
                var response = await _agoraIOService.StopRecording(resourceId, sid, model);

                return ReturnResponse(response);

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// AGORAIO - METODO PARA CONSULTAR O STATUS DA GRAVAÇÃO (SOMENTE GRAVAÇÃO EM ANDAMENTO)
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{resourceId}/{sid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> Query([FromRoute] string resourceId, [FromRoute] string sid)
        {
            try
            {
                var response = await _agoraIOService.Query(resourceId, sid);

                return ReturnResponse(response);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}