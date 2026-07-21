using Microsoft.Extensions.DependencyInjection;
using VehicleSalesFIAP.Application.Vehicles.DeleteVehicle;
using VehicleSalesFIAP.Application.Vehicles.GetVehicleById;
using VehicleSalesFIAP.Application.Vehicles.ListAvailableVehicles;
using VehicleSalesFIAP.Application.Vehicles.ListSoldVehicles;
using VehicleSalesFIAP.Application.Vehicles.PurchaseVehicle;
using VehicleSalesFIAP.Application.Vehicles.RegisterVehicleForSale;
using VehicleSalesFIAP.Application.Vehicles.UpdateVehicle;

namespace VehicleSalesFIAP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterVehicleForSaleUseCase>();
        services.AddScoped<UpdateVehicleUseCase>();
        services.AddScoped<DeleteVehicleUseCase>();
        services.AddScoped<GetVehicleByIdUseCase>();
        services.AddScoped<ListAvailableVehiclesUseCase>();
        services.AddScoped<ListSoldVehiclesUseCase>();
        services.AddScoped<PurchaseVehicleUseCase>();

        return services;
    }
}
