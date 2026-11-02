"""Purpose: combine rendered Word pages into labeled review sheets for visual QA."""

from pathlib import Path

from PIL import Image, ImageDraw


PROJECT_ROOT = Path(__file__).resolve().parents[1]
PAGE_ROOT = PROJECT_ROOT / "docs" / "WORD_RENDER" / "connection_factory_pages"
SHEET_ROOT = PROJECT_ROOT / "docs" / "WORD_RENDER" / "connection_factory_sheets"
THUMBNAIL_WIDTH = 420
GAP = 24
LABEL_HEIGHT = 28
PAGES_PER_SHEET = 6


def main() -> int:
    """Write review sheets with six legible page thumbnails each."""
    SHEET_ROOT.mkdir(parents=True, exist_ok=True)
    pages = sorted(PAGE_ROOT.glob("*.png"))
    for sheet_index in range(0, len(pages), PAGES_PER_SHEET):
        current_pages = pages[sheet_index : sheet_index + PAGES_PER_SHEET]
        thumbnails: list[tuple[Path, Image.Image]] = []
        for page_path in current_pages:
            page = Image.open(page_path).convert("RGB")
            height = round(page.height * THUMBNAIL_WIDTH / page.width)
            thumbnails.append((page_path, page.resize((THUMBNAIL_WIDTH, height))))
        row_heights = [max(image.height for _, image in thumbnails[row : row + 2]) for row in range(0, len(thumbnails), 2)]
        width = GAP + (THUMBNAIL_WIDTH + GAP) * 2
        height = GAP + sum(LABEL_HEIGHT + row_height + GAP for row_height in row_heights)
        sheet = Image.new("RGB", (width, height), "#E7D9BF")
        draw = ImageDraw.Draw(sheet)
        y = GAP
        for row_index, row_height in enumerate(row_heights):
            for column in range(2):
                item_index = row_index * 2 + column
                if item_index >= len(thumbnails):
                    break
                page_path, image = thumbnails[item_index]
                x = GAP + column * (THUMBNAIL_WIDTH + GAP)
                draw.text((x, y), page_path.stem, fill="#2C1B12")
                sheet.paste(image, (x, y + LABEL_HEIGHT))
            y += LABEL_HEIGHT + row_height + GAP
        target = SHEET_ROOT / f"word-qa-sheet-{sheet_index // PAGES_PER_SHEET + 1:02d}.png"
        sheet.save(target)
        print(target)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
