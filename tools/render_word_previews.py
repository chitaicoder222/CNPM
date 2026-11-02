"""Purpose: render every page of generated Word-preview PDFs to PNG for visual QA."""

from pathlib import Path

import pymupdf


PROJECT_ROOT = Path(__file__).resolve().parents[1]
PDF_ROOT = PROJECT_ROOT / "docs" / "WORD_RENDER" / "connection_factory_pdf"
IMAGE_ROOT = PROJECT_ROOT / "docs" / "WORD_RENDER" / "connection_factory_pages"


def main() -> int:
    """Render all PDF pages at legible resolution and report the page total."""
    IMAGE_ROOT.mkdir(parents=True, exist_ok=True)
    rendered = 0
    for pdf_path in sorted(PDF_ROOT.glob("*.pdf")):
        document = pymupdf.open(pdf_path)
        for page_number, page in enumerate(document, start=1):
            image_path = IMAGE_ROOT / f"{pdf_path.stem}-page-{page_number:02d}.png"
            page.get_pixmap(matrix=pymupdf.Matrix(1.4, 1.4), alpha=False).save(image_path)
            rendered += 1
        document.close()
    print(f"RENDERED_PAGES={rendered}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
