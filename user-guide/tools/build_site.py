#!/usr/bin/env python3
"""Build the offline-friendly HTML chapters and local search index."""

from __future__ import annotations

import html
import json
import re
import unicodedata
from dataclasses import dataclass
from pathlib import Path
from urllib.parse import urlsplit


ROOT = Path(__file__).resolve().parents[1]
CONTENT = ROOT / "content"
CHAPTERS = ROOT / "chapters"
SEARCH_INDEX = ROOT / "assets" / "js" / "search-index.js"
INLINE_PATTERN = re.compile(
    r"(!\[[^\]]*\]\([^)]+\)|\[[^\]]+\]\([^)]+\)|\*\*.+?\*\*|`.+?`|\*.+?\*)"
)
HEADING_PATTERN = re.compile(r"^(#{1,6})\s+(.+?)\s*#*\s*$")
LIST_PATTERN = re.compile(r"^\s*(?:\d+\.\s+|[-*]\s+)")
TABLE_SEPARATOR_PATTERN = re.compile(r"^\|?\s*:?-{3,}:?\s*(?:\|\s*:?-{3,}:?\s*)+\|?$")


@dataclass
class Chapter:
    title: str
    order: int
    slug: str
    filename: str
    roles: list[str]
    markdown: str
    body: str = ""
    headings: list[tuple[str, str]] | None = None


def parse_chapter(path: Path) -> Chapter:
    text = path.read_text(encoding="utf-8-sig")
    lines = text.replace("\r\n", "\n").splitlines()
    if not lines or lines[0].strip() != "---":
        raise ValueError(f"Missing front matter in {path}")

    try:
        end = next(i for i in range(1, len(lines)) if lines[i].strip() == "---")
    except StopIteration as error:
        raise ValueError(f"Unclosed front matter in {path}") from error

    metadata: dict[str, str] = {}
    for line in lines[1:end]:
        if ":" in line:
            key, value = line.split(":", 1)
            metadata[key.strip()] = value.strip()

    title = metadata.get("title", "").strip("\"'")
    order = int(metadata.get("order", "0"))
    if not title or order < 1:
        raise ValueError(f"Invalid title or order in {path}")

    roles_value = metadata.get("roles", "").strip()
    roles = [role.strip().strip("\"'") for role in roles_value.strip("[]").split(",") if role.strip()]
    filename = path.stem
    slug = re.sub(r"^\d+-", "", filename)
    if not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", slug):
        raise ValueError(f"Invalid chapter filename: {path.name}")

    return Chapter(title, order, slug, f"{filename}.html", roles, "\n".join(lines[end + 1 :]).strip())


def heading_id(value: str) -> str:
    value = unicodedata.normalize("NFKD", value).encode("ascii", "ignore").decode("ascii")
    value = re.sub(r"[^\w\s-]", "", value.lower())
    return re.sub(r"[-\s]+", "-", value).strip("-")


def inline_markdown(value: str) -> str:
    output: list[str] = []
    cursor = 0
    for match in INLINE_PATTERN.finditer(value):
        output.append(html.escape(value[cursor : match.start()]))
        token = match.group(0)
        if token.startswith("!["):
            image = re.fullmatch(r"!\[([^\]]*)\]\(([^)]+)\)", token)
            alt, source = image.groups()
            output.append(
                f'<img src="{safe_url(source)}" alt="{html.escape(alt, quote=True)}" loading="lazy">'
            )
        elif token.startswith("["):
            link = re.fullmatch(r"\[([^\]]+)\]\(([^)]+)\)", token)
            label, href = link.groups()
            output.append(f'<a href="{safe_url(href)}">{html.escape(label)}</a>')
        elif token.startswith("**"):
            output.append(f"<strong>{html.escape(token[2:-2])}</strong>")
        elif token.startswith("`"):
            output.append(f"<code>{html.escape(token[1:-1])}</code>")
        else:
            output.append(f"<em>{html.escape(token[1:-1])}</em>")
        cursor = match.end()
    output.append(html.escape(value[cursor:]))
    return "".join(output)


def safe_url(value: str) -> str:
    value = value.strip()
    if not value or value.startswith("//"):
        return "#"
    scheme = urlsplit(value).scheme.lower()
    if scheme and scheme not in {"http", "https", "mailto"}:
        return "#"
    return html.escape(value, quote=True)


def table_cells(line: str) -> list[str]:
    return [cell.strip() for cell in line.strip().strip("|").split("|")]


def render_markdown(markdown: str) -> tuple[str, list[tuple[str, str]]]:
    lines = [line for line in markdown.splitlines() if not line.strip().startswith("<!--")]
    rendered: list[str] = []
    headings: list[tuple[str, str]] = []
    index = 0

    while index < len(lines):
        line = lines[index]
        if not line.strip():
            index += 1
            continue

        heading = HEADING_PATTERN.match(line)
        if heading:
            level = len(heading.group(1))
            label = heading.group(2).strip()
            identifier = heading_id(label)
            rendered.append(
                f'<h{level} id="{identifier}">{inline_markdown(label)}</h{level}>'
            )
            if level in (2, 3):
                headings.append((label, identifier))
            index += 1
            continue

        if line.lstrip().startswith("|") and index + 1 < len(lines) and TABLE_SEPARATOR_PATTERN.match(lines[index + 1]):
            headers = table_cells(line)
            index += 2
            rows: list[list[str]] = []
            while index < len(lines) and lines[index].lstrip().startswith("|"):
                rows.append(table_cells(lines[index]))
                index += 1
            rendered.append('<div class="table-scroll"><table><thead><tr>')
            rendered.extend(f"<th>{inline_markdown(cell)}</th>" for cell in headers)
            rendered.append("</tr></thead><tbody>")
            for row in rows:
                rendered.append("<tr>")
                rendered.extend(f"<td>{inline_markdown(cell)}</td>" for cell in row)
                rendered.append("</tr>")
            rendered.append("</tbody></table></div>")
            continue

        if LIST_PATTERN.match(line):
            ordered = bool(re.match(r"^\s*\d+\.\s+", line))
            tag = "ol" if ordered else "ul"
            rendered.append(f"<{tag}>")
            while index < len(lines) and LIST_PATTERN.match(lines[index]):
                item = re.sub(r"^\s*(?:\d+\.\s+|[-*]\s+)", "", lines[index]).strip()
                rendered.append(f"<li>{inline_markdown(item)}</li>")
                index += 1
            rendered.append(f"</{tag}>")
            continue

        if line.lstrip().startswith(">"):
            quote: list[str] = []
            while index < len(lines) and lines[index].lstrip().startswith(">"):
                quote.append(re.sub(r"^\s*>\s?", "", lines[index]))
                index += 1
            rendered.append(f"<blockquote><p>{inline_markdown(' '.join(quote))}</p></blockquote>")
            continue

        paragraph = [line.strip()]
        index += 1
        while index < len(lines):
            following = lines[index]
            if not following.strip() or HEADING_PATTERN.match(following) or LIST_PATTERN.match(following):
                break
            if following.lstrip().startswith("|") or following.lstrip().startswith(">"):
                break
            paragraph.append(following.strip())
            index += 1
        text = " ".join(paragraph)
        if text.startswith("Figure:"):
            rendered.append(f'<p class="figure-caption">{inline_markdown(text)}</p>')
        else:
            rendered.append(f"<p>{inline_markdown(text)}</p>")

    return "\n".join(rendered), headings


def page_shell(chapter: Chapter, chapters: list[Chapter], body: str, headings: list[tuple[str, str]]) -> str:
    active = chapter.filename
    previous = next((item for i, item in enumerate(chapters) if item.filename == active and i > 0), None)
    next_chapter = next(
        (chapters[i + 1] for i, item in enumerate(chapters[:-1]) if item.filename == active),
        None,
    )
    sidebar = "\n".join(
        f'<li><a class="chapter-link{" active" if item.filename == active else ""}" '
        f'href="{html.escape(item.filename, quote=True)}"><span>{item.order:02}</span> '
        f'{html.escape(item.title)}</a></li>'
        for item in chapters
    )
    toc = ""
    if headings:
        toc = '<nav class="page-toc" aria-label="On this page"><strong>On this page</strong><ol>'
        toc += "".join(
            f'<li><a href="#{html.escape(identifier, quote=True)}">{html.escape(title)}</a></li>'
            for title, identifier in headings
        )
        toc += "</ol></nav>"

    role_note = ""
    if chapter.roles:
        role_note = '<p class="chapter-audience">For: ' + ", ".join(map(html.escape, chapter.roles)) + "</p>"
    previous_link = (
        f'<a rel="prev" href="{html.escape(previous.filename, quote=True)}">← Previous: {html.escape(previous.title)}</a>'
        if previous
        else ""
    )
    next_link = (
        f'<a rel="next" href="{html.escape(next_chapter.filename, quote=True)}">Next: {html.escape(next_chapter.title)} →</a>'
        if next_chapter
        else ""
    )

    return f"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="description" content="{html.escape(chapter.title)} — NLI-POS User Guide">
  <title>{html.escape(chapter.title)} · NLI-POS User Guide</title>
  <link rel="stylesheet" href="../assets/css/site.css">
  <script defer src="../assets/js/search-index.js"></script>
  <script defer src="../assets/js/site.js"></script>
</head>
<body data-chapter-prefix="./">
  <a class="skip-link" href="#main">Skip to chapter</a>
  <header class="mobile-header">
    <a class="brand" href="../index.html">NLI-POS <span>User Guide</span></a>
    <button class="menu-toggle" type="button" aria-expanded="false" aria-controls="sidebar">Chapters</button>
  </header>
  <div class="layout">
    <aside class="sidebar" id="sidebar" aria-label="Guide navigation">
      <a class="brand" href="../index.html">NLI-POS <span>User Guide</span></a>
      <label class="search-label" for="guide-search">Search this guide</label>
      <input id="guide-search" type="search" placeholder="Search topics…" autocomplete="off">
      <div id="search-results" class="search-results" aria-live="polite"></div>
      <nav aria-label="Chapters"><ol class="chapter-list">{sidebar}</ol></nav>
      <a class="home-link" href="../index.html">Guide home</a>
    </aside>
    <main id="main" class="main">
      <div class="article-grid">{toc}
        <article class="chapter">
          <div class="chapter-meta">Chapter {chapter.order:02}</div>
          {role_note}
          {body}
          <nav class="chapter-nav" aria-label="Previous and next chapter">{previous_link}{next_link}</nav>
        </article>
      </div>
    </main>
  </div>
  <div class="image-viewer" role="dialog" aria-modal="true" aria-label="Expanded screenshot" hidden>
    <button class="image-viewer-close" type="button" aria-label="Close expanded screenshot">×</button>
    <img alt="">
  </div>
</body>
</html>
"""


def index_page(chapters: list[Chapter]) -> str:
    cards = "\n".join(
        f'<a class="chapter-card" href="chapters/{html.escape(item.filename, quote=True)}">'
        f'<span>Chapter {item.order:02}</span><strong>{html.escape(item.title)}</strong>'
        f'<span class="card-arrow" aria-hidden="true">→</span></a>'
        for item in chapters
    )
    sidebar = "\n".join(
        f'<li><a class="chapter-link" href="chapters/{html.escape(item.filename, quote=True)}">'
        f'<span>{item.order:02}</span> {html.escape(item.title)}</a></li>'
        for item in chapters
    )
    return f"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="description" content="Standalone user guide for the NLI-POS application">
  <title>NLI-POS User Guide</title>
  <link rel="stylesheet" href="assets/css/site.css">
  <script defer src="assets/js/search-index.js"></script>
  <script defer src="assets/js/site.js"></script>
</head>
<body data-chapter-prefix="chapters/">
  <a class="skip-link" href="#main">Skip to guide chapters</a>
  <header class="mobile-header">
    <a class="brand" href="index.html">NLI-POS <span>User Guide</span></a>
    <button class="menu-toggle" type="button" aria-expanded="false" aria-controls="sidebar">Chapters</button>
  </header>
  <div class="layout">
    <aside class="sidebar" id="sidebar" aria-label="Guide navigation">
      <a class="brand" href="index.html">NLI-POS <span>User Guide</span></a>
      <label class="search-label" for="guide-search">Search this guide</label>
      <input id="guide-search" type="search" placeholder="Search topics…" autocomplete="off">
      <div id="search-results" class="search-results" aria-live="polite"></div>
      <nav aria-label="Chapters"><ol class="chapter-list">{sidebar}</ol></nav>
    </aside>
    <main id="main" class="main home-main">
      <section class="welcome">
        <p class="eyebrow">NobleLife International</p>
        <h1>NLI-POS User Guide</h1>
        <p>Choose a chapter to learn about sales, customers, products, inventory, reports, and account settings. This guide works offline in your browser.</p>
      </section>
      <section class="chapter-cards" aria-label="Guide chapters">{cards}</section>
      <footer class="site-footer">Standalone guide · No POS sign-in required</footer>
    </main>
  </div>
</body>
</html>
"""


def main() -> None:
    if not CONTENT.is_dir():
        raise SystemExit(f"Chapter source directory not found: {CONTENT}")
    chapters = sorted((parse_chapter(path) for path in CONTENT.glob("*.md")), key=lambda item: (item.order, item.title.lower()))
    if not chapters:
        raise SystemExit("No Markdown chapters found.")
    if len({chapter.order for chapter in chapters}) != len(chapters):
        raise SystemExit("Chapter order values must be unique.")

    CHAPTERS.mkdir(parents=True, exist_ok=True)
    index: list[dict[str, object]] = []
    for chapter in chapters:
        chapter.body, chapter.headings = render_markdown(chapter.markdown)
        output = CHAPTERS / chapter.filename
        page = page_shell(chapter, chapters, chapter.body, chapter.headings)
        output.write_text(
            "\n".join(line.rstrip() for line in page.splitlines()) + "\n",
            encoding="utf-8",
        )
        plain = re.sub(r"!\[([^\]]*)\]\([^)]+\)", r"\1", chapter.markdown)
        plain = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", plain)
        plain = re.sub(r"<!--.*?-->", " ", plain, flags=re.DOTALL)
        plain = re.sub(r"[*`#|>-]", " ", plain)
        plain = re.sub(r"\s+", " ", plain).strip()
        index.append(
            {"title": chapter.title, "order": chapter.order, "filename": chapter.filename, "text": plain}
        )

    script = "window.POSGuideSearchIndex = " + json.dumps(index, ensure_ascii=False)
    script = script.replace("<", "\\u003c").replace(">", "\\u003e").replace("&", "\\u0026") + ";\n"
    SEARCH_INDEX.parent.mkdir(parents=True, exist_ok=True)
    SEARCH_INDEX.write_text(script, encoding="utf-8")
    index = index_page(chapters)
    (ROOT / "index.html").write_text(
        "\n".join(line.rstrip() for line in index.splitlines()) + "\n",
        encoding="utf-8",
    )
    print(f"Built index.html and {len(chapters)} chapter pages with a local search index.")


if __name__ == "__main__":
    main()
