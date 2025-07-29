using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Hangfire.Dashboard;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using UtilityFramework.Application.Core3;

namespace Meca.ApplicationService.Services.HangFire
{
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MyAuthorizationFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public bool Authorize([NotNull] DashboardContext context)
        {
            try
            {
                var allowAccess = Utilities.GetConfigurationRoot().GetSection("allowAccess")?.Get<List<string>>() ?? new List<string>();

                return allowAccess.Count == 0 || allowAccess.Count(x => x == _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString()) > 0;

            }
            catch (Exception)
            {

                return false;
            }

        }
    }
}