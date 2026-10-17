# E1–E4 experiment record

This file is a reproducible evidence template. No result is considered PASS until the command is run and its output is attached.

| Experiment | Command/scenario | Result | Evidence | Status |
|---|---|---|---|---|
| E1 translation/TTS | Create a place, submit Vietnamese text, poll parent and TTS child | Pending environment with edge-tts | API output + MP3 checksum | BLOCKED locally |
| E2 cache/retry | Repeat identical request; force provider failure; retry failed job | Not run | Job rows and timings | NOT RUN |
| E3 quality/latency | Fixed sample set across supported target languages | Not run | CSV/logs | NOT RUN |
| E4 load | 10/50/100 concurrent requests and 20 jobs, within available resources | Not run | load-test report | NOT RUN |

Record machine, SDK, provider versions, sample count, elapsed time, failures, and limitations for every run.
