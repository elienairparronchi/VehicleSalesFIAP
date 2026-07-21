using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace VehicleSalesFIAP.Api.Security;

public sealed class KeycloakRoleClaimsTransformation : IClaimsTransformation
{
    private readonly string clientId;

    public KeycloakRoleClaimsTransformation(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("The Keycloak client identifier is required.", nameof(clientId));
        }

        this.clientId = clientId;
    }

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

        try
        {
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
        catch (JsonException)
        {
            // Ignore malformed optional role claims from external identity providers.
        }
    }

    private void AddClientRoles(ClaimsIdentity identity)
    {
        var resourceAccess = identity.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccess))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(resourceAccess);
            if (!document.RootElement.TryGetProperty(clientId, out var client) ||
                !client.TryGetProperty("roles", out var roles) ||
                roles.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var role in roles.EnumerateArray())
            {
                AddRole(identity, role.GetString());
            }
        }
        catch (JsonException)
        {
            // Ignore malformed optional role claims from external identity providers.
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
