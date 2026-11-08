# Requirements Traceability Matrix — CELD Group 4 / Profile D

**Source:** Instructor-approved CELD SRS v1.1.1. **All observed statuses: NOT RUN.** This table tracks planned design/test locations, not proof. `Verified revision` is blank until an identified executed revision is recorded.

| SRS ID | Design / responsibility | Implementation location | Verification | Observed result | Verified revision |
|---|---|---|---|---|---|
| `BR-AUT-001` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **NOT RUN** | — |
| `BR-AUT-002` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **NOT RUN** | — |
| `BR-AUT-003` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **NOT RUN** | — |
| `BR-RES-001` | CreateReservation + DB filtered unique index | `server/Celd.Api/Features/Reservations/CreateReservation.cs; Data/CeldDbContext.cs` | AC03/AC04 real SQLite race/index | **NOT RUN** | — |
| `BR-RES-002` | CreateReservation + DB filtered unique index | `server/Celd.Api/Features/Reservations/CreateReservation.cs; Data/CeldDbContext.cs` | AC03/AC04 real SQLite race/index | **NOT RUN** | — |
| `BR-RES-003` | Shared transition policy + CAS | `server/Celd.Api/Domain/ReservationLifecycle.cs; Features/Reservations/ReservationTransitions.cs` | LifecycleUnitTests; AC06/AC07/AC08/AC09 | **NOT RUN** | — |
| `BR-RES-004` | Atomic creation/transition/history transaction | `server/Celd.Api/Features/Reservations/CreateReservation.cs; ReservationTransitions.cs` | AC10 real SQLite trigger failure | **NOT RUN** | — |
| `BR-RES-005` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `BR-RES-006` | Shared transition policy + CAS | `server/Celd.Api/Domain/ReservationLifecycle.cs; Features/Reservations/ReservationTransitions.cs` | LifecycleUnitTests; AC06/AC07/AC08/AC09 | **NOT RUN** | — |
| `BR-RES-007` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `BR-RES-008` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `FR-AUT-001` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **NOT RUN** | — |
| `FR-AUT-002` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **NOT RUN** | — |
| `FR-AUT-003` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **NOT RUN** | — |
| `FR-CAT-001` | Public Catalog + authoritative availability | `server/Celd.Api/Features/Catalog/CatalogEndpoints.cs; Data/CeldDbContext.cs` | AC01 | **NOT RUN** | — |
| `FR-CAT-002` | Public Catalog + authoritative availability | `server/Celd.Api/Features/Catalog/CatalogEndpoints.cs; Data/CeldDbContext.cs` | AC01 | **NOT RUN** | — |
| `FR-CAT-003` | Public Catalog + authoritative availability | `server/Celd.Api/Features/Catalog/CatalogEndpoints.cs; Data/CeldDbContext.cs` | AC01 | **NOT RUN** | — |
| `FR-RES-001` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `FR-RES-002` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `FR-RES-003` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `FR-RES-004` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `FR-RES-005` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `FR-RES-006` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `FR-RES-007` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `FR-ERR-001` | ProblemDetails HTTP errors | `server/Celd.Api/Common/ApiErrors.cs; Program.cs` | API_Validation; AC09 | **NOT RUN** | — |
| `FR-ERR-002` | Visible React states and labelled keyboard actions | `client/src/App.tsx; client/src/styles.css` | AC11 browser/manual keyboard procedure | **NOT RUN** | — |
| `DR-001` | Public Catalog + authoritative availability | `server/Celd.Api/Features/Catalog/CatalogEndpoints.cs; Data/CeldDbContext.cs` | AC01 | **NOT RUN** | — |
| `DR-002` | Hashed seeded credentials and role/session records | `server/Celd.Api/Data/; Security/; Features/Authentication/` | AC02; inspect DB | **NOT RUN** | — |
| `DR-003` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `DR-004` | Atomic creation/transition/history transaction | `server/Celd.Api/Features/Reservations/CreateReservation.cs; ReservationTransitions.cs` | AC10 real SQLite trigger failure | **NOT RUN** | — |
| `DR-005` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `DR-006` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **NOT RUN** | — |
| `NFR-SEC-001` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **NOT RUN** | — |
| `NFR-SEC-002` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **NOT RUN** | — |
| `NFR-REL-001` | CreateReservation + DB filtered unique index | `server/Celd.Api/Features/Reservations/CreateReservation.cs; Data/CeldDbContext.cs` | AC03/AC04 real SQLite race/index | **NOT RUN** | — |
| `NFR-REL-002` | Atomic creation/transition/history transaction | `server/Celd.Api/Features/Reservations/CreateReservation.cs; ReservationTransitions.cs` | AC10 real SQLite trigger failure | **NOT RUN** | — |
| `NFR-USAB-001` | Visible React states and labelled keyboard actions | `client/src/App.tsx; client/src/styles.css` | AC11 browser/manual keyboard procedure | **NOT RUN** | — |
| `NFR-PERF-001` | Local 50/100 response-time benchmark | `tests/Celd.Api.Tests/PerformanceTest.cs` | AC16 performance | **NOT RUN** | — |
| `NFR-TEST-001` | Feature integration and manual E2E | `tests/Celd.Api.Tests/; docs/TEST_PLAN.md` | AC12; witnessed E2E | **NOT RUN** | — |
| `NFR-TEST-002` | Feature integration and manual E2E | `tests/Celd.Api.Tests/; docs/TEST_PLAN.md` | AC12; witnessed E2E | **NOT RUN** | — |
| `NFR-TRC-001` | RTM mapped to revision/evidence | `docs/RTM.md` | AC12; verify complete evidence | **NOT RUN** | — |
| `NFR-DOC-001` | README; architecture; API; ADR; recovery | `README.md; docs/ADR-001.md; docs/ARCHITECTURE.md; docs/API.md` | AC13 fresh-clone | **NOT RUN** | — |
| `NFR-CI-001` | GitHub Actions build, test, evidence | `.github/workflows/ci.yml` | AC14 red/green CI | **NOT RUN** | — |
| `NFR-DEP-001` | Publish; Dockerfile; secret config/rollback | `Dockerfile; README.md` | AC15 deploy/smoke | **NOT RUN** | — |

## Acceptance criteria (required linked execution evidence)

| Acceptance | Planned procedure / test | Observed result | Revision |
|---|---|---|---|
| `AC-01` | AC01_Public_catalog_search_category_empty_and_not_found | **NOT RUN** | — |
| `AC-02` | AC02_Invalid_credentials_logout_and_ended_session_refused | **NOT RUN** | — |
| `AC-03` | AC03_Create_sets_reserved_and_availability_false_with_history | **NOT RUN** | — |
| `AC-04` | AC04_Concurrent_http_requests_real_sqlite_at_most_one_active + AC04_Physical_unique_index_prevents_bypassing_handler | **NOT RUN** | — |
| `AC-05` | AC05_Ownership_isolated_student_cannot_read_or_cancel_other | **NOT RUN** | — |
| `AC-06` | AC06_Cancel_releases_item_and_preserves_past_history | **NOT RUN** | — |
| `AC-07` | AC07_Only_staff_can_pick_up_and_queue_filters_by_status | **NOT RUN** | — |
| `AC-08` | AC08_Return_releases_and_new_reservation_succeeds | **NOT RUN** | — |
| `AC-09` | AC09_Invalid_repeated_and_stale_transitions_do_not_append_history + AC09_Concurrent_cancel_and_pickup_only_one_transition_commits | **NOT RUN** | — |
| `AC-10` | AC10_Forced_history_insert_failure_rolls_back_status_change_real_sqlite + AC10_Forced_creation_history_failure_leaves_no_partial_reservation | **NOT RUN** | — |
| `AC-11` | Manual keyboard/empty/loading/error walkthrough (docs/TEST_PLAN.md) | **NOT RUN** | — |
| `AC-12` | Execute required tests + witness E2E; capture TRX and Git SHA | **NOT RUN** | — |
| `AC-13` | Fresh clone READMEs and architecture/API/ADR audit | **NOT RUN** | — |
| `AC-14` | GitHub CI deliberately failing check then corrected green check, URL/logs | **NOT RUN** | — |
| `AC-15` | Approved deployment: identified SHA + witnessed auth/reserve/pickup/return/restart | **NOT RUN** | — |
| `AC-16` | AC16_Fifty_items_and_hundred_historical_reservations_meet_two_second_bound | **NOT RUN** | — |

**Evidence fields to fill after execution:** test run ID / logs or TRX path, observed outcome, date, environment, source revision (`git rev-parse HEAD`), tester/witness. A fail or blocker must stay visible; never silently mark it passed.
