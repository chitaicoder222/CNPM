# Purpose: one-click VIVOICE Demo launcher for Windows collaborators

This folder is the shared starting point after pulling the repository. Double-click [Start-VIVOICE-Demo.bat](Start-VIVOICE-Demo.bat) to start the local VIVOICE web host, wait for `/ready`, create a verified Cloudflare Quick Tunnel, and open the new public link in the default browser.

## What each member needs once, before the first launch

1. A complete repository checkout with the `src`, `tools`, and this folder intact.
2. The compatible .NET SDK, Docker Desktop, and `cloudflared` available on `PATH`.
3. The reviewed local SQL Server configuration for that machine: a private `.env` for the SQL Server container password, a private User Secret for `ConnectionStrings:Vivoice`, and the approved database initialization performed separately.
4. Internet access for Cloudflare Quick Tunnel.

No credential is stored in this folder or committed to Git. The launcher does not create, migrate, reset, seed, or delete a database or Docker resource.

## Daily use

1. Double-click `Start-VIVOICE-Demo.bat`.
2. Wait for the message `Demo is ready:`. The script opens the verified `https://…trycloudflare.com` address automatically and keeps the command window open so the URL remains visible.
3. Share the newly opened URL while the launcher-owned web host and `cloudflared` processes remain running.
4. When finished, double-click `Stop-VIVOICE-Demo.bat`.

## Important Cloudflare behavior

Cloudflare Quick Tunnel generates a new random `trycloudflare.com` address whenever a new tunnel is created. A previously shared address can show Error 1033 after the tunnel stops or reconnects; use the new URL printed by the launcher instead. The launcher validates the public home page and public POI API before reporting success.

## Safe failure behavior

If port `5078` is occupied by an unknown process, the local database is not ready, prerequisites are missing, or an unowned `cloudflared` process is already running, the launcher stops safely and explains the condition. It never kills an arbitrary process, exposes a secret, or changes database/Docker data.
