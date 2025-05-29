using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Meca.WebApi.Services
{
    public static class HealthCheckExtensions
    {
        public static Task WriteResponse(
            HttpContext context,
            HealthReport report)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

            string json = JsonSerializer.Serialize(
                new
                {
                    Status = report.Status.ToString(),
                    TemplateVersion = "1.0.18 - Atualizado em 09/12/2024, 09:48:46"
                },
                jsonSerializerOptions); ;

            context.Response.ContentType = MediaTypeNames.Application.Json;
            return context.Response.WriteAsync(json);
        }
    }
}
