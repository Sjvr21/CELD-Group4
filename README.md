# CELD — Group 4 / Profile D (feature modules)

**Software:** Campus Equipment Lending Desk (COMP 3402)  
**SRS:** Instructor-approved v1.1.1 (included under `docs/SRS-v1.1.1.md`)  
**Architecture:** Feature modules and feature handlers, *not* a central ReservationService (`docs/ADR-001.md`)  
**Build/test evidence:** **NOT RUN** at deliverable generation — .NET SDK and Node package dependencies were unavailable. No claim of deployment or passing acceptance criteria is made.

> **Environment approval blocker:** The SRS requires an instructor environment sheet fixing SDK, Node version, database provider and deployment. That sheet was **not supplied**, and the separate revised architecture/ADR was not attached. This runnable candidate uses **.NET 9**, **Node 22.16.0**, **React 19**, and **real SQLite/EF Core 9** as documented provisional choices. Do **not** claim these specific versions/provider are course-approved without the sheet. A provider switch requires updating filtered-index schema and rerunning AC-04/AC-10 on the final provider.

## Features

- Public catalog, combined case-insensitive name + category filtering, details/availability, missing/empty/loading/error feedback.
- Provisioned Student + Staff sign-in/out with ASP.NET cookie auth, hashed passwords and revocable server-side sessions.
- Session-derived role and identity; Students see their own past/current reservations, can cancel only their own `Reserved` records. Staff sees queue/status filter, performs pickup and return.
- Legal transitions: `Reserved→Cancelled` (owner), `Reserved→CheckedOut` (staff), `CheckedOut→Returned` (staff). Prior records never silently reactivate or disappear.
- SQLite UNIQUE partial index prevents **two active** (`Reserved` or `CheckedOut`) reservations for one item; stale mutations use status + version conditional update.
- Creation and every successful transition recorded in History with actor and UTC time; one EF transaction enforces atomicity.
- Global CSRF checks for all POST, explicit public/protected routes, no JWT, no account-registration UI, no delete-history UI.
- Integration tests using **real file-backed SQLite**, including concurrent HTTP requests and trigger-forced History failure, plus a 50-item/100-history performance test and repeatable manual E2E procedure.

## Repository map

```text
server/Celd.Api/
  Program.cs                         pipeline, cookies, CSRF, policies, routes
  Domain/Entities.cs                 core entities / statuses
  Security/                          validated session + identity
  Data/                              EF model / filtered index / seed
  Features/Authentication/           sign-in/out/csrf/me
  Features/Catalog/                  public list/search/details
  Features/Reservations/             reserve/own queries/cancel/lifecycle/history
  Features/Lending/                  staff queue/pickup/return
  Common/ApiErrors.cs                uniform failures
client/                               React + TypeScript + Vite
  src/App.tsx                         public/student/staff UI
  src/api.ts                          typed cookie+CSRF API client
tests/Celd.Api.Tests/                feature/integration + AC04/AC10 + performance
docs/                                 SRS, ADR-001, architecture, API, RTM, testing
scripts/                              secure local demo credential bootstrap
.github/workflows/ci.yml              PR/push build and test plan
Dockerfile                            provisional deployment packaging
```

## Requirements / setup

- **Approved** .NET SDK compatible with `net9.0` (candidate `9.0.300` with latest patch); check `dotnet --version`.
- **Approved** Node and npm (candidate Node `22.16.0`); check `node --version`, `npm --version`.
- Restore NuGet dependencies online for first build; install npm packages online for first client build.
- SQLite is file-backed, no separate server required. Never run a production SQLite database on ephemeral hosting without a persistent volume and validated backup/restore strategy.

### Demo login accounts — shared local test credentials

For the course's **local test environment**, the three seeded accounts use these email addresses. To give every tester **the same demo passwords**, the team must distribute the *separate private* `CELD_Test_Credentials_PRIVATE.txt` handout through an approved private channel (not through GitHub or published README files):

| User | Email | Password source |
|---|---|---|
| Student 1 | `student1@celd.local` | Private CELD test-credentials handout |
| Student 2 | `student2@celd.local` | Private CELD test-credentials handout |
| Lending Staff | `staff@celd.local` | Private CELD test-credentials handout |

**Important:** The SRS specifically prohibits embedding the actual demo passwords in committed source code or published documentation. The password handout is intentionally **not included** in this repository/ZIP. The same private credentials must be supplied to the seed process **before the database is created**. If each tester invents their own password, the test environments will not share the same credentials.

On Windows, in the **backend PowerShell terminal**, after setting `Development`, enter the three passwords from the handout when prompted:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_ENVIRONMENT = "Development"
$env:Seed__Student1Password = Read-Host "Student 1 password (private handout)"
$env:Seed__Student2Password = Read-Host "Student 2 password (private handout)"
$env:Seed__StaffPassword = Read-Host "Staff password (private handout)"
dotnet run --urls http://127.0.0.1:5050
```

Run those commands from `server/Celd.Api`; these environment variables are **per terminal session**. Enter the handout passwords *exactly* (case-sensitive). **The seed only creates missing accounts; it never resets passwords already stored in the database.** If login says `Invalid credentials` on an existing database, do not assume changing `$env:Seed__...` has reset the account. Do not delete or replace a database containing reservations/history just to fix the password; preserve the data and use an approved account-reset procedure. For a *brand-new, disposable demo database only*, the team may create a fresh database after safely preserving any existing local data.

### Windows 10/11 — VS Code + PowerShell quick start (local development)

These commands are written for **Windows PowerShell**, not Bash. Run them from the **repository root** (`CELD_Group4_ProfileD`) unless otherwise stated. Start the API and React in **separate VS Code terminals**, and keep both running.

1. **Check installed tools.** The checked-in `global.json` requests a **.NET 9.0.300-compatible SDK** (roll-forward `latestPatch`); having only .NET 10 installed is not enough for that SDK selection. .NET 9 and 10 can coexist. The client uses Node 22.16.0 provisionally, subject to instructor approval.

   ```powershell
   dotnet --list-sdks
   node --version
   npm.cmd --version
   git --version
   ```

   If `npm` is blocked by PowerShell's script execution policy, use **`npm.cmd`** rather than changing system-wide settings.

2. **Terminal A — set the local Development environment and seed passwords, then start the API.** The `Development` environment is **required for HTTP-only local testing** with this candidate's antiforgery-cookie settings. Without it, the API may start in `Production` and `/api/auth/csrf` will fail with `AntiforgeryOptions.Cookie.SecurePolicy = Always` on HTTP. Do **not** disable antiforgery or change the secure Production policy to fix this.

   ```powershell
   # In the repository root:
   cd .\server\Celd.Api
   $env:ASPNETCORE_ENVIRONMENT = "Development"
   $env:DOTNET_ENVIRONMENT = "Development"

   # Set passwords (minimum 12 characters) from the privately shared
   # CELD test-credentials handout BEFORE the initial database seed.
   # Never commit password values or write them into appsettings.json.
   $env:Seed__Student1Password = Read-Host "Student 1 password (private handout)"
   $env:Seed__Student2Password = Read-Host "Student 2 password (private handout)"
   $env:Seed__StaffPassword = Read-Host "Staff password (private handout)"

   dotnet run --urls http://127.0.0.1:5050
   ```

   Successful startup should say `Now listening on: http://127.0.0.1:5050` and `Hosting environment: Development`. Check `http://127.0.0.1:5050/api/health` in a browser. Keep Terminal A running. **Environment variables set with `$env:` apply only to this PowerShell process**; set them again when using a new terminal. The API's demo account emails are `student1@celd.local`, `student2@celd.local`, and `staff@celd.local`. Passwords are whichever private values you provided **when the accounts were first seeded**; changing these variables later does *not* reset existing users.

3. **Terminal B — start React.** Open a new terminal at the repository root (not inside the API directory).

   ```powershell
   cd .\client
   npm.cmd install
   npm.cmd run dev
   ```

   Browse `http://127.0.0.1:5173/`. Vite forwards `/api` to `127.0.0.1:5050`. If the Vite console reports `ECONNREFUSED 127.0.0.1:5050`, React is running but the API isn't reachable: ensure Terminal A is still listening on port 5050. `Cache-Control`/`Pragma` antiforgery warnings alone are normal and are not login failures.

4. **Terminal C — run tests from the repository root.** See the Windows test commands below; don't put Bash's `\` line-continuation character at the end of PowerShell lines. All tests and acceptance checks remain **NOT RUN** until actually executed and recorded with their results and commit revision.

> **Local-only:** `Development` and HTTP are intended for your machine. A deployed release must use **Production** with HTTPS, secure cookies, proper secrets handling, and approved hosting. Do not copy developer passwords into a repository, README, or screenshots.

### 1. Provision a local demo securely

```bash
./scripts/dev-setup.sh
# creates .env.local with independent high-entropy demo passwords (chmod 0600)
# file is gitignored; do NOT commit, screenshot or paste credentials in public notes
```

The script provisions credentials for `student1@celd.local`, `student2@celd.local`, and `staff@celd.local`. View the generated secret values **locally** in `.env.local`; no production/demo password is embedded in the source. The API seeds at least six distinct equipment items and the three accounts at first startup. The seed does not reset an existing account password. Delete your local `celd.db` only when intentionally resetting your private demo database.

### 2. Start API

```bash
# Bash / Git Bash / WSL: explicit Development mode for the local HTTP endpoint
ASPNETCORE_ENVIRONMENT=Development DOTNET_ENVIRONMENT=Development ./scripts/run-api.sh
# listens at http://127.0.0.1:5050 (local development only)
```

**Windows PowerShell:** follow the **Windows quick start** above, including *both* `ASPNETCORE_ENVIRONMENT=Development` and `DOTNET_ENVIRONMENT=Development` in the API terminal. Do not start the HTTP API in `Production` mode: its antiforgery cookie requires HTTPS. Supply `Seed__Student1Password`, `Seed__Student2Password`, and `Seed__StaffPassword` before the **first** seed. The Bash-based `scripts/dev-setup.sh` can be run via Git Bash/WSL. Do not store secret values in tracked settings files. Without seed passwords, the catalog still seeds but the provisioned users are not created. Existing account passwords are **not** overwritten by reseeding.

### 3. Start React

```bash
cd client
npm install --no-audit --no-fund
npm run dev
# browse http://127.0.0.1:5173
```

Vite proxies `/api` to localhost:5050, keeping cookie+CSRF same-origin in the browser.

### 4. Build / production package

```bash
cd client && npm run build && cd ..
cp -a client/dist/. server/Celd.Api/wwwroot/
dotnet publish server/Celd.Api/Celd.Api.csproj -c Release -o publish
# publish contains API plus compiled SPA; configure HTTPS termination and secrets
```

Optional container packaging (course approval required): `docker build -t celd:group4 .`. Mount a persistent volume for SQLite and supply `ConnectionStrings__Celd` and the three `Seed__...` environment variables using your target's secret manager. In production, the browser must use HTTPS; cookies are configured `Secure` and HttpOnly and APIs require anti-CSRF tokens. No public HTTPS endpoint was deployed by this deliverable.

## Automated verification — executable commands; results **NOT RUN** here

**Windows PowerShell (recommended for this team):** open a *new terminal at the repository root* (`CELD_Group4_ProfileD`, not `client` or `server/Celd.Api`). Execute one command **per line**:

```powershell
dotnet restore .\tests\Celd.Api.Tests\Celd.Api.Tests.csproj
dotnet test .\tests\Celd.Api.Tests\Celd.Api.Tests.csproj -c Release --logger "trx;LogFileName=celd-tests.trx" --results-directory TestResults
```

This produces a TRX report at `TestResults/celd-tests.trx` when the test runner completes and writes the report. **Do not use `\` as a line-continuation operator in PowerShell** (it is Bash syntax and causes errors such as `Missing expression after unary operator '--'`). Prefer the entire command on a single line. If you must split commands in PowerShell, use a trailing backtick `` ` `` carefully instead.

**Run the two critical SRS checks separately** (also from the repository root):

```powershell
# AC-04 — simultaneous reservation requests + SQLite unique index (BR-RES-002)
dotnet test .\tests\Celd.Api.Tests\Celd.Api.Tests.csproj --filter "FullyQualifiedName~AC04"

# AC-10 — forced History insert failures and atomic rollback (BR-RES-004)
dotnet test .\tests\Celd.Api.Tests\Celd.Api.Tests.csproj --filter "FullyQualifiedName~AC10"
```

**Bash / macOS / Linux** alternatives:

```bash
dotnet restore tests/Celd.Api.Tests/Celd.Api.Tests.csproj
dotnet test tests/Celd.Api.Tests/Celd.Api.Tests.csproj -c Release --logger 'trx;LogFileName=celd-tests.trx' --results-directory TestResults
dotnet test tests/Celd.Api.Tests/Celd.Api.Tests.csproj --filter 'FullyQualifiedName~AC04'
dotnet test tests/Celd.Api.Tests/Celd.Api.Tests.csproj --filter 'FullyQualifiedName~AC10'
```

`dotnet restore` succeeding does **not** mean tests have passed. Review the output for passed/failed/skipped totals, preserve the TRX file and console output, and record the tested commit SHA, environment, and real provider in the RTM. A failing test is evidence of a problem, not a passing requirement. See `docs/TEST_PLAN.md` for browser E2E, negative paths, performance, CI negative-check and release-evidence procedures. See `docs/RTM.md` for mandatory SRS requirements and `AC-01..16` traceability. **All result cells remain NOT RUN until your team performs the checks and records actual results.**

## GitHub Actions CI

`.github/workflows/ci.yml` builds the API, runs tests against real SQLite, builds React, publishes the combined app, and uploads TRX/revision artifacts. It is **not executed** in this deliverable. The repo does not include an npm lockfile because npm registry access was unavailable during generation; on your approved environment run `npm install`, commit `client/package-lock.json`, then change CI and Docker installs from `npm install` to **`npm ci`**. Pin GitHub Action commits and container digests under course policy. Before that change the current CI npm installation is not fully reproducible, so `NFR-CI-001` is **pending**, not passed. Tests are wired to fail the CI job on failed assertions.

## Recovery / operations

- For SQLite deployment, stop writes before backing up the database, use SQLite backup API or an online-safe backup procedure; do not copy the open DB file casually while WAL writes are active. Keep a securely stored verified backup of the database including records/history.
- On failed release, stop app, restore previous **identified source revision / deployment artifact** and matching validated DB backup, configure HTTPS and secrets, and rerun sign-in + Reserve → Pickup → Return smoke check. Never apply unreviewed destructive schema changes.
- `EnsureCreated()` provisions the initial baseline **only**; upgrading an existing schema requires reviewed EF migrations. Do not treat `EnsureCreated` as a production schema migration strategy.
- Troubleshooting: 401 means login absent/expired; 403 wrong role; 400 includes validation/CSRF; 404 missing/inaccessible record; 409 unavailable/stale state. On dropped response, refresh authoritative record rather than assuming success.
- If CI/deployment quota or Azure access is blocked, obtain an instructor-approved alternate target and document formal acceptance; do not claim Azure was deployed.

## Known trade-offs / smells under review

- SQLite-specific filtered index and lock semantics: **provider lock-in** until approved provider established. Database constraint must be reimplemented on other RDBMS.
- Explicit `EnsureCreated` rather than schema migrations: simple first setup but **schema evolution debt**.
- Duplicated status/error guard logic can grow; consider a small shared policy service while retaining feature boundaries.
- List endpoints include history per reservation; **possible N+1/overfetch/payload growth** with larger datasets. The SRS limits size; if benchmark fails, use targeted projection and detail-only history.
- Current UI navigation is a small single-component SPA; if it grows, split UI into feature components to match Profile D more closely.
- CI's absent npm lockfile means dependency reproducibility not yet complete.
- Test cases and design are code proposals, **not observed evidence**. Release is incomplete until SRS-required tests, manual keyboard checks, CI red/green demonstration, performance and deployment evidence are executed.

### Extra API smoke check (sequential, **not** AC-04/AC-10 evidence)

While the API is running, run `./scripts/smoke.sh`. The Python standard-library smoke checks real HTTP sign-in, ownership, unauthorized pickup, reserve/pickup/return, duplicate prevention, stale/repeated calls, retained history, re-reservation and sign-out. It uses private generated demo credentials. **This is a sequential smoke, not a substitute for simultaneous real-provider AC-04 or trigger-based AC-10 integration tests.** Do not rerun blindly on exhausted demo inventory; use a fresh isolated DB.
