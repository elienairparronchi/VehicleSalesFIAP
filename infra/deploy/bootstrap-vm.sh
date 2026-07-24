#!/usr/bin/env bash
set -euo pipefail

if [[ "${EUID}" -ne 0 ]]; then
  echo "Run this script with sudo."
  exit 1
fi

public_host="${1:-}"
repository="${2:-}"
admin_username="${3:-vehiclesales.admin}"
app_dir="/opt/vehiclesalesfiap"
env_file="${app_dir}/.env.production"
release_file="${app_dir}/.release.env"

if [[ ! "${public_host}" =~ ^[a-z0-9.-]+$ ]]; then
  echo "A valid public hostname is required."
  exit 1
fi

if [[ ! "${repository}" =~ ^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$ ]]; then
  echo "A GitHub repository in owner/name format is required."
  exit 1
fi

if ! command -v docker >/dev/null || ! docker compose version >/dev/null 2>&1; then
  echo "Docker Engine and the Docker Compose plugin are required."
  exit 1
fi

install -d -m 0750 "${app_dir}"

if [[ -e "${env_file}" ]]; then
  echo "${env_file} already exists. It was not changed."
  exit 1
fi

umask 077
sqlserver_password="Sql7!$(openssl rand -hex 24)"
keycloak_db_password="KcDb7!$(openssl rand -hex 24)"
keycloak_admin_password="KcAdmin7!$(openssl rand -hex 24)"
image_base="ghcr.io/${repository,,}"

cat >"${env_file}" <<EOF
PUBLIC_HOST=${public_host}
API_IMAGE=${image_base}
SQLSERVER_DATABASE=VehicleSalesFIAP
SQLSERVER_SA_PASSWORD=${sqlserver_password}
KEYCLOAK_DB_NAME=keycloak
KEYCLOAK_DB_USERNAME=keycloak
KEYCLOAK_DB_PASSWORD=${keycloak_db_password}
KEYCLOAK_BOOTSTRAP_ADMIN_USERNAME=${admin_username}
KEYCLOAK_BOOTSTRAP_ADMIN_PASSWORD=${keycloak_admin_password}
EOF

printf 'IMAGE_TAG=latest\n' >"${release_file}"
chmod 0600 "${env_file}" "${release_file}"

echo "Production configuration created in ${env_file}."
echo "Keycloak administrator: ${admin_username}"
echo "The generated password is stored only on the VM."
echo "Read it once with: sudo grep KEYCLOAK_BOOTSTRAP_ADMIN_PASSWORD ${env_file}"
