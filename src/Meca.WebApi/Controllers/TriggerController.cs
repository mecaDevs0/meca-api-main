using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Meca.ApplicationService.Interface;
using Meca.Data.Entities;
using Meca.Data.Enum;
using Meca.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Core3.Interface;
using UtilityFramework.Services.Iugu.Core3;
using UtilityFramework.Services.Iugu.Core3.Enums;
using UtilityFramework.Services.Iugu.Core3.Interface;
using UtilityFramework.Services.Iugu.Core3.Models;
using UtilityFramework.Services.Iugu.Core3.Response;

namespace Meca.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class TriggerController : MainController
    {

        private readonly ISenderMailService _senderMailService;
        private readonly ISenderNotificationService _senderNotificationService;
        private readonly IBusinessBaseAsync<Profile> _profileRepository;
        private readonly IBusinessBaseAsync<FinancialHistory> _financialhistoryRepository;
        private readonly IBusinessBaseAsync<Workshop> _workshopRepository;
        private readonly IIuguChargeServices _iuguChargeServices;
        private readonly IFinancialHistoryService _financialHistoryService;
        private readonly string _projectName;
        private readonly bool _isSandBox;

        public TriggerController(
        ISenderMailService senderMailService,
        IBusinessBaseAsync<Profile> profileRepository,
        ISenderNotificationService senderNotificationService,
        IIuguChargeServices iuguChargeServices,
        IHttpContextAccessor httpContext,
        IConfiguration configuration,
        IHostingEnvironment env,
        IFinancialHistoryService financialHistoryService,
        IBusinessBaseAsync<FinancialHistory> financialhistoryRepository,
        IBusinessBaseAsync<Workshop> workshopRepository) : base(null, httpContext, configuration)
        {
            _senderMailService = senderMailService;
            _senderNotificationService = senderNotificationService;
            _profileRepository = profileRepository;
            _iuguChargeServices = iuguChargeServices;
            _projectName = Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0]?.ToString();
            _isSandBox = Util.IsSandBox(env);
            _financialHistoryService = financialHistoryService;
            _financialhistoryRepository = financialhistoryRepository;
            _workshopRepository = workshopRepository;
        }

        /// <summary>
        /// GATILHOS DA IUGU
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ChangeStatusGatway/{projectName}")]
        [HttpPost("ChangeStatusGateway/{projectName}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ChangeStatusGateway([FromForm] IuguTriggerModel model, [FromRoute] string projectName)
        {
            try
            {
                if (string.IsNullOrEmpty(projectName) || Utilities.EqualsIgnoreCase(projectName, _projectName) == false)
                    return Ok(Utilities.ReturnSuccess(DefaultMessages.InvoiceOfAnotherProject));

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                model.SetAllProperties(Request.Form);
                IuguInvoiceResponseMessage invoiceIugu = null;

                var registerLog = false;
                var suportEmail = Utilities.GetConfigurationRoot().GetSection("suportEmail").Get<List<string>>();
                var subAccountSettings = Utilities.GetConfigurationRoot().GetSection("IUGU:Client").Get<MegaClientIuguViewModel>();

                switch (model.Event)
                {
                    case Constants.TriggerEvents.InvoiceStatusChanged:
                        if (_isSandBox == false)
                            return Ok(Utilities.ReturnSuccess("Apenas SandBox"));

                        invoiceIugu = await _iuguChargeServices.GetFaturaAsync(model.Data.Id);

                        if (invoiceIugu == null)
                        {
                            await _senderMailService.SendMessageEmailAsync(_projectName, suportEmail, $"Fatura não encontrada id da fatura = {model.Data.Id} DATA:{JsonConvert.SerializeObject(model).JsonPrettify()}", "Fatura não encontradda");
                            break;
                        }

                        switch (invoiceIugu.Status)
                        {
                            case "paid":
                                await _financialHistoryService.ConfirmPayment(model.Data.Id, PaymentStatus.Paid, model);
                                break;
                            case "expired":
                                await _financialHistoryService.Expired(model.Data.Id);
                                break;
                            case "canceled":
                                await _financialHistoryService.Canceled(model.Data.Id);
                                break;
                        }

                        break;

                    case Constants.TriggerEvents.InvoiceReleased:

                        invoiceIugu = await _iuguChargeServices.GetFaturaAsync(model.Data.Id);

                        if (invoiceIugu == null)
                        {
                            _senderMailService.SendMessageEmail(_projectName, suportEmail, $"Fatura não encontrada id da fatura = {model.Data.Id} DATA:{JsonConvert.SerializeObject(model).JsonPrettify()}", "Fatura não encontradda");
                            break;
                        }

                        await _financialHistoryService.ConfirmPayment(model.Data.Id, PaymentStatus.Released, model);

                        break;
                    case Constants.TriggerEvents.InvoicePaymentFailed:

                        invoiceIugu = await _iuguChargeServices.GetFaturaAsync(model.Data.Id);

                        if (invoiceIugu == null)
                        {
                            _senderMailService.SendMessageEmail(_projectName, suportEmail, $"Fatura não encontrada id da fatura = {model.Data.Id} DATA:{JsonConvert.SerializeObject(model).JsonPrettify()}", "Fatura não encontradda");
                            break;
                        }

                        await _financialHistoryService.PaymentFailed(model.Data.Id);

                        break;
                    case Constants.TriggerEvents.ReferralsVerification:
                    case Constants.TriggerEvents.ReferralsBankVerification:

                        // profileEntity = await _profileRepository.FindOneByAsync(x => x.AccountKey == model.Data.AccountId).ConfigureAwait(false);

                        // /*CASO SEJA DONO DA SUBCONTA*/
                        // if (profileEntity != null)
                        // {
                        //     var status = model.Data.Status.ToLower();

                        //     var dataBody = Util.GetTemplateVariables();

                        //     var title = "Verificação de dados bancários";

                        //     dataBody.Add("{{ name }}", profileEntity.FullName.GetFirstName());
                        //     dataBody.Add("{{ title }}", title);
                        //     dataBody.Add("{{ message }}", Util.GetTemplateVerificationDataBank(status));

                        //     // if (Equals(status, "accepted"))
                        //     // {
                        //     //     profileEntity.HasDataBank = true;
                        //     //     profileEntity.LastConfirmDataBank = DateTimeOffset.Now.ToUnixTimeSeconds();
                        //     //     profileEntity.LastRequestVerification = null;
                        //     //     profileEntity.DataBankStatus = DataBankStatus.Valid;
                        //     // }
                        //     // else
                        //     // {
                        //     //     profileEntity.LastConfirmDataBank = null;
                        //     //     profileEntity.LastRequestVerification = null;
                        //     //     profileEntity.DataBankStatus = DataBankStatus.Invalid;
                        //     // }

                        //     var customBody = _senderMailService.GerateBody("custom", dataBody);

                        //     await _senderMailService.SendMessageEmailAsync(
                        //         Startup.ApplicationName,
                        //         [profileEntity.Email],
                        //         customBody,
                        //         title,
                        //         ccoEmails: suportEmail);

                        //     await _profileRepository.UpdateAsync(profileEntity);
                        // }
                        // break;

                        var body = new StringBuilder();

                        var status = model.Data.Status.ToLower();

                        var workshopEntity = await _workshopRepository.FindOneByAsync(x => x.AccountKey == model.Data.AccountId).ConfigureAwait(false);

                        var dataBody = Util.GetTemplateVariables();
                        string email = null;

                        if (workshopEntity != null)
                        {
                            body.AppendLine($"<p>Caro(a) {workshopEntity.CompanyName.GetFirstName()}</p>");

                            if (Equals(status, "accepted"))
                            {
                                workshopEntity.HasDataBank = true;
                                workshopEntity.DataBankStatus = DataBankStatus.Valid;
                                workshopEntity.LastConfirmDataBank = DateTimeOffset.Now.ToUnixTimeSeconds();
                                workshopEntity.LastRequestVerification = null;

                                body.AppendLine("<p>Informamos que seus dados bancários foram válidados com sucesso, você já pode receber suas transações com cartão de credito</p>");
                            }
                            else
                            {
                                workshopEntity.DataBankStatus = DataBankStatus.Invalid;
                                workshopEntity.LastConfirmDataBank = null;
                                workshopEntity.LastRequestVerification = null;

                                body.AppendLine("<p>Informamos que seus dados bancários encontram-se inválidos, por favor verifique os dados informados e atualize os mesmos</p>");
                            }

                            email = workshopEntity.Email;

                            await _workshopRepository.UpdateAsync(workshopEntity);
                        }

                        dataBody.Add("{{ title }}", "Verificação de dados bancários");
                        dataBody.Add("{{ message }}", body.ToString());

                        var customBody = _senderMailService.GerateBody("custom", dataBody);

                        await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, [email], customBody, "Verificação de dados bancários", ccoEmails: suportEmail);

                        break;
                }

                if (registerLog)
                {
                    var unused = Task.Run(() =>
                   {
                       var json = JsonConvert.SerializeObject(model, new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }).JsonPrettify();

                       _senderMailService.SendMessageEmail("Gatilho", suportEmail, json,
                           $"Gatilho {_projectName} - {model.Event}");
                   });
                }

                return Ok(Utilities.ReturnSuccess());
            }
            catch (Exception ex)
            {

                return BadRequest(ex.ReturnErro());
            }
        }
    }
}