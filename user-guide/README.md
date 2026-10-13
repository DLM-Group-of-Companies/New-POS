# NLI-POS User Guide

This folder is a standalone, static guide website. Open `index.html` directly in a browser to use it offline. It does not require the POS application, a web server, a login, or external JavaScript/CSS libraries.

## Edit the guide

1. Edit the Markdown chapter sources in `content/`. Each file has a title, order, and optional roles in its front matter.
2. Keep filenames numbered and use a short, lowercase slug, such as `14-new-topic.md`. The number controls chapter order.
3. Link to another chapter using its generated HTML filename, for example `[Receipts](04-receipts.html)`.
4. Use normal Markdown headings; level 2 and level 3 headings are included in the chapter's “On this page” navigation.
5. Add a screenshot using descriptive alt text and a one-line caption:

   ```markdown
   ![Screenshot of the report filters](../images/report-filters.png)
   Figure: Choose an office and date range before filtering the report.
   ```

## Add screenshots

1. Save the screenshot in `images/` using a descriptive filename.
2. Remove or replace the nearby `<!-- TODO-SCREENSHOT: filename.png -->` comment.
3. Reference the image from the Markdown with `../images/filename.png`.
4. Add a caption on the line immediately after the image.

Use screenshots approved for publication; do not include real customer, account, or live transaction data. The 25 POS screenshots currently included in this guide are redacted copies: customer, employee, transaction, product-price, and stock values are concealed. The raw source screenshots in `docs/screenshots/` remain unchanged and are not published.

## Regenerate HTML pages and the search index

Install Python 3.10 or later. The build script uses only Python's standard library.

From this folder, run:

```text
python tools/build_site.py
```

The script creates `index.html`, one HTML page per chapter in `chapters/`, and the local `assets/js/search-index.js` file. The search index is bundled as JavaScript so searching also works when `index.html` is opened with a `file://` URL, where browsers restrict loading local JSON with `fetch`.

## Upload to regular web hosting

Upload the contents of this `user-guide/` folder, preserving its directory structure, to a static hosting location. Set that host's default document to `index.html`. No server-side code or database is needed.

All site assets and chapter links use relative paths, so the guide can be hosted in a subfolder. GitHub Pages deployment is configured in the repository workflow at `.github/workflows/pages.yml`.

## Pages

| Order | Page file | Chapter |
| ---: | --- | --- |
| 1 | `chapters/01-introduction.html` | Introduction |
| 2 | `chapters/02-getting-started.html` | Getting Started |
| 3 | `chapters/03-making-a-sale.html` | Making a Sale |
| 4 | `chapters/04-receipts.html` | Receipts |
| 5 | `chapters/05-voids-refunds-and-returns.html` | Voids, Refunds, and Returns |
| 6 | `chapters/06-holding-and-recalling-transactions.html` | Holding and Recalling Transactions |
| 7 | `chapters/07-shift-and-cash-drawer.html` | Shift and Cash Drawer |
| 8 | `chapters/08-products-and-inventory.html` | Products and Inventory |
| 9 | `chapters/09-customers.html` | Customers |
| 10 | `chapters/10-reports-and-reading-them.html` | Reports and Reading Them |
| 11 | `chapters/11-users-roles-and-settings.html` | Users, Roles, and Settings |
| 12 | `chapters/12-troubleshooting-and-faq.html` | Troubleshooting and FAQ |
| 13 | `chapters/13-glossary.html` | Glossary |
