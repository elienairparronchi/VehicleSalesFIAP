#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${ConnectionStrings__VehicleSales:-}" ]]; then
  echo "ConnectionStrings__VehicleSales is required."
  exit 1
fi

echo "Restoring local .NET tools..."
dotnet tool restore

echo "Restoring solution packages for the container runtime..."
dotnet restore VehicleSalesFIAP.slnx --force-evaluate -p:RestoreFallbackFolders=

echo "Applying Entity Framework Core migrations..."
dotnet tool run dotnet-ef database update \
  --project src/VehicleSalesFIAP.Infrastructure/VehicleSalesFIAP.Infrastructure.csproj \
  --startup-project src/VehicleSalesFIAP.Api/VehicleSalesFIAP.Api.csproj \
  --context VehicleSalesDbContext \
  --connection "${ConnectionStrings__VehicleSales}"

echo "Database migrations applied successfully."
