# Statistics+ (Statistle) — manual testing

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

## Round 2.0 — Statistics+: the Course, Start here and Modules 1–4 (2026-11-11)

Build: `manual-test\Statistle.exe` (made 2026-11-11, version 2.0.0; the content package is in
`manual-test\content\egn3443`). Close any copy that is open first. Plan phases 7–11 (`docs/DEVELOPMENT-PLAN.md`).
The Module 2 pilot was approved (plan 11.1 ✅). **This round: the program (part P), the new modules 0, 1, 3 and 4
(part Q), and the new analysis Simulation and the Descriptive statistics additions (part S).**

### P. The program, as a student meets it

P1. Start it. Expected: "Who is learning?" covers the window. The title bar and every word say **Statistics+**.
P2. Choose **New learner…**, type your name and a PIN, and write down the recovery code. Expected: you are signed in,
    your name is in the dark strip at the top right, and the Course shows **My progress**.
P3. The strip has **Course** and **Workbench** (Ctrl+L, Ctrl+B). Switch to the Workbench and back. Expected: the
    Workbench is the calculator as in 1.1, unchanged.
P4. Open **2. Descriptive Statistics** in the tree. Expected: the module page lists the five outcomes and the six
    items, with **Start with 2.1 …**.
P5. Open **2.2 Measures of central tendency**. Read the **Learn** tab (Ctrl+1). Look at the **Example** tab (Ctrl+2):
    the data, every result, the histogram and box plot, and the Python and R. Press **Copy as Python** and paste it
    into Colab or Python. Does it run?
P6. On the Example, press **Open in the Workbench**. Expected: the Workbench opens on Descriptive statistics with the
    cement bag weights. Ctrl+Z puts the project back. Ctrl+L returns to the Course.
P7. **Exercise** (Ctrl+3): type a wrong mean, such as 50.5, and press **Check my answers**. Expected: ✗ "too high", never
    the answer. Press **Show a hint**. Type 50.19 and 50.10 and check. Expected: Passed; the worked solution opens below;
    the tree marks 2.2 ✓ and My progress counts it.
P8. Do the questions on 2.1, 2.4 and 2.5, then the **2.6 Module check** (10 questions, 80% to pass). Expected: module 2
    shows ✓ in the tree and on My progress, and the outcomes show ★ on the module page.
P9. **Sign out**, close the program, start it again and sign in with your PIN. Expected: your progress is still there,
    and the exercise boxes still hold your last answers.
P10. On the start screen choose **Just the Workbench (no sign-in)**. Expected: the Workbench, with the Course still
     readable (Ctrl+L) under a yellow "Not signed in: … progress is not kept" line with a **Sign in** link.
P11. Common mistakes (plan 10.13). On **2.2**'s Exercise type the median (50.10) as the mean, and 50.15 as the median
     (the middle of the values as listed, not sorted). Expected: ✗ with "That is the median…" and "…Sort the values…",
     not "too high/too low". On **2.3**'s Exercise type a variance divided by n (about 56) and a standard deviation
     divided by n. Expected: ✗ "You divided by n = 12. … divide by n − 1 = 11". Is each sentence what you would say?
P12. A picture in a question (plan 10.15). On **2.4**'s Exercise (questions), question 9 shows the histogram of the
     sensor reply times inside the question. Expected: the histogram is drawn (not the text `[[figure: …]]`), the
     question reads well beside it, and the right answer is positively skewed.

### Q. The new modules (plan 11.2–11.5)

Read each module as a student would: the module page, every item's four tabs, its exercise or questions, and the
module check. Each item that ends in a calculation is graded by the program; try one wrong answer on each.

Q1. **0. Start here** (3 items, no check). Does it describe the program as it is? Does 0.2 (run the Code tab in
    Colab or R and type a printed value) work for you in Colab?
Q2. **1. Statistics for Engineers** (6 items + check). 1.4 and 1.5 preview distributions and inference that later
    modules teach. 1.6 shows the control chart of the wheel-nut torques (x̄ ± 3s); its text says other control-chart
    methods (moving range, subgroups) come later. Is x̄ ± 3s the form you want taught here?
Q3. **3. Probability Concepts** (5 items + check). 3.3's exercise asks the theoretical probability of a sum of 10 or
    more with two dice, then the count and proportion from the program's simulation **with seed 1**: set it up in
    the Workbench as the intro says and check you get 145. Is reading a seeded result fine, or should it ask theory
    only? The check's three-card 21 question shows the simulation's figure.
Q4. **4. Discrete Probability Distributions** (6 items + check). Probabilities must be typed to three decimals
    (tolerance 0.001). 4.6 covers all six models of the course's Distribution Detective; only binomial and Poisson are
    graded calculations. Right scope?
Q5. The open content questions, answer any you have a view on:
    a. Start here is numbered module 0 ("0. Start here"). Fine, or fold it into Module 1?
    b. Module 4 avoids the numbers of the graded group assignment (wafers n = 25, p = 0.03; 2.5 per hour; p = 0.15),
       so it doesn't hand over its answers. And the Workbench's Code tab for the three-card 21 simulation is a
       complete Python/R answer to Assignment 3's Monte Carlo program. Acceptable?
    c. Bayes' theorem is worked in full in 3.5 (weld inspection, leak detector); the course only names it. Right depth?
    d. Permutations and combinations are taught briefly in 3.2; Module 4 relies on C(n, x). Enough?
    e. 2.4's "symmetric" shaft diameters histogram has an empty class in the middle; is a cleaner example wanted?

### S. New in the Workbench (plan 10.7, 10.11, 10.16)

S1. Workbench ▸ **Simulation**. Try each **Experiment**: Coins, Dice, Cards, Darts, Sample means, Interval coverage,
    Familywise error. Expected: the running proportion settles near the theoretical line as Trials grows (figure 1);
    figure 2 makes sense for each; the same **Seed** always gives the same counts; a new seed gives new ones.
S2. Simulation's **Code** tab: the Python and R run, and say your counts will differ from the program's.
S3. Workbench ▸ **Descriptive statistics** on any column: the new results **Lower/Upper control limit**, **Points
    outside the limits**, **Number of outliers** and **Mode, when there is one**, and figure 3, the control chart.
    The limits use the sample standard deviation s (plan 10.11 asks whether you want the moving range instead).
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
B16 (6.17, the crash). Start the rebuilt `manual-test\Statistle.exe`, open the sample project, click each data table
    and then each analysis in the rail — expected: every form draws, nothing closes Statistle. (The build from earlier
    on 2026-10-31 closed with "Cannot find resource named 'RejectionText'" at the first table.)
B17 (6.18). Change a value, then the window's ✕ — the question's title bar reads "Statistle — Save changes?". Cancel.
B18 (6.19). Open Confidence interval and click in **Confidence level** — the help panel's title is "Confidence level",
    with no "()" after it. A field with a unit (Coefficient of variation in Descriptive statistics) still shows "(%)".
B19 (6.20). Open a PDF you saved from Statistle in a PDF viewer that locks it (or make the file read-only), then
    **Save report as PDF…** over it — a box "Statistle — Could not export" says the file could not be written, to close it
    in the other program or choose a folder in Documents, and ends with "Details for support:". Same for **Export data as
    CSV…** over a CSV open in Excel.
B20 (6.21). With Narrator on (Ctrl+Win+Enter), move through the Project tree — each data table is read by its name
    ("Tensile strength"), not "Statistle.Core.Domain.DataTable".
B21 (6.22). Start Statistle without resizing it. Open Descriptive statistics, Two-sample test, Confidence interval and
    One-way ANOVA on the sample — no horizontal scrollbar under the form; Mode, Decision, Interval and Group summaries
    are read in full; in One-way ANOVA scroll to **Tukey HSD pairs** — every pair can be read on the Properties tab.
    Then make the window narrower — a scrollbar appears only when the two columns no longer fit.
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
