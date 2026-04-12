from __future__ import annotations

import argparse
import re
from pathlib import Path

import markdown


RECORD_ROWS = [
    ("0.1", "2026-04-05", "QA team", "Completed project audit, submission structure, and test plan baseline"),
    ("0.5", "2026-04-05", "QA team", "Added UI test cases, API test cases, test data, and automation design"),
    ("0.8", "2026-04-05", "QA team", "Added automation implementation, execution evidence, defect workflow, and metrics"),
    ("1.0", "2026-04-06", "Hoang Van Thien", "Consolidated final report content, analysis, and appendix linkage"),
    ("1.1", "2026-04-10", "QA team", "Expanded verified UI execution evidence, refreshed metrics, and synchronized the report with the updated test case baseline"),
    ("1.2", "2026-04-11", "QA team", "Removed UI Not Run status through real execution, refreshed defect log, metrics, report, and slides"),
]


def resolve_images(markdown_text: str, source_dir: Path) -> str:
    pattern = re.compile(r"!\[(.*?)\]\((.*?)\)")

    def repl(match: re.Match[str]) -> str:
        alt, rel_path = match.group(1), match.group(2).strip()
        abs_path = (source_dir / rel_path).resolve()
        return f"![{alt}]({abs_path.as_uri()})"

    return pattern.sub(repl, markdown_text)


def add_page_breaks_to_h1(html_text: str) -> str:
    parts = re.split(r"(<h1>.*?</h1>)", html_text, flags=re.S)
    if len(parts) <= 1:
        return html_text

    rebuilt: list[str] = []
    seen_h1 = 0
    for part in parts:
        if part.startswith("<h1>") and part.endswith("</h1>"):
            if seen_h1 > 0:
                rebuilt.append('<div class="page-break"></div>')
            rebuilt.append(part)
            seen_h1 += 1
        else:
            rebuilt.append(part)
    return "".join(rebuilt)


def polish_html(html_text: str) -> str:
    html_text = re.sub(r"<table>", '<table class="report-table">', html_text)
    html_text = re.sub(r"<p>(Figure [A-Z0-9\\-\\. ]+.*?)</p>", r'<p class="figure-caption">\1</p>', html_text)
    html_text = re.sub(r"<p><img ", r'<p class="figure-image"><img style="width: 5.8in;" ', html_text)
    return html_text


def build_cover_html() -> str:
    return """
<div class="cover-page">
  <p class="center lead">MINISTRY OF EDUCATION AND TRAINING</p>
  <p class="center lead">UNIVERSITY OF ECONOMICS AND FINANCE HO CHI MINH CITY</p>
  <p class="center divider">---------------------------</p>
  <div class="cover-spacer"></div>
  <p class="center report-title">FINAL PROJECT REPORT</p>
  <p class="center report-subtitle">SOFTWARE QUALITY VERIFICATION</p>
  <p class="center topic">TOPIC: ONLINE SALES MANAGEMENT SYSTEM</p>
  <div class="cover-meta">
    <p>Class: 252.ITE1231E.A02E</p>
    <p>Instructor: MSc. Nguyen Ngoc Tu</p>
    <p>Performed by Student:</p>
    <p>Hoang Van Thien - 22D1ITE-SWE03 - 225051915</p>
    <p>Nguyen Thanh Dat - 22D1ITE-SWE03 - 225050896</p>
    <p>Le Quang Duy - 22D1ITE-SWE03 - 225051169</p>
  </div>
  <p class="center city-date">Ho Chi Minh City, April 2026</p>
</div>
<div class="page-break"></div>
"""


def extract_toc_entries(markdown_text: str) -> list[tuple[int, str]]:
    entries: list[tuple[int, str]] = []
    for line in markdown_text.splitlines():
        match = re.match(r"^(#{1,3})\s+(.*)$", line.strip())
        if not match:
            continue
        entries.append((len(match.group(1)), strip_md(match.group(2))))
    return entries


def strip_md(text: str) -> str:
    text = re.sub(r"\[([^\]]+)\]\(([^)]+)\)", r"\1", text)
    text = text.replace("**", "")
    text = text.replace("`", "")
    return text.strip()


def build_record_html(toc_entries: list[tuple[int, str]]) -> str:
    rows = "".join(
        f"<tr><td>{version}</td><td>{date}</td><td>{author}</td><td>{summary}</td></tr>"
        for version, date, author, summary in RECORD_ROWS
    )
    toc_rows = "".join(
        f'<li class="toc-level-{level}">{entry}</li>'
        for level, entry in toc_entries
    )
    return f"""
<h1>Record Of Changes</h1>
<table class="report-table">
  <thead>
    <tr><th>Version</th><th>Date</th><th>Author</th><th>Change Summary</th></tr>
  </thead>
  <tbody>{rows}</tbody>
</table>
<div class="page-break"></div>
<h1>Table Of Contents</h1>
<ul class="toc-list">{toc_rows}</ul>
<div class="page-break"></div>
"""


def build_html(source_markdown: Path) -> str:
    text = source_markdown.read_text(encoding="utf-8")
    marker = "# I. Overview"
    if marker not in text:
        raise RuntimeError(f"Cannot find report body marker: {marker}")

    body_markdown = text[text.index(marker):]
    toc_entries = extract_toc_entries(body_markdown)
    body_markdown = resolve_images(body_markdown, source_markdown.parent)
    body_html = markdown.markdown(body_markdown, extensions=["tables", "sane_lists"])
    body_html = add_page_breaks_to_h1(body_html)
    body_html = polish_html(body_html)

    style = """
<style>
  @page { margin: 1in; }
  body { font-family: "Times New Roman"; font-size: 12pt; line-height: 1.5; color: #1d1d1d; }
  h1 { font-size: 15pt; font-weight: bold; margin: 20pt 0 10pt; }
  h2 { font-size: 13pt; font-weight: bold; margin: 14pt 0 8pt; }
  h3, h4 { font-size: 12pt; font-weight: bold; margin: 10pt 0 6pt; }
  p, li { margin: 0 0 8pt; }
  ul, ol { margin: 0 0 10pt 18pt; }
  .report-table { width: 100%; border-collapse: collapse; margin: 10pt 0 14pt; }
  .report-table th, .report-table td { border: 1px solid #444; padding: 6pt; vertical-align: top; }
  .report-table th { background: #e9eef7; font-weight: bold; }
  .figure-caption { text-align: center; font-style: italic; margin-top: 12pt; margin-bottom: 6pt; }
  .figure-image { text-align: center; margin: 0 0 14pt; }
  .page-break { page-break-before: always; }
  .toc-list { list-style: none; padding-left: 0; margin-left: 0; }
  .toc-list li { margin: 0 0 6pt; }
  .toc-level-2 { margin-left: 20pt; }
  .toc-level-3 { margin-left: 40pt; }
  .center { text-align: center; }
  .lead { font-size: 13pt; font-weight: bold; }
  .divider { margin-top: 6pt; }
  .cover-page { min-height: 9.2in; }
  .cover-spacer { height: 1.0in; }
  .report-title { font-size: 18pt; font-weight: bold; margin-top: 12pt; }
  .report-subtitle { font-size: 16pt; font-weight: bold; margin-top: 8pt; }
  .topic { font-size: 14pt; font-weight: bold; margin-top: 18pt; }
  .cover-meta { margin-top: 36pt; }
  .cover-meta p { text-align: left; margin: 0 0 6pt 1.2in; }
  .city-date { margin-top: 48pt; }
</style>
"""

    return f"""<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8" />
  <title>Powered by GPT - Software Quality Verification</title>
  {style}
</head>
<body>
{build_cover_html()}
{build_record_html(toc_entries)}
{body_html}
</body>
</html>
"""


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", required=True)
    parser.add_argument("--output-html", required=True)
    args = parser.parse_args()

    source = Path(args.source).resolve()
    output_html = Path(args.output_html).resolve()
    output_html.write_text(build_html(source), encoding="utf-8")


if __name__ == "__main__":
    main()
