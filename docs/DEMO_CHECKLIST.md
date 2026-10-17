# Demo checklist

## Local API

1. Start MySQL and LibreTranslate, or copy `.env.example` to `.env` and run `docker compose up --build`.
2. Configure `ConnectionStrings:Default` and `Jwt:Secret` with user-secrets or environment variables.
3. Open `http://localhost:5130/` (local) or `http://localhost:8080/` (Compose).
4. Confirm `/health`, select a seeded place and target language, submit Vietnamese text, and wait for `done` plus `audioPath`.
5. Play the MP3 in the page. If TTS fails, inspect the TTS child job and retry it with an admin token.

## Evidence to capture

- API health response and migration list.
- Translation parent job and linked TTS child (`parentJobId`).
- MP3 file under `storage/audio/` and HTTP playback response.
- `dotnet test` output and CI run URL.

## Docker verification commands

```powershell
Copy-Item .env.example .env
# edit .env and set MYSQL_* plus JWT_SECRET; never commit it
docker compose build api
docker compose up -d
docker compose ps
Invoke-WebRequest http://localhost:8080/health
```

After submitting a translation, verify the real artifact without trusting only the job state:

```powershell
Get-ChildItem .\storage\audio\*.mp3
Invoke-WebRequest http://localhost:8080/jobs/{id}/audio -OutFile .\storage\audio\verify.mp3
Get-Item .\storage\audio\verify.mp3 | Select-Object Length
```

The API runtime must use the same MySQL application password that was used when the existing database volume was initialized. Compose does not change that password when the volume already exists; provide the matching value through the process environment or a local secret manager without committing it. Do not remove the volume to work around a mismatch.
