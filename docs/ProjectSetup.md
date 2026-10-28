# Project Setup

## Prerequisites and tech stack

Install Git, the .NET 10 SDK, and Node.js 24 LTS with npm. The client declares npm 11.7.0 as its package manager. A separate SQLite server and a global Angular CLI installation are not needed.

| Area | Technologies |
| --- | --- |
| Backend | C#, ASP.NET Core 10 Web API, built-in OpenAPI, Swagger UI |
| Database | EF Core 10 Code-First, SQLite, migrations |
| Frontend | Angular 21, TypeScript, RxJS; generated SSR support uses Express |
| Backend testing | xUnit, Microsoft.NET.Test.Sdk, SQLite in-memory databases |
| Coverage | Coverlet collector and ReportGenerator |

Check the installed prerequisites:

```bash
git --version
dotnet --version
node --version
npm --version
```

## Clone the repository

```bash
git clone --branch main https://github.com/prateekrauniyar345/Transactions-Ingest.git
cd Transactions-Ingest
```

The commands below assume this repository layout:

```text
Transactions-Ingest/
├── README.md
├── client
│   ├── README.md
│   ├── angular.json
│   ├── package-lock.json
│   ├── package.json
│   ├── public
│   │   └── favicon.ico
│   ├── src
│   │   ├── app
│   │   │   ├── app.config.server.ts
│   │   │   ├── app.config.ts
│   │   │   ├── app.css
│   │   │   ├── app.html
│   │   │   ├── app.routes.server.ts
│   │   │   ├── app.routes.ts
│   │   │   ├── app.spec.ts
│   │   │   └── app.ts
│   │   ├── index.html
│   │   ├── main.server.ts
│   │   ├── main.ts
│   │   ├── server.ts
│   │   └── styles.css
│   ├── tsconfig.app.json
│   ├── tsconfig.json
│   └── tsconfig.spec.json
├── docs
│   ├── Coding_Exercise_3 1.pdf
│   ├── ProjectSetup.md
│   └── TimeTracked.md
└── server
    ├── TransactionsIngest.Api
    │   ├── Controllers
    │   │   ├── ImportController.cs
    │   │   └── TransactionsController.cs
    │   ├── DTOs
    │   │   ├── ImportResultDto.cs
    │   │   ├── TransactionDto.cs
    │   │   └── TransactionSourceDto.cs
    │   ├── Data
    │   │   ├── AppDbContext.cs
    │   │   ├── transactions.db
    │   │   ├── transactions.db-shm
    │   │   └── transactions.db-wal
    │   ├── Interfaces
    │   │   ├── ITransactionImportService.cs
    │   │   └── ITransactionSource.cs
    │   ├── Migrations
    │   │   ├── 20261027053139_InitialCreate.Designer.cs
    │   │   ├── 20261027053139_InitialCreate.cs
    │   │   ├── 20261027053722_AddTransactionDetails.Designer.cs
    │   │   ├── 20261027053722_AddTransactionDetails.cs
    │   │   ├── 20261027060138_AddTransactionAudits.Designer.cs
    │   │   ├── 20261027060138_AddTransactionAudits.cs
    │   │   ├── 20261027164446_AddImportBatches.Designer.cs
    │   │   ├── 20261027164446_AddImportBatches.cs
    │   │   ├── 20261027181146_AddImportBatchUnchangedCount.Designer.cs
    │   │   ├── 20261027181146_AddImportBatchUnchangedCount.cs
    │   │   └── AppDbContextModelSnapshot.cs
    │   ├── Models
    │   │   ├── ImportBatch.cs
    │   │   ├── Transaction.cs
    │   │   ├── TransactionAudit.cs
    │   │   └── TransactionStatus.cs
    │   ├── Program.cs
    │   ├── Properties
    │   │   └── launchSettings.json
    │   ├── Services
    │   │   ├── Sources
    │   │   │   ├── DowntownSource.cs
    │   │   │   ├── OnlineSource.cs
    │   │   │   └── UptownSource.cs
    │   │   └── TransactionImportService.cs
    │   ├── TransactionsIngest.Api.csproj
    │   ├── TransactionsIngest.Api.http
    │   ├── appsettings.Development.json
    │   └── appsettings.json
    ├── TransactionsIngest.Tests
    │   ├── Fakes
    │   │   └── FakeTransactionSource.cs
    │   ├── TransactionImportServiceTests.cs
    │   └── TransactionsIngest.Tests.csproj
    ├── TransactionsIngest.slnx
    └── dotnet-tools.json
```


## Backend setup and packages

From the repository root, enter `server`. Run all backend, migration, test, and coverage commands from this folder:

```bash
cd server
dotnet restore TransactionsIngest.slnx
dotnet tool restore
dotnet build TransactionsIngest.slnx
```

`dotnet restore` installs all packages already declared in the API and test project files. `dotnet tool restore` installs the local EF CLI and ReportGenerator versions from `dotnet-tools.json`.

The API packages are:

- `Microsoft.AspNetCore.OpenApi`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `Swashbuckle.AspNetCore.SwaggerUI`

The test packages are:

- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`
- `coverlet.collector`

For reference, these commands add the packages to a project being set up from scratch. They are **not required after cloning**, because the package references are already committed:

```bash
dotnet add TransactionsIngest.Api/TransactionsIngest.Api.csproj package Microsoft.AspNetCore.OpenApi --version 10.0.13
dotnet add TransactionsIngest.Api/TransactionsIngest.Api.csproj package Microsoft.EntityFrameworkCore.Sqlite --version "10.0.*"
dotnet add TransactionsIngest.Api/TransactionsIngest.Api.csproj package Microsoft.EntityFrameworkCore.Design --version "10.0.*"
dotnet add TransactionsIngest.Api/TransactionsIngest.Api.csproj package Swashbuckle.AspNetCore.SwaggerUI --version 10.3.1

dotnet add TransactionsIngest.Tests/TransactionsIngest.Tests.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.14
dotnet add TransactionsIngest.Tests/TransactionsIngest.Tests.csproj package Microsoft.NET.Test.Sdk --version 17.14.1
dotnet add TransactionsIngest.Tests/TransactionsIngest.Tests.csproj package xunit --version 2.9.3
dotnet add TransactionsIngest.Tests/TransactionsIngest.Tests.csproj package xunit.runner.visualstudio --version 3.1.4
dotnet add TransactionsIngest.Tests/TransactionsIngest.Tests.csproj package coverlet.collector --version 6.0.4
```

### Create the database and run the API

Still in `server`, apply the committed migrations:

```bash
dotnet tool run dotnet-ef database update --project TransactionsIngest.Api
```

Configuration is in `TransactionsIngest.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=Data/transactions.db"
}
```

With these launch commands, the database is created at `server/TransactionsIngest.Api/Data/transactions.db`. It starts empty on a fresh clone. SQLite files are ignored by Git; migrations are committed.

Start the API:

```bash
dotnet run --project TransactionsIngest.Api --launch-profile http --urls http://localhost:5052
```

- API status: http://localhost:5052/
- Swagger UI: http://localhost:5052/swagger
- OpenAPI document: http://localhost:5052/openapi/v1.json
- `POST /api/import`: import the current mock snapshots.
- `GET /api/transactions`: retrieve active and revoked transactions.

Use Swagger's **Try it out → Execute** to call an endpoint. Keep the terminal running; press `Ctrl+C` to stop. Port 5052 avoids the port 5000 conflict observed with macOS Control Center. The `http` launch profile enables the Development environment, where Swagger is available.

When changing a database model later, generate a new migration and apply it:

```bash
dotnet tool run dotnet-ef migrations add DescribeYourSchemaChange --project TransactionsIngest.Api
dotnet tool run dotnet-ef database update --project TransactionsIngest.Api
```

Do not regenerate the initial migration when setting up an existing checkout.


### Run backend automated tests

In a terminal inside `server`:

```bash
dotnet test TransactionsIngest.slnx
```

To run only the test project:

```bash
dotnet test TransactionsIngest.Tests/TransactionsIngest.Tests.csproj
```

The xUnit suite covers transaction insertion, updates, audit history, idempotency, revocation, reactivation, shared IDs across sources, batch counts, source failures, rollback after SQL writes, and the 24-hour boundary. Each test applies the migrations to an isolated in-memory SQLite database. The API does not need to be running, and tests do not use the development database.

### Generate and view backend coverage

From `server`, run:

```bash
dotnet test TransactionsIngest.slnx --collect:"XPlat Code Coverage"
```

The XML coverage file is written to:

```text
TransactionsIngest.Tests/TestResults/<run-id>/coverage.cobertura.xml
```

Convert it to HTML using the restored local ReportGenerator tool:

```bash
dotnet tool run reportgenerator \
  "-reports:TransactionsIngest.Tests/TestResults/**/coverage.cobertura.xml" \
  "-targetdir:coverage-report" \
  "-reporttypes:Html"
```

The wildcard combines matching coverage files from all saved runs. To inspect only one run, replace the report pattern with that run's exact XML path.

Open the HTML report on macOS:

```bash
open coverage-report/index.html
```

On Windows, use `start coverage-report/index.html`; on Linux, use `xdg-open coverage-report/index.html`. You can also open the file directly in a browser.

Click a class to inspect covered and uncovered lines and branches. Generated OpenAPI code is included in the default totals, so review the import service's coverage separately. Generated-source warnings from ReportGenerator may appear while the HTML report is still produced successfully.

`TestResults` and `server/coverage-report` are ignored by Git. If setting up the tools in a new project without the committed manifest entries, install them locally from `server`:

```bash
dotnet tool install dotnet-ef --version 10.0.14
dotnet tool install dotnet-reportgenerator-globaltool --version 5.5.11
```


## Frontend setup and packages

Open a second terminal at the repository root, then run all frontend commands from `client`:

```bash
cd client
npm ci
npm start
```

Open http://localhost:4200. Stop the development server with `Ctrl+C`.

`npm ci` installs both runtime and development packages from `package-lock.json`:

| Group | Packages |
| --- | --- |
| Angular | `@angular/common`, `@angular/compiler`, `@angular/core`, `@angular/forms`, `@angular/platform-browser`, `@angular/platform-server`, `@angular/router`, `@angular/ssr` |
| Runtime | `express`, `rxjs`, `tslib` |
| Build and tooling | `@angular/build`, `@angular/cli`, `@angular/compiler-cli`, `typescript`, `prettier` |
| Testing and types | `vitest`, `jsdom`, `@types/express`, `@types/node` |

There is no need to install these individually. Versions are declared in `package.json` and resolved in `package-lock.json`. The frontend displays a basic transaction table loaded from `GET /api/transactions`. Its Refresh button reloads backend data. Keep both servers running. `client/proxy.conf.json` forwards `/api/**` requests from the Angular development server to `http://127.0.0.1:5052`, so browser requests use the frontend origin and do not require a CORS policy. Restart `npm start` after changing proxy settings. This proxy applies to `ng serve`; a deployed frontend will need its host to forward `/api` to the backend.

To build the client or run its current tests, still from `client`:

```bash
npm run build
npm test -- --watch=false
```


## Run both applications with Docker

Install and start Docker Desktop (or Docker Engine with Compose v2). From the repository root:

```bash
./run.sh
```

The script builds both images, starts the services, waits for their health checks, and prints the actual URLs. Defaults:

- Frontend: http://localhost:4200
- Backend: http://localhost:5052
- Swagger: http://localhost:5052/swagger

Stop any native development servers using those ports first. To choose different ports:

```bash
FRONTEND_PORT=4201 BACKEND_PORT=5053 ./run.sh
```

The equivalent Compose command is:

```bash
docker compose up --build --detach --wait
```

The backend automatically applies committed migrations when started with the Docker configuration. Its database is stored in a named Docker volume, separate from the local `Data/transactions.db`. A fresh volume has no imported transactions: execute `POST /api/import` through Swagger, then refresh the frontend. Mock source timestamps are fixed, so their age affects the 24-hour revocation rule.

The frontend image serves the built Angular browser files through Nginx and forwards `/api` to the backend container. It does not use the Angular development server or run the generated Node SSR server. Compose uses the Development backend environment to make Swagger available for local evaluation.

```bash
docker compose logs -f
docker compose down
```

Stopping with `down` preserves the database volume. `docker compose down --volumes` deletes that Docker database; use it only when intentionally resetting demo data.

Individual images can also be built and run from the repository root:

```bash
docker build -t transactions-backend ./server
docker build -t transactions-frontend ./client
docker network create transactions-demo
docker volume create transactions-demo-data

docker run -d --name backend --network transactions-demo \
  -p 127.0.0.1:5052:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -v transactions-demo-data:/data transactions-backend

docker run -d --name frontend --network transactions-demo \
  -p 127.0.0.1:4200:8080 \
  -e BACKEND_URL=http://backend:8080 transactions-frontend
```

These individual commands are an alternative to Compose; do not run both setups on the same ports. Compose is recommended because it also manages startup readiness. Container images restore their own build dependencies, so host .NET and Node installations are only needed for the native development steps below.


