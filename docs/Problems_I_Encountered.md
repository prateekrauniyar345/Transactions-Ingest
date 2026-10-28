# Problems Encountered and How I Solved Them

During the development of the Transactions Ingest application, I encountered a few technical challenges. Below are four of the main problems and how I worked through them.

### 1. Database Setup and EF Core Migration Issues

**Problem:** While setting up SQLite with Entity Framework Core, I encountered issues with database migrations. Some migrations were empty, and the database was not initially being created in the expected location.

**How I solved it:** I reviewed the EF Core configuration, checked the migration files, and corrected the database location. I then regenerated the initial migration and verified that the tables and relationships were created correctly in SQLite.

**Technical change:** I set the connection string to `Data Source=Data/transactions.db` and ran EF commands against the API project from `server/`. I recreated the initial migration while the database was still empty. For later changes, such as adding `UnchangedCount`, I added a new migration instead of resetting the database. This kept the schema history and existing records intact.

I also checked that each model was registered in `AppDbContext`, because EF uses that configuration to decide which tables and relationships to create. After applying migrations, I inspected the actual SQLite tables and migration history. This helped distinguish a successful build from a database that had actually been updated.

### 2. Handling Transaction Updates and Revocations

**Problem:** One of the challenging parts was figuring out how to correctly identify new, updated, and missing transactions. I also needed to make sure repeated imports would not create duplicate transactions or unnecessary audit records.

**How I solved it:** I implemented comparison logic using the Source System and Transaction ID together. I tested the process by changing transaction amounts, removing transactions from mock source data, and running imports multiple times. I used Swagger and SQLite queries to confirm that updates, revocations, and audit records worked as expected.

**Technical change:** I added a unique index on `(SourceSystem, TransactionId)` and compared existing fields before saving updates. Only changed fields create audit entries. Missing active transactions are revoked only when their age is between zero and 24 hours. Repeating the same snapshot therefore leaves transaction state and audit history unchanged, while still recording a new import execution.

For example, changing Uptown transaction `100001` from `24.99` to `33.99` produced one Amount audit with both values. Repeating the import kept that audit count unchanged. Removing transaction `100002` from the source snapshot changed its status to Revoked without deleting its database row. Keeping the row preserves its history and allows it to be reactivated if the source publishes it again.

### 3. Handling Import Failures Without Losing Data

**Problem:** I needed to make sure that if one of the source systems failed during an import, the application would not accidentally change existing transactions or record incorrect results.

**How I solved it:** I added exception handling and database transactions to the import service. I also added import batch tracking to record whether an execution succeeded or failed. To verify the behavior, I simulated a source failure and checked that the API returned an error, the import batch was marked as Failed, and the existing transactions and audit records remained unchanged.

**Technical change:** I fetch all source snapshots before changing transaction records, so a failed request cannot be mistaken for an empty snapshot. Transaction changes, audits, and the successful batch summary are saved together. On processing failure, that work is rolled back and pending EF changes are cleared before saving the Failed batch separately. An automated test also forces a failure after SQL writes to verify that the saved amount and audit history are restored correctly.

The Running batch is created before fetching data, so even a source-fetch failure can be recorded. After rollback, failure tracking uses a separate, short cancellation timeout so it can still attempt to save the result when the original request was cancelled. Failed batches store zero committed counts. If saving the failure also fails, the service logs that problem and preserves the original exception instead of hiding the cause of the import failure.

### 4. Understanding and Improving Automated Test Coverage

**Problem:** After implementing automated tests, I initially found it difficult to understand the code coverage report. The overall coverage percentage was lower than expected, even though most of the transaction import logic was being tested.

**How I solved it:** I used Coverlet and ReportGenerator to generate an HTML coverage report. After reviewing the results, I found that generated OpenAPI code and other untested application components were affecting the overall percentage. I focused on the core transaction import service, which reached 93.8% line coverage and 92.1% branch coverage in the recorded report. I also ran the automated test suite and confirmed that all 20 tests passed.

**Technical change:** Each backend test uses a fresh in-memory SQLite database with the real migrations, so tests exercise actual database constraints without touching development data. I also introduced an injectable `TimeProvider` to freeze time in boundary tests. This made it possible to test exactly 24 hours, one second beyond it, and future timestamps reliably. I reviewed service coverage separately from generated code rather than treating the overall percentage as proof of correctness.

Fake source implementations let tests return a controlled snapshot or throw an exception without calling an external API. For the rollback test, an EF save interceptor deliberately throws after database writes but before commit; the test then rereads the database to confirm the original amount remains and no new audit was saved. This checks a different failure path from simply making a source request fail before processing starts.

---

These challenges helped me better understand database migrations, transaction reconciliation, error handling, and automated testing. I also used AI assistance to troubleshoot issues, explore possible solutions, and review implementation approaches, while verifying the results through builds, API requests, database queries, and tests.
