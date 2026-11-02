# Bảo — E1/E3 evidence matrix

Updated: 2026-11-03

This record separates automated evidence from device evidence. It does not claim that a translation or a voice was reviewed by a native speaker when that review was not performed.

## E1 — catalog coverage and content checks

| Date | Evidence | Steps | Result | Status |
|---|---|---|---|---|
| 2026-11-03 | `BaselineContentTests` | Load the embedded baseline and verify supported languages, POI count, topic count, translation fields, source URLs, GPS radius and distinct Vietnamese narration | 11 POI, 33 topics, 55 POI translation records, 165 topic translation records, five language keys, required text fields and source URLs are covered by automated tests | Automated pass |

The tests validate data shape and coverage. Editorial/native-speaker acceptance of every translation remains a separate review task and is not marked complete here.

## E3 — browser narration and language fallback

| Date | Commit | Device/browser | Steps | Evidence/result | Status |
|---|---|---|---|---|---|
| 2026-11-03 | final branch commit | Node VM with mocked `SpeechSynthesis` | Exercise language normalization, exact/same-language voice matching, language changes, stale narration protection, cancellation, missing Speech API and narration errors | Frontend tests cover the client contract without pretending to be real audio playback | Automated pass; device listening pending |
| — | — | Physical device/browser | Play at least one narration in vi/ko/ja/zh/en; record available voice, actual voice, pause/resume/stop and fallback behavior | No device audio evidence was collected in this workspace | Not performed |

## Sequence and handoff

1. The GPS module decides when a POI enters or leaves the radius and cancels the current narration on exit.
2. `public.js` loads the selected narration and keeps the displayed text/language separate from UI language.
3. `public-narration.main.js` owns Web Speech API state: voice selection, start evidence, pause/resume/stop/replay, timeout and stale callback protection.
4. A missing voice or browser Speech API leaves the narration text visible and reports the playback limitation; it does not silently change the content language.
