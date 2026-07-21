using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using VehicleSalesFIAP.Api.Security;

namespace VehicleSalesFIAP.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Authentication");
        var authority = section["Authority"];
        var audience = section["Audience"];
        var requireHttpsMetadata = section.GetValue("RequireHttpsMetadata", true);
        var validIssuers = section.GetSection("ValidIssuers").Get<string[]>() ?? [];

        if (string.IsNullOrWhiteSpace(authority))
        {
            throw new InvalidOperationException("Authentication authority was not configured.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("Authentication audience was not configured.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = requireHttpsMetadata;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuer = true,
                    ValidIssuers = validIssuers.Length > 0 ? validIssuers : null,
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddTransient<IClaimsTransformation, KeycloakRoleClaimsTransformation>();

        return services;
    }

    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.VehicleManagement,
                policy => policy.RequireAuthenticatedUser().RequireRole(ApplicationRoles.VehicleManager));

            options.AddPolicy(
                AuthorizationPolicies.Buyer,
                policy => policy.RequireAuthenticatedUser().RequireRole(ApplicationRoles.Buyer));
        });

        return services;
    }

    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "VehicleSalesFIAP API",
                Version = "v1",
                Description = "API para revenda de veiculos do Tech Challenge FIAP/SOAT."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe um token JWT emitido pelo Keycloak."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
            });
        });

        return services;
    }
}
