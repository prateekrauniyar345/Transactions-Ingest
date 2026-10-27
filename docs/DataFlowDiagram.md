# Data Flow Diagram

This diagram shows how the current backend imports snapshots and returns stored transactions. The three data stores are tables in the same SQLite database.

![Transaction ingestion data flow diagram](<data-flow diagram.png>)

- All snapshots are fetched before transaction records are changed. A failed fetch is not an empty snapshot.
- New records are inserted. Changed fields are updated and audited. Identical records are left unchanged.
- Missing active records aged zero to 24 hours are revoked and audited. Reappearing revoked records are reactivated.
- Repeated identical imports leave transaction state and audits unchanged, but create a new execution batch.
- Failed processing rolls back transaction changes, audits, and the successful batch summary before failure tracking runs separately. If the database is unavailable or the process crashes, a batch may remain Running.
- GET requests only read data. Angular integration and scheduling are not implemented in this flow.
