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
compose_up_help="$(docker compose up --help)"
case "$compose_up_help" in
  *--watch*) ;;
  *)
    echo "Compose Watch is required. Update Docker Compose to v2.22 or newer." >&2
    exit 1
    ;;
esac
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
echo "Watching source files: edits automatically rebuild and restart the affected service."
echo "Wait for the rebuild to finish before importing again or refreshing the browser."
echo "This terminal stays attached to logs. Press Ctrl+C to stop both services."
echo "SQLite data is preserved in the transaction-data Docker volume."

# Attach to the already-started services and enable automatic rebuilds.
# exec forwards Ctrl+C directly to Compose, which stops the services on exit.
exec docker compose -f compose.yaml up --watch
