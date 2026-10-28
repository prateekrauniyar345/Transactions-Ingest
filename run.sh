#!/usr/bin/env bash
set -euo pipefail

# Resolve paths from the script so it can be invoked from any directory.
project_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd "$project_dir"

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker is not installed. Install Docker Desktop, then run ./run.sh again." >&2
  exit 1
fi
if ! docker compose version >/dev/null 2>&1; then
  echo "Docker Compose v2 is required. Install or update Docker Desktop." >&2
  exit 1
fi
if ! docker info >/dev/null 2>&1; then
  echo "Docker is not running. Start Docker Desktop, then run ./run.sh again." >&2
  exit 1
fi

echo "Building and starting the backend and frontend..."
if ! docker compose -f compose.yaml up --build --detach --wait --wait-timeout 120; then
  echo "Startup failed. Check the logs below and ensure ports are available." >&2
  docker compose -f compose.yaml logs --tail=80 >&2
  exit 1
fi

backend_address="$(docker compose -f compose.yaml port backend 8080)"
frontend_address="$(docker compose -f compose.yaml port frontend 8080)"
echo
echo "Transactions Ingest is ready."
echo "Frontend: http://localhost:${frontend_address##*:}"
echo "Backend:  http://localhost:${backend_address##*:}"
echo "Swagger:  http://localhost:${backend_address##*:}/swagger"
echo
echo "Run an import in Swagger to populate the transaction table, then refresh the frontend."
echo "Logs: docker compose logs -f"
echo "Stop: docker compose down"
echo "SQLite data is preserved in the transaction-data Docker volume."
