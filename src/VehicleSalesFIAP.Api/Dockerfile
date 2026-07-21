FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["NuGet.config", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["global.json", "./"]
COPY ["VehicleSalesFIAP.slnx", "./"]
COPY ["src/VehicleSalesFIAP.Domain/VehicleSalesFIAP.Domain.csproj", "src/VehicleSalesFIAP.Domain/"]
COPY ["src/VehicleSalesFIAP.Application/VehicleSalesFIAP.Application.csproj", "src/VehicleSalesFIAP.Application/"]
COPY ["src/VehicleSalesFIAP.Infrastructure/VehicleSalesFIAP.Infrastructure.csproj", "src/VehicleSalesFIAP.Infrastructure/"]
COPY ["src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj", "src/VehicleSalesFIAP.Api/"]
COPY ["tests/VehicleSalesFIAP.Tests/VehicleSalesFIAP.Tests.csproj", "tests/VehicleSalesFIAP.Tests/"]

RUN dotnet restore "VehicleSalesFIAP.slnx"

COPY . .
RUN dotnet publish "src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VehicleSalesFIAP.Api.dll"]
