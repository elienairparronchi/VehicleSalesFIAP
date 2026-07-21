using System.Net;
using System.Net.Http.Json;
using VehicleSalesFIAP.Api.Security;

namespace VehicleSalesFIAP.Tests.Integration;

public sealed class VehicleEndpointsEndToEndTests
{
    [Fact]
    public async Task VehiclePurchaseFlowCompletesEndToEnd()
    {
        using var factory = new VehicleSalesApiFactory();
        using var anonymousClient = factory.CreateClient();
        using var managerClient = factory.CreateAuthenticatedClient("manager-sub", ApplicationRoles.VehicleManager);
        using var buyerClient = factory.CreateAuthenticatedClient("buyer-sub", ApplicationRoles.Buyer);
        var request = new CreateVehicleTestRequest("Volkswagen", "T-Cross", 2024, "Gray", 145000);

        var anonymousCreateResponse = await anonymousClient.PostAsJsonAsync("/api/v1/vehicles", request);
        var createResponse = await managerClient.PostAsJsonAsync("/api/v1/vehicles", request);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousCreateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleTestResponse>();
        var availableVehicles = await anonymousClient.GetFromJsonAsync<List<VehicleTestResponse>>("/api/v1/vehicles/available");
        var managerPurchaseResponse = await managerClient.PostAsync($"/api/v1/vehicles/{createdVehicle!.Id}/purchase", content: null);
        var buyerPurchaseResponse = await buyerClient.PostAsync($"/api/v1/vehicles/{createdVehicle.Id}/purchase", content: null);
        Assert.Equal(HttpStatusCode.Forbidden, managerPurchaseResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, buyerPurchaseResponse.StatusCode);

        var purchase = await buyerPurchaseResponse.Content.ReadFromJsonAsync<PurchaseVehicleTestResponse>();
        var purchasedVehicle = await anonymousClient.GetFromJsonAsync<VehicleTestResponse>($"/api/v1/vehicles/{createdVehicle.Id}");
        var availableVehiclesAfterPurchase = await anonymousClient.GetFromJsonAsync<List<VehicleTestResponse>>("/api/v1/vehicles/available");
        var soldVehicles = await managerClient.GetFromJsonAsync<List<VehicleTestResponse>>("/api/v1/vehicles/sold");
        var secondPurchaseResponse = await buyerClient.PostAsync($"/api/v1/vehicles/{createdVehicle.Id}/purchase", content: null);

        Assert.Contains(availableVehicles!, vehicle => vehicle.Id == createdVehicle.Id);
        Assert.Equal(createdVehicle.Id, purchase!.VehicleId);
        Assert.Equal("buyer-sub", purchase.BuyerId);
        Assert.Equal(145000, purchase.PurchasePrice);
        Assert.Equal("Sold", purchasedVehicle!.Status);
        Assert.DoesNotContain(availableVehiclesAfterPurchase!, vehicle => vehicle.Id == createdVehicle.Id);
        Assert.Contains(soldVehicles!, vehicle => vehicle.Id == createdVehicle.Id && vehicle.Status == "Sold");
        Assert.Equal(HttpStatusCode.BadRequest, secondPurchaseResponse.StatusCode);
    }

    [Fact]
    public async Task SoldVehiclesEndpointRequiresVehicleManagerRole()
    {
        using var factory = new VehicleSalesApiFactory();
        using var anonymousClient = factory.CreateClient();
        using var buyerClient = factory.CreateAuthenticatedClient("buyer-sub", ApplicationRoles.Buyer);
        using var managerClient = factory.CreateAuthenticatedClient("manager-sub", ApplicationRoles.VehicleManager);

        var anonymousResponse = await anonymousClient.GetAsync("/api/v1/vehicles/sold");
        var buyerResponse = await buyerClient.GetAsync("/api/v1/vehicles/sold");
        var managerResponse = await managerClient.GetAsync("/api/v1/vehicles/sold");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, buyerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, managerResponse.StatusCode);
    }

    [Fact]
    public async Task HealthEndpointsArePublic()
    {
        using var factory = new VehicleSalesApiFactory();
        using var client = factory.CreateClient();

        var apiHealthResponse = await client.GetAsync("/api/v1/health");
        var platformHealthResponse = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, apiHealthResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, platformHealthResponse.StatusCode);
    }

    private sealed record CreateVehicleTestRequest(
        string Brand,
        string Model,
        int Year,
        string Color,
        decimal Price);

    private sealed record VehicleTestResponse(
        Guid Id,
        string Brand,
        string Model,
        int Year,
        string Color,
        decimal Price,
        string Currency,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt,
        DateTimeOffset? SoldAt);

    private sealed record PurchaseVehicleTestResponse(
        Guid SaleId,
        Guid VehicleId,
        string BuyerId,
        decimal PurchasePrice,
        string Currency,
        DateTimeOffset PurchasedAt);
}
