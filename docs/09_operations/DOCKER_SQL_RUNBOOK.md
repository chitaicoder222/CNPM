# Docker SQL Server Development Runbook

> **Document ID:** DOC-OPS-002 | **Purpose:** Thiết lập có kiểm soát SQL Server local cho VIVOICE bằng Docker Desktop mà không làm ảnh hưởng container hay dữ liệu của dự án khác.  
> **Version / Status:** 0.5.0 / SQL CONTAINER HEALTHY; FACTORY ALIGNED — MIGRATION NOT RUN | **Owner:** Operations Engineer  
> **Last updated:** 2026-10-26 | **Related:** [Local Setup](LOCAL_SETUP.md), [Database Design](../03_database/DATABASE_DESIGN.md), [Security](../06_security/SECURITY.md), [Test Plan](../07_testing/TEST_PLAN.md)

## Verified preflight

| Check | Result | Evidence |
|---|---|---|
| Host OS | Compatible | Windows 11 Home Single Language, build 26200 |
| Docker Desktop and daemon | Ready | Docker CLI and server report version 29.8.1 |
| WSL | Ready | Default WSL version is 2; docker-desktop is running |
| Virtualization | Ready | HypervisorPresent is True |
| Host capacity | Suitable for local development | 15.2 GB RAM and 230.7 GB free on C drive |
| Local SQL services | Not discovered | No MSSQL/SQL service or sqlservr process was found |
| Existing Docker state | Preserve | Several stopped, unrelated containers exist, including a SQL Server container from another project; VIVOICE must not start, remove, rename, attach to, or reuse them |

This preflight does not prove that another SQL instance cannot exist. It proves only that Docker Desktop is ready and a separate VIVOICE environment can be prepared safely.

## Verified VIVOICE SQL runtime

| Check | Result | Evidence boundary |
|---|---|---|
| Container | Running | `vivoice-sqlserver-dev` created only by the VIVOICE Compose project. |
| Image | Pinned SQL Server 2022 Developer | The configured tag and digest are the running container image. |
| Health | Healthy | Compose healthcheck finds the SQL Server ready log entry. |
| Host exposure | Loopback only | `127.0.0.1:14333` maps to SQL Server port 1433. |
| Data volume | Present | `vivoice_sqlserver_data` is mounted at `/var/opt/mssql`. |
| SQL connectivity | Verified | A container-local authenticated SQL query succeeds without exposing credentials. |

No EF migration, application startup, seed operation, reset, public network exposure or Cloudflare tunnel has been run. The application and EF design-time factory now use aligned private connection-string sources and the factory fails without `ConnectionStrings:Vivoice`; the next approved phase must configure that private value and explicitly approve migration.

## Proposed VIVOICE development topology

The proposed service is a new container named vivoice-sqlserver-dev. It uses the verified amd64 image tag mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04, with manifest digest sha256:ea73825f3d88a23c355ac2f9fdc6bd960fec90171c12c572109b36a558f77bb8. The project targets EF Core SQL Server on .NET 9, so SQL Server 2022 is a compatible development target.

| Setting | Planned value | Reason |
|---|---|---|
| Image | mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04 | Pinned, verified tag; no latest tag. |
| Edition | Developer | Local non-production development only. |
| Container port | 1433 | SQL Server default inside the Docker network. |
| Host bind | 127.0.0.1:14333 | Loopback only; avoids Internet exposure and reduces collision risk. |
| Volume | vivoice_sqlserver_data | Named, VIVOICE-specific persistent data; never deleted automatically. |
| Secret source | Untracked .env file | MSSQL_SA_PASSWORD only; no real secret in source, docs or logs. |
| App connection | User Secrets or environment variable | ConnectionStrings:Vivoice remains outside tracked files. |

The VIVOICE web host does not perform EF migrations, content seed, or Admin bootstrap during normal startup. A privileged operator runs the explicit `--initialize-database` command once at a time; normal web runtime uses an account without DDL permission. Add `--bootstrap-admin` only to create the first Admin from private configuration. Do not use the sa account for normal operation beyond the controlled local initialization phase.

## Files prepared before container creation

The project now contains:

- `docker-compose.yml` with only the VIVOICE SQL Server service, named volume, digest-pinned image and loopback bind.
- `.env.example` with exactly the `MSSQL_SA_PASSWORD` placeholder; `.env` is ignored by Git.
- The operator-created `.env` remains untracked; its contents are never read into reports or source control. The container, named volume and local image now exist. No application connection string, migration, seed or database reset has been run.

The environment template contains no MySQL, JWT or Admin variables. Those legacy names are not referenced by the VIVOICE container or application. The SQL Server password is the only Docker `.env` setting; application connection and bootstrap-admin settings belong in User Secrets or a private application environment, not the Docker `.env` file.

`docker compose --file docker-compose.yml config --no-interpolate` passed. That validates Compose syntax and confirms the requested host bind is `127.0.0.1:14333`; it neither resolves the secret nor creates any Docker state. The configured healthcheck is a readiness design and is not runtime evidence until an approved container runs.

The compose configuration will not use the existing unrelated image/container. It will not publish 1433 to all interfaces, delete volumes, run database resets, create a Cloudflare tunnel, or create a public network endpoint.

## Approval gate before creating state

Creating the container will pull an approximately multi-gigabyte image if it is not already cached, create the named volume, reserve host loopback port 14333, and accept the SQL Server Developer license. It does not require a Windows feature or BIOS change because Docker Desktop, WSL 2 and virtualization are already operating.

Before running Docker Compose, confirm all of the following:

1. Create the new VIVOICE-only container and named volume.
2. Use loopback host port 14333.
3. Supply a strong private SQL password in an untracked .env file.
4. Accept the Microsoft SQL Server Developer Edition license for local development.

## Verification sequence after approval

1. Run Docker Compose and confirm the new container is running without touching existing containers.
2. Check container logs for SQL Server readiness without echoing environment values.
3. Verify loopback connectivity with an existing client or a container-local tool.
4. Set only the VIVOICE development connection string through User Secrets/environment.
5. Compare the two migrations with Database Design, confirm a new approved development database, then run the explicit initializer once.
6. Verify tables, indexes, six seeded POIs and the first USD 0.10 rate. Use `--bootstrap-admin` only if the database has no Admin and the three private BootstrapAdmin keys are configured.
7. Start the ASP.NET Core host without initialization flags. Verify generic `/health`, database-aware `/ready`, public POI API, then complete browser and admin integration tests.

Microsoft references: [SQL Server container quickstart](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/quickstart-install-docker?preserve-view=true&view=sql-server-linux-ver16), [container configuration and named volumes](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-configure), and [environment variable guidance](https://learn.microsoft.com/en-us/sql/linux/configure/environment-variables?view=sql-server-ver17).
