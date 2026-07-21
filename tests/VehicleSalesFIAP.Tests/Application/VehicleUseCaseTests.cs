using VehicleSalesFIAP.Application.Abstractions.Clock;
using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Application.Common.Exceptions;
using VehicleSalesFIAP.Application.Vehicles.DeleteVehicle;
using VehicleSalesFIAP.Application.Vehicles.GetVehicleById;
using VehicleSalesFIAP.Application.Vehicles.ListAvailableVehicles;
using VehicleSalesFIAP.Application.Vehicles.PurchaseVehicle;
using VehicleSalesFIAP.Application.Vehicles.RegisterVehicleForSale;
using VehicleSalesFIAP.Application.Vehicles.UpdateVehicle;
using VehicleSalesFIAP.Domain.Common;
using VehicleSalesFIAP.Domain.Sales;
using VehicleSalesFIAP.Domain.ValueObjects;
using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Tests.Application;

public sealed class VehicleUseCaseTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 7, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RegisterVehicleForSaleCreatesVehicleAndSavesChanges()
    {
        var repository = new InMemoryVehicleRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var useCase = new RegisterVehicleForSaleUseCase(repository, unitOfWork, new FixedDateTimeProvider(FixedNow));

        var response = await useCase.HandleAsync(new RegisterVehicleForSaleCommand(
            "Toyota",
            "Corolla",
            2022,
            "Silver",
            95000));

        Assert.Equal("Toyota", response.Brand);
        Assert.Equal("Available", response.Status);
        Assert.Equal(FixedNow, response.CreatedAt);
        Assert.True(unitOfWork.WasSaved);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task UpdateVehicleChangesAvailableVehicleAndSavesChanges()
    {
        var repository = new InMemoryVehicleRepository();
        var vehicle = Vehicle.RegisterForSale("Honda", "Civic", 2021, "Black", Money.From(100000), FixedNow.AddDays(-1));
        repository.Items.Add(vehicle);
        var unitOfWork = new InMemoryUnitOfWork();
        var useCase = new UpdateVehicleUseCase(repository, unitOfWork, new FixedDateTimeProvider(FixedNow));

        var response = await useCase.HandleAsync(new UpdateVehicleCommand(
            vehicle.Id,
            "Honda",
            "HR-V",
            2023,
            "White",
            130000));

        Assert.Equal("HR-V", response.Model);
        Assert.Equal(130000, response.Price);
        Assert.Equal(FixedNow, response.UpdatedAt);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task GetVehicleByIdThrowsWhenVehicleDoesNotExist()
    {
        var useCase = new GetVehicleByIdUseCase(new InMemoryVehicleRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.HandleAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListAvailableVehiclesReturnsVehiclesOrderedByPrice()
    {
        var repository = new InMemoryVehicleRepository();
        repository.Items.Add(Vehicle.RegisterForSale("Brand B", "Model B", 2020, "Blue", Money.From(70000), FixedNow));
        repository.Items.Add(Vehicle.RegisterForSale("Brand A", "Model A", 2020, "Black", Money.From(50000), FixedNow));
        var useCase = new ListAvailableVehiclesUseCase(repository);

        var response = await useCase.HandleAsync();

        Assert.Collection(
            response,
            first => Assert.Equal(50000, first.Price),
            second => Assert.Equal(70000, second.Price));
    }

    [Fact]
    public async Task DeleteVehicleRemovesAvailableVehicleAndSavesChanges()
    {
        var repository = new InMemoryVehicleRepository();
        var vehicle = Vehicle.RegisterForSale("Fiat", "Pulse", 2024, "Red", Money.From(100000), FixedNow);
        repository.Items.Add(vehicle);
        var unitOfWork = new InMemoryUnitOfWork();
        var useCase = new DeleteVehicleUseCase(repository, unitOfWork);

        await useCase.HandleAsync(vehicle.Id);

        Assert.Empty(repository.Items);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task DeleteVehicleRejectsSoldVehicle()
    {
        var repository = new InMemoryVehicleRepository();
        var vehicle = Vehicle.RegisterForSale("Jeep", "Compass", 2024, "Gray", Money.From(180000), FixedNow.AddDays(-1));
        vehicle.SellTo("buyer-123", FixedNow);
        repository.Items.Add(vehicle);
        var useCase = new DeleteVehicleUseCase(repository, new InMemoryUnitOfWork());

        await Assert.ThrowsAsync<DomainException>(() => useCase.HandleAsync(vehicle.Id));
    }

    [Fact]
    public async Task PurchaseVehicleSellsAvailableVehicleAndSavesSale()
    {
        var vehicleRepository = new InMemoryVehicleRepository();
        var saleRepository = new InMemorySaleRepository();
        var vehicle = Vehicle.RegisterForSale("Nissan", "Kicks", 2023, "White", Money.From(120000), FixedNow.AddDays(-1));
        vehicleRepository.Items.Add(vehicle);
        var unitOfWork = new InMemoryUnitOfWork();
        var useCase = new PurchaseVehicleUseCase(
            vehicleRepository,
            saleRepository,
            unitOfWork,
            new FixedDateTimeProvider(FixedNow));

        var response = await useCase.HandleAsync(new PurchaseVehicleCommand(vehicle.Id, "buyer-keycloak-sub"));

        Assert.Equal(vehicle.Id, response.VehicleId);
        Assert.Equal("buyer-keycloak-sub", response.BuyerId);
        Assert.Equal(120000, response.PurchasePrice);
        Assert.Equal(FixedNow, response.PurchasedAt);
        Assert.Equal(VehicleStatus.Sold, vehicle.Status);
        Assert.Equal(FixedNow, vehicle.SoldAt);
        Assert.Single(saleRepository.Items);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task PurchaseVehicleThrowsWhenVehicleDoesNotExist()
    {
        var useCase = new PurchaseVehicleUseCase(
            new InMemoryVehicleRepository(),
            new InMemorySaleRepository(),
            new InMemoryUnitOfWork(),
            new FixedDateTimeProvider(FixedNow));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.HandleAsync(new PurchaseVehicleCommand(Guid.NewGuid(), "buyer-keycloak-sub")));
    }

    [Fact]
    public async Task PurchaseVehicleRejectsVehicleAlreadySold()
    {
        var vehicleRepository = new InMemoryVehicleRepository();
        var saleRepository = new InMemorySaleRepository();
        var vehicle = Vehicle.RegisterForSale("Hyundai", "HB20", 2022, "Silver", Money.From(85000), FixedNow.AddDays(-2));
        vehicle.SellTo("first-buyer", FixedNow.AddDays(-1));
        vehicleRepository.Items.Add(vehicle);
        var unitOfWork = new InMemoryUnitOfWork();
        var useCase = new PurchaseVehicleUseCase(
            vehicleRepository,
            saleRepository,
            unitOfWork,
            new FixedDateTimeProvider(FixedNow));

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.HandleAsync(new PurchaseVehicleCommand(vehicle.Id, "second-buyer")));
        Assert.Empty(saleRepository.Items);
        Assert.False(unitOfWork.WasSaved);
    }

    private sealed class FixedDateTimeProvider(DateTimeOffset utcNow) : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public bool WasSaved { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            WasSaved = true;

            return Task.FromResult(1);
        }
    }

    private sealed class InMemoryVehicleRepository : IVehicleRepository
    {
        public List<Vehicle> Items { get; } = [];

        public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            Items.Add(vehicle);

            return Task.CompletedTask;
        }

        public void Remove(Vehicle vehicle)
        {
            Items.Remove(vehicle);
        }

        public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.FirstOrDefault(vehicle => vehicle.Id == id));
        }

        public Task<IReadOnlyList<Vehicle>> ListAvailableOrderedByPriceAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Vehicle>>(
                Items
                    .Where(vehicle => vehicle.Status == VehicleStatus.Available)
                    .OrderBy(vehicle => vehicle.Price.Amount)
                    .ThenBy(vehicle => vehicle.Brand)
                    .ThenBy(vehicle => vehicle.Model)
                    .ToList());
        }

        public Task<IReadOnlyList<Vehicle>> ListSoldOrderedByPriceAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Vehicle>>(
                Items
                    .Where(vehicle => vehicle.Status == VehicleStatus.Sold)
                    .OrderBy(vehicle => vehicle.Price.Amount)
                    .ThenBy(vehicle => vehicle.Brand)
                    .ThenBy(vehicle => vehicle.Model)
                    .ToList());
        }
    }

    private sealed class InMemorySaleRepository : ISaleRepository
    {
        public List<Sale> Items { get; } = [];

        public Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            Items.Add(sale);

            return Task.CompletedTask;
        }
    }
}
