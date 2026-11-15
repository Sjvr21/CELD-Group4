# Estado de implementación y verificación — CELD Grupo 4

**Grupo / perfil:** Grupo 4, perfil D — módulos y handlers por funcionalidad.  
**Referencia:** CELD SRS v1.1.1 (`docs/SRS-v1.1.1.md`), que aún figura como *"Proposed baseline — instructor approval required"*.  
**Actualizado:** 15 de noviembre de 2026.  
**Estado general:** Implementación del backend y frontend compilada; pruebas automatizadas aprobadas en CI. **Aceptación integral pendiente** de validación manual, despliegue y ciertos criterios adicionales.

## 1. Evidencia ejecutada y reproducible

| Elemento | Implementación disponible | Estado observado | Evidencia |
|---|---|---|---|
| Backend C# / ASP.NET Core / EF Core | `server/Celd.Api/`, módulos de Authentication, Catalog, Reservations y Lending | **BUILD APROBADO EN CI**: 0 advertencias, 0 errores | [CI 15-nov-2026](https://github.com/Sjvr21/CELD-Group4/actions/runs/43771404293) |
| Frontend React / TypeScript / Vite | `client/src/` | **BUILD APROBADO EN CI**: `npm run build` finalizó correctamente; no prueba por sí solo la interfaz en navegador | [CI 15-nov-2026](https://github.com/Sjvr21/CELD-Group4/actions/runs/43771404293) |
| Pruebas unitarias del ciclo de vida | `tests/Celd.Api.Tests/LifecycleUnitTests.cs` | **EJECUTADAS EN CI** dentro de la suite: casos legales e ilegales incluidos | [Pruebas](https://github.com/Sjvr21/CELD-Group4/blob/main/tests/Celd.Api.Tests/LifecycleUnitTests.cs); [CI](https://github.com/Sjvr21/CELD-Group4/actions/runs/43771404293) |
| Pruebas de integración HTTP y SQLite | `tests/Celd.Api.Tests/FeatureIntegrationTests.cs`, `TestApp.cs` | **EJECUTADAS EN CI** con SQLite real en archivo temporal; casos positivos y negativos incluidos | [Pruebas](https://github.com/Sjvr21/CELD-Group4/blob/main/tests/Celd.Api.Tests/FeatureIntegrationTests.cs); TRX del CI |
| AC-04: reservas concurrentes e índice físico | Dos clientes Student autenticados, acceso simultáneo al mismo Item; índice único filtrado SQLite | **VERIFICADO EN CI**: una respuesta 201, otra 409; un activo persistido; inserción directa duplicada rechazada | `AC04_Concurrent_http_requests_real_sqlite_at_most_one_active`; `AC04_Physical_unique_index_prevents_bypassing_handler` |
| AC-05: privacidad de reservas | Consulta/cancelación ajena y aislamiento de `/mine` | **VERIFICADO EN CI**: acceso ajeno rechazado con 404 y sin mutación | `AC05_Ownership_isolated_student_cannot_read_or_cancel_other` |
| AC-07/08: operaciones Staff | Cola Staff, pickup y return | **VERIFICADO EN CI PARA RUTAS PROBADAS**: Student/Visitor denegados en pickup; retorno válido; falta test negativo específico de return sin Staff | `AC07_Only_staff_can_pick_up_and_queue_filters_by_status`; `AC08_Return_releases_and_new_reservation_succeeds` |
| AC-09: transiciones no válidas/concurrentes | `ReservationLifecycle` y actualizaciones condicionales de status/version | **VERIFICADO EN CI**: transición inválida o repetida 409 y una sola transición aceptada durante carrera | `AC09_Invalid_repeated_and_stale_transitions_do_not_append_history`; `AC09_Concurrent_cancel_and_pickup_only_one_transition_commits` |
| AC-10: fallos forzados de HistoryEvents | Transacciones de creación y de cambio de estado; trigger SQLite de error | **VERIFICADO EN CI**: rollback impide una Reservation o transición sin HistoryEvent | `AC10_Forced_history_insert_failure_rolls_back_status_change_real_sqlite`; `AC10_Forced_creation_history_failure_leaves_no_partial_reservation` |
| DR-005: persistencia de registros | Entidades y eventos consultados desde otro DbContext | **PARCIAL**: la prueba reabre el contexto EF; **no** equivale a reiniciar el proceso de aplicación | `DR05_Records_persist_across_dbcontext_reopen` |
| AC-16: 50 Items / 100 reservas históricas | `tests/Celd.Api.Tests/PerformanceTest.cs` | **PARCIAL**: prueba automatizada pasó en CI; faltan mediciones y condiciones documentadas en equipo local aprobado por docente | `AC16_Fifty_items_and_hundred_historical_reservations_meet_two_second_bound` |
| Prueba anti-CSRF | `Program.cs`, `api.ts`, `FeatureIntegrationTests.cs` | **VERIFICADA EN CI para endpoints probados**: POST sin token recibe 400 | `API_Requires_csrf_on_all_mutating_requests_even_login` |
| GitHub Actions CI | `.github/workflows/ci.yml` | **RUN VERDE OBSERVADO**: 30 aprobadas, 0 fallidas, 0 omitidas; compilación y publicación completas | [Última ejecución examinada](https://github.com/Sjvr21/CELD-Group4/actions/runs/43771404293) |
| Artefactos de prueba y publicación | `celd-test-evidence`, `celd-publish` | **GENERADOS EN CI**: TRX, revisión/entorno y binarios publicables | [Artefactos de la ejecución](https://github.com/Sjvr21/CELD-Group4/actions/runs/43771404293) |

### Resumen verificable de GitHub Actions

- **Ejecución más reciente examinada:** [run 43771404293](https://github.com/Sjvr21/CELD-Group4/actions/runs/43771404293), 15-nov-2026 UTC, conclusión **success**.
- **SHA de esa ejecución:** `28ecf10345283b32fcd3d721b5eea61a3b8b2723` (actualización del RTM, sin cambios del código de aplicación).
- **SHA de implementación ya probado previamente:** `609eb14dd390368bf49d14895d24a7822d6b092b`, también con [CI verde del 8-nov-2026](https://github.com/Sjvr21/CELD-Group4/actions/runs/42527446677).
- **Entorno CI registrado:** GitHub Actions `ubuntu-24.04`, .NET SDK `9.0.300`, Node `22.16.0`, EF Core + SQLite file-backed en pruebas.
- **Comando:** `dotnet test tests/Celd.Api.Tests/Celd.Api.Tests.csproj --configuration Release --logger 'trx;LogFileName=celd-tests.trx' --results-directory TestResults`.
- **Resultado de test (15-nov):** `Passed: 30; Failed: 0; Skipped: 0; Total: 30`.
- **Resultado de build:** backend, frontend Vite y `dotnet publish` correctos.
- **Evidencia retenida:** artefacto `celd-test-evidence` con `TestResults/celd-tests.trx` y `artifacts/revision.txt`; artefacto `celd-publish` con archivos compilados. La publicación de artefactos **no prueba despliegue**.

## 2. Verificaciones pendientes o incompletas

| Comprobación / ID | Estado | Acción necesaria |
|---|---|---|
| AC-11 / `FR-ERR-002` / `NFR-USAB-001` | **PENDIENTE** | Ejecutar y documentar navegador real: teclado, etiquetas, errores, carga, vacío, mensajes y foco. |
| AC-12 / `NFR-TEST-002` | **PARCIAL** | Automatización y TRX sí ejecutados; falta demostración E2E presenciada de reservar → pickup → return y ruta negativa. |
| AC-13 / `NFR-DOC-001` | **PENDIENTE** | Verificar setup desde un clon nuevo; actualizar README y documentación restante para reflejar CI ejecutado. |
| AC-14 / `NFR-CI-001` | **PARCIAL** | CI verde observado, pero falta prueba deliberada de error (rojo) seguida de corrección (verde). Cambiar `npm install` a `npm ci` aprovechando el `client/package-lock.json` existente. |
| AC-15 / `NFR-DEP-001` | **PENDIENTE** | Despliegue a destino autorizado, autenticación, workflow completo, HTTPS, reinicio, recuperación, URL y SHA. |
| AC-16 / `NFR-PERF-001` | **PARCIAL** | Guardar tiempos/condiciones de 50 Items y 100 históricos en el equipo local autorizado. |
| `NFR-REL-002` / `DR-005` | **PARCIAL** | Verificar persistencia después de reiniciar el proceso completo, no solo DbContext. |
| `BR-AUT-003` | **PARCIAL** | Añadir prueba negativa directa de retorno por Student y Visitor. |
| `FR-RES-007` / `DR-004` | **PARCIAL** | Afirmar explícitamente orden, actor y UTC de todos los HistoryEvents. |
| Aprobación de SRS y del entorno | **SIN CONFIRMAR** | Obtener aprobación formal del SRS, SDK/Node/proveedor BD y objetivo de despliegue según la hoja del profesor. |

## 3. Riesgos y mejoras de ingeniería identificadas

1. **Archivo de base de datos versionado:** el repositorio público contiene `server/Celd.Api/celd.db.backup-20261108-103502`. Puede incorporar información sensible; analizar contenido y exposición, retirar de Git, evaluar limpieza del historial y rotación de secretos si procede. Actualizar `.gitignore` para excluir futuros `*.db.backup*`.
2. **Documentación desfasada:** `README.md` todavía indica que los tests no se ejecutaron; debe alinearse con esta evidencia.
3. **Dependencias de Node:** el flujo CI usa `npm install`, aunque existe `client/package-lock.json`; `npm ci` ofrecería mayor reproducibilidad.
4. **Cambios de esquema:** `DbInitializer.cs` usa `EnsureCreatedAsync`. Para evolucionar el esquema, planificar migraciones EF Core aprobadas.
5. **Pruebas vs aceptación:** el éxito de CI acredita ejecución de los checks presentes; no sustituye una revisión de producto, pruebas manuales ni despliegue verificado.

## 4. Conclusión para defensa del Día 4

La arquitectura Profile D está implementada y las verificaciones automatizadas esenciales se han ejecutado con éxito contra ASP.NET Core y SQLite real. En particular, AC-04 (competencia y reserva única), AC-05 (aislamiento entre Students), AC-07 a AC-09 (permisos y transiciones) y AC-10 (atomicidad de historial) cuentan con evidencia de pruebas aprobadas en CI. El equipo **no** declara aceptación global completa: quedan pruebas de interfaz, evidencia E2E presenciada, CI rojo→verde, aprobación del entorno y despliegue/reinicio.

**Matriz asociada:** [RTM actualizado](https://github.com/Sjvr21/CELD-Group4/blob/main/docs/RTM.md).  
**Control de versiones:** esta actualización documenta evidencia existente. Cualquier ejecución posterior a este commit requiere un nuevo SHA y run URL; no se considera probada automáticamente.
