#!/usr/bin/env bash
set -euo pipefail

revision="${1:-}"
repository="${2:-}"
app_dir="/opt/vehiclesalesfiap"
env_file="${app_dir}/.env.production"
release_file="${app_dir}/.release.env"
compose_file="${app_dir}/compose.prod.yml"
caddy_file="${app_dir}/infra/deploy/Caddyfile"
realm_file="${app_dir}/infra/keycloak/realm-production.json"

if [[ ! "${revision}" =~ ^[0-9a-f]{40}$ ]]; then
  echo "A full Git commit SHA is required."
  exit 1
fi

if [[ ! "${repository}" =~ ^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$ ]]; then
  echo "A GitHub repository in owner/name format is required."
  exit 1
fi

if [[ ! -f "${env_file}" ]]; then
  echo "${env_file} does not exist. Run bootstrap-vm.sh first."
  exit 1
fi

exec 9>/var/lock/vehiclesalesfiap-deploy.lock
if ! flock -n 9; then
  echo "Another deployment is already running."
  exit 1
fi

public_host="$(sed -n 's/^PUBLIC_HOST=//p' "${env_file}")"
if [[ ! "${public_host}" =~ ^[a-z0-9.-]+$ ]]; then
  echo "PUBLIC_HOST is invalid or missing in ${env_file}."
  exit 1
fi

install -d -m 0750 "${app_dir}" "${app_dir}/infra/deploy" "${app_dir}/infra/keycloak"
temporary_dir="$(mktemp -d)"
backup_dir="$(mktemp -d)"
trap 'rm -rf "${temporary_dir}" "${backup_dir}"' EXIT

base_url="https://raw.githubusercontent.com/${repository}/${revision}"
curl -fsSL --retry 3 "${base_url}/compose.prod.yml" -o "${temporary_dir}/compose.prod.yml"
curl -fsSL --retry 3 "${base_url}/infra/deploy/Caddyfile" -o "${temporary_dir}/Caddyfile"
curl -fsSL --retry 3 "${base_url}/infra/keycloak/realm-production.json" -o "${temporary_dir}/realm-production.json"

previous_tag="$(sed -n 's/^IMAGE_TAG=//p' "${release_file}" 2>/dev/null || true)"
had_previous_deployment=false

if [[ -f "${compose_file}" ]]; then
  had_previous_deployment=true
  cp "${compose_file}" "${backup_dir}/compose.prod.yml"
  cp "${caddy_file}" "${backup_dir}/Caddyfile"
  cp "${realm_file}" "${backup_dir}/realm-production.json"
fi

install -m 0644 "${temporary_dir}/compose.prod.yml" "${compose_file}"
install -m 0644 "${temporary_dir}/Caddyfile" "${caddy_file}"
install -m 0644 "${temporary_dir}/realm-production.json" "${realm_file}"
printf 'IMAGE_TAG=%s\n' "${revision}" >"${release_file}"
chmod 0600 "${release_file}"

compose=(docker compose --env-file "${env_file}" --env-file "${release_file}" -f "${compose_file}")

rollback() {
  if [[ "${had_previous_deployment}" == "true" && -n "${previous_tag}" ]]; then
    echo "Deployment failed. Restoring revision ${previous_tag}."
    install -m 0644 "${backup_dir}/compose.prod.yml" "${compose_file}"
    install -m 0644 "${backup_dir}/Caddyfile" "${caddy_file}"
    install -m 0644 "${backup_dir}/realm-production.json" "${realm_file}"
    printf 'IMAGE_TAG=%s\n' "${previous_tag}" >"${release_file}"
    chmod 0600 "${release_file}"
    "${compose[@]}" pull api migrations
    "${compose[@]}" up -d --remove-orphans
  fi
}

if ! "${compose[@]}" pull; then
  rollback
  exit 1
fi

if ! "${compose[@]}" up -d --remove-orphans; then
  "${compose[@]}" logs --no-color --tail=200
  rollback
  exit 1
fi

healthy=false
for _ in $(seq 1 36); do
  if curl --fail --silent --show-error \
    --resolve "${public_host}:443:127.0.0.1" \
    "https://${public_host}/health" >/dev/null; then
    healthy=true
    break
  fi

  sleep 10
done

if [[ "${healthy}" != "true" ]]; then
  echo "The public health check did not become ready."
  "${compose[@]}" ps --all
  "${compose[@]}" logs --no-color --tail=200
  rollback
  exit 1
fi

"${compose[@]}" ps --all
docker image prune -f
echo "Revision ${revision} deployed successfully at https://${public_host}."
echo "DEPLOYMENT_SUCCEEDED:${revision}"
