using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag.Generation.Processors.Security;
using NSwag;
using Microsoft.AspNetCore.Mvc.Versioning;
using NSwag.Generation.AspNetCore;

namespace TimeTracker.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(
                    options =>
                    {
                        var tokenValidationParameters = new TokenValidationParameters
                        {
                            ValidIssuer = configuration["Tokens:Issuer"],
                            ValidAudience = configuration["Tokens:Issuer"],
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    System.Text.Encoding.UTF8.GetBytes(configuration["Tokens:Key"]))
                        };

                        options.TokenValidationParameters = tokenValidationParameters;
                    });
        }

        private static void InitializeOpenApiDocumentOptions(
            AspNetCoreOpenApiDocumentGeneratorSettings options, string version)
        {
            options.DocumentName = version;
            options.Title = $"Time Tracker API {version}";
            options.ApiGroupNames = new[] { version };

            options.Description = "An API for ASP.NET Core Workshop";
            options.IgnoreObsoleteProperties = true;

            options.OperationProcessors.Add(
                new OperationSecurityScopeProcessor("jwt-token"));
            options.DocumentProcessors.Add(
                new SecurityDefinitionAppender(
                    "jwt-token", new[] { "" }, new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        Description =
                            "Enter \"Bearer jwt-token\" as value. " +
                            "Use https://localhost:6001/get-token to get read-only JWT token. " +
                            "Use https://localhost:6001/get-token?admin=true to get admin (read-write) JWT token.",
                        In = OpenApiSecurityApiKeyLocation.Header
                    }));

            options.PostProcess = document =>
            {
                document.Info.Contact = new OpenApiContact
                {
                    Name = "Miroslav Popovic",
                    Email = string.Empty,
                    Url = "https://miroslavpopovic.com"
                };
                document.Info.License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = "https://opensource.org/licenses/MIT"
                };
            };
        }

        public static void AddOpenApi(this IServiceCollection services)
        {
            services
                .AddOpenApiDocument(
                    options => InitializeOpenApiDocumentOptions(options, "v2"))
                .AddOpenApiDocument(
                    options => InitializeOpenApiDocumentOptions(options, "v1"));
        }

        public static void AddVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(
                options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                });
            services.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstitutionFormat = "VVV";
                    options.SubstituteApiVersionInUrl = true;
                    options.ApiVersionParameterSource = new UrlSegmentApiVersionReader();
                });
        }
    }
}
