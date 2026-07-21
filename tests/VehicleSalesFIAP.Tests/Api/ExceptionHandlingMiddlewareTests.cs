using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleSalesFIAP.Api.Middleware;
using VehicleSalesFIAP.Application.Common.Exceptions;

namespace VehicleSalesFIAP.Tests.Api;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsyncMapsConflictExceptionToHttp409()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ConflictException("The vehicle has already been sold."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("Conflict", response.RootElement.GetProperty("title").GetString());
        Assert.Equal(
            "The vehicle has already been sold.",
            response.RootElement.GetProperty("detail").GetString());
    }
}
