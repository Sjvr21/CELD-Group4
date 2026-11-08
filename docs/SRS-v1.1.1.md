# Campus Equipment Lending Desk (CELD)
## Software Requirements Specification (SRS)

| Document control | Value |
|---|---|
| Course | COMP 3402 — Software Engineering II |
| Document version | **1.1.1** |
| Status | **Proposed baseline — instructor approval required** |
| Product | Campus Equipment Lending Desk (CELD) |
| Audience | Student engineering teams, instructor, reviewers |
| Planned delivery | Five-week AI-Assisted Software Engineering Studio |
| Companion documents | Studio Experiment Protocol; Weekly Student Guides; instructor environment sheet |

> **Revision note (1.1.1):** Moved nonfunctional requirements ahead of acceptance criteria, traceability, and release acceptance; renumbered the affected sections. Preserved the business-rule-first ordering and the step-anchored basic, alternate, and exception paths introduced in v1.1.0. No product requirements or acceptance criteria were changed.

> **Scope principle:** CELD is a **small, complete product**, not a prototype of a larger application. Every mandatory requirement below applies to every group. Assigned AI architecture/implementation profiles are defined in the *Studio Experiment Protocol*, **not** in this product SRS; they never override product requirements.

---

# Table of Contents

- [Campus Equipment Lending Desk (CELD)](#campus-equipment-lending-desk-celd)
	- [Software Requirements Specification (SRS)](#software-requirements-specification-srs)
	- [1. Introduction](#1-introduction)
		- [1.1 Purpose](#11-purpose)
		- [1.2 Product boundary](#12-product-boundary)
		- [1.3 Actors](#13-actors)
		- [1.4 Terms](#14-terms)
		- [1.5 Requirement conventions](#15-requirement-conventions)
	- [2. Product Overview and Operational Constraints](#2-product-overview-and-operational-constraints)
		- [2.1 Representative workflow](#21-representative-workflow)
		- [2.2 Shared technical baseline](#22-shared-technical-baseline)
		- [2.3 Environment and sample data](#23-environment-and-sample-data)
	- [3. Business Rules](#3-business-rules)
		- [3.1 Lifecycle reference](#31-lifecycle-reference)
	- [4. Use Cases](#4-use-cases)
		- [4.1 UC-001 — Find Equipment](#41-uc-001--find-equipment)
		- [4.2 UC-002 — Reserve Equipment](#42-uc-002--reserve-equipment)
		- [4.3 UC-003 — View and Cancel Own Reservation](#43-uc-003--view-and-cancel-own-reservation)
		- [4.4 UC-004 — Record Pickup](#44-uc-004--record-pickup)
		- [4.5 UC-005 — Record Return](#45-uc-005--record-return)
	- [5. Functional Requirements](#5-functional-requirements)
		- [5.1 Authentication and access](#51-authentication-and-access)
		- [5.2 Equipment discovery](#52-equipment-discovery)
		- [5.3 Reservations and lending](#53-reservations-and-lending)
		- [5.4 Failure feedback](#54-failure-feedback)
	- [6. Data Requirements](#6-data-requirements)
	- [7. Nonfunctional Requirements and Quality Constraints](#7-nonfunctional-requirements-and-quality-constraints)
	- [8. Acceptance Criteria](#8-acceptance-criteria)
	- [9. Traceability and Review Expectations](#9-traceability-and-review-expectations)
		- [9.1 Minimum traceability artifact](#91-minimum-traceability-artifact)
		- [9.2 Engineering freedom and accountability](#92-engineering-freedom-and-accountability)
		- [9.3 Change control](#93-change-control)
	- [10. Release Acceptance and Definition of Done](#10-release-acceptance-and-definition-of-done)
	- [Annex A — Informative Implementation Boundaries (Not Requirements)](#annex-a--informative-implementation-boundaries-not-requirements)
	- [Annex B — Informative Studio Alignment (Not Product Requirements)](#annex-b--informative-studio-alignment-not-product-requirements)


## 1. Introduction

### 1.1 Purpose

This SRS defines the externally observable behavior and acceptance conditions for CELD, a browser-based system through which students reserve individually identified campus equipment and lending-desk staff record pickup and return.

The system is intentionally limited to one coherent workflow. It creates realistic engineering decisions involving responsibilities, state changes, persistence, concurrency, security, testability, maintainability, documentation, and repeatable delivery without requiring a large feature set.

### 1.2 Product boundary

**In scope:** public equipment browsing; seeded user accounts and authenticated sessions; equipment reservations; viewing one's reservations; cancellation before pickup; staff queues; pickup and return; reliable status/history; failures and concurrent requests; documentation, testing, CI, and deployment.

**Out of scope:** account registration and password recovery; equipment acquisition or inventory-management UI; fines, fees, payments, waitlists, overdue rules, due dates, renewals, email/SMS, notifications, barcode scanning, photo uploads, multi-campus support, analytics, and external identity providers. Equipment and initial accounts are provisioned as controlled seed/setup data.

### 1.3 Actors

| Actor | Description | Permitted operations |
|---|---|---|
| Visitor | Unauthenticated browser user | Browse and search the public equipment catalog |
| Student | Authenticated borrower | Browse; reserve available equipment; view own reservations; cancel own uncollected reservation |
| Lending Staff | Authenticated staff member | Browse; view reservations and equipment; record pickup and return |

A user has exactly one course-assigned role for this product. Staff do not act as borrowers in the required workflows. A user cannot grant or change their own role through the application.

### 1.4 Terms

- **Equipment Item:** One physically distinguishable lendable item, identified by a unique asset code; not a stock quantity.
- **Reservation:** The persistent record of one student's request for one Equipment Item and its subsequent lending lifecycle.
- **Active Reservation:** A Reservation with status `Reserved` or `CheckedOut`.
- **Available Item:** An Item with **no** Active Reservation.
- **History Event:** A retained record of an accepted creation or status transition, including actor and time.
- **Acceptance Evidence:** Observed output of a performed check, identified by procedure, environment, and source revision.

### 1.5 Requirement conventions

- **Shall** denotes a mandatory requirement.
- Stable identifiers use `FR-*` (functional), `BR-*` (business rule), `DR-*` (data), `NFR-*` (quality/delivery), `UC-*` (use case), and `AC-*` (acceptance criterion).
- This SRS states required **behavior**, not a preferred internal design. A particular pattern, layer count, repository interface, class structure, or file layout is **not** a product requirement.
- Every change to mandatory behavior requires an approved Change Request (CR), revised SRS version, and updated affected acceptance/traceability entries. A team must not silently redefine the SRS to suit generated code.

---

## 2. Product Overview and Operational Constraints

### 2.1 Representative workflow

1. A Visitor sees the catalog and equipment availability.
2. A Student signs in, selects an Available Item, and reserves it.
3. The Item becomes unavailable for competing reservations.
4. Lending Staff see the reservation and mark the item as picked up.
5. Lending Staff later record its return.
6. The Item becomes Available again; the Reservation and its history remain accessible to authorized users.

Students may alternatively cancel their own reservation **before** pickup; cancellation also makes the Item Available again.

### 2.2 Shared technical baseline

All teams shall use the same instructor-approved course stack to support meaningful design comparisons:

- **Client:** browser-based React and TypeScript application.
- **Server:** C# / ASP.NET Core Web API.
- **Persistence:** Entity Framework Core with a relational database.
- **Authentication:** server-validated, cookie-based session authentication using an approved ASP.NET Core mechanism; no JWT implementation is required.
- **Version control and CI:** Git and GitHub, with GitHub Actions.
- **Delivery target:** instructor-approved Azure environment where available, or an announced equivalent if account quotas/eligibility prevent Azure use.

The instructor environment sheet shall fix the supported SDK, Node.js version, relational database provider, local configuration, and deployment route **before Day 1**. Teams shall not independently change the common stack without instructor approval. The SRS does not mandate controllers versus Minimal APIs, layered versus feature-oriented design, generic repositories, a specific dependency-injection abstraction scheme, or any named object-oriented design pattern.

### 2.3 Environment and sample data

A reproducible setup shall provision **at least six Equipment Items**, **two Student accounts**, and **one Lending Staff account**. Items shall have distinct asset codes and enough variation in name/category to exercise catalog search. Demo credentials shall be supplied securely, not embedded in committed source code or published documentation. No production user account creation screen is required.

---

## 3. Business Rules

| ID | Mandatory rule |
|---|---|
| **BR-AUT-001** | The server shall determine the current user's identity and role from a validated session, never from caller-submitted role or user identifiers. |
| **BR-AUT-002** | Students shall access or cancel only their own Reservations. Staff may access Reservations for lending operations. |
| **BR-AUT-003** | Only Lending Staff may record pickup or return. Browser navigation controls are not an authorization boundary. |
| **BR-RES-001** | A Student may reserve only an Available Item; unavailable items remain visible but cannot be successfully reserved. |
| **BR-RES-002** | At most **one Active Reservation** may exist for a given Equipment Item at any time, including when requests are concurrent. |
| **BR-RES-003** | Legal status transitions are exactly: `Reserved → CheckedOut` (Staff); `Reserved → Cancelled` (owning Student); `CheckedOut → Returned` (Staff). No other transition shall be accepted. |
| **BR-RES-004** | Each accepted Reservation creation or status transition shall be persisted together with its corresponding History Event as one successful logical operation; on failure, the operation shall not leave contradictory partial state. |
| **BR-RES-005** | Cancellation and return release the Item for a new Reservation. Previously Cancelled or Returned Reservations remain part of history and shall not be reactivated. |
| **BR-RES-006** | A refused operation shall not change the Reservation's status or create a success History Event. Repeated processing of an invalid or already-completed transition shall not create a second successful transition. |
| **BR-RES-007** | Equipment availability and Reservation status displayed to users shall reflect authoritative persisted server state after a completed operation; client-side state is not authoritative. |
| **BR-RES-008** | Reservations and their History Events shall not be removed through the product's user interface. |

### 3.1 Lifecycle reference

| Current status | Request | Actor | Result |
|---|---|---|---|
| No Reservation, Item Available | Reserve Item | Student | Create `Reserved` Reservation |
| `Reserved` | Cancel | Owning Student | `Cancelled`; Item Available |
| `Reserved` | Record pickup | Lending Staff | `CheckedOut`; Item unavailable |
| `CheckedOut` | Record return | Lending Staff | `Returned`; Item Available |
| Any other current status | Any unsupported transition | Any actor | Refuse; no state change |

If two users act on the same Item based on outdated screens, the server shall accept only a transition consistent with the current authoritative state and return a controlled conflict or equivalent informative failure for stale intent.

---

## 4. Use Cases

Use cases describe actor interactions with the product and organize the behavior required by the business rules and functional requirements. **They do not introduce additional requirements.** Every basic flow uses numbered steps; alternate paths use `A<basic-step>.<sequence>`, and exception paths use `E<basic-step>.<sequence>`. Each path names the basic-flow step where it branches. A path that ends does not resume the basic flow.

### 4.1 UC-001 — Find Equipment

**Primary actor:** Visitor, Student, or Lending Staff.  
**Preconditions:** None.  
**Trigger:** The user opens the equipment catalog.

**Basic path**

1. The user opens the catalog.
2. The client requests the equipment catalog from the server.
3. The server returns Equipment Items with their current availability.
4. The client displays the list, including name, category, asset code, and Available/Unavailable status.
5. The user selects an Equipment Item.
6. The client requests that Item's details from the server.
7. The client displays the Item's description and current availability.

**Alternate paths**

- **A2.1 — Search by name (branches at step 2):** The user supplies a name search term. The client includes the term in the catalog request; the server performs a case-insensitive match; resume at step 3.
- **A2.2 — Filter by category (branches at step 2):** The user selects a category. The client includes it in the catalog request; resume at step 3. Search and category filtering may be combined.
- **A4.1 — No matching Items (branches at step 4):** The client displays a clear empty state; the use case ends unless the user changes the search or filter.
- **A5.1 — Browse without selecting an Item (branches at step 5):** The user remains on the catalog without opening details; the use case ends.

**Exception paths**

- **E2.1 — Catalog request failure (branches at step 2):** The request fails. The client displays a controlled failure state without fabricated results; the user may retry from step 2.
- **E6.1 — Item no longer exists (branches at step 6):** The server returns a controlled not-found result; the client displays the missing-Item state and does not display stale details as current.
- **E6.2 — Item details request failure (branches at step 6):** The client displays a controlled failure state and does not claim successful retrieval.

**Postconditions:** Displayed catalog and Item information originate from authoritative server responses; no Item or Reservation is modified.  
**References:** `FR-CAT-001`–`FR-CAT-003`; `FR-ERR-002`; `BR-RES-007`.

### 4.2 UC-002 — Reserve Equipment

**Primary actor:** Student.  
**Preconditions:** The Student has an authenticated session and has selected an existing Item shown as Available; the server shall independently recheck availability.  
**Trigger:** The Student chooses to reserve the Item.

**Basic path**

1. The Student views the selected Item and its displayed availability.
2. The Student chooses **Reserve**.
3. The client submits a reservation request identifying the selected Item.
4. The server validates the session and derives the Student's identity and role from it.
5. The server verifies that the Item exists and is currently Available, enforcing the single-active-reservation invariant under concurrent requests.
6. The server accepts the operation and persists a `Reserved` Reservation, its creation time, and the corresponding History Event as one successful logical operation.
7. The server returns the accepted Reservation and current state.
8. The client displays the new Reservation and refreshes or updates availability from authoritative server state; the Item is Unavailable.

**Alternate paths**

- **A2.1 — Student does not submit (branches at step 2):** The Student leaves the Item without choosing Reserve. No Reservation or History Event is created; the use case ends.

**Exception paths**

- **E4.1 — Authentication missing or expired (branches at step 4):** The server refuses the protected operation; the client presents the authentication failure without claiming a Reservation was made.
- **E4.2 — Not a Student account (branches at step 4):** The server refuses the operation, regardless of visible client controls.
- **E5.1 — Missing Item (branches at step 5):** The server returns a controlled not-found result; no Reservation is created.
- **E5.2 — Item unavailable or competing reservation (branches at step 5):** The server refuses the request with a controlled state/conflict response; no second Active Reservation is created and the client revalidates availability.
- **E6.1 — Persistence failure (branches at step 6):** The operation is not accepted with contradictory partial Reservation/History state; the client reports failure rather than success.
- **E7.1 — Response not received (branches at step 7):** The client does not assume success or failure solely from the missing response; it revalidates authoritative state before displaying a definitive result.

**Postconditions:** On success, one new `Reserved` Reservation belongs to the authenticated Student, a corresponding History Event is retained, and the Item has exactly one Active Reservation. On failure, no forbidden or duplicate successful mutation is accepted.  
**References:** `FR-RES-001`; `FR-ERR-001`–`FR-ERR-002`; `BR-AUT-001`; `BR-RES-001`–`BR-RES-002`; `BR-RES-004`; `BR-RES-006`–`BR-RES-007`.

### 4.3 UC-003 — View and Cancel Own Reservation

**Primary actor:** Student.  
**Preconditions:** The Student has an authenticated session.  
**Trigger:** The Student opens **My Reservations** or selects a Reservation to cancel.

**Basic path**

1. The Student opens **My Reservations**.
2. The server validates the session and retrieves Reservations belonging to that Student.
3. The client displays the Student's active and completed/cancelled Reservations with their current status and creation time.
4. The Student selects a `Reserved` Reservation and can inspect its chronological History Events.
5. The Student chooses **Cancel**.
6. The server independently verifies the Student's ownership and the Reservation's current `Reserved` status.
7. The server persists `Cancelled` and the corresponding History Event as one successful logical operation.
8. The client displays the updated Reservation and Item availability using authoritative server state.

**Alternate paths**

- **A3.1 — No Reservations (branches at step 3):** The client displays an empty state; the use case ends.
- **A4.1 — View history without cancellation (branches at step 4):** The Student inspects an authorized Reservation (including a non-`Reserved` record) and its events, then returns to the list; no mutation occurs and the use case ends.
- **A5.1 — Student decides not to cancel (branches at step 5):** No mutation occurs; the use case ends.

**Exception paths**

- **E2.1 — Missing or expired authentication (branches at step 2):** The server refuses access; the client shows an authentication failure.
- **E4.1 — Attempt to retrieve another Student's Reservation (branches at step 4):** The server refuses the request without exposing that Reservation's private details.
- **E6.1 — Not the owner (branches at step 6):** The server refuses cancellation; no state change or successful History Event occurs.
- **E6.2 — Status changed or cannot be cancelled (branches at step 6):** The Reservation is `CheckedOut`, `Cancelled`, or `Returned`, or is otherwise stale. The server rejects cancellation and the client revalidates current state.
- **E7.1 — Persistence failure (branches at step 7):** Neither a successful cancellation nor a success History Event is accepted independently of the other.
- **E8.1 — Failed or missing response (branches at step 8):** The client displays controlled failure or uncertain state and revalidates rather than falsely reporting a completed cancellation.

**Postconditions:** On cancellation, the Reservation remains in history as `Cancelled`, its History Event is retained, and the Item becomes Available. On view-only paths, no mutation occurs.  
**References:** `FR-RES-002`–`FR-RES-003`; `FR-RES-007`; `BR-AUT-002`; `BR-RES-003`–`BR-RES-008`.

### 4.4 UC-004 — Record Pickup

**Primary actor:** Lending Staff.  
**Preconditions:** Staff has an authenticated session; a target Reservation is currently `Reserved`.  
**Trigger:** Staff chooses to record the Item's pickup.

**Basic path**

1. Staff opens the Staff Reservations view.
2. The server validates the Staff session and returns Reservations and their current statuses.
3. The client displays the Staff queue, including Item, Student, status, and creation time.
4. Staff selects a `Reserved` Reservation and may inspect its History Events.
5. Staff chooses **Record Pickup**.
6. The server verifies Staff authority and that the Reservation still has status `Reserved`.
7. The server persists the transition to `CheckedOut` and its History Event as one successful logical operation.
8. The client displays the updated `CheckedOut` state using authoritative server data; the Item remains Unavailable.

**Alternate paths**

- **A2.1 — Status filter (branches at step 2):** Staff chooses a status filter; the server returns the matching Reservations and the flow resumes at step 3.
- **A3.1 — No matching Reservations (branches at step 3):** The client displays an empty state and the use case ends.
- **A5.1 — Staff does not record pickup (branches at step 5):** Staff returns to the queue; no mutation occurs and the use case ends.

**Exception paths**

- **E2.1 — Missing authentication or Staff authority (branches at step 2):** The server refuses access to the protected Staff queue.
- **E4.1 — Reservation not found (branches at step 4):** The server returns a controlled not-found result.
- **E6.1 — Unauthorized pickup (branches at step 6):** A Student, Visitor, or otherwise unauthorized caller is refused, irrespective of browser controls.
- **E6.2 — Stale or invalid status (branches at step 6):** The Reservation is no longer `Reserved`; the server rejects the transition and the client revalidates current state.
- **E7.1 — Persistence failure (branches at step 7):** No accepted partial status change or successful History Event remains.
- **E8.1 — Failed or missing response (branches at step 8):** The client does not claim confirmed pickup without revalidating authoritative state.

**Postconditions:** On success, the Reservation is `CheckedOut`, a History Event is retained, and the Item remains Unavailable.  
**References:** `FR-RES-004`–`FR-RES-005`; `FR-RES-007`; `BR-AUT-001`; `BR-AUT-003`; `BR-RES-003`–`BR-RES-004`; `BR-RES-006`–`BR-RES-007`.

### 4.5 UC-005 — Record Return

**Primary actor:** Lending Staff.  
**Preconditions:** Staff has an authenticated session; a target Reservation is currently `CheckedOut`.  
**Trigger:** Staff chooses to record the Item's return.

**Basic path**

1. Staff opens the Staff Reservations view and selects a `CheckedOut` Reservation.
2. The client requests that Reservation's current details and history.
3. The server validates Staff authority and returns the current Reservation state.
4. Staff chooses **Record Return**.
5. The client submits the return action.
6. The server verifies Staff authority and that the Reservation is still `CheckedOut`.
7. The server persists the transition to `Returned` and its History Event as one successful logical operation.
8. The client displays the completed Reservation and the Item as Available, using authoritative server state.

**Alternate paths**

- **A1.1 — No checked-out Reservations (branches at step 1):** Staff finds no matching records; the client shows an empty state, and the use case ends.
- **A4.1 — Staff does not record return (branches at step 4):** Staff views the Reservation and its history without mutation; the use case ends.

**Exception paths**

- **E3.1 — Missing authentication or Staff authority (branches at step 3):** The server refuses access to the protected Reservation.
- **E3.2 — Reservation not found (branches at step 3):** The server returns a controlled not-found result.
- **E6.1 — Unauthorized return (branches at step 6):** The server refuses a caller without Staff authority, regardless of the client UI.
- **E6.2 — Invalid, repeated, or stale return (branches at step 6):** The current status is not `CheckedOut`; the server rejects the action and does not record a second successful return.
- **E7.1 — Persistence failure (branches at step 7):** No accepted partial return or successful History Event remains.
- **E8.1 — Failed or missing response (branches at step 8):** The client revalidates authoritative state before claiming that return was completed.

**Postconditions:** On success, the Reservation is `Returned`, its history remains available, and the Item can be reserved again.  
**References:** `FR-RES-004`; `FR-RES-006`–`FR-RES-007`; `BR-AUT-001`; `BR-AUT-003`; `BR-RES-003`–`BR-RES-008`.

---

## 5. Functional Requirements

### 5.1 Authentication and access

| ID | Requirement |
|---|---|
| **FR-AUT-001** | The system shall allow a provisioned Student or Lending Staff account to sign in using the approved session mechanism and shall refuse invalid credentials without revealing which credential failed. |
| **FR-AUT-002** | The system shall allow an authenticated user to sign out and prevent continued use of that ended session for protected actions. |
| **FR-AUT-003** | The application shall display the authenticated user's role and provide navigation appropriate to that role, without relying on the UI to enforce permissions. |

### 5.2 Equipment discovery

| ID | Requirement |
|---|---|
| **FR-CAT-001** | Any Visitor or authenticated user shall be able to list Equipment Items with name, category, asset code, and current Available/Unavailable status. |
| **FR-CAT-002** | Users shall be able to filter the catalog by a case-insensitive name search and/or category. An empty result shall display a clear empty state. |
| **FR-CAT-003** | Users shall be able to view an Item's description and current availability; requests for missing Items shall produce a controlled not-found result. |

### 5.3 Reservations and lending

| ID | Requirement |
|---|---|
| **FR-RES-001** | An authenticated Student shall be able to create a Reservation for an Available Item. The server shall associate the Reservation with the authenticated Student, assign status `Reserved`, and retain its creation time. |
| **FR-RES-002** | An authenticated Student shall be able to list and inspect their own Reservations, including current status and creation time, with active and completed/cancelled records distinguishable. |
| **FR-RES-003** | An authenticated Student shall be able to cancel their own `Reserved` Reservation. Cancellation shall update availability and preserve the Reservation and its history. |
| **FR-RES-004** | Lending Staff shall be able to list and inspect Reservations, including Item, Student, status, and creation time, and filter by status. |
| **FR-RES-005** | Lending Staff shall be able to record pickup for a `Reserved` Reservation, resulting in status `CheckedOut`. |
| **FR-RES-006** | Lending Staff shall be able to record return for a `CheckedOut` Reservation, resulting in status `Returned`. |
| **FR-RES-007** | Authorized users viewing a Reservation shall be able to inspect its chronological History Events (event type, timestamp, and acting user). |

### 5.4 Failure feedback

| ID | Requirement |
|---|---|
| **FR-ERR-001** | The API shall return a consistent, non-secret error representation for validation errors, missing authentication, insufficient permissions, missing records, and state/conflict failures, distinguishing these categories. |
| **FR-ERR-002** | The client shall show clear loading, empty, success, and failure states for its main catalog and lending workflows; a failed operation shall not be presented as a success. |

---

## 6. Data Requirements

| ID | Required information / constraint |
|---|---|
| **DR-001** | Each Equipment Item shall have a persistent unique identifier, a unique asset code, name, category, and description. |
| **DR-002** | Each provisioned user shall have a stable identifier, display name, credential/identity information maintained through the approved authentication mechanism, and role. Passwords shall not be stored in plaintext. |
| **DR-003** | Each Reservation shall preserve its stable identifier, Equipment Item reference, Student reference, current status, and creation time. |
| **DR-004** | Each accepted Reservation creation or status transition shall have a retained History Event with Reservation reference, resulting status, actor reference, and UTC timestamp. |
| **DR-005** | The database shall preserve the relationships among Items, users, Reservations, and History Events, including past Cancelled and Returned Reservations, across application restarts. |
| **DR-006** | The displayed Item availability shall be unambiguously determined from authoritative persisted state. Any denormalized availability representation shall be kept consistent with BR-RES-002 and BR-RES-005. |

The SRS does not require a particular database schema or a particular choice between computing and persisting Item availability. Any chosen design must preserve the same externally visible behavior and invariants.

---

## 7. Nonfunctional Requirements and Quality Constraints

| ID | Verifiable requirement |
|---|---|
| **NFR-SEC-001** | Authentication and role/ownership checks shall be enforced by the server for every protected action; attempts to access another Student's Reservation shall be refused without exposing its private details. |
| **NFR-SEC-002** | Browser sessions shall use an appropriate secure cookie configuration in the deployed environment, and state-changing cookie-authenticated operations shall have appropriate CSRF protection. No secrets or plaintext passwords shall be committed or exposed in responses/logs. |
| **NFR-REL-001** | Concurrent attempts to reserve the same Available Item shall not produce more than one Active Reservation; the losing request shall receive a controlled non-success result. The check shall be demonstrated using the chosen real relational provider or a provider-faithful test environment. |
| **NFR-REL-002** | Status changes and History Events shall succeed together or leave no accepted partial state. The system shall retain completed records across restarts. |
| **NFR-USAB-001** | Core user actions shall be keyboard operable, visibly labeled, and expose status and errors as readable text, not only by color. |
| **NFR-PERF-001** | With at least 50 Items and 100 historical Reservations in the instructor-approved local test environment, an ordinary catalog or Reservation list request shall complete within **2 seconds** after startup warm-up. The test conditions and observed time shall be recorded. |
| **NFR-TEST-001** | Automated tests shall exercise legal and illegal Reservation transitions, authorization/ownership, and a representative integration path using real persistence semantics. Test execution results shall be retained and identified by source revision. |
| **NFR-TEST-002** | A reproducible end-to-end acceptance procedure shall cover the Reserve → Pickup → Return workflow and one negative path; an automated browser test is preferred but a witnessed, documented manual procedure is acceptable. |
| **NFR-TRC-001** | The release shall provide a Requirements Traceability Matrix (RTM) linking each mandatory `FR`, `BR`, and `NFR` to design/implementation location and at least one verification method/result. Unverified requirements shall be labeled, not counted as passed. |
| **NFR-DOC-001** | The repository shall provide a README with prerequisites, safe configuration, seed/setup, build, run, test, and troubleshooting procedures; architecture/component overview; API contract documentation; meaningful ADRs; known limitations; and deployment/recovery instructions. Instructions shall match the released revision. |
| **NFR-CI-001** | GitHub Actions CI shall trigger on pull requests and relevant pushes, use reproducible/pinned toolchain and dependency installation, build the server/client, run required automated tests, and show a failed job when an enforced check fails. |
| **NFR-DEP-001** | The team shall deliver a deployable application and database configuration to the instructor-approved target, identify the exact Git source revision, and demonstrate post-deployment authentication and the full Reserve → Pickup → Return workflow. Deployment credentials shall use approved secure configuration; a failed release shall have a documented rollback/recovery procedure. |

**Acceptance interpretation:** A written script or expected result alone is not execution evidence. If an Azure account is ineligible or blocked, the instructor may approve an equivalent target under the Studio Protocol; no team may claim an unperformed Azure deployment.

---

## 8. Acceptance Criteria

Each criterion describes an observable check. The approved team test plan shall specify its steps, expected results, observed results, environment, and source revision.

| ID | Acceptance scenario / expected outcome | Principal references |
|---|---|---|
| **AC-01** | Anonymous user can list/search Items and view availability without signing in; no-match and missing-item states are handled. | FR-CAT-001–003; FR-ERR-002 |
| **AC-02** | Valid Student and Staff accounts sign in and sign out; invalid credentials are refused; an ended session cannot invoke protected operations. | FR-AUT-001–003; NFR-SEC-001–002 |
| **AC-03** | A Student reserves an Available Item and sees a `Reserved` record; the Item becomes Unavailable to another Student. | FR-RES-001–002; BR-RES-001–002, 007 |
| **AC-04** | Two competing reservation requests for the same Item yield **at most one** Active Reservation, with a controlled failure for the other request. | BR-RES-002; NFR-REL-001 |
| **AC-05** | A Student can list their Reservations but cannot retrieve or cancel another Student's record. | FR-RES-002–003; BR-AUT-002; NFR-SEC-001 |
| **AC-06** | A Student cancels their own `Reserved` record; it becomes `Cancelled`, remains in history, and Item becomes Available. | FR-RES-003, 007; BR-RES-003–005 |
| **AC-07** | Staff can list Reservations and record `Reserved → CheckedOut` while Student/Visitor attempts to do so are refused. | FR-RES-004–005; BR-AUT-003; BR-RES-003 |
| **AC-08** | Staff records `CheckedOut → Returned`; the Item becomes Available and can be reserved again by a Student. | FR-RES-006; BR-RES-005 |
| **AC-09** | Invalid/repeated transitions and stale actions are rejected without duplicate successful transitions or success History Events. | BR-RES-003, 006; FR-ERR-001 |
| **AC-10** | Every accepted creation and status change has a corresponding History Event with actor/time; a forced persistence failure leaves no accepted partial change. | FR-RES-007; BR-RES-004; DR-004; NFR-REL-002 |
| **AC-11** | Core screens show usable loading/empty/failure states, keyboard-accessible actions, and appropriate controlled error feedback. | FR-ERR-001–002; NFR-USAB-001 |
| **AC-12** | Required unit, integration, negative-path and end-to-end checks are executed and linked to requirement IDs; recorded results identify revision/environment. | NFR-TEST-001–002; NFR-TRC-001 |
| **AC-13** | A reviewer can follow the README to set up, seed, run, and test the project without relying on the original AI chat; docs and ADRs reflect the code. | NFR-DOC-001; DR-001–006 |
| **AC-14** | CI visibly builds/tests the project and fails on a deliberately introduced failing required check, then succeeds after the defect is fixed. | NFR-CI-001 |
| **AC-15** | Deployed release identifies its source revision and passes a witnessed post-deployment sign-in, reservation, pickup, return, and persistence check. | NFR-DEP-001; NFR-REL-002 |
| **AC-16** | A documented 50-Item/100-history performance run satisfies the local response-time bound or discloses the unmet criterion. | NFR-PERF-001 |

The instructor may use unseen input values or permitted users during acceptance, but shall not introduce previously unstated product obligations as if they were part of the approved baseline.

---

## 9. Traceability and Review Expectations

### 9.1 Minimum traceability artifact

The team-owned RTM shall contain one row for every mandatory `FR`, `BR`, and `NFR`, including:

| Required column | Meaning |
|---|---|
| Requirement ID | Stable SRS identifier |
| Design decision / component | Where and how the team believes it is addressed |
| Implementation location | Relevant source file, function, module, or API operation |
| Verification procedure / test ID | Repeatable check tied to the obligation |
| Observed result and evidence | Pass, fail, blocked, or not run; evidence reference |
| Verified revision | Commit SHA or equivalent immutable release identity |

Acceptance criteria `AC-01`–`AC-16` shall also be linked to their executed evidence. A matrix entry is not proof merely because the team filled it in.

### 9.2 Engineering freedom and accountability

Teams may choose different internal architectures and patterns. No team earns acceptance solely by using an advertised pattern or loses acceptance solely because an instructor-assigned initial architecture is suboptimal. Claims about maintainability, abstraction, coupling, transaction safety, and testability shall be supported by code and observed behavior.

Any experimentally assigned AI strategy remains subordinate to all mandatory requirements and acceptance criteria in this SRS. Security violations, fabricated test results, silent requirement removal, and intentionally broken final releases are not authorized experimental variations.

### 9.3 Change control

A CR shall identify the affected SRS requirements, rationale, expected acceptance impact, and approval date. Each team shall perform impact analysis and update implementation, tests, traceability, and documentation as needed. Existing IDs shall remain stable where requirement meaning is preserved.

The instructor may issue one **small, common CR** during studio reviews to examine adaptability. That CR is **not part of this proposed baseline** and shall not be graded as an existing requirement until it is formally issued and accepted into a new baseline. The instructor should allow equivalent time and conditions for every group.

---

## 10. Release Acceptance and Definition of Done

CELD is **complete** only if:

1. Every mandatory requirement in the approved final SRS is implemented, or any exception has been formally approved through change control.
2. All five use cases operate end to end, including material failure paths; there is no known violation of the single-active-reservation rule or protected-role rules.
3. Required automated tests and end-to-end acceptance procedures have been **executed** against an identified revision; failures are honestly reported.
4. The RTM links requirements to observed evidence and does not silently mark gaps as passed.
5. Architecture/implementation choices and substantive revisions are explained in team ADRs and review findings.
6. The documented repository can be built, configured, seeded, run, and tested by another developer.
7. GitHub Actions CI executes required checks and represents failures accurately.
8. The deployed (or approved equivalent) release is verified against the identified source revision, with known limitations and recovery steps documented.
9. There are no published credentials, plaintext passwords, or intentionally unsafe final configurations.

**Product completion** and **individual course assessment** are distinct. A successfully generated and deployed application may satisfy the SRS but does not independently demonstrate that every student understands its design. Individual technical defense is governed by the Studio Protocol.

---

## Annex A — Informative Implementation Boundaries (Not Requirements)

This annex helps students recognize open choices without telling them which design is correct.

Potential choices include controller-focused versus feature-oriented API modules; a service layer versus direct use-case handlers; entity persistence versus explicit public DTOs (the latter still must avoid leaking sensitive data); central versus local validation; domain methods versus application policy objects for lifecycle rules; single or multiple persistence transactions; and test doubles versus provider-faithful integration tests. Teams shall evaluate alternatives against the mandatory requirements and observed evidence.

Avoid treating any of these as a prescribed architecture. A design that compiles and looks elegant is not automatically correct, and a short solution is not automatically maintainable.

## Annex B — Informative Studio Alignment (Not Product Requirements)

| Studio stage | CELD-focused evidence students can develop |
|---|---|
| Week 1 — Architecture | Map actors, use cases, invariants, responsibility boundaries, alternatives, and acceptance criteria. |
| Week 2 — Implementation and tests | Produce the complete CELD workflow, observe failures, compare code structures, run automated checks, and revise. |
| Week 3 — Documentation | Explain how CELD works, how to run/tests it, and why decisions were made; verify docs against the implementation. |
| Week 4 — CI/CD and deployment | Automate tests, deploy to the approved environment, and demonstrate release identity and operational checks. |
| Week 5 — Final engineering review | Verify all ACs, trace the requirements, explain remaining debt, and defend the released system. |

The Studio Protocol and weekly guides, not this annex, govern student presentation timing, group-number AI profile selection, grading, and feedback/revision cycles.

---

**End of proposed SRS v1.1.1.**
