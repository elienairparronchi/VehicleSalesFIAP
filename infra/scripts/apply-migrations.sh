#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${ConnectionStrings__VehicleSales:-}" ]]; then
  echo "ConnectionStrings__VehicleSales is required."
  exit 1
fi

echo "Applying Entity Framework Core migrations..."
./efbundle --connection "${ConnectionStrings__VehicleSales}"

echo "Database migrations applied successfully."
