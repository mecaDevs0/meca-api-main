using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meca.Data.Entities;
using Meca.Domain;
using Meca.Domain.Services.Interface;
using Meca.Domain.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Services.Iugu.Core3.Interface;
using UtilityFramework.Services.Iugu.Core3.Models;
using Profile = Meca.Data.Entities.Profile;

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CreditCardController : MainController
    {
        private readonly IBusinessBaseAsync<CreditCard> _creditCardRepository;
        private readonly IBusinessBaseAsync<Profile> _profileUserBusiness;
        private readonly IIuguCustomerServices _iuguCustomerServices;
        private readonly IIuguPaymentMethodService _iuguPaymentMethodService;
        private readonly IUtilService _utilService;
        private readonly bool _isSandBox;
        private readonly IMapper _mapper;

        public CreditCardController(
            IIuguPaymentMethodService iuguPaymentMethodService,
            IBusinessBaseAsync<Profile> profileUserBusiness,
            IBusinessBaseAsync<CreditCard> creditCardRepository,
            IIuguCustomerServices iuguCustomerServices,
            IMapper mapper,
            IUtilService utilService,
            IHttpContextAccessor httpContext,
            IConfiguration configuration,
            IHostingEnvironment env) : base(null, httpContext, configuration)
        {
            _creditCardRepository = creditCardRepository;
            _iuguPaymentMethodService = iuguPaymentMethodService;
            _profileUserBusiness = profileUserBusiness;
            _iuguCustomerServices = iuguCustomerServices;
            _mapper = mapper;
            _utilService = utilService;
            _isSandBox = Util.IsSandBox(env);
        }

        /// <summary>
        ///     LISTAR CARTÕES DE CRÉDITO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<List<CreditCardViewModel>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get()
        {
            var response = new List<CreditCardViewModel>();
            try
            {
                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredentials));

                var profileEntity = await _profileUserBusiness.FindByIdAsync(userId);

                if (profileEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound, responseList: true));

                var accountKey = profileEntity.GetAccountKey(_isSandBox);

                var listCreditCard =
                    await _creditCardRepository.FindByAsync(x => x.ProfileId == userId) as List<CreditCard>;

                if (string.IsNullOrEmpty(accountKey))
                    return Ok(Utilities.ReturnSuccess(data: response));

                var listIuguCards = await _iuguPaymentMethodService.ListarCredCardsAsync(accountKey) as List<IuguCreditCard>;

                if (listIuguCards != null)
                {

                    for (var i = 0; i < listIuguCards.Count; i++)
                    {
                        var cardIugu = listIuguCards[i];

                        if (cardIugu == null)
                            continue;

                        var card = listCreditCard.Find(x => x.TokenCard == cardIugu.Id);
                        var map = _mapper.Map<CreditCardViewModel>(cardIugu);

                        if (card == null || map == null)
                            continue;

                        map.Id = card._id.ToString();
                        map.Flag = _utilService.GetFlag(cardIugu.Data.Brand?.ToLower());

                        response.Add(map);
                    }
                }
                return Ok(Utilities.ReturnSuccess(data: response));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(responseList: true));
            }
        }

        /// <summary>
        /// DETALHES DO CARTÃO
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnGenericViewModel<CreditCardViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Details([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredentials));

                var profileEntity = await _profileUserBusiness.FindByIdAsync(userId);

                if (profileEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                var accountKey = profileEntity.GetAccountKey(_isSandBox);

                var cardEntity = await _creditCardRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (cardEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                var creditCardIugu = await _iuguPaymentMethodService.BuscarCredCardsAsync(accountKey, cardEntity.TokenCard);

                if (creditCardIugu == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFoundIugu));

                var cardViewModel = _mapper.Map<CreditCardViewModel>(creditCardIugu);

                if (cardViewModel == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                cardViewModel.Flag = _utilService.GetFlag(creditCardIugu.Data.Brand?.ToLower());
                cardViewModel.Id = cardEntity._id.ToString();

                return Ok(Utilities.ReturnSuccess(data: cardViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REGISTRAR CARTÃO DE CREDITO
        /// </summary>
        /// <remarks>
        ///     OBJ DE ENVIO
        ///
        ///     POST api/v1/CreditCard
        ///     {
        ///         "name": "string",
        ///         "number": "string", // #### #### #### ####
        ///         "expMonth": 0, // FORMAT MM
        ///         "expYear": 0, // FORMAT AAAA
        ///         "cvv": "string" // MINLENGTH 3 / MAX 4
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] CreditCardViewModel model)
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var now = DateTime.Now;

                if (model.Number?.Length < 14)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNumberRequired));
                if (model.ExpMonth > 12 || model.ExpMonth < 1)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.MonthInvalid));
                if (model.ExpYear < now.Year)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.YearInvalid));
                if (model.Cvv?.Length < 3 || model.Cvv?.Length > 4)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCvv));

                model.Name = model.Name.ToUpper();

                var profileEntity = await _profileUserBusiness.FindByIdAsync(userId).ConfigureAwait(false);

                if (profileEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                if (_isSandBox == false && string.IsNullOrEmpty(profileEntity.AccountKey) ||
                    _isSandBox && string.IsNullOrEmpty(profileEntity.AccountKeyDev))
                {

                    var iuguCustomerResponse = await _iuguCustomerServices.SaveClientAsync(new IuguCustomerModel
                    {
                        Email = profileEntity.Email,
                        Name = profileEntity.FullName
                    });

                    if (iuguCustomerResponse.HasError)
                        return BadRequest(Utilities.ReturnErro(iuguCustomerResponse.MessageError));

                    if (_isSandBox)
                        profileEntity.AccountKeyDev = iuguCustomerResponse.Id;
                    else
                        profileEntity.AccountKey = iuguCustomerResponse.Id;

                    profileEntity = _profileUserBusiness.Update(profileEntity);
                }

                var tokenCardIugu = await _iuguPaymentMethodService.SaveCreditCardAsync(new IuguPaymentMethodToken
                {
                    Method = "credit_card",
                    Test = _isSandBox.ToString().ToLower(),
                    Data = new IuguDataPaymentMethodToken
                    {
                        FirstName = model.Name.GetFirstName(),
                        LastName = model.Name.GetLastName(),
                        Number = model.Number,
                        Month = $"{model.ExpMonth:00}",
                        VerificationValue = model.Cvv,
                        Year = $"{model.ExpYear:0000}"
                    }
                });

                var accountKey = profileEntity.GetAccountKey(_isSandBox);

                if (string.IsNullOrEmpty(tokenCardIugu.MessageError) == false)
                    return BadRequest(Utilities.ReturnErro(tokenCardIugu.MessageError));

                var iuguCreditCardResponse = await _iuguPaymentMethodService.LinkCreditCardClientAsync(
                    new IuguCustomerPaymentMethod
                    {
                        CustomerId = accountKey,
                        Description = $"Meu {profileEntity.CreditCards.Count + 1} Cartão de credito",
                        SetAsDefault = (!profileEntity.CreditCards.Any()).ToString(),
                        Token = tokenCardIugu.Id
                    }, accountKey);

                if (iuguCreditCardResponse.HasError)
                    return BadRequest(Utilities.ReturnErro(iuguCreditCardResponse.MessageError));

                var creditCardId = _creditCardRepository.Create(new CreditCard
                {
                    ProfileId = userId,
                    TokenCard = iuguCreditCardResponse.Id
                });

                profileEntity.CreditCards.Add(creditCardId);

                await _profileUserBusiness.UpdateAsync(profileEntity);

                return Ok(Utilities.ReturnSuccess(DefaultMessages.Registered));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// REMOVER CARTÃO DE CRÉDITO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredentials));

                var profileEntity = await _profileUserBusiness.FindByIdAsync(userId);

                if (profileEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                var accountKey = profileEntity.GetAccountKey(_isSandBox);

                var creditCardLoad = await _creditCardRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (creditCardLoad == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                await _iuguPaymentMethodService.RemoverCredCardAsync(accountKey, creditCardLoad.TokenCard);

                _creditCardRepository.DeleteOne(id);

                profileEntity.CreditCards.Remove(id);
                await _profileUserBusiness.UpdateAsync(profileEntity);

                return Ok(Utilities.ReturnSuccess(DefaultMessages.Deleted));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}