using System.Net;

namespace ManageFiles.API.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? userApiKey = context.Request.Headers["X-API-KEY"];
            string? apiKey = _configuration.GetValue<string>("ApiKey");

            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new {status = false, Message = "Unauthorized" });
                return;
            }

            if (string.IsNullOrWhiteSpace(apiKey) || !apiKey.Equals(userApiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new { status = false, Message = "Unauthorized" });
                return;
            }

            await _next(context);
        }
    }
}
