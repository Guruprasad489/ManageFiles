using ManageFiles.API.Attributes;
using ManageFiles.API.Middlewares;
using ManageFilesUtility.Interfaces;
using ManageFilesUtility.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Configuration;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IConfiguration configuration = new ConfigurationBuilder()
                                       .AddJsonFile("appsettings.json")
                                       .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
                                       .AddEnvironmentVariables()
                                       .Build();

        var logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(configuration)
                   .Enrich.FromLogContext()
                   .WriteTo.Debug()
                   .WriteTo.Console()
                   .WriteTo.File("logs/ManageFileAPI.txt", rollingInterval: RollingInterval.Month)
                   .CreateLogger();


        // Add services to the container.

        //builder.Logging.ClearProviders();
        //builder.Logging.AddSerilog(logger);

        builder.Host.UseSerilog(logger);

        builder.Services.AddControllers();

        builder.Services.AddTransient<IPgpService, PgpService>();
        builder.Services.AddTransient<IFileService, FileService>();
        builder.Services.AddScoped<ApiKeyAuthFilter>();

        builder.Services.AddHttpContextAccessor();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Manage Files API", Version = "v1" });
            opt.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Description = "Input apikey to access this API",
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme",
            });
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" },
                        In = ParameterLocation.Header
                    },
                    new string[] { }
                }
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
            });
        }

        app.UseSerilogRequestLogging();

        //app.UseMiddleware<ApiKeyMiddleware>();
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}