using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace VehicleSalesFIAP.Api.Security;

public sealed class KeycloakRoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        AddRealmRoles(identity);
        AddClientRoles(identity);

        return Task.FromResult(principal);
    }

    private static void AddRealmRoles(ClaimsIdentity identity)
    {
        var realmAccess = identity.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return;
        }

        using var document = JsonDocument.Parse(realmAccess);
        if (!document.RootElement.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var role in roles.EnumerateArray())
        {
            AddRole(identity, role.GetString());
        }
    }

    private static void AddClientRoles(ClaimsIdentity identity)
    {
        var resourceAccess = identity.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccess))
        {
            return;
        }

        using var document = JsonDocument.Parse(resourceAccess);
        foreach (var client in document.RootElement.EnumerateObject())
        {
            if (!client.Value.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var role in roles.EnumerateArray())
            {
                AddRole(identity, role.GetString());
            }
        }
    }

    private static void AddRole(ClaimsIdentity identity, string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return;
        }

        if (!identity.HasClaim(ClaimTypes.Role, role))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
    }
}
