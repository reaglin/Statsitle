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

## Round 2.0 — Statistics+: the Course, Start here and Modules 1–14 (2026-11-11)

Build: `manual-test\Statistle.exe` (made 2026-11-12, version 2.0.0; the content package is in
`manual-test\content\egn3443`). Close any copy that is open first. Plan phases 7–11 (`docs/DEVELOPMENT-PLAN.md`).
The Module 2 pilot was approved (plan 11.1 ✅). **This round: the program (part P), modules 0, 1, 3 and 4 (part Q),
modules 5–9 (part R), what is new in the Workbench (part S), modules 10–14 (part T) and the Workbench additions made
for them (part U), handing in a report (part V, plan 14), and the release: the UX sweep's fixes, the Store text, privacy and site (part W, plan 15).** Module 9 now uses the course's variance rule (Ron's answer). Read as much of Q, R and T as you have time for;
your view on the shape of one or two modules is as useful as a full read.

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

### R. Modules 5–9 (plan 12.1–12.5)

R1. **5. Continuous Probability Distributions** (6 items + check). 5.6 goes beyond the course's topics: the Weibull,
    its hazard and B10 life, and fitting a model to failure times (maximum likelihood, the Q–Q plot and its r). Right
    depth for EGN3443? Its exercise asks you to fit a Weibull in the Workbench from the data (Copy the data → **+ Data
    table** → paste): does that work for you?
R2. **6. Sampling Distributions** (5 items + check). The CLT rule of thumb (normal: any n; roughly symmetric: about 15;
    skewed: about 30) and the |z| guide in 6.5 (under 1 ordinary, about 2 unusual, 3 or more very unlikely) — as you
    teach them?
R3. **7. Point Estimation and Confidence Intervals** (7 items + check). 7.7's Example uses the 30 battery values from
    the course's Bonus 7-1 page. Allowed? The text uses n ≥ 30 and np̂ ≥ 10 and shows Wilson beside Wald; 7.3 keeps a
    paragraph on one-sided bounds.
R4. **8. Hypothesis Testing** (7 items + check). Five graded exercises (t from a column, a proportion test, χ² goodness
    of fit, power and the n for 0.90 power, χ² independence); "choose the test" and the wording of conclusions are in
    the Learn pages and the check. H₀ is written μ = μ₀ even for "at least" claims, as the course does. Power uses the
    normal approximation (R's power.t.test gives slightly lower values).
R5. **9. Hypothesis Testing II** (6 items + check). The rank-test exercise asks the normal-approximation p (R and scipy
    give the exact p by default for small samples; the exercise says how to get the approximation). 9.6 is deliberately
    not significant (p = 0.051) with a large effect (d = 0.84): the lesson "not rejected is not the same as no
    difference" — the one you want there?
R6. Your course's own pages: the Module 5 worked-example page ("Continuous Distribution Analysis Example Problem")
    seems to print wrong answers for its Weibull and lognormal problems (e.g. P(fail before 200 000 cycles) with
    β = 2.5, η = 500 000 is about 9.6 %, not 6.19 %). Worth a look.

### S. New in the Workbench (plan 10.7, 10.11, 10.16)

S1. Workbench ▸ **Simulation**. Try each **Experiment**: Coins, Dice, Cards, Darts, Sample means, Interval coverage,
    Familywise error. Expected: the running proportion settles near the theoretical line as Trials grows (figure 1);
    figure 2 makes sense for each; the same **Seed** always gives the same counts; a new seed gives new ones.
S2. Simulation's **Code** tab: the Python and R run, and say your counts will differ from the program's.
S3. Workbench ▸ **Descriptive statistics** on any column: the new results **Lower/Upper control limit**, **Points
    outside the limits**, **Number of outliers** and **Mode, when there is one**, and figure 3, the control chart.
    The limits use the sample standard deviation s (plan 10.11 asks whether you want the moving range instead).
S4. Workbench ▸ **Chi-square tests** (plan 10.8). Goodness of fit: type observed counts, then expected proportions.
    Independence: make a data table whose columns are the table's columns and choose it. Expected: χ², df, p, the
    expected counts, a warning when one is below 5; figure 1 the χ² curve with the rejection region, figure 2 the
    observed and expected bars named by category. No Yates correction unless chosen (is that the default you want?).
S5. Workbench ▸ **Continuous distribution** ▸ Family **Weibull** (shape β, scale η): probabilities, the hazard rate,
    the figures. Then **Source: Fitted to data** on a column of failure times: the fitted parameters, figure 3 (the
    histogram with the fitted curve) and figure 4 (the Q–Q plot with its r). A fitted normal's σ divides by n, as
    scipy's norm.fit does (plan 10.9 asks whether you want s).
S6. Workbench ▸ **Sampling distribution**: **Population shape** Normal removes the "n < 30" warning; **Shade on the
    figure** Above shades the right-hand tail.
S7. The **Code** tab's R for a one- or two-sample test now runs in R (it stopped on 'two-sided' before).


### T. Modules 10–14 (plan 13.1–13.5)

T1. **10. Two-Sample Hypothesis Testing** (6 items + check). It revisits Module 9 with new cases and adds the
    two-proportion z-test (10.5), one-sided tests and tests against a difference Δ₀ (10.4), and deciding from an
    interval (10.6). Questions: 10.3 calls the course's "variance ratio above 2–3" the cautious rule, while Module 9
    uses "SD ratio under 2" (a variance ratio up to 4) — should one rule be used in both? Is 10.6 (the decision table,
    n per group, the written conclusion) the right depth? 10.4 finishes the course page's own pooled example with
    means it adds (27.3 and 25.6) — all right?
T2. **11. Regression Analysis** (6 items + check). The course reading's four points (1,2), (2,3), (3,5), (4,4) are
    worked by hand through the module. 11.3 and 11.6 use R's built-in real data (`stackloss`, `mtcars`, `trees`) —
    allowed? σ̂ (÷ n − 2) is taught as the typical miss, with the course's RMSE (÷ n) beside it — as you want it?
T3. **12. Multiple Regression** (5 items + check). It opens with a confounded concrete case: neither one-predictor
    regression is significant, but the two together explain 83 %. Is that the first message you want? Selection uses
    p < 0.05 as the Workbench does; R's AIC `step()` is only mentioned. All data sets are generated.
T4. **13. Validating Regression** (6 items + check, the check shaped like the course's *Fit for Purpose*). Is 13.4 the
    right depth (PRESS, the contiguous folds, a table of six hold-out seeds)? Durbin–Watson: keep the Savin–White bounds
    (d_L 1.20, d_U 1.41 at n = 20), or say only "below about 1.5"? Look at the quiz figures (residual patterns) at quiz
    size, and at the six-column tables in a narrow window.
T5. **14. ANOVA** (6 items + check), ending with a paragraph over the whole course. 14.3 works the course reading's
    fertilizer data by hand (F = 21.6) — free to use, or is it graded? 14.2 teaches Welch's ANOVA for unequal variances,
    which the Workbench now calculates (yes, Ron 2026-11-11; U6). 14.1's simulation
    runs independent tests (1 − (1 − α)ᵐ); the text also gives the lower rate of correlated pairwise tests.
T6. Your course's own pages, from the writers' reading (now kept in `docs/COURSE-SITE-NOTES.md` for you to fix later):
    - **Module 10:** labelled Week 11/Module 10 in one place and Module 11 in another. It gives no formula for the
      p₁ − p₂ interval, and its Example 2 claims a direction ("significantly higher in Group 1") from a two-sided test.
      Levene's test is called an "F-test". d is x₂ − x₁ on one page and x₁ − x₂ on another.
    - **Module 11:** a CI of [0.2, 0.4] with SE = 0.1 is inconsistent (±1.96·SE ≈ ±0.2). The Topic 4 adjusted R² table
      (0.774, 0.799, 0.791) doesn't match its own formula (0.775, 0.802, 0.800).
    - **Module 12:** the landing page links its lecture as "Module 12 - Hypothesis Testing for 2 Samples". The study
      page's Python uses `pd`, `X` and `y` without defining them.
    - **Module 14:** the FWER formula is given without saying pairwise tests are correlated.

### U. New in the Workbench for Modules 10–14 (plan 10.10 and the module writers' fixes)

U1. Workbench ▸ **Multiple regression** on a data table: the new **Hold-out share** (0.2) and **Hold-out seed** (1) give
    **Hold-out test rows**, **Rows in the test set** and **Hold-out test RMSE**, beside the in-sample and cross-validated
    RMSE. Change the seed: the rows and the test RMSE change. The Code tab lists the drawn rows (numpy and R would draw
    others from a seed) and reproduces the RMSE.
U2. Multiple regression ▸ **Coefficient to read**: type a predictor's name (or Intercept). Expected: **Chosen
    coefficient (b)**, **Standard error of b**, **t statistic of b** and **p-value of b** match that row of the
    Coefficients table; a name not in the model gives "—".
U3. **Two-sample test** ▸ Paired t from two columns: the box plot of the differences is drawn (it was blank, also in
    Module 9's 9.2).
U4. **Simple linear regression**: with rising data the legend sits in the lower right, clear of the points; a fit in
    1/x or ln x no longer shoots off the plot near x = 0.
U5. **One-way ANOVA** and **Two-way ANOVA** opened from a Course Example (Open in the Workbench) load their table. The
    box plots and the means plot name each group on its axis, and the interaction plot names each level of A (coded
    levels 1, 2, 3 had ticks 1.1, 1.2 …); each line is named once in its legend.
U6. **One-way ANOVA** with unequal spreads: open Module 14 ▸ 14.2's Example (the welds) in the Workbench. Under
    Kruskal–Wallis, expected: **Welch F** 11.8252, **Welch denominator df** 12.19 and **Welch p-value** 0.0014, as R's
    `oneway.test(y ~ g, var.equal = FALSE)` gives (F = 11.825, num df = 2, denom df = 12.194, p = 0.001396). The
    ANOVA's own F is 1.42 (p = 0.265). The Code tab prints the same three lines in Python and in R. Click each: the help panel
    says when to read it (Levene's p small).

### V. Handing in a report (plan 14.1–14.3)

V1. Sign in as a test learner. Pass one exercise in a module, then **Learner ▸ Hand in a progress report…**
    (also a button on My progress, and **Hand in a progress report…** at the top of a module check). Expected: 1. the module checks
    and "Everything so far", each with where you are; 2. a card that says Complete or Incomplete, the score, the items, the
    outcomes and the time, and what is still to do.
V2. **Save the progress report (PDF)…**. The first time, **My details for reports** opens (student ID, course, section, your e-mail,
    your instructor's e-mail, class code; all optional). Continue, save, and open the PDF. Expected: the cover, the summary,
    each item, the outcomes, **Exercise evidence** (the answers you typed beside the values Statistics+ calculated, ✓/✗,
    and the exercise's figure) and Verification with a QR code. Is the evidence what you want to see as the instructor?
V3. Under **3. Hand it in**: the result line starts `STAT1|`; **Copy the result line**, **Send to my instructor…** (opens
    your e-mail with the line, and the folder with the PDF) and **Show the file in its folder**. The report is listed under
    **Reports you have made**.
V4. As the instructor: **File ▸ Check reports and class codes (for instructors)…** (or the button on Who is learning?). **Make a new class
    code**, then check the PDF and the pasted line. Expected: "The report is genuine". Open the PDF in a PDF editor, save it,
    check again: "The report was changed". With a class code typed in My details, a line checks with the code alone.
V5. Signed out: **Learner ▸ Hand in a progress report** is grayed with "Sign in first"; the page says to sign in.
V6. The instructors' help page https://softwareplus.ai/statistle/help/reports.html (not linked from the site until 2.0
    ships, plan 15.4): is it what an instructor needs?

### W. Before the Store: the full UX sweep's fixes and the release text (plan 15.1–15.4)

The sweep (`docs/ux-reviews/2026-11-12-pre-release.md`) found 1 Critical, 7 Major and 31 Minor; all were fixed. Try these:

W1. **Who is learning?** The menu bar and the Course | Workbench strip are grayed while it shows; F1 points to the help on
    the right; the status line says what to do on this screen. Alt+F, Alt+P do nothing behind it.
W2. **Idle lock.** Sign in, type an answer in an exercise without checking it, and leave Statistics+ alone past the lock time
    (Learner settings; set 1 minute to try it). Sign in again as the same learner: you are back on that item with your
    answer still there. Sign in as someone else: their boxes are empty.
W3. **Enter in an exercise.** Type the first answer and press Enter: the cursor goes to the next box, nothing is counted.
    Press **Check my answers** with a box empty: "Fill in every box … Nothing has been counted yet."
W4. **A module check with blanks.** Answer some questions and press Check: it names the unanswered questions, marks each
    "No answer yet", and counts nothing.
W5. **Learner ▸ Change my PIN…** (signed in). Copy buttons in an exercise say "Copied ✓". The Learn/Example/Exercise/Reference
    tabs are Ctrl+1 to Ctrl+4, and Alt+L opens the Learner menu.
W6. **Workbench, import.** Import a CSV with `NA` in one row of y and `#DIV/0!` in another of x: the preview says which rows
    will be left out, the whole rows go, and a regression on x and y uses the rest in their own order.
W7. **Workbench, Undo.** Add a data table, paste numbers, change α in an analysis, then Ctrl+Z (outside a text box) a few
    times: each press undoes one step and the status line says which; nothing typed earlier is lost all at once.
W8. **Workbench, tree.** Choose a table in the Project tree, then an analysis on the left, then the same table again: it
    opens. Out-of-range values (α = 0.7) show a sentence under the box, not only a red border.
W9. **Store text.** Read `resources/STORE-LISTING.md` (the new description, features and What's new) and the screenshots in
    `resources/images/store/screenshots-2.0/`. Right for the listing?
W10. **Privacy policy** (live): https://softwareplus.ai/statistle/privacy/, rewritten for learners, the signing key and reports.
W11. **The site page for 2.0** (a draft, not linked, not indexed): https://softwareplus.ai/statistle/index-next.html. It becomes
    the Statistics+ page when 2.0 is live.
W12. **Store-installed copy only, after release:** a learner can be made (files in C:\ProgramData\Statistle\Learners),
    Help ▸ Check for a new version says you have the latest, and a .statistle file opens by double-click.

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
