# Requirements Traceability Matrix — CELD Group 4 / Profile D

**Fuente:** CELD SRS v1.1.1 incluido en el repositorio. El archivo SRS indica *"Proposed baseline — instructor approval required"*; la aprobación formal no está verificada en esta evidencia.

**Actualización:** 15 de noviembre de 2026. **Ejecución examinada:** [CELD CI, run #2](https://github.com/Sjvr21/CELD-Group4/actions/runs/42527446677), realizada el **8 de noviembre de 2026 (UTC)**, código probado **[`609eb14`](https://github.com/Sjvr21/CELD-Group4/commit/609eb14dd390368bf49d14895d24a7822d6b092b)** (SHA completo: `609eb14dd390368bf49d14895d24a7822d6b092b`). El commit de esta actualización de documentación es posterior y **no** sustituye el SHA del código probado.

**Resultado observado del trabajo CI:** 30 pruebas aprobadas, 0 fallidas, 0 omitidas; build .NET sin advertencias/errores; build React/Vite satisfactorio; `dotnet publish` satisfactorio. Ambiente CI: GitHub Actions `ubuntu-24.04`, .NET SDK `9.0.300`, Node `22.16.0`, EF Core con SQLite real (base temporal en archivo). Comando ejecutado: `dotnet test tests/Celd.Api.Tests/Celd.Api.Tests.csproj --configuration Release --logger 'trx;LogFileName=celd-tests.trx' --results-directory TestResults`.

**Artefactos:** en el [run de GitHub Actions](https://github.com/Sjvr21/CELD-Group4/actions/runs/42527446677), `celd-test-evidence` incluye `TestResults/celd-tests.trx` y `artifacts/revision.txt`; `celd-publish` contiene binarios publicables. El artefacto de publicación no es evidencia de despliegue. La lista de pruebas se enlaza al archivo [FeatureIntegrationTests.cs](https://github.com/Sjvr21/CELD-Group4/blob/main/tests/Celd.Api.Tests/FeatureIntegrationTests.cs), [LifecycleUnitTests.cs](https://github.com/Sjvr21/CELD-Group4/blob/main/tests/Celd.Api.Tests/LifecycleUnitTests.cs) y [PerformanceTest.cs](https://github.com/Sjvr21/CELD-Group4/blob/main/tests/Celd.Api.Tests/PerformanceTest.cs). El resumen CI es agregado; el TRX permite auditar casos individuales.

**Leyenda de resultados:** **VERIFICADO (CI)** = existe prueba automatizada ejecutada para la parte indicada; **PARCIAL (CI)** = evidencia ejecutada para una parte del requisito/criterio, pero falta otra validación; **PENDIENTE** = no se ha identificado evidencia ejecutada suficiente. **Un test existente o un flujo descrito en el README no cuentan por sí solos como ejecución.** Estado de aceptación final ≠ resultado automatizado.

## Matriz de requisitos y evidencia

| SRS ID | Design / responsibility | Implementation location | Verification | Observed result | Verified revision |
|---|---|---|---|---|---|
| `BR-AUT-001` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **PARCIAL (CI)**: AC02, AC05 y AC07 validan sesiones/roles; falta prueba específica de identidad falsificada. | `609eb14` |
| `BR-AUT-002` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **VERIFICADO (CI)**: AC05 rechaza lectura/cancelación ajena (404), sin cambiar el registro. | `609eb14` |
| `BR-AUT-003` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **PARCIAL (CI)**: AC07 rechaza pickup de Student/Visitor; añadir rechazo explícito de return. | `609eb14` |
| `BR-RES-001` | CreateReservation + DB filtered unique index | `server/Celd.Api/Features/Reservations/CreateReservation.cs; Data/CeldDbContext.cs` | AC03/AC04 real SQLite race/index | **VERIFICADO (CI)**: AC03 crea solo si está disponible; segunda solicitud recibe 409. | `609eb14` |
| `BR-RES-002` | CreateReservation + DB filtered unique index | `server/Celd.Api/Features/Reservations/CreateReservation.cs; Data/CeldDbContext.cs` | AC03/AC04 real SQLite race/index | **VERIFICADO (CI)**: AC04 prueba carrera con 2 Students y el índice UNIQUE real de SQLite. | `609eb14` |
| `BR-RES-003` | Shared transition policy + CAS | `server/Celd.Api/Domain/ReservationLifecycle.cs; Features/Reservations/ReservationTransitions.cs` | LifecycleUnitTests; AC06/AC07/AC08/AC09 | **VERIFICADO (CI)**: LifecycleUnitTests y AC06–AC09 prueban las tres transiciones legales y rechazos. | `609eb14` |
| `BR-RES-004` | Atomic creation/transition/history transaction | `server/Celd.Api/Features/Reservations/CreateReservation.cs; ReservationTransitions.cs` | AC10 real SQLite trigger failure | **VERIFICADO (CI)**: AC10 inyecta fallos reales de HistoryEvents y verifica rollback. | `609eb14` |
| `BR-RES-005` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **VERIFICADO (CI)**: AC06 y AC08 liberan Item y conservan historial. | `609eb14` |
| `BR-RES-006` | Shared transition policy + CAS | `server/Celd.Api/Domain/ReservationLifecycle.cs; Features/Reservations/ReservationTransitions.cs` | LifecycleUnitTests; AC06/AC07/AC08/AC09 | **VERIFICADO (CI)**: AC09 rechaza repeticiones/carreras sin eventos de éxito adicionales. | `609eb14` |
| `BR-RES-007` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **PARCIAL (CI)**: AC03/AC06/AC08 verifican disponibilidad desde BD; falta recorrido visual React. | `609eb14` |
| `BR-RES-008` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **PENDIENTE**: revisar UI/API para ausencia de borrado; no existe prueba específica ejecutada. | — |
| `FR-AUT-001` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **VERIFICADO (API/CI)**: AC02 prueba login de Student/Staff y credenciales inválidas (401). | `609eb14` |
| `FR-AUT-002` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **VERIFICADO (API/CI)**: AC02 prueba logout y rechazo de sesión finalizada (401). | `609eb14` |
| `FR-AUT-003` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **PARCIAL (CI)**: roles autenticados comprobados; navegación React por rol sin prueba manual. | `609eb14` |
| `FR-CAT-001` | Public Catalog + authoritative availability | `server/Celd.Api/Features/Catalog/CatalogEndpoints.cs; Data/CeldDbContext.cs` | AC01 | **PARCIAL (CI)**: AC01 prueba catálogo público/API; presentación visual pendiente. | `609eb14` |
| `FR-CAT-002` | Public Catalog + authoritative availability | `server/Celd.Api/Features/Catalog/CatalogEndpoints.cs; Data/CeldDbContext.cs` | AC01 | **PARCIAL (CI)**: AC01 prueba búsqueda insensible a mayúsculas, categoría y vacío; UI pendiente. | `609eb14` |
| `FR-CAT-003` | Public Catalog + authoritative availability | `server/Celd.Api/Features/Catalog/CatalogEndpoints.cs; Data/CeldDbContext.cs` | AC01 | **PARCIAL (CI)**: AC01 prueba detalle y 404; pantalla de error pendiente. | `609eb14` |
| `FR-RES-001` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **VERIFICADO (API/CI)**: AC03 crea Reserved con equipo/estudiante e historial. | `609eb14` |
| `FR-RES-002` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **PARCIAL (CI)**: AC05 prueba /mine y aislamiento; distinción visual de estados pendiente. | `609eb14` |
| `FR-RES-003` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **VERIFICADO (API/CI)**: AC06 cancela, preserva historial y libera Item. | `609eb14` |
| `FR-RES-004` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **VERIFICADO (API/CI)**: AC07 verifica cola Staff y filtro CheckedOut. | `609eb14` |
| `FR-RES-005` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **VERIFICADO (API/CI)**: AC07 verifica pickup Reserved → CheckedOut por Staff. | `609eb14` |
| `FR-RES-006` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **VERIFICADO (API/CI)**: AC08 verifica return CheckedOut → Returned por Staff. | `609eb14` |
| `FR-RES-007` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **PARCIAL (CI)**: AC03/AC06/AC08 comprueban eventos; falta aseverar actor, UTC y orden completo. | `609eb14` |
| `FR-ERR-001` | ProblemDetails HTTP errors | `server/Celd.Api/Common/ApiErrors.cs; Program.cs` | API_Validation; AC09 | **PARCIAL (CI)**: pruebas comprueban 400/401/403/404/409; falta revisar uniformidad del cuerpo de errores. | `609eb14` |
| `FR-ERR-002` | Visible React states and labelled keyboard actions | `client/src/App.tsx; client/src/styles.css` | AC11 browser/manual keyboard procedure | **PENDIENTE**: falta evidencia de estados de carga, vacío, éxito y error en navegador. | — |
| `DR-001` | Public Catalog + authoritative availability | `server/Celd.Api/Features/Catalog/CatalogEndpoints.cs; Data/CeldDbContext.cs` | AC01 | **PARCIAL (CI)**: AC01 usa 6 Items; índice de AssetCode en modelo, sin prueba negativa directa. | `609eb14` |
| `DR-002` | Hashed seeded credentials and role/session records | `server/Celd.Api/Data/; Security/; Features/Authentication/` | AC02; inspect DB | **PARCIAL (CI)**: AC02 autentica usuarios; falta inspección explícita de hashes y secretos. | `609eb14` |
| `DR-003` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **VERIFICADO (CI)**: AC03/AC05/AC08 prueban persistencia, relaciones y estados de Reservations. | `609eb14` |
| `DR-004` | Atomic creation/transition/history transaction | `server/Celd.Api/Features/Reservations/CreateReservation.cs; ReservationTransitions.cs` | AC10 real SQLite trigger failure | **PARCIAL (CI)**: AC10 verifica atomicidad y eventos; auditar actor/fecha de todos los eventos. | `609eb14` |
| `DR-005` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **PARCIAL (CI)**: DR05 reabre DbContext y conserva datos; no comprueba reinicio del proceso. | `609eb14` |
| `DR-006` | Reservation + lending handlers / persistent query DTO | `server/Celd.Api/Features/Reservations/; Features/Lending/; Data/` | AC03/AC05/AC06/AC07/AC08/AC09/DR05 | **VERIFICADO (CI)**: AC03/AC06/AC08 prueban disponibilidad según reservas persistidas. | `609eb14` |
| `NFR-SEC-001` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **VERIFICADO (API/CI)**: AC02/AC05/AC07 prueban sesión, permisos y aislamiento por propietario. | `609eb14` |
| `NFR-SEC-002` | Authentication; SessionValidator; ASP.NET policies; CSRF | `server/Celd.Api/Features/Authentication/AuthEndpoints.cs; Security/SessionValidator.cs; Program.cs` | AC02/AC05/AC07; API_Requires_csrf | **PARCIAL (CI)**: prueba anti-CSRF aprobada; HTTPS/cookies de despliegue y secretos sin auditoría completa. | `609eb14` |
| `NFR-REL-001` | CreateReservation + DB filtered unique index | `server/Celd.Api/Features/Reservations/CreateReservation.cs; Data/CeldDbContext.cs` | AC03/AC04 real SQLite race/index | **VERIFICADO (CI/SQLite real)**: AC04 confirma un solo activo y segundo 409; índice físico probado. | `609eb14` |
| `NFR-REL-002` | Atomic creation/transition/history transaction | `server/Celd.Api/Features/Reservations/CreateReservation.cs; ReservationTransitions.cs` | AC10 real SQLite trigger failure | **PARCIAL (CI)**: AC10 demuestra rollback; queda pendiente reinicio completo de aplicación. | `609eb14` |
| `NFR-USAB-001` | Visible React states and labelled keyboard actions | `client/src/App.tsx; client/src/styles.css` | AC11 browser/manual keyboard procedure | **PENDIENTE**: no hay recorrido manual registrado de teclado, etiquetas y estados. | — |
| `NFR-PERF-001` | Local 50/100 response-time benchmark | `tests/Celd.Api.Tests/PerformanceTest.cs` | AC16 performance | **PARCIAL (CI)**: AC16 pasó con 50 Items/100 históricos; falta medición documentada en entorno local aprobado. | `609eb14` |
| `NFR-TEST-001` | Feature integration and manual E2E | `tests/Celd.Api.Tests/; docs/TEST_PLAN.md` | AC12; witnessed E2E | **VERIFICADO (CI)**: 30 pruebas ejecutadas (0 fallos, 0 omitidas), TRX y SHA retenidos. | `609eb14` |
| `NFR-TEST-002` | Feature integration and manual E2E | `tests/Celd.Api.Tests/; docs/TEST_PLAN.md` | AC12; witnessed E2E | **PENDIENTE**: procedimiento E2E escrito, pero sin ejecución manual presenciada/documentada. | — |
| `NFR-TRC-001` | RTM mapped to revision/evidence | `docs/RTM.md` | AC12; verify complete evidence | **PARCIAL**: RTM vincula requisitos a CI y señala faltantes; pendiente auditoría de cobertura total. | — |
| `NFR-DOC-001` | README; architecture; API; ADR; recovery | `README.md; docs/ADR-001.md; docs/ARCHITECTURE.md; docs/API.md` | AC13 fresh-clone | **PARCIAL**: archivos de documentación existen; falta fresh-clone y sincronizar README/IMPLEMENTATION_STATUS. | — |
| `NFR-CI-001` | GitHub Actions build, test, evidence | `.github/workflows/ci.yml` | AC14 red/green CI | **PARCIAL (CI)**: build/test/publish verdes; falta evidencia red→green y sustituir npm install por npm ci. | `609eb14` |
| `NFR-DEP-001` | Publish; Dockerfile; secret config/rollback | `Dockerfile; README.md` | AC15 deploy/smoke | **PENDIENTE**: publish artifact en CI no equivale a despliegue aprobado/validado. | — |

## Criterios de aceptación — evidencia de ejecución

| Acceptance | Planned procedure / test | Observed result | Revision |
|---|---|---|---|
| `AC-01` | AC01_Public_catalog_search_category_empty_and_not_found | **PARCIAL (CI)**: AC01 API pasó; falta catálogo/error/vacío verificado en navegador. | `609eb14` |
| `AC-02` | AC02_Invalid_credentials_logout_and_ended_session_refused | **PARCIAL (CI)**: AC02 API pasó; falta navegación visual por rol. | `609eb14` |
| `AC-03` | AC03_Create_sets_reserved_and_availability_false_with_history | **PARCIAL (CI)**: AC03 API pasó; falta demo React → servidor → BD. | `609eb14` |
| `AC-04` | AC04_Concurrent_http_requests_real_sqlite_at_most_one_active + AC04_Physical_unique_index_prevents_bypassing_handler | **VERIFICADO (CI)**: AC04 concurrencia 201+409, 1 activo y UNIQUE SQLite. | `609eb14` |
| `AC-05` | AC05_Ownership_isolated_student_cannot_read_or_cancel_other | **VERIFICADO (CI)**: AC05 /mine, lectura y cancelación ajenas rechazadas. | `609eb14` |
| `AC-06` | AC06_Cancel_releases_item_and_preserves_past_history | **PARCIAL (CI)**: AC06 API pasó; falta validación visual de cancelación. | `609eb14` |
| `AC-07` | AC07_Only_staff_can_pick_up_and_queue_filters_by_status | **PARCIAL (CI)**: AC07 pickup y restricciones pasaron; falta rechazo específico de return por Student. | `609eb14` |
| `AC-08` | AC08_Return_releases_and_new_reservation_succeeds | **PARCIAL (CI)**: AC08 return/nueva reserva pasó; falta demostración visual. | `609eb14` |
| `AC-09` | AC09_Invalid_repeated_and_stale_transitions_do_not_append_history + AC09_Concurrent_cancel_and_pickup_only_one_transition_commits | **VERIFICADO (CI)**: AC09 rechaza transiciones repetidas y carreras sin history extra. | `609eb14` |
| `AC-10` | AC10_Forced_history_insert_failure_rolls_back_status_change_real_sqlite + AC10_Forced_creation_history_failure_leaves_no_partial_reservation | **VERIFICADO (CI)**: AC10 falla HistoryEvents con trigger SQLite y comprueba rollback. | `609eb14` |
| `AC-11` | Manual keyboard/empty/loading/error walkthrough (docs/TEST_PLAN.md) | **PENDIENTE**: ejecutar y registrar accesibilidad/estados en navegador. | — |
| `AC-12` | Execute required tests + witness E2E; capture TRX and Git SHA | **PARCIAL (CI)**: tests y TRX ejecutados; falta E2E presenciado y matriz cerrada. | `609eb14` |
| `AC-13` | Fresh clone READMEs and architecture/API/ADR audit | **PENDIENTE**: realizar fresh-clone audit/documentar setup en equipo nuevo. | — |
| `AC-14` | GitHub CI deliberately failing check then corrected green check, URL/logs | **PARCIAL (CI)**: pipeline verde; falta prueba deliberada roja seguida de verde. | `609eb14` |
| `AC-15` | Approved deployment: identified SHA + witnessed auth/reserve/pickup/return/restart | **PENDIENTE**: no hay despliegue validado con auth/reserve/pickup/return/reinicio. | — |
| `AC-16` | AC16_Fifty_items_and_hundred_historical_reservations_meet_two_second_bound | **PARCIAL (CI)**: prueba de 50/100 pasó; faltan tiempos/condiciones del entorno local aprobado. | `609eb14` |

**Pendiente por registrar:** para cada validación manual, fecha, ejecutor/testigo, entorno, pasos y resultado observado, capturas o logs sin secretos y SHA del código. No se deben convertir criterios parciales en aprobados sin evidencia adicional.


## Brechas de evidencia y acciones para el Día 4

| Prioridad | Criterios | Evidencia faltante o corrección |
|---|---|---|
| Alta | AC-11 / AC-12 | Ejecutar y documentar flujo en navegador (login, reservar, pickup, return, cancelación, rutas negativas, teclado); capturar resultados y testigo. |
| Alta | AC-15 / NFR-DEP-001 | Desplegar en destino aprobado, verificar HTTPS/cookies, SHA, workflow completo, reinicio y recuperación. |
| Alta | AC-14 / NFR-CI-001 | Demostrar CI rojo por check intencional y verde tras corrección; conservar ambos enlaces. Cambiar `npm install` a `npm ci` con lockfile. |
| Alta | NFR-SEC-002 | Investigar el backup SQLite rastreado en el repositorio público (`server/Celd.Api/celd.db.backup-20261108-103502`), retirar y evaluar limpieza de historial/rotación si contiene datos sensibles. |
| Media | AC-16 / NFR-PERF-001 | Repetir 50/100 en entorno local aprobado y guardar tiempos medidos, máquina y versiones. |
| Media | AC-13 / NFR-DOC-001 | Actualizar README y `docs/IMPLEMENTATION_STATUS.md` (todavía reportan "NOT RUN"), comprobar instalación desde clon nuevo. |
| Media | AC-07 / BR-AUT-003 | Añadir prueba explícita de denegación de `return` a Student y Visitor. |
| Media | DR-005 / NFR-REL-002 | Reiniciar proceso de aplicación y comprobar Reservations/HistoryEvents preservados. |
| Media | FR-RES-007 / DR-004 | Verificar actor, fecha UTC y orden cronológico de eventos en la respuesta. |
| Media | Aprobación de entorno | Confirmar SRS/proveedor SQLite, versiones y destino con la hoja de entorno del profesor. |

**Nota de control:** esta actualización del RTM registra evidencia preexistente; no ejecuta nuevas pruebas, no verifica el entorno local aprobado y no cambia el código del producto. La ejecución automática de CI al guardar el RTM, si ocurriera, debe registrarse aparte con su propio SHA y enlace.
