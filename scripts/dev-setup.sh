#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
FILE="$ROOT/.env.local"
if [[ -e "$FILE" ]]; then echo ".env.local already exists; keeping current credentials."; exit 0; fi
python3 - "$FILE" <<'PY'
import os, secrets, sys
p=sys.argv[1]
keys=['Seed__Student1Password','Seed__Student2Password','Seed__StaffPassword']
with open(p,'x') as f:
    for k in keys: f.write(f'{k}={secrets.token_urlsafe(22)}\n')
os.chmod(p,0o600)
print('Created private .env.local (mode 0600).')
print('Test users: student1@celd.local, student2@celd.local, staff@celd.local')
print('Keep passwords in .env.local; never commit or publish this file.')
PY
