<!-- Purpose: Records the verified source-level coverage of the redesigned VIVOICE baseline narration. -->
<!-- Responsibility: Separates static evidence from database/API proof so editorial and deployment work is not overstated. -->

# VIVOICE content coverage report — 2026-10-28

## Scope and boundary

This is a source-level report for `VivoiceBaselineContent`, produced without running the initializer, connecting to a database, or changing existing VIVOICE content. It must not be read as proof that a particular deployed database contains these records.

## Verified static coverage

| Check | Result | Evidence |
| --- | --- | --- |
| Baseline POIs | 6 / 6 | `BaselineContentTests` asserts six source POIs. |
| Topic categories | 18 / 18 | Three categories per POI: `culture`, `food`, `tourism`. |
| POI translation records | 30 / 30 | Five language records for every POI. |
| Topic translation records | 90 / 90 | Five language records for every topic. |
| Required language keys | `vi`, `ko`, `ja`, `zh`, `th` | Static test checks every POI/topic key set. |
| Vietnamese narration length | 18 / 18 in 350–520 whitespace-delimited words | Static test guards the source target. |
| Duplicate Vietnamese narration | 0 exact duplicates | Static test requires 18 distinct source texts. |
| Existing Admin edits | Preserved by design | `DatabaseSeeder` only adds absent POIs/topics/translations. |

## Editorial status

The Vietnamese source is city- and category-specific. Korean, Japanese, Simplified Chinese and Thai entries are source-informed localized reference copy, but they have **not** had qualified native-language editorial review in this pass. They must not be represented as editorially certified or as length-equivalent to Vietnamese source narration.

## Database/API coverage

| Check | Status | Reason |
| --- | --- | --- |
| Fresh isolated seed | NOT RUN | This pass did not create or connect to a new isolated SQL database. |
| Idempotent rerun against revised baseline | NOT RUN | Requires the same fresh isolated database and explicit initializer command. |
| Public API actual-language/fallback report | NOT RUN | Requires a host connected to the isolated seeded database. |
| Existing VIVOICE database update | INTENTIONALLY NOT RUN | Add-only policy prevents silent overwrite of Admin-managed content. |

## Required next verification

Create a new, uniquely named isolated database under the controlled initializer workflow. Run the initializer twice, edit one seeded translation between runs, then capture API coverage by language and confirm: 6 active POIs, 18 active topics, 30 POI translations, 90 topic translations, zero missing required keys, explicit fallback only where a translation is deliberately removed, and no overwritten Admin edit. Native-language reviewers should then approve or revise the four localized language sets before production publication.
