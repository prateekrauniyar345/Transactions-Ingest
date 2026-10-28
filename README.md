# Transactions Ingest

University of Iowa take-home assignment: a transaction ingestion system for Store Uptown, Store Downtown, and Online. The system will import snapshots, detect changes and revoked transactions, maintain audit history, and display transactions in Angular.

## Tech stack

- ASP.NET Core 10 Web API
- Entity Framework Core 10 with SQLite
- Angular 21

## Current progress

The backend and frontend are scaffolded. The `Transactions` table and migrations are implemented, with a unique index on `(SourceSystem, TransactionId)`.

### Current Tables

- **Transactions**
  - `Id` (PK)
  - `SourceSystem`
  - `TransactionId`
  - `CardNumber`
  - `LocationCode`
  - `ProductName`
  - `Amount`
  - `TransactionTime`
  - `Status`

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
Frontend: http://localhost:4200

## AI usage disclosure

I used VSCode GitHub Copilot to assisted me with design, implementation, debugging, testing, and documentation. 
See the [AI usage disclosure](docs/AIUsageDisclosure.md) and [prompt and design discussion log](docs/AIPrompts.md) for details, conventions, and changes made to generated output.
