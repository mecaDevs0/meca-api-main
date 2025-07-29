using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Meca.Data.Entities;
using Meca.Data.Enum;
using Meca.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.JwtMiddleware;
using UtilityFramework.Infra.Core3.MongoDb.Business;
using UtilityFramework.Infra.Core3.MongoDb.Data.Modelos;
using Microsoft.Extensions.DependencyInjection;


namespace Meca.WebApi.Services
{
    public class BlockMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IBusinessBaseAsync<Data.Entities.Profile> _profileRepository;
        private readonly IBusinessBaseAsync<UserAdministrator> _userAdministratorRepository;
        private readonly IHostingEnvironment _env;

        public BlockMiddleware(
            RequestDelegate next,
            IBusinessBaseAsync<Data.Entities.Profile> profileRepository,
            IBusinessBaseAsync<UserAdministrator> userAdministratorRepository,
            IHostingEnvironment env
        )
        {
            _next = next;
            _profileRepository = profileRepository;
            _userAdministratorRepository = userAdministratorRepository;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var userId = context.Request.GetUserId();
                var isBlocked = false;
                var isNotFound = false;
                string customMessage = null;

                if (context.Request.Path.Value.ToLower().Contains("api/") && string.IsNullOrEmpty(userId) == false)
                {

                    var role = context.Request.GetRole().ToString();

                    ModelBase entity = null;

                    switch (role)
                    {
                        case nameof(TypeProfile.UserAdministrator):
                            entity = await GetEntity<UserAdministrator>(userId);
                            customMessage = DefaultMessages.UserAdministratorNotFound;
                            break;
                        case nameof(TypeProfile.Workshop):
                            entity = await GetEntity<Workshop>(userId);
                            customMessage = DefaultMessages.WorkshopNotFound;
                            break;
                        default:
                            entity = await GetEntity<Data.Entities.Profile>(userId);
                            customMessage = DefaultMessages.ProfileNotFound;
                            break;
                    }

                    if (entity == null)
                    {
                        isNotFound = true;
                    }
                    else if (entity.DataBlocked != null)
                    {
                        customMessage = DefaultMessages.AccessBlocked;
                        isBlocked = true;
                    }

                }

                if (isNotFound)
                {
                    await MapErro(context, 400, null, customMessage);
                    return;

                }
                else if (isBlocked)
                {
                    await MapErro(context, 423, null, customMessage);
                    return;
                }
                else
                {
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                await MapErro(context, (int)HttpStatusCode.InternalServerError, ex);
            }
        }

        private static async Task MapErro(HttpContext context, int statusCode, Exception ex = null, string customMessage = null)
        {
            using var newBody = new MemoryStream();
            var response = ex != null ? ex.ReturnErro(DefaultMessages.MessageException) : Utilities.ReturnErro(customMessage ?? DefaultMessages.AccessBlocked);

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }

        private async Task<TEntity> GetEntity<TEntity>(string id) where TEntity : ModelBase
        {
            try
            {
                // Obtém o repositório via DI do contexto da requisição
                var httpContext = _env as IHttpContextAccessor != null ? ((IHttpContextAccessor)_env).HttpContext : null;
                if (httpContext == null)
                    throw new InvalidOperationException("HttpContext não disponível para resolver dependências.");

                var repository = httpContext.RequestServices.GetRequiredService<IBusinessBaseAsync<TEntity>>();
                return await repository.FindByIdAsync(id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public static class BlockMiddlewareExtensions
    {
        public static IApplicationBuilder UseBlockMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<BlockMiddleware>();
    }
}