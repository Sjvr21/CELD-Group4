# Executable Verification Plan — Group 4 / Profile D

## Evidence policy

**Status on delivery: NOT RUN.** Creating tests or listing expected outcomes is NOT execution evidence. A tester must record command, UTC date, OS, runtime versions, DB provider/version, test run artifact, observed result, and Git SHA. `NFR-TEST`, `NFR-TRC`, AC-12, AC-14 and AC-15 are not passed without this evidence.

## Automated feature/integration tests

From repository root with SDK 9 and SQLite provider restored:

```bash
dotnet restore tests/Celd.Api.Tests/Celd.Api.Tests.csproj
dotnet test tests/Celd.Api.Tests/Celd.Api.Tests.csproj -c Release \
  --logger 'trx;LogFileName=celd-tests.trx' --results-directory TestResults
```

**AC-04 mandatory relational check:**

```bash
dotnet test tests/Celd.Api.Tests/Celd.Api.Tests.csproj \
  --filter 'FullyQualifiedName~AC04' --logger 'console;verbosity=detailed'
```

It starts a **real, file-backed SQLite database** (not EF InMemory), two separately authenticated HTTP clients, synchronizes requests after CSRF setup, posts to reserve the same item, checks 201 + 409, counts exactly one persisted active record, and validates the physical UNIQUE index with a forbidden direct insert. If SQLite provider choice changes, rewrite and rerun these checks on the instructor-approved actual provider.

**AC-10 mandatory relational failure-injection check:**

```bash
dotnet test tests/Celd.Api.Tests/Celd.Api.Tests.csproj \
  --filter 'FullyQualifiedName~AC10' --logger 'console;verbosity=detailed'
```

The test creates a SQLite `BEFORE INSERT` trigger that raises an error when History Events are inserted. It then attempts a pickup and a new creation through real authenticated HTTP endpoints and verifies persisted status/events after failure: no partial accepted state. This is not a mocked transaction test.

**Negative cases:** unauthenticated access (401), unauthorized role (403), non-owner access/cancel (404 without disclosure), invalid item/status/filter (400/404), repeated and stale transitions (409), CSRF-less POST (400), double reservation (409), and History write failure (non-success with rollback).

**AC-16 performance:** `PerformanceTest.cs` uses 50+ items and 100 completed historical reservations, warms both endpoints, measures ordinary catalog and staff list requests and asserts a 2000 ms bound. The observed times and exact instructor-approved machine conditions MUST be recorded; CI runner times are not interchangeable with the course's local environment.

## Manual browser/E2E procedure (AC-01..11, NFR-TEST-002)

1. Start clean seeded database, sign in as Student 1 and open catalog. Confirm six distinct items, public search, combined category filter, empty state, detail and availability.
2. Reserve an Available item. Confirm persisted status `Reserved`, unavailable catalog item and one creation event with actor/time.
3. Sign out, sign in as Staff; filter queue to `Reserved`, inspect same record and record Pickup. Confirm status `CheckedOut` and second event.
4. Record Return. Confirm status `Returned`, third event, item Available.
5. Sign out, sign in as Student 2; reserve the same released item successfully. Verify previous record remains.
6. Negative path: as Student 2 attempt to open/cancel Student 1's reservation via API URL; expect controlled denial without leaked private details. Try repeated return and direct no-CSRF POST; expect controlled non-success.
7. Using keyboard only, complete sign-in, catalog search, reservation navigation, action and sign-out; confirm visible text status/errors and usable focus indication.
8. Restart application and confirm previous reservations/events remain. Capture test operator, actual results, timestamps, exact SHA, screenshots or recorded logs (without secrets).

## CI and deployment acceptance (not yet performed)

- AC-13: fresh-clone README setup using approved SDK/Node/database, record observed build/test result.
- AC-14: open a test branch that *deliberately* changes an assertion to fail; observe CI red job and retain URL/log. Restore assertion; observe CI green job and retain URL/log. Avoid publishing the intentionally failing revision as final.
- AC-15: deploy to approved Azure or instructor-authorized equivalent. Record deployment URL, source SHA, HTTPS/session configuration, seed through secure secrets, reserve → pickup → return demonstration and database persistence after restart, plus rollback procedure.
- AC-16: run 50/100 benchmark on instructor-approved local environment and capture wall-time readings. Never claim passed merely because test is authored.

## Additional executable API smoke (not concurrency/atomicity evidence)

With the app running and `.env.local` created, execute `./scripts/smoke.sh`. It returns a nonzero exit code if any expected HTTP status or final state check fails. Record output and commit revision. This sequential smoke does not replace the AC-04 synchronized race test or AC-10 forced-history-failure integration test.
