# AI Prompts and Design Discussion Log

## Purpose

This document summarizes how I used AI assistance throughout the development of the Transactions Ingest application.

I primarily used AI as a development assistant to clarify requirements, explore implementation approaches, troubleshoot technical issues, review code, and identify potential edge cases. AI also helped generate initial code examples, implementation scaffolding, tests, and documentation, which I reviewed and validated during development.

Rather than relying on a single prompt to generate the entire application, I followed an incremental development process. I broke the assignment into smaller tasks, implemented and tested individual components, examined the results, and used follow-up discussions to resolve problems or improve the implementation.

The prompts below are paraphrased for readability unless explicitly marked as original excerpts. They summarize the development conversations and are not intended to represent verbatim transcripts.

## 1. Requirements Analysis and Planning

| How I used AI | My involvement | Outcome |
|---|---|---|
| Asked AI to explain the assignment requirements and identify the main technical challenges. | I reviewed the requirements and organized development into smaller milestones. | Established a development roadmap covering database design, source integrations, reconciliation, APIs, testing, frontend, and documentation. |
| Discussed which components were essential and which features were optional. | I prioritized the required backend functionality over additional features and cross-verified with the requirements doc. | Kept the initial implementation focused on the assignment requirements. |
| Asked about project structure and separation of responsibilities. | Selected a layered structure using controllers, services, interfaces, DTOs, and EF Core. | Organized the application for maintainability and testability. |

## 2. Database Design and EF Core

| How I used AI | My involvement | Outcome |
|---|---|---|
| Discussed the appropriate schema for transactions and their audit history. | I reviewed the required fields, data types, keys, and relationships from the doc before implementing the models. | First designed the Transactions, TransactionAudits, and ImportBatches tables myself and then asked AI to identify gaps, mismatches, and a better schema model for this task. |
| Requested assistance troubleshooting migrations and database setup. | Inspected migration output and verified the resulting SQLite tables. | Resolved migration issues and confirmed the database schema. |
| Discussed whether source-specific import tracking required an additional table. | Chose to retain three tables for the initial implementation and defer additional source-level reporting. | Avoided unnecessary schema complexity while preserving room for extension. |

## 3. Mock Source Integration and Transaction Reconciliation

| How I used AI | My involvement | Outcome |
|---|---|---|
| Requested examples of interfaces and mock implementations for the three source systems. | Integrated the source classes and verified that they returned the expected transaction snapshots. | Implemented interchangeable source integrations using ITransactionSource. |
| Discussed how to identify new, updated, unchanged, and missing transactions. | Reviewed the reconciliation approach and tested each scenario through the API. | Implemented source-aware transaction reconciliation. |
| Asked how to record individual field changes rather than only indicating that a transaction was updated. | Inspected database audit records after modifying sample transaction values. | Verified field-level audit history with old and new values. |
| Discussed the 24-hour revocation requirement and missing transaction behavior. | Simulated missing records and verified transaction status and audit history in SQLite. | Implemented and manually verified revocation behavior. |
| Asked how to prevent duplicate processing during repeated imports. | Executed identical imports multiple times and compared database results. | Verified idempotency for the tested scenarios. |

## 4. Troubleshooting, Debugging, and Reliability

| Problem or question | How AI assisted | Verification or resulting action |
|---|---|---|
| EF Core migration issues | Explained possible causes and migration commands. | Inspected the generated schema and verified the database tables. |
| Database model and API inconsistencies | Helped identify potential validation and schema issues. | Reviewed and revised the affected implementation. |
| Repeated imports and duplicate audit concerns | Suggested idempotency checks and database queries. | Confirmed unchanged imports did not create duplicate transaction changes or audit entries. |
| Failed source-system requests | Explained the difference between a failed source retrieval and a successful empty snapshot. | Simulated an exception and verified the Failed import batch and unchanged transaction records. |
| Database rollback behavior | Discussed transaction boundaries, exception handling, and rollback testing. | Added test scenarios intended to verify atomic persistence and rollback. |
| Angular styling errors | Helped identify a CSS syntax issue introduced during formatting. | Corrected the stylesheet and verified the frontend. |

## 5. Automated Testing and Code Coverage

| How I used AI | My involvement | Outcome |
|---|---|---|
| Asked which automated tests were most important for transaction reconciliation. | Reviewed test scenarios and prioritized edge cases. | Developed xUnit tests for the import service. |
| Requested example test implementations using mock sources and SQLite. | Integrated the test setup, ran tests, and reviewed failures and results. | Tested transaction insertion, updates, auditing, revocation, reactivation, idempotency, and failure handling. |
| Asked how to test database rollback behavior. | Reviewed transaction handling and test results. | Added rollback-focused test coverage. |
| Asked how to generate and interpret code coverage reports. | Ran the coverage tools and inspected the HTML report. | Used Coverlet and ReportGenerator to identify covered and uncovered code. |

During development, one recorded test run completed with **20 passing tests, zero failures, and zero skipped tests**.

The recorded TransactionImportService coverage was **93.8% line coverage and 92.1% branch coverage**. These figures refer to that coverage run, not necessarily the final submission state.

## 6. Frontend Development and UI Improvements

| How I used AI | My involvement | Outcome |
|---|---|---|
| Consulted on resolving browser CORS policy blocking errors when connecting Angular (localhost:4200) to the ASP.NET Core API (localhost:5052). | Configured CORS policy middleware in ASP.NET Core (Program.cs) to allow the Angular origin, and implemented an Angular proxy.conf.json setup for local development. | Successfully bypassed CORS restrictions and enabled seamless data fetching between the Angular client and backend endpoints. |
| Discussed how to structure the transaction table and handle loading or request errors. | Reviewed the interface and tested the displayed data. | Added transaction display and request-state handling. |
| Requested UI ideas using a University of Iowa visual reference. | Reviewed the design and requested revisions. | Added university-inspired styling, a responsive layout, and navigation elements. |
| Asked about displaying local time without changing stored UTC timestamps. | Reviewed and tested the formatting behavior. | Kept backend timestamps in UTC while displaying local time in the UI. |

## 7. Design Discussions and Scope Decisions

AI was also used to evaluate alternative approaches before making implementation decisions.

**Database scope:** Discussed adding source-level import reporting but retained the three-table schema for the initial implementation.

**Architecture:** Used interfaces and dependency injection so source integrations could be changed without rewriting the reconciliation service.

**Scheduling:** Did not implement a background scheduler because the assignment explicitly excludes scheduling and job orchestration.

**Admin snapshot editor:** Explored an optional interface for editing source snapshots and demonstrating reconciliation interactively. This feature was deferred to future development rather than included in the initial required scope.

**Authentication:** Discussed a frontend-only password gate for the proposed Admin interface. Recognized that this would not provide actual API authentication or authorization. No production authentication mechanism is claimed.

## 8. Selected Original Prompt Excerpts

The following excerpts preserve examples of the original requests made during development.

> Read this instruction document and help me identify the core business problem and main requirements, and help me understand them clearly. 

> `[Database migration error message; ~50 lines]` Considering my database schema setup and models, can you help me fix this issue. 

> `[university of iowa take home assignment  % ng new client .      
Node.js version v25.4.0 detected.
Odd numbered Node.js versions will not enter LTS status and should not be used for production. For more information,please see https://nodejs.org/en/about/previous-releases/.]` Help me fix this command to create the Angular project in my current working directory.

> `[obj\Debug/net10.0/staticwebassets.development.json^Backslash ; MAC OS path separator mismatch bug in the .NET SDK's]` help me fix this path separator issue on macOS. 

> I have developed the Transaction model/schema for our database, and these are my other two tables. Based on my existing models, can you generate the schema/models for the `TransactionAudit` and `ImportBatch` tables? 

> I have implemented the tests like : `Import_NewTransaction_InsertsIntoDatabase`, `Import_ChangedAmount_UpdatesAndCreatesAudit`, `Import_MissingRecentTransaction_RevokesAndAudits`, `Import_SuccessfulExecution_SavesBatchCounts` and `Import_SameIdDifferentSources_InsertsBoth` can you help me identify some edge case tests that I should cover? 

> Based on my project, can you assist me in writing my documentation? I used it to help write documentation, generate READMEs, generate level-0 and data-flow diagrams, and fix grammar and typos in my documentation. 

> Based on the University of Iowa web pages' style, interface, and brands, can you help me generate the custom CSS for our frontend so that it matches the University of Iowa interface?  


These excerpts illustrate the use of AI for understanding requirements, planning, troubleshooting, implementation assistance, testing, and documentation.

## 9. AI-Generated Code and Review Process

AI assistance included code generation and revision for parts of the database configuration, source integrations, transaction import service, controllers, automated tests, frontend, and documentation.

The development process included:

1. Reviewing proposed implementations against the assignment requirements.
2. Applying and integrating code changes incrementally.
3. Building the application and resolving compilation or runtime errors.
4. Manually verifying transaction behavior using Swagger and SQLite queries.
5. Running automated tests and reviewing coverage results.
6. Revising implementations based on observed behavior and further design discussions.

The Git history and source code provide additional context for the implementation and subsequent revisions.

## 10. Disclosure and Supporting Documents

This document provides a categorized summary of AI-assisted development discussions. It does not claim to be a complete verbatim transcript of every prompt or response.
