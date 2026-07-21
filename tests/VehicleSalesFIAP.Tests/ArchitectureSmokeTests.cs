using System.Reflection;
using ApplicationDependencyInjection = VehicleSalesFIAP.Application.DependencyInjection;
using InfrastructureDependencyInjection = VehicleSalesFIAP.Infrastructure.DependencyInjection;
using Vehicle = VehicleSalesFIAP.Domain.Vehicles.Vehicle;

namespace VehicleSalesFIAP.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void ApplicationAndInfrastructureAssembliesExposeDependencyRegistration()
    {
        Assert.Equal("VehicleSalesFIAP.Application", typeof(ApplicationDependencyInjection).Assembly.GetName().Name);
        Assert.Equal("VehicleSalesFIAP.Infrastructure", typeof(InfrastructureDependencyInjection).Assembly.GetName().Name);
    }

    [Fact]
    public void DomainDoesNotReferenceOuterLayersOrFrameworks()
    {
        var references = GetReferenceNames(typeof(Vehicle).Assembly);

        Assert.DoesNotContain("VehicleSalesFIAP.Application", references);
        Assert.DoesNotContain("VehicleSalesFIAP.Infrastructure", references);
        Assert.DoesNotContain("VehicleSalesFIAP.Api", references);
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void ApplicationDoesNotReferenceInfrastructureOrApi()
    {
        var references = GetReferenceNames(typeof(ApplicationDependencyInjection).Assembly);

        Assert.DoesNotContain("VehicleSalesFIAP.Infrastructure", references);
        Assert.DoesNotContain("VehicleSalesFIAP.Api", references);
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void InfrastructureDoesNotReferenceApi()
    {
        var references = GetReferenceNames(typeof(InfrastructureDependencyInjection).Assembly);

        Assert.DoesNotContain("VehicleSalesFIAP.Api", references);
    }

    private static HashSet<string> GetReferenceNames(Assembly assembly)
    {
        return assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
    }
}
