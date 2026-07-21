using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VehicleSalesFIAP.Infrastructure.Persistence;

namespace VehicleSalesFIAP.Tests.Integration;

internal sealed class VehicleSalesApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"VehicleSalesFIAPTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            var databaseServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.RemoveAll<DbContextOptions<VehicleSalesDbContext>>();
            services.AddDbContext<VehicleSalesDbContext>(options =>
            {
                options
                    .UseInMemoryDatabase(databaseName)
                    .UseInternalServiceProvider(databaseServiceProvider)
                    .AddInterceptors(new TestRowVersionInterceptor());
            });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    configureOptions: null);
        });
    }

    public HttpClient CreateAuthenticatedClient(string userId, params string[] roles)
    {
        var client = CreateClient();

        client.DefaultRequestHeaders.Add(TestAuthHeaderNames.UserId, userId);
        if (roles.Length > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHeaderNames.Roles, string.Join(',', roles));
        }

        return client;
    }
}
