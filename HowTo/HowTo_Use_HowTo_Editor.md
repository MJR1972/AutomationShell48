# How To Use the How To Editor

## Overview

This guide explains how to create a new How To document, edit an existing one, insert new sections at a specific point in the markdown draft, preview changes, save, cancel editing, and delete a document.

## Open The Editor For A New Document

Use this when you want to create a brand-new How To.

- Open the **How To** feature.
- Click **New How To** in the top action bar.
- The **Editor** tab appears and opens automatically.
- Enter a **Document Title**.
- Enter a **File Name** (optional). If left blank, the title can be used to generate the file name when saving.

## Edit An Existing Document

Use this when you want to update an existing How To.

- Select a document from the **Documents** list on the left.
- Click **Edit Selected**.
- The **Editor** tab opens with the selected markdown loaded.
- Update the title, file name, or markdown as needed.

## Build A Section With The Section Builder

The section builder helps you add structured markdown without typing all the syntax manually.

### Section Builder Fields

- **Header Level**: choose `2` (`##`) or `3` (`###`).
- **Section Header**: the section heading text.
- **Sub Text / Instructions**: body text that appears under the heading.
- **Optional Code Block**: code or commands to place in a fenced code block.
- **Code Language**: language label for the fenced code block (for example: `powershell`, `csharp`, `json`).
- **Insert At Line**: optional 1-based line number where the generated section/code should be inserted. Leave blank to append at the end.

### Add A Section

- Fill in **Section Header** (required).
- Optionally fill in body text and code.
- Optionally set **Insert At Line**.
- Click **Add Section**.

### Insert Only A Code Block Template

- Optionally set **Code Language**.
- Optionally set **Insert At Line**.
- Click **Insert Code Template**.
- Replace the sample line with your actual code.

## Insert At A Specific Point In The Draft

Use **Insert At Line** when you want a new section inserted in the middle of the markdown instead of appended to the end.

- Enter a line number (1-based).
- Click **Add Section** or **Insert Code Template**.
- The generated markdown is inserted starting at that line.

### Tips

- Leave the field blank to append to the end.
- If the line is larger than the document length, the content is inserted at the end.
- If the line is invalid (for example text or `0`), the content is inserted at the end.

## Edit The Markdown Draft Directly

You can always type directly in the **Markdown Draft** box.

- Add headings (`#`, `##`, `###`)
- Add bullets (`-`)
- Add fenced code blocks (```language)
- Reorder sections manually

The section builder is optional and can be mixed with manual editing.

## Preview Your Draft

- Click **Preview Draft** to render the current draft in the **Preview** tab panel.
- Use **Collapse All / Expand All** to manage long documents.

## Save A Document

- Click **Save**.
- The file is written to the repository `HowTo` folder as a `.md` file.
- If you changed the file name while editing an existing document, the file is effectively renamed.

## Cancel Editing

Use **Cancel Editing** to close the editor without saving the current unsaved changes.

- Click **Cancel Editing**.
- The editor tab closes and the view returns to **Preview**.

## Delete A Document

Use this only when the document is no longer needed.

- Load an existing document into the editor with **Edit Selected**.
- Click **Delete**.
- Confirm the delete prompt.

This removes the markdown file from the `HowTo` folder and removes it from the in-app list after refresh.

## Suggested Workflow

1. Click **New How To**
2. Enter title and file name
3. Add sections with the section builder
4. Use **Insert At Line** when you need to place content in the middle
5. Review and refine the **Markdown Draft**
6. Click **Preview Draft**
7. Click **Save**
8. Later, use **Edit Selected** to update the document
