using System;
using System.Linq;
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
    public class PagBankController : MainController
    {
        private readonly IPagBankService _pagBankService;

        public PagBankController(
            IPagBankService pagBankService,
            IHttpContextAccessor context,
            IConfiguration configuration) : base(null, context, configuration)
        {
            _pagBankService = pagBankService;
        }

        /// <summary>
        /// PAGBANK - CRIAR PEDIDO
        /// </summary>
        /// <param name="request">Dados do pedido</param>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("orders")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<PagBankOrderResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateOrder([FromBody] PagBankOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(Utilities.ReturnErro("Modelo de dados inválido"));
                }
                
                request.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _pagBankService.CreateOrderAsync(request);

                return ReturnResponse(response, "Pedido criado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// PAGBANK - OBTER DETALHES DO PEDIDO
        /// </summary>
        /// <param name="orderId">ID do pedido</param>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("orders/{orderId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<PagBankOrderDetails>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetOrderDetails([FromRoute] string orderId)
        {
            try
            {
                var response = await _pagBankService.GetOrderDetailsAsync(orderId);

                return ReturnResponse(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// PAGBANK - PROCESSAR PAGAMENTO
        /// </summary>
        /// <param name="request">Dados do pagamento</param>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("payments")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<PagBankPaymentResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ProcessPayment([FromBody] PagBankPaymentRequest request)
        {
            try
            {
                request.TrimStringProperties();
                _service.SetModelState(ModelState);

                var response = await _pagBankService.ProcessPaymentAsync(request);

                return ReturnResponse(response, "Pagamento processado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// PAGBANK - CANCELAR PEDIDO
        /// </summary>
        /// <param name="orderId">ID do pedido</param>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("orders/{orderId}/cancel")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CancelOrder([FromRoute] string orderId)
        {
            try
            {
                var success = await _pagBankService.CancelOrderAsync(orderId);

                if (success)
                {
                    return ReturnResponse(null, "Pedido cancelado com sucesso");
                }
                else
                {
                    return BadRequest(Utilities.ReturnErro("Erro ao cancelar pedido"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// PAGBANK - WEBHOOK
        /// </summary>
        /// <param name="payload">Payload do webhook</param>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("webhook")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Webhook([FromBody] string payload)
        {
            try
            {
                var signature = Request.Headers["X-PagSeguro-Signature"].FirstOrDefault();
                
                var response = await _pagBankService.ProcessWebhookAsync(payload, signature);

                return ReturnResponse(response, "Webhook processado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}
