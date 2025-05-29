using System.Collections.Generic;
using System.Linq;
using Meca.ApplicationService.Interface;
using Meca.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UtilityFramework.Application.Core3;

namespace Meca.WebApi.Controllers
{
    public abstract class MainController : ControllerBase
    {
        protected IService _service;
        public IHttpContextAccessor _httpContextAccessor { get; private set; }

        public MainController() { }

        public MainController(IService serviceEntity, IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            _httpContextAccessor = httpContext;
            _httpContextAccessor.HttpContext?.Request.EnableBuffering();
            _service = serviceEntity;
            _service?.SetAccess(_httpContextAccessor);
        }

        protected IActionResult ReturnResponse(object result, string message = null)
        {
            var returnViewModel = _service?.GetReturnViewModel();
            var errosValidation = _service?.ReturnValidations()?.Errors?.Select(x => x.ErrorMessage).ToList();

            var erros = new List<string>();

            if (errosValidation != null && errosValidation.Count > 0)
                erros.AddRange(errosValidation.ToList());

            if (returnViewModel != null)
                return BadRequest(returnViewModel);

            else if (erros.Count != 0)
                return BadRequest(Utilities.ReturnErro(erros[0], result));

            else if (result != null || string.IsNullOrEmpty(message) == false)
                return Ok(Utilities.ReturnSuccess(data: result, message: message));

            else
                return BadRequest(Utilities.ReturnErro(DefaultMessages.DefaultError, null));
        }
    }
}
