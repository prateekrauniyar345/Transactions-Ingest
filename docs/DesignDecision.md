# Design Decisions

## Backend design

### 1. Three tables with separate responsibilities

We use EF Core Code-First to define the database through C# models. Migrations record schema changes so another developer can recreate the database. SQLite keeps setup simple because it stores data in a local file and needs no separate database server.

- **Transactions** stores the latest state of each transaction.
- **TransactionAudits** records which field changed, its old and new values, and when the change happened.
- **ImportBatches** records each import execution, its status, timestamps, and inserted, updated, revoked, and unchanged counts.

Each transaction has an internal `Id`. The source identifies it through `(SourceSystem, TransactionId)`, which has a unique database index. This allows two stores to use the same transaction number without creating a conflict.

Audit records reference the internal transaction `Id`. Deleting a transaction with audit records is restricted to preserve its history. Amounts use C# `decimal`, and application timestamps use UTC. SQLite does not enforce decimal precision or string lengths through the current model settings alone; input validation remains to be completed.

#### Transactions schema

| Column | C# type | SQLite type | Rules / purpose |
| --- | --- | --- | --- |
| `Id` | `int` | INTEGER | Primary key, auto-increment. |
| `SourceSystem` | `string` | TEXT | Not null; model limit 19 characters. |
| `TransactionId` | `int` | INTEGER | Not null; identifier from the source. |
| `CardNumber` | `string` | TEXT | Not null; model limit 19 characters; empty string allowed. |
| `LocationCode` | `string` | TEXT | Not null; model limit 19 characters. |
| `ProductName` | `string` | TEXT | Not null; model limit 19 characters. |
| `Amount` | `decimal` | TEXT | Not null; stored as text by the EF Core SQLite provider to retain decimal precision. |
| `TransactionTime` | `DateTime` | TEXT | Not null; transaction timestamp in UTC. |
| `Status` | `TransactionStatus` | TEXT | Not null; `Active` or `Revoked`; new C# objects start as `Active`. |

**Unique index:** `(SourceSystem, TransactionId)`.

#### TransactionAudits schema

| Column | C# type | SQLite type | Rules / purpose |
| --- | --- | --- | --- |
| `Id` | `int` | INTEGER | Primary key, auto-increment. |
| `TransactionRecordId` | `int` | INTEGER | Not null; foreign key to `Transactions.Id`. |
| `ChangeType` | `string` | TEXT | Not null; model limit 30 characters; currently `Updated` or `Revoked`. |
| `FieldName` | `string` | TEXT | Not null; model limit 50 characters; field that changed. |
| `OldValue` | `string?` | TEXT | Nullable; value before the change. |
| `NewValue` | `string?` | TEXT | Nullable; value after the change. |
| `ChangedAt` | `DateTime` | TEXT | Not null; change timestamp in UTC. |

**Relationship:** one transaction can have many audits. Deletion of a referenced transaction is restricted. An index on `(TransactionRecordId, ChangedAt)` supports history lookups.

#### ImportBatches schema

| Column | C# type | SQLite type | Rules / purpose |
| --- | --- | --- | --- |
| `Id` | `int` | INTEGER | Primary key, auto-increment. |
| `StartedAt` | `DateTime` | TEXT | Not null; execution start time in UTC. |
| `CompletedAt` | `DateTime?` | TEXT | Nullable; filled when execution ends. |
| `Status` | `string` | TEXT | Not null; model limit 20 characters; database default `Running`. |
| `InsertedCount` | `int` | INTEGER | Not null; database default `0`. |
| `UpdatedCount` | `int` | INTEGER | Not null; database default `0`. |
| `RevokedCount` | `int` | INTEGER | Not null; database default `0`. |
| `UnchangedCount` | `int` | INTEGER | Not null; database default `0`. |

A batch covers all three sources, so it has no `SourceSystem` column. It currently has no foreign-key links to the other tables. The service uses `Running`, `Completed`, and `Failed`; `PartialFailure` is reserved for possible future source-by-source processing. Status values are application conventions, not database check constraints.

Model length limits describe the intended schema; SQLite does not enforce them as maximum text lengths. Date/time values use TEXT storage and are treated as UTC by the application. C# navigation properties such as `AuditHistory` are relationships, not extra database columns.

### 2. Small API controllers and a separate import service

The API exposes two main operations:

- `POST /api/import` runs an import and returns a summary.
- `GET /api/transactions` returns transaction data, including active and revoked records.

Controllers handle HTTP requests. `TransactionImportService` handles the business rules. Keeping these separate means the same import logic can later be called by a scheduled job.

The three source systems implement `ITransactionSource`. Dependency injection supplies them to the import service. We currently use mock sources, but real API clients could implement the same interface later.

Response DTOs return only the fields callers need instead of exposing database relationships. Swagger UI lets us explore and manually test the endpoints. Errors are logged, and unexpected failures return a general error message without exposing internal exception details.

### 3. Compare snapshots, preserve history, and test repeated imports

Each source returns a complete snapshot. The service fetches all snapshots before changing transaction records. A failed request is treated as a failure, not as an empty snapshot, so a source outage cannot accidentally revoke its transactions.

For each source, the service:

1. Inserts transactions that do not exist.
2. Compares existing fields and updates only changed values, creating an audit record for each changed field.
3. Leaves identical transactions unchanged.
4. Marks missing active transactions as revoked when their age is between zero and 24 hours, including exactly 24 hours.
5. Reactivates a revoked transaction if it appears again and records the status change.

Missing transactions older than 24 hours, or dated in the future, are not revoked. We have not implemented optional finalization: older transactions can still be updated when they appear in a snapshot. Duplicate transaction IDs within one source snapshot and mismatched source names are rejected.

Transaction changes, audits, and the successful batch summary are committed together in one database transaction. If processing fails, these changes are rolled back. The batch is then marked `Failed` separately. Cancelled imports also use `Failed`; cancellation is logged separately. A process crash or unavailable database can leave a batch marked `Running`.

**Idempotency** means importing the same data again does not create duplicate transactions or repeated audit events. Each execution still creates a new batch because a batch records an attempt, not a transaction change.

We verified this manually through Swagger and SQL queries, then added xUnit tests using fresh in-memory SQLite databases and the real migrations. Tests cover repeated imports, updates, revocation, reactivation, source failures, batch counts, and rollback after database writes. An injectable clock makes the exact 24-hour boundary predictable in tests. Concurrent imports are not currently serialized and may conflict; imports should run one at a time.

## Version control workflow

We use three Git branches to separate development, validation, and release work:

| Branch | Purpose |
| --- | --- |
| `development` | Build features, fix issues, and make day-to-day changes. |
| `test` | Validate changes through application testing, user acceptance testing (UAT), regression testing, security review, and other checks before release. |
| `main` | Hold the final reviewed changes intended for deployment. |

Changes move from `development` to `test`, then to `main` after validation. Fixes are checked again before promotion. These branches separate stages of work; they do not automatically create separate running environments. Security review, UAT, and deployment automation are workflow intentions, not claims that they have already been completed or configured.

## Frontend design
