# Transactions Ingest

University of Iowa take-home assignment: a transaction ingestion system for Store Uptown, Store Downtown, and Online. The system will import snapshots, detect changes and revoked transactions, maintain audit history, and display transactions in Angular.

## Tech stack

- ASP.NET Core 10 Web API
- Entity Framework Core 10 with SQLite
- Angular 21

## Current progress

The import pipeline, audit history, and batch tracking are implemented on the backend. The Angular client displays imported transactions.

- Imports read mock snapshots from three sources: Store Uptown, Store Downtown, and Online (`ITransactionSource`).
- Each source is reconciled separately. New transactions are inserted, changed fields are updated, previously revoked transactions that reappear are reactivated, and transactions missing from a snapshot are revoked if they are within 24 hours of their transaction time.
- Every update and revocation writes a row to `TransactionAudits`.
- Each import run is recorded in `ImportBatches` with status and counts. A failed fetch or database error rolls back the data changes and marks the batch as `Failed`.
- The Angular Transactions tab lists imported transactions. The Admin tab is a placeholder.
- xUnit tests cover the import service using SQLite in-memory connections.

### Current Tables

- **Transactions**
  - `Id` (PK)
  - `SourceSystem`
  - `TransactionId` (unique together with `SourceSystem`)
  - `CardNumber`
  - `LocationCode`
  - `ProductName`
  - `Amount`
  - `TransactionTime`
  - `Status` (`Active` or `Revoked`)
- **TransactionAudits**
  - `Id` (PK)
  - `TransactionRecordId` (FK to `Transactions.Id`)
  - `ChangeType` (`Updated` or `Revoked`)
  - `FieldName`, `OldValue`, `NewValue`
  - `ChangedAt`
- **ImportBatches**
  - `Id` (PK)
  - `StartedAt`, `CompletedAt`
  - `Status` (`Running`, `Completed`, or `Failed`)
  - `InsertedCount`, `UpdatedCount`, `RevokedCount`, `UnchangedCount`

### API endpoints

- `GET /` - health check
- `POST /api/import` - runs an import across all sources
- `GET /api/transactions` - returns all transactions, including revoked ones
- Swagger UI (Development only): http://localhost:5052/swagger/index.html

## Run locally

Prerequisites: .NET 10 SDK, Node.js compatible with Angular 21, and npm.

Backend, from the project root:

```bash
cd server
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef database update --project TransactionsIngest.Api
dotnet run --project TransactionsIngest.Api --urls http://localhost:5052
```

Frontend, in a separate terminal from the project root:

```bash
cd client
npm ci
npm start
```

API: http://localhost:5052
Frontend: http://localhost:4200 (proxies `/api` requests to the backend on port 5052)

To run an import, open Swagger, call `POST /api/import`, then click Refresh in the Angular app.

Tests, from the `server` folder:

```bash
dotnet test TransactionsIngest.slnx
```

## Documentation

- [Project setup](docs/ProjectSetup.md)
- [Design decisions](docs/DesignDecision.md)
- [Level-0 diagram](docs/Level-0Diagram.md) and [data flow diagram](docs/DataFlowDiagram.md)
- [Time tracking](docs/TimeTracked.md)

## AI usage disclosure

I used GitHub Copilot to assist with design, implementation, debugging, testing, and documentation.
See the [AI usage disclosure](docs/AiDisclosure.md) for details and the changes made to generated output.
