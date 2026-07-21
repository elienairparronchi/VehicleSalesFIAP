using ApplicationDependencyInjection = VehicleSalesFIAP.Application.DependencyInjection;
using InfrastructureDependencyInjection = VehicleSalesFIAP.Infrastructure.DependencyInjection;

namespace VehicleSalesFIAP.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void ApplicationAndInfrastructureAssembliesExposeDependencyRegistration()
    {
        Assert.Equal("VehicleSalesFIAP.Application", typeof(ApplicationDependencyInjection).Assembly.GetName().Name);
        Assert.Equal("VehicleSalesFIAP.Infrastructure", typeof(InfrastructureDependencyInjection).Assembly.GetName().Name);
    }
}
