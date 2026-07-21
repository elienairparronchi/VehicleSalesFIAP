FROM mcr.microsoft.com/dotnet/sdk:10.0.203 AS build
WORKDIR /src

COPY ["NuGet.config", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["global.json", "./"]
COPY ["dotnet-tools.json", "./"]
COPY ["src/VehicleSalesFIAP.Domain/VehicleSalesFIAP.Domain.csproj", "src/VehicleSalesFIAP.Domain/"]
COPY ["src/VehicleSalesFIAP.Application/VehicleSalesFIAP.Application.csproj", "src/VehicleSalesFIAP.Application/"]
COPY ["src/VehicleSalesFIAP.Infrastructure/VehicleSalesFIAP.Infrastructure.csproj", "src/VehicleSalesFIAP.Infrastructure/"]
COPY ["src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj", "src/VehicleSalesFIAP.Api/"]

RUN dotnet tool restore && \
    dotnet restore "src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj"

COPY ["src", "src"]

RUN dotnet build "src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj" \
      --configuration Release \
      --no-restore && \
    dotnet tool run dotnet-ef migrations bundle \
      --project "src/VehicleSalesFIAP.Infrastructure/VehicleSalesFIAP.Infrastructure.csproj" \
      --startup-project "src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj" \
      --context VehicleSalesDbContext \
      --configuration Release \
      --no-build \
      --output /migrations/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS final
WORKDIR /migrations

COPY --from=build /migrations/efbundle ./efbundle
COPY ["infra/scripts/apply-migrations.sh", "./apply-migrations.sh"]

USER $APP_UID

ENTRYPOINT ["bash", "./apply-migrations.sh"]
