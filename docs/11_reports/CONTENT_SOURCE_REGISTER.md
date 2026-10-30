<!-- Purpose: Records the factual anchors and editorial status for VIVOICE's baseline narration source. -->
<!-- Responsibility: Makes content provenance reviewable without storing credentials or mutable visitor data. -->

# VIVOICE baseline narration source register

## Status and editorial boundary

The baseline currently defines all 18 POI/topic combinations in Vietnamese and provides matching-language narration entries for Korean, Japanese, Simplified Chinese, and Thai. Vietnamese source narrations target roughly 350–500 whitespace-delimited words each so that browser narration is substantive without becoming a fixed itinerary. The entries are source-informed reference copy for the explicit database initializer, not a substitute for professional local-language editorial review. They deliberately omit volatile information such as ticket prices, opening hours, restaurant recommendations, seasonal events, and transport schedules.

`DatabaseSeeder` is add-only: an existing POI, topic, or translation is not overwritten by a later initializer run.  Updating already-published wording therefore remains an explicit Administrator editorial operation rather than an automatic seed change.

## Factual anchors

| POI | Cultural / visitor claims anchored in | Topics covered | Editorial status |
| --- | --- | --- | --- |
| Hà Nội | [UNESCO — Central Sector of the Imperial Citadel of Thang Long](https://whc.unesco.org/en/list/1328/); [Vietnam Tourism — Hanoi](https://vietnam.travel/places-to-go/northern-vietnam/ha-noi); [Vietnam Tourism — Hanoi cuisine](https://vietnam.travel/things-to-do/cuisine-gateway-hanoi%E2%80%99s-culture/) | long-lived capital, Old Quarter, phở, bún chả, coffee culture, heritage walking | Source-informed; local-language editorial review required |
| Thành phố Hồ Chí Minh | [Vietnam Tourism — Ho Chi Minh City](https://vietnam.travel/places-to-go/southern-vietnam/ho-chi-minh-city) | southern urban history, central civic architecture, bánh mì, cơm tấm, bánh xèo, walkable central landmarks | Source-informed; local-language editorial review required |
| Đà Nẵng | [Danang Fantasticity — culture and heritage](https://danangfantasticity.com/en/experiencing-rich-culture-and-heritage/experiencing-rich-culture-and-heritage); [Danang Fantasticity — Marble Mountains](https://danangfantasticity.com/en/culture-en/the-marble-mountains) | river/sea/mountain setting, Marble Mountains, local noodles, heritage-aware exploration | Source-informed; local-language editorial review required |
| Huế | [UNESCO — Complex of Hué Monuments](https://whc.unesco.org/en/list/678); [Vietnam Tourism — Huế](https://vietnam.travel/places-to-go/north-central-coast/hue); [Vietnam Tourism — Huế food](https://www.vietnam.travel/things-to-do/how-eat-local-hue) | Nguyễn-era capital, Citadel and Perfume River setting, refined culinary culture | Source-informed; local-language editorial review required |
| Hội An | [UNESCO — Hoi An Ancient Town](https://whc.unesco.org/en/list/948); [Vietnam Tourism — Hội An](https://vietnam.travel/node/99); [Vietnam Tourism — food in Hội An](https://beta-v2.vietnam.travel/things-to-do/explore-food-hoi-an) | fifteenth-to-nineteenth-century trading port, built heritage, Cao Lầu and white-rose dishes, river/lantern streets | Source-informed; local-language editorial review required |
| Nha Trang | [Vietnam Tourism — Nha Trang](https://vietnam.travel/places-to-go/south-central-coast-central-highlands/nha-trang/); [Khánh Hòa Tourism — Po Nagar Cham Towers](https://dulichso.khanhhoa.gov.vn/en/article/po-nagar-cham-towers-a-millennium-old-heritage-7f7) | coastal bay, islands, Long Sơn and Po Nagar context, respectful cultural visits | Source-informed; local-language editorial review required |

## Coverage review procedure

Before publishing an expanded baseline to a real database, an editor should verify each claim against the linked source, have the four non-Vietnamese narration versions reviewed by qualified native-language editors, and run the isolated-database seed coverage report.  The report must distinguish source-code coverage from actual database coverage, identify fallback responses, duplicates, missing translations, and unexpectedly short text.  The normal web startup never applies this content automatically.
