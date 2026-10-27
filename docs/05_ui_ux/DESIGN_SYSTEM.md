# VIVOICE Design System

> **Document ID:** DOC-UX-001 | **Purpose:** Official palette, UI component and accessibility rules.  
> **Version / Status:** 0.2.0 / UI CORRECTION BASELINED | **Owner:** UI/UX Designer  
> **Last updated:** 2026-10-26 | **Related:** [PRD](../01_requirements/PRD.md), [Test Plan](../07_testing/TEST_PLAN.md)

## Immutable brand tokens

| Token | HEX | Approved role |
|---|---:|---|
| `--color-velvet-crimson` | `#450A0A` | Premium navbar, dark brand areas. |
| `--color-champagne-silk` | `#E7D9BF` | Light content/card background. |
| `--color-espresso-brown` | `#2C1B12` | Primary text, footer and dark supporting panels. |
| `--color-romance-red` | `#7A2116` | Primary action and selected state. |
| `--color-gold-highlight` | `#C6A15B` | Logo, borders and restrained ornamental accent. |

No component may scatter raw brand HEX values; all use these CSS variables plus named semantic tokens for focus, error, success, muted text and borders. The visual direction is Luxury Tourism — Elegant — Minimalist — Vietnamese Cultural Identity: generous whitespace, restrained gold, readable Unicode type, no neon/cyberpunk style and no excessive gradients/glow.

## Typography and encoding

All UI body text uses the local fallback stack Arial, Segoe UI, and sans-serif. Headings use the same stack with intentional weight, size, spacing, border and color treatment; no externally downloaded or decorative font is required. HTML declares UTF-8, source files are UTF-8, and API JSON is UTF-8. The supported Vietnamese review strings are VIVOICE, Hành trình Việt Nam đa ngôn ngữ, Chạm vào một địa điểm. Lắng nghe một câu chuyện., Khám phá, Sáu điểm đến nổi bật, Văn hóa đặc sắc, Ẩm thực nổi tiếng, and Điểm du lịch nổi bật.

## Public rules

Logo VIVOICE and language selector live in the public header. The header, footer and other public navigation never expose an Admin link. The direct admin route is intentionally not a public-navigation item and is protected by authentication/authorization rather than obscurity. The map is the primary exploration surface; below it, large POI cards expand to the three canonical categories. Selecting a topic is the user gesture that triggers text + auto TTS attempt. Controls may pause/resume/replay speech, but no redundant “Thuyết minh” button exists. Ads have a localized “Quảng cáo/Tài trợ” label and never obstruct content.

## Admin rules

Admin uses the same tokens with a clear sidebar, labeled fields, validation near inputs, destructive-action confirmation and tables that remain readable. Dashboard number cards must state the definition/period and render zero/empty state, not sample data.

## Accessibility and responsive rules

All controls have visible focus, keyboard operation and text labels. Color is never the only state cue. Text contrast is checked on its actual background; Champagne text is reserved for dark backgrounds. Desktop may show rails; tablet collapses them; mobile hides them and avoids horizontal overflow. Honor `prefers-reduced-motion`.
