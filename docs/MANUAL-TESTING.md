# Statistle — manual testing

Hand-tests for Ron (and other testers) before anything goes to the Microsoft Store. Each round is a
part, newest at the top. Record each step as ✅ / ❌ with a note; ❌ and notes go back into
`docs/DEVELOPMENT-PLAN.md`.

## The hand-test build

Framework-dependent (the machine has the .NET 10 runtime). Close any copy of Statistle that is open
first, or the exe is locked:

```powershell
dotnet publish src/Statistle.App/Statistle.App.csproj -c Release -r win-x64 `
    --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o manual-test
Move-Item manual-test/Statistle.App.exe manual-test/Statistle.exe -Force
```

`manual-test\` is gitignored. This copy is not installed from the Store, so Help ▸ Check for a new
version says it cannot check and offers the Store page — that is expected.

---

## Round 2026-11 — usability (Ron, 2026-10-31)

Build: `manual-test\Statistle.exe` (made 2026-10-31, version 1.1.0). Close any copy that is open first.
Covers the pre-release UX review (`docs/ux-reviews/2026-10-31-pre-release.md`) and the usability
checklist (`..\USABILITY-CHECKLIST.md`); plan phase 6.

### A. First start, as a new user

A0. On this computer an old recovery copy is waiting in %LOCALAPPDATA%\Statistle (left by earlier builds, which never deleted it), so the first start asks "Statistle closed with unsaved work in an untitled project. Restore it?" Yes shows what it holds; No throws it away. Either way it is not asked again unless work is left unsaved.

A1. Start it. The Properties tab says what Statistle is for, with **Open the sample project** and **+ Data table**
    buttons; the help panel starts "New here?". Could you tell what to do first?
A2. Click **Open the sample project** — expected: the demonstration project opens; the status line says
    "Opened demo-statistics.statistle — n data tables, n analyses" (plain plurals, not "table(s)").
A3. Click **Descriptive statistics** in the rail, choose a column in Column — expected: the rail button is lit
    (lighter, bold, a yellow bar on its left); the results fill in; Plots shows the histogram and box plot.
A4. Click **Code**, then **Copy code** — expected: the status line says the code was copied; the text says to check
    that it prints the same numbers as the Properties tab.
A5. Click **Report**, then **Save report as PDF…** — expected: a PDF; the status line says "Saved the report as …".
    At each step: could you tell what to do next without knowing the program?

### B. This round's fixes (one step per plan row 6.x)

B1 (6.1, work never lost). Change any value. Then in turn: File ▸ New project, File ▸ Open…, the window's ✕.
    Each time: "Save changes to {file} before …? Yes saves them, No discards them, Cancel goes back." Cancel keeps
    everything; No goes on without the change; Yes saves (asks where for an untitled project) and then goes on.
B2 (6.1, recovery). Change a value, then end Statistle from Task Manager. Start it again — expected: "Statistle
    closed with unsaved work in … Restore it?" Yes brings the change back (title ends with *); No throws it away
    and it is not asked again.
B3 (6.2, undo). Add a data table, then File ▸ New project (No). Edit ▸ Undo is greyed and Ctrl+Z does nothing.
    Same after File ▸ Open….
B4 (6.3, typed value kept). Type a new value in a box and, without leaving it, press Ctrl+S (and once more with **File ▸ Save** from the menu); close and reopen the
    file — the new value is there. Type a value and press **Enter** — the results update at once and the box keeps
    the focus.
B5 (6.4, rejected entry). Type `12,5x` in a number box and press Enter — under the box, in red: "'12,5x' is not a
    number, so Statistle is still using {old value}. Correct it or press Esc." and the same at the top of the help
    panel. Press **Esc** — the old value comes back and the message goes. In a data table's Data box, paste a cell
    `abc` — "Line n, column 'name': 'abc' is not a number. The paste was not used; your earlier data is kept. …"
B6 (6.5, open/save failures). Open a file that is not a Statistle project (rename a .txt to .statistle) — a message
    box "Statistle — Could not open the project" names the file and says the open project is still open. Save to a
    read-only folder (or a file open elsewhere) — "Statistle could not save to … Try Save As… to a folder in Documents."
B7 (6.6, first run). File menu has **Open the sample project**; the empty Properties tab offers it (A1).
B8 (6.7, no tables yet). File ▸ New project, then open One-way ANOVA — the red note says "This analysis reads a data
    table, and the project has none yet. Click + Data table on the left, paste or Import your data, then come back and
    choose it here." Same for Two-way ANOVA, Multiple regression, and a Confidence interval / test set to read columns.
B9 (6.8, delete). With an analysis open, **Delete selected data table…** is greyed and its tooltip says why. Select a data
    table, click it — "Delete the data table '{name}'? … Ctrl+Z brings it back." No keeps it; Yes deletes it and the
    status line names it; Ctrl+Z brings it back. An analysis that read it says the table "has been deleted".
B10 (6.9, table import). On a data table click **Import…** and pick an Excel file with two sheets — a preview window
    shows the file, a Sheet drop-down switches the preview, and the line at the bottom says how many columns will
    replace the table's columns (columns with no numbers are left out). Import — the status line says "Imported n columns from {file}, sheet '{sheet}'. Ctrl+Z
    puts the old data back." Import into a sample box (e.g. Descriptive statistics, Column = Entered below) — "Imported
    n values from {file}. …"
B11 (6.10, wording). Report tab text ends "Save it as PDF for your report."; File ▸ "Export selected item as XML
    (advanced)…"; "Save report as PDF…" in both the File menu and the Report tab.
B12 (6.11, help). F1 (or Help ▸ Help for this screen) opens the Instructions tab for what is selected, with a
    "More on the web: softwareplus.ai/statistle/help/…" link to the matching page. Help ▸ Help on the web opens the help
    index. Press F1 in the Import window — the data-tables help page opens.
B13 (6.12, tooltips). Hover every menu item and button in the main window, the Import window and About — each has a
    tooltip; greyed ones say what enables them.
B14 (6.13, About). Help ▸ About Statistle… — version 1.1.0; links to the Statistle page, the help, the privacy policy,
    the Microsoft Store page and all programs all open in the browser.
B15 (6.14, instructions). The Instructions tab of a data table says Import… shows a preview where the sheet is chosen
    and every column replaces the table's; Descriptive statistics no longer mentions a "Calculate…" link.

### C. Every screen (checklist 1–4)

The screens: the main window (rail, Project tree, Properties / Report / Plots / Code / Instructions tabs), the
Import window, About, and the message boxes (save question, recovery, delete, open/save failures, Check for a new
version). For each: hover every control — tooltip? Disabled ones say why? F1 opens help about this screen? The empty
state says what to do? A wrong value says why?

### D. Same as every program (checklist 5–6)

D1. Change something, then Close / New / Open: asked to save? (B1)
D2. Help menu, in order: Help for this screen (F1) · Help on the web (softwareplus.ai/statistle) · Check for a new
    version… · About Statistle….
D3. About: version, and the links work (B14).
D4. Check for a new version — this copy says it was not installed from the Store and offers the Store page; the
    message box title is "Statistle — Check for a new version".

### E. Look (checklist 7)

At 150 % scaling and the smallest window size: is anything cut off (the red rejection line under a box, the empty-state
panel, the lit rail button, the Instructions tab link), hard to read, or shown by colour alone?
