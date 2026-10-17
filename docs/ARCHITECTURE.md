# Architecture

The API follows the intended flow:

`Controller -> Service -> Repository -> EF Core/MySQL`

External translation and speech providers are isolated behind provider interfaces. `JobWorker` claims pending jobs atomically, executes translation, creates a linked TTS child job, and copies the generated audio path back to the parent job. Audio is stored beneath the configured storage root and exposed through both static storage and `GET /jobs/{id}/audio`.

Authentication uses JWT. Self-registration creates only the `tourist` role; elevated `admin`/`staff` accounts must be provisioned by an operator. Place writes and language/job administration are role protected.

The database is recreated through the EF Core migrations under `src/TourGuide.Api/Migrations`. The startup seed is idempotent for languages and a demo place.
