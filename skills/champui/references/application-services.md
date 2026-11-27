# Application services in an Interactive Server app

Define a use-case interface in the consuming application, such as a customer lookup or an appointment command. Inject it into the page. Give it typed input, a cancellation token and a result DTO. Keep EF entities, HttpResponseMessage and database contexts out of component parameters. The Starter provides a replaceable IDashboardData abstraction and fictional sample implementation. Its Getting Started page includes a typed REST client and service registration. [Source and examples](https://github.com/aqeelhashem2-maker/ChampUI/blob/3d4c98bfbc12b0d23dbd1266ecaefccd3a39e9ff/templates/ChampUI.Dashboard/Components/Pages/GettingStarted.razor)

## EF Core

Choose the database provider and matching EF Core 10 packages in the host application. Register AddDbContextFactory for the application context. Inject IDbContextFactory into the service and create and dispose a context per operation. A Blazor Server circuit can span multiple operations; sharing a tracking context across them can race. Start from the authorized tenant query, project read-only fields with AsNoTracking, allowlist the requested sort/filter fields, count filtered records, then apply stable ordering and page or range limits. Pass cancellation into async queries. Validate writes and handle concurrency conflicts before notifying the UI.

## REST APIs

Register a typed client with AddHttpClient and configure its HTTPS base URI in server configuration. Validate it at startup and keep credentials on the server. Propagate cancellation and distinguish an empty result from a failed request. Authorize each endpoint, including bulk export. When a newer search supersedes an older response, keep only the latest result. Surface a localized error or retry action through the page.

## Financial and scheduling operations

Display a returned decimal amount with the currency and culture from the business context. Keep ledger writes, refunds, payment authorization, duplicate-command protection, appointment availability and booking conflicts in the application service or backend. A chart, a grid and a date picker supply presentation. Core 0.3.3 does not supply a POS engine or an advanced appointment scheduler.

For an EF read path, test with rows from two tenants and two dates. Assert the exact projected rows and sort order, then pass a cancelled token. An in-memory provider can exercise this service contract. Validate SQL translation, constraints, concurrency and transactions against the application's real relational provider before relying on them in production.
