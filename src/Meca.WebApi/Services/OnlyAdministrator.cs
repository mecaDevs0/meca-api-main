using Meca.Data.Entities;
using Meca.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Application.Core3.ViewModels;
using UtilityFramework.Infra.Core3.MongoDb.Business;

namespace Meca.WebApi.Services
{
    public class OnlyAdministrator : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var _userAdministratorRepository = (IBusinessBaseAsync<UserAdministrator>)context.HttpContext.RequestServices.GetService(typeof(IBusinessBaseAsync<UserAdministrator>));
                var userId = context.HttpContext.Request.GetUserId();

                var response = new ReturnViewModel();
                response.Erro = true;
                response.Errors = null;
                response.Message = DefaultMessages.OnlyAdministrator;
                var path = context.HttpContext.Request.Path.ToString().ToLower();


                if (string.IsNullOrEmpty(userId))
                {
                    context.Result = new BadRequestObjectResult(response);
                    return;
                }
                else
                {
                    var userAdministrator = _userAdministratorRepository.FindById(userId);

                    if (userAdministrator == null)
                    {
                        context.Result = new BadRequestObjectResult(response);

                        return;
                    }
                    else if (userAdministrator.DataBlocked != null || userAdministrator.Disabled != null)
                    {
                        response.Message = DefaultMessages.UserAdministratorBlocked;
                        context.Result = new BadRequestObjectResult(response);
                        return;
                    }

                }
            }
            catch (System.Exception) { /*unused*/ }
        }

    }
}