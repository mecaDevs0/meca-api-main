using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;

namespace Meca.WebApi.Services
{
    public class PreventSpanFilter : ActionFilterAttribute
    {
        public int DelayRequest { get; set; } = 1;
        public string ErrorMessage { get; set; } = null;
        private List<string> ignoreEndPoints = ["/trigger", "/loaddata", "/token", "/availablescheduling"];



        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {

                if (filterContext.HttpContext.Request.Method.ToLower() == "get" || (filterContext.HttpContext.Request.HasFormContentType && filterContext.HttpContext.Request.Form.ContainsKey("columns")) || (ignoreEndPoints.Count(endpoint => filterContext.HttpContext.Request.Path.Value.IndexOf(endpoint, StringComparison.OrdinalIgnoreCase) != -1) > 0))
                    return;

                if (filterContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    if (controllerActionDescriptor.MethodInfo.CustomAttributes.Count(x => x.AttributeType == typeof(PreventSpanFilter)) > 0 && (DelayRequest == 1 && string.IsNullOrEmpty(ErrorMessage)))
                        return;
                }

                var cacheService = filterContext.HttpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;

                var request = filterContext.HttpContext.Request;

                var originationInfo = request.HttpContext.Connection.RemoteIpAddress.ToString();

                if (request.Headers.Count(x => x.Key == "User-Agent") > 0)
                    originationInfo += request.Headers["User-Agent"].ToString();

                try
                {
                    if (request.Headers.Count(x => x.Key == "Authorization") > 0)
                        originationInfo += request.Headers["Authorization"].ToString();
                }
                catch (Exception)
                { }

                try
                {
                    var inputStream = request.Body;

                    byte[] jsonBodyBytes = new byte[inputStream.Length];
                    inputStream.Seek(0, SeekOrigin.Begin);
                    inputStream.ReadAsync(jsonBodyBytes, 0, jsonBodyBytes.Length);

                    string jsonBody = Encoding.UTF8.GetString(jsonBodyBytes);

                    originationInfo += jsonBody.UnprettyJson();
                }
                catch (Exception)
                {
                }

                var targetInfo = UriHelper.GetDisplayUrl(request);

                var hashValue = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(originationInfo + targetInfo)).Select(s => s.ToString("x2")));

                if (cacheService.TryGetValue(hashValue, out var resultCache))
                {
                    var response = new ReturnViewModel()
                    {
                        Erro = true,
                        Message = ErrorMessage ?? $"Você realizou essa mesma requisição em menos de {DelayRequest}s"
                    };

                    filterContext.Result = new BadRequestObjectResult(response);
                }
                else
                {
                    var cache = cacheService.GetOrCreate(hashValue, entry =>
                    {
                        entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(DelayRequest));
                        entry.Priority = CacheItemPriority.Normal;
                        return entry;
                    });
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}