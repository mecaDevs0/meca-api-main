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

        public BlockMiddleware(RequestDelegate next)
        {
            _next = next;
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
                            entity = await GetEntity<UserAdministrator>(userId, context.RequestServices);
                            customMessage = DefaultMessages.UserAdministratorNotFound;
                            break;
                        case nameof(TypeProfile.Workshop):
                            entity = await GetEntity<Workshop>(userId, context.RequestServices);
                            customMessage = DefaultMessages.WorkshopNotFound;
                            break;
                        default:
                            entity = await GetEntity<Data.Entities.Profile>(userId, context.RequestServices);
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

        private async Task<TEntity> GetEntity<TEntity>(string id, IServiceProvider serviceProvider) where TEntity : ModelBase
        {
            try
            {
                var repository = serviceProvider.GetRequiredService<IBusinessBaseAsync<TEntity>>();
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