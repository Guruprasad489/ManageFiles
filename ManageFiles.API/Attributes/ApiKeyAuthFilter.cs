using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ManageFiles.API.Attributes
{
    public class ApiKeyAuthFilter : IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string? userApiKey = context.HttpContext.Request.Headers["X-API-KEY"].ToString();
            string? apiKey = _configuration.GetValue<string>("ApiKey");

            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (string.IsNullOrWhiteSpace(apiKey) || !apiKey.Equals(userApiKey))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}