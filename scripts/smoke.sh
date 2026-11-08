#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ ! -f "$ROOT/.env.local" ]]; then echo 'Missing .env.local. Run scripts/dev-setup.sh.' >&2; exit 1; fi
set -a; source "$ROOT/.env.local"; set +a
python3 "$ROOT/scripts/smoke.py" "${1:-http://127.0.0.1:5050}"
