# Implementation / verification ledger

**Profile:** Group 4, D. **Baseline:** SRS v1.1.1. **Evidence state:** NOT RUN unless otherwise specified.

| Area | Authored / supplied | Executed in authoring environment? | Evidence |
|---|---|---|---|
| C# API / feature handlers / EF model | Source code supplied | **NO** — no .NET SDK | None; build required |
| React/TypeScript client | Source code supplied | **SYNTAX-ONLY** TypeScript transpile check executed; package type-check/build **NOT RUN** | Local syntax diagnostic 0; not evidence of runtime behavior |
| Unit tests (legal transitions) | Authored | **NOT RUN** | No TRX |
| Feature/HTTP integration tests | Authored | **NOT RUN** | No TRX |
| AC-04 real SQLite concurrent race/index | Authored to use actual SQLite EF provider | **NOT RUN** | No test result |
| AC-10 history-trigger failure + rollback | Authored to use actual SQLite EF provider | **NOT RUN** | No test result |
| AC-16 50/100 response-time check | Authored | **NOT RUN** | No timing evidence |
| Manual browser E2E & keyboard checks | Procedure supplied | **NOT RUN** | No witnessed evidence |
| GitHub CI | Workflow supplied | **NOT RUN** | No run URL; npm lock not yet generated |
| Deployment to approved target | Docker/publish approach supplied | **NOT DEPLOYED** | No URL/SHA/recovery drill |
| Approval of SDK/Node/DB provider | Instructor environment sheet missing | **UNCONFIRMED** | Need instructor sheet |

**Required next verification:** Install approved tooling and dependencies, confirm/fix any compilation/test errors, commit source, run `dotnet test` against real provider and `npm run build`, perform manual acceptance, establish reproducible npm lockfile, observe CI red/green, deploy and retain exact SHA and observed results. Do not mark pending requirements green without executed evidence.
