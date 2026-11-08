# CELD API contract (planned, feature-oriented)

**Base path** `/api`; JSON camelCase; all errors use JSON ProblemDetails (`title`, `detail`, `status`) without secrets. Cookie authentication (`CELD.Session`); a user must call `GET /api/auth/csrf` and send returned `requestToken` in `X-CSRF-TOKEN` for **all POST** operations. Staff/Student role is derived only from validated signed/session data; never from an input role or studentId.

| Verb/path | Access | Success / notes |
|---|---|---|
| `GET /auth/csrf` | Public | `{ requestToken }`; sets antiforgery cookie |
| `POST /auth/login` | Public + CSRF | `{ email, password }` → `{ id,displayName,role }`; invalid credentials 401 |
| `POST /auth/logout` | Signed-in + CSRF | Revokes server-side session, removes cookie |
| `GET /auth/me` | Signed-in | Current provisioned user/role or 401 |
| `GET /equipment` | Public | List items; optional `search`, `category` (combined, case-insensitive) |
| `GET /equipment/categories` | Public | Distinct category names |
| `GET /equipment/{id}` | Public | Item and computed `available`; missing 404 |
| `POST /reservations` | Student + CSRF | `{equipmentId}` → 201 + reservation with History; unavailable 409 |
| `GET /reservations/mine` | Student | Own active + completed/cancelled records and History |
| `GET /reservations/{id}` | Student owner or Staff | Details, UTC timestamps, chronological History; missing or other-student record 404 |
| `POST /reservations/{id}/cancel` | Owning Student + CSRF | Reserved → Cancelled; stale/invalid 409 |
| `GET /staff/reservations` | Staff | All records, optional status filter |
| `POST /staff/reservations/{id}/pickup` | Staff + CSRF | Reserved → CheckedOut; stale/invalid 409 |
| `POST /staff/reservations/{id}/return` | Staff + CSRF | CheckedOut → Returned; stale/invalid 409 |
| `GET /health` | Public | Health probe (not a substitute for acceptance testing) |

Example reservation response shape:

```json
{
  "id": 8,
  "equipmentId": 1,
  "assetCode": "LAP-101",
  "equipmentName": "Laptop Dell Latitude",
  "studentId": 2,
  "studentName": "Student One",
  "status": "Reserved",
  "createdUtc": "2026-11-06T20:00:00Z",
  "history": [
    {"id": 10, "eventType": "Created", "resultingStatus": "Reserved", "actorUserId": 2,
     "actorName": "Student One", "occurredUtc": "2026-11-06T20:00:00Z"}
  ]
}
```

The example is illustrative; its IDs/timestamps are not observed test evidence.
