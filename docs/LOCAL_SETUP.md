# VIVOICE local setup

On a new Windows machine:

1. Install the .NET 9 SDK and Docker Desktop.
2. Copy `.env.example` to `.env` and set `MSSQL_SA_PASSWORD` to a strong local-only password.
3. Run `start-VIVOICE-Demo/Start-VIVOICE-Demo.bat`.

The launcher loads `.env` only into its own process, starts the pinned SQL Server container, waits for its health check, creates the database schema and seeds the baseline idempotently, then starts the web host. The password is never committed to Git and User Secrets are not required for the launcher path.

To stop a launcher-owned web process, use `start-VIVOICE-Demo/Stop-VIVOICE-Demo.bat`. The SQL Server container is intentionally left available for the next local run.
