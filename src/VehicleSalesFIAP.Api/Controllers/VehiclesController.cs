using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleSalesFIAP.Api.Contracts.Vehicles;
using VehicleSalesFIAP.Api.Security;
using VehicleSalesFIAP.Application.Vehicles;
using VehicleSalesFIAP.Application.Vehicles.DeleteVehicle;
using VehicleSalesFIAP.Application.Vehicles.GetVehicleById;
using VehicleSalesFIAP.Application.Vehicles.ListAvailableVehicles;
using VehicleSalesFIAP.Application.Vehicles.ListSoldVehicles;
using VehicleSalesFIAP.Application.Vehicles.PurchaseVehicle;
using VehicleSalesFIAP.Application.Vehicles.RegisterVehicleForSale;
using VehicleSalesFIAP.Application.Vehicles.UpdateVehicle;

namespace VehicleSalesFIAP.Api.Controllers;

[ApiController]
[Route("api/v1/vehicles")]
[Produces("application/json")]
public sealed class VehiclesController(
    RegisterVehicleForSaleUseCase registerVehicleForSale,
    UpdateVehicleUseCase updateVehicle,
    DeleteVehicleUseCase deleteVehicle,
    GetVehicleByIdUseCase getVehicleById,
    ListAvailableVehiclesUseCase listAvailableVehicles,
    ListSoldVehiclesUseCase listSoldVehicles,
    PurchaseVehicleUseCase purchaseVehicle)
    : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.VehicleManagement)]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VehicleResponse>> Create(
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await registerVehicleForSale.HandleAsync(
            new RegisterVehicleForSaleCommand(
                request.Brand,
                request.Model,
                request.Year,
                request.Color,
                request.Price),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.VehicleManagement)]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse>> Update(
        Guid id,
        [FromBody] UpdateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await updateVehicle.HandleAsync(
            new UpdateVehicleCommand(
                id,
                request.Brand,
                request.Model,
                request.Year,
                request.Color,
                request.Price),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await getVehicleById.HandleAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpPost("{id:guid}/purchase")]
    [Authorize(Policy = AuthorizationPolicies.Buyer)]
    [ProducesResponseType<PurchaseVehicleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseVehicleResponse>> Purchase(Guid id, CancellationToken cancellationToken)
    {
        var buyerId = User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(buyerId))
        {
            return Unauthorized();
        }

        var response = await purchaseVehicle.HandleAsync(
            new PurchaseVehicleCommand(id, buyerId),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.VehicleManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteVehicle.HandleAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpGet("available")]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<VehicleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> ListAvailable(CancellationToken cancellationToken)
    {
        var response = await listAvailableVehicles.HandleAsync(cancellationToken);

        return Ok(response);
    }

    [HttpGet("sold")]
    [Authorize(Policy = AuthorizationPolicies.VehicleManagement)]
    [ProducesResponseType<IReadOnlyList<VehicleResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> ListSold(CancellationToken cancellationToken)
    {
        var response = await listSoldVehicles.HandleAsync(cancellationToken);

        return Ok(response);
    }
}
