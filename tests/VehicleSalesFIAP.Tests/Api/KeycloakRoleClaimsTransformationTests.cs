using System.Security.Claims;
using VehicleSalesFIAP.Api.Security;

namespace VehicleSalesFIAP.Tests.Api;

public sealed class KeycloakRoleClaimsTransformationTests
{
    [Fact]
    public async Task TransformAsyncAddsRealmRolesAsAspNetRoles()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim("realm_access", """{"roles":["vehicle-manager","buyer"]}""")
            ],
            authenticationType: "Bearer");
        var principal = new ClaimsPrincipal(identity);
        var transformation = new KeycloakRoleClaimsTransformation();

        var transformed = await transformation.TransformAsync(principal);

        Assert.True(transformed.IsInRole(ApplicationRoles.VehicleManager));
        Assert.True(transformed.IsInRole(ApplicationRoles.Buyer));
    }

    [Fact]
    public async Task TransformAsyncDoesNotDuplicateExistingRoles()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim("realm_access", """{"roles":["vehicle-manager"]}"""),
                new Claim(ClaimTypes.Role, ApplicationRoles.VehicleManager)
            ],
            authenticationType: "Bearer");
        var principal = new ClaimsPrincipal(identity);
        var transformation = new KeycloakRoleClaimsTransformation();

        var transformed = await transformation.TransformAsync(principal);

        Assert.Single(transformed.Claims, claim =>
            claim.Type == ClaimTypes.Role &&
            claim.Value == ApplicationRoles.VehicleManager);
    }
}
