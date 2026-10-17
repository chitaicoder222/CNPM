# Staging/CD handoff

`cd-staging.yml` is manual-only. It builds the API image and publishes it to GHCR using the ephemeral `GITHUB_TOKEN`; it does not deploy to a host.

The staging owner must provide the environment's `MYSQL_*`, `JWT_SECRET`, provider URL, and deployment credentials through GitHub Environment Secrets. A deployment should pin an immutable image tag (commit SHA), run migrations before traffic switch, verify `/health` and a translation smoke test, then retain the previous tag for rollback. Rollback means redeploying that previous tag and checking the same health/smoke-test gates. No credentials or host-specific commands are stored in this repository.
