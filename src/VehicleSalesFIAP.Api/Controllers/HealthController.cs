using Microsoft.AspNetCore.Mvc;

namespace VehicleSalesFIAP.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<HealthCheckResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthCheckResponse> Get()
    {
        return Ok(new HealthCheckResponse("Healthy", "VehicleSalesFIAP.Api", DateTimeOffset.UtcNow));
    }
}

public sealed record HealthCheckResponse(string Status, string Service, DateTimeOffset CheckedAt);
