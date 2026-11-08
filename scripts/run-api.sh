#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ ! -f "$ROOT/.env.local" ]]; then echo 'Run ./scripts/dev-setup.sh first.' >&2;exit 1;fi
set -a; source "$ROOT/.env.local"; set +a
cd "$ROOT/server/Celd.Api"
exec dotnet run --urls http://127.0.0.1:5050
