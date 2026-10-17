# Reproducible E1–E4 runbook

Run from the repository root after configuring a test database and provider services. Save terminal output and timestamps; never fill values by estimation.

## E1 — end-to-end

1. `GET /health`, `GET /places`, `GET /languages/provider-check`.
2. `POST /places/{id}/translate` with a fixed Vietnamese sample and one provider-supported target.
3. Poll `GET /jobs/{id}` until the parent and `ttsStatus` reach a terminal state.
4. If successful, verify `GET /jobs/{id}/audio`, file size, SHA-256, and browser playback.

## E2 — cache and retry

Submit the identical request twice and compare job IDs/audio hashes. Configure a provider failure in a disposable environment, record retry timestamps and terminal `failed`, then use an admin token to call `POST /jobs/{id}/retry`.

## E3 — quality and latency

Use a versioned sample set with at least one short and one chunked text per supported language. Record provider response, elapsed milliseconds, chunk count, output length, and failures in CSV; do not infer translation quality without a defined reference set.

## E4 — load

Run 10/50/100 concurrent clients and 20 jobs only on an isolated test database/provider. Record machine/container limits, throughput, p50/p95 latency, error count, queue depth, and audio storage growth. Stop if external provider rate limits are reached.
