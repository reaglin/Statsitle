# Statistle — development plan (executable breakdown)

`PLAN.md` is the strategy; this is the work breakdown — numbered tasks with file targets and
acceptance criteria. Task IDs are stable; do not renumber. `METHODS-AND-SOURCES.md` is the
register the engine tasks refer to: every statistic, its formula, source, and the Python and R
that reproduce it.

Status legend: `[ ]` not started · `[~]` in progress · `[x]` done.

Runtime target **.NET 10** (`net10.0` / `net10.0-windows`), WPF, MSIX by script. Toolchain on
this machine as for SMADA (SDK 10.0.400, VS 2022 17.14, Windows SDK 10.0.26100). Third-party:
ScottPlot (MIT), PDFsharp (MIT), MathNet.Numerics (MIT).

**Every phase ends with something Ron can open and hand-test**, not just green tests. The
hand-test build lives in `manual-test\Statistle.exe` (gitignored); testers get the zip from
`packaging\make-tester-package.ps1`.

---

## Phase 0 — Foundations (framework from SMADA)

Exit: the app opens a `.statistle` project with a data table and two analyses; tests green.

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 0.1 | Repo, `.gitignore`, this documentation | root, `docs/` | plan committed | [x] |
| 0.2 | Solution and three projects from SMADA, `Directory.Build.props` (`StatistleVersion`, nullable, warnings-as-errors) | `Statistle.sln`, `src/`, `tests/` | `dotnet build` clean | [x] |
| 0.3 | Copy SMADA's core and rename (`StatistleObject`, `StatistlePropertyAttribute`, `[StatistleHelp]`, `Analysis`, `AnalysisCatalog`, `StatistleDocument`); drop hydrology; `UnitKind` → `ValueKind` (None, Percent, Probability, Count) | `src/Statistle.Core` | SMADA's framework tests pass renamed | [x] |
| 0.4 | `DataTable` artifact with `ColumnTable` (named, ragged columns as text; paste; Import…); `DataSources.ColumnChoices` for analyses | `Core/Domain/DataTable.cs`, `Persistence/ColumnTable.cs` | round trip; ragged columns; bad cell rejected | [x] |
| 0.5 | WPF shell from SMADA: rail (PROJECT: + Data table; ANALYSES), tree, Properties / Report / Plots, help panel | `src/Statistle.App` | opens; New/Open/Save; add a table; run Descriptive statistics on a column | [x] |
| 0.6 | Packaging: manifest identity `Statistle`, `.statistle` association, tiles, `pack.ps1`, tester zip script | `packaging/` | installable `.msix`; `artifacts\Statistle-<version>-win-x64.zip` | [x] zip (0.1.0, 0.2.0); MSIX untested |
| 0.7 | CI: build, test, pack | `.github/workflows/build.yml` | green on push | [ ] |
| 0.8 | App icon (a real one, not the placeholder) | `packaging/Assets`, `src/App` | crisp at 150 % / 200 % | [ ] |
| 0.9 | `CLAUDE.md` oriented to the real code | root | | [x] |
| 0.10 | Sample project: the course's tensile-strength alloys (Module 14) and solar-panel pairs (Module 11) | `samples/demo-statistics.statistle` | opens from the command line | [x] |

## Phase 1 — Descriptive statistics and the Code tab

Exit: a student enters a column, reads every descriptive statistic with its meaning, sees the
histogram and box plot, and takes Python and R that print the same numbers.

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 1.1 | `SummaryStatistics`: n, mean, median, mode(s), min, max, range, sample and population variance and SD, CV, standard error, skewness (sample, adjusted), kurtosis (excess), quartiles by the course's method (Q1/Q3 as medians of halves *and* the R default type 7 — the help says which is shown), IQR, fences, outliers | `Core/Methods/Descriptive/` | Excel's documented SKEW/KURT example, R fivenum and quantile by hand; invariants: SD ≥ 0, min ≤ Q1 ≤ median ≤ Q3 ≤ max, CV blank when mean 0 | [x] |
| 1.2 | `FrequencyTable`: bins by Sturges / square-root / user width; class marks, frequencies, relative and cumulative | `SummaryStatistics.FrequencyTable` | frequencies sum to n | [x] (Sturges / √n / user count; a user width is not offered) |
| 1.3 | **Descriptive statistics** analysis: column pick-list; `ResultsBeside`; every result with `Help`; figures: histogram with normal overlay, box plot with fences and outliers | `Core/Analyses/DescriptiveStatistics.cs`, `Plotting/DescriptiveFigures.cs` | | [x] |
| 1.4 | **Code generation**: `ICodeSource` on every analysis method; `PythonWriter` / `RWriter` (variables: `x = np.array([...])` / `x <- c(...)`; a table as `pd.DataFrame` / `data.frame`; results printed in the app's order with the same labels); the **Code** tab (Python | R toggle, Copy, Save as .py / .R) | `Core/Code/`, `App/Views/CodeView.xaml`, `ViewModels/CodeViewModel.cs` | the generated Python for the sample prints the app's numbers (cross-check test, skipped when no Python) | [x] |
| 1.5 | Report: the analysis report reads as the course template's *Methodology* + *Conclusions* (method, assumptions, results, citation) with the code as an appendix | `Core/Reporting/` | HTML and PDF carry the appendix | [x] (appendices; the methodology/conclusions wording is the existing sections) |
| 1.6 | Hand-test build; tester zip 0.1.x; Ron's review | | | [ ] |

## Phase 2 — Probability and distributions (Modules 3–6)

**Status 2026-09-08: built.** Four analyses, 8 + 6 + 7 + 2 method classes, figures, Python and R; tester zip 0.2.0.

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 2.1 | `ProbabilityRules` analysis: P(A), P(B), P(A∩B) or independence → complement, union, conditional, Bayes; counting nPr / nCr | `Core/Analyses/ProbabilityRules.cs` | course Module 3 problems | [x] (independent, mutually exclusive, known joint, known conditional, Bayes; nPr, nCr) |
| 2.2 | Discrete distributions over MathNet: binomial, Poisson, geometric, negative binomial, hypergeometric, typed PMF; P(X = k), ≤, ≥, between; mean, variance; PMF and CDF figure | `Core/Methods/Distributions/DiscreteModels.cs`, `Analyses/DiscreteDistribution.cs` | Module 4 problem 1: B(20, 0.05), P(X = 2) = 0.1887 | [x] |
| 2.3 | Continuous: uniform, normal, exponential, t, χ², F, lognormal; P(X ≤ x), between, quantile; z-score; empirical rule; density/CDF figure with the shaded area | `Distributions/ContinuousModels.cs`, `Analyses/ContinuousDistribution.cs` | Φ(1.96) = 0.9750; t₀.₀₂₅,₁₀ = 2.228 | [x] (plus χ²₀.₀₅,₁₀ = 18.307, F₀.₀₅,₅,₁₀ = 3.326) |
| 2.4 | Sampling distribution of x̄ and p̂: SE, CLT probability for a sample mean, spread vs n figure, simulation of k samples | `Analyses/SamplingDistribution.cs` | Module 6 problem 1 (μ 4200, σ 350, n 5 / 30) | [x] (the simulation of k samples is not built; the population/sampling curves are) |
| 2.5 | Code for 2.1–2.4 (`scipy.stats.binom/poisson/norm/t/…`; R `dbinom/pnorm/qt/…`) | `Core/Code/ScriptBuilder.cs`, each analysis | cross-check: 19 configurations pass against numpy/scipy | [x] |

## Phase 3 — Estimation and hypothesis tests (Modules 7–10)

**Status 2026-09-08: built.** Confidence interval (8 kinds), Sample size, One-sample test (4), Two-sample test (8, with Shapiro–Wilk and Levene from columns); tester zip 0.3.0.

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 3.1 | `ConfidenceIntervals`: mean (z, t), proportion (Wald, Wilson), variance and SD (χ²), difference of means (pooled, Welch, paired), difference of proportions; **Sample size** for a margin of error (mean, proportion) | `Core/Methods/Inference/Inference.cs` (`Intervals`), `Analyses/ConfidenceInterval.cs`, `SampleSize.cs` | M&R Examples 8-1, 8-5, 8-7 (p̂ rounding noted), 8-2, 8-8 | [x] |
| 3.2 | `OneSampleTests`: z and t for a mean, z for a proportion, χ² for a variance; tail choice; statistic, critical value, p, decision, CI, power for a stated alternative and the n for a target power | `Inference/Inference.cs` (`HypothesisTests`), `Analyses/OneSampleTest.cs` | M&R Examples 9-2, 9-3 (power, n = 43), 9-6, 9-8; Module 8 problem 1 (p = 0.052, not rejected) | [x] |
| 3.3 | `TwoSampleTests`: independent t pooled / Welch, paired t, two-sample z, two proportions pooled, F for variances; Cohen's d; Shapiro–Wilk, Levene; Mann–Whitney U, Wilcoxon signed-rank | `Inference/Inference.cs`, `Inference/Nonparametric.cs` (Shapiro–Wilk AS R94, Levene, Mann–Whitney, Wilcoxon as scipy computes them), `Analyses/TwoSampleTest.cs` | M&R Examples 10-5, 10-9; pooled = Welch at equal s and n; rank tests and checks agree with scipy in the cross-check | [x] |
| 3.4 | Figures: sampling distribution with rejection region and the observed statistic; two-group box plots; paired differences | `Plotting/InferenceFigures.cs` | | [x] |
| 3.5 | Code (`scipy.stats.ttest_1samp/ttest_ind/ttest_rel/norm`, `statsmodels.stats.proportion`; R `t.test`, `prop.test`, `var.test`, `wilcox.test`, `shapiro.test`) | each analysis: the arithmetic, then the library call as a check | cross-check: 51 configurations pass against numpy/scipy | [x] |

## Phase 4 — Regression (Modules 11–13)

**Status 2026-09-08: built.** One OLS engine (`LinearModel`, MathNet linear algebra) behind two
analyses; the Module 13 validation (diagnostics, transformations, k-fold) is folded into them
rather than a third analysis. The phase-1 six-form curve comparison was dropped in favour of the
course's transformations of x or y, fitted linearly and drawn back in the original units. Tester
zip 0.4.0.

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 4.1 | Simple regression completed: SEs, t and p per coefficient, CIs, ANOVA table, F, residual SE, r, prediction with confidence and prediction intervals at x₀, residual plots | `Methods/Regression/LinearModel.cs`, `Analyses/SimpleRegression.cs` | `RegressionTests`: x = 1..5, y = 2, 4, 5, 4, 5 by hand (b₁ = 0.6, SS_E = 2.4, F = 4.5, h₁₁ = 0.6); M&R Ch. 11 formulas | [x] |
| 4.2 | Multiple regression over MathNet linear algebra: coefficient table, R², adjusted R², F, VIF, forward / backward selection | `LinearModel.cs`, `Analyses/MultipleRegression.cs` (`AllPredictors`, `ForwardSelection`, `BackwardElimination` as the method classes) | exact plane y = 1 + 2x₁ + 3x₂ recovered; collinear design refused; housing table (Module 12's shape) selects and cross-validates | [x] |
| 4.3 | Validation: residuals vs fitted, Q–Q, Durbin–Watson, leverage, Cook's D, transformations compared (log, √, 1/x), hold-out and k-fold RMSE | `LinearModel` (leverage, Cook's D, DW, standardized residuals, `CrossValidate`), `Transformations`, `Plotting/RegressionFigures.cs`; shown in both analyses | ln y on an exponential relation gives R² = 1 and predicts back in y units; Σhᵢᵢ = p; DW, Cook's D and leverage agree with statsmodels | [x] |
| 4.4 | Code (`statsmodels.api.OLS`, `get_prediction`, `get_influence`, `variance_inflation_factor`, `durbin_watson`, a hand k-fold loop; R `lm`, `confint`, `predict`, `cooks.distance`, `hatvalues`) | each analysis writes both | cross-check: 62 configurations pass against numpy/scipy/statsmodels (11 regression) | [x] |

## Phase 5 — ANOVA (Module 14) and release

**Status 2026-09-09: 5.1–5.4 built; the Store submission is prepared and 5.4 is all but done.** The name is reserved (`DeanEaglin.statistle`), the privacy page is live, the artwork pipeline runs off Ron's logo, and `artifacts/Statistle_1.0.0.0_x64_bundle.msixupload` is built with the real Partner Center identity and verified against its package family name. WACK passes 23/24. Outstanding: the screenshots. See `resources/STORE-SUBMISSION.md`.

**Status 2026-09-08: 5.1–5.3 built.** Two analyses over `Methods/Inference/Anova.cs`;
the studentized range (R's `ptukey`, AS 190) and the noncentral F (for power) written in
`Methods/Distributions/StudentizedRange.cs`. Tester zip 0.5.0. Note: the course page for Module
14 problem 1 quotes F ≈ 53.7 and p ≈ 5.85e-11, but on the data as listed scipy's `f_oneway` gives
F = 48.12 and p = 1.25e-9; the engine agrees with scipy, and the Tukey differences (37.7, 24.2,
13.5) match the page.

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 5.1 | One-way ANOVA: SS/df/MS table, F, p, η² (and ω², Cohen's f, power); Tukey HSD; Bartlett and Levene; Kruskal–Wallis; groups as columns or as factor + response | `Methods/Inference/Anova.cs`, `Distributions/StudentizedRange.cs`, `Analyses/OneWayAnova.cs` (six method classes: two sources, F test, Tukey, checks, Kruskal–Wallis) | `AnovaTests`: Module 14 problem 1 alloys (F, p, η², Tukey differences, power > 0.99); q_{0.05;3,27} = 3.506 and q_{0.05;4,20} = 3.958 from the table; λ = 0 is the central F | [x] |
| 5.2 | Two-way ANOVA from a table with two factor columns and a response (balanced; additive model when n = 1) | `Anova.TwoWay`, `Analyses/TwoWayAnova.cs`, `Plotting/AnovaFigures.cs` (boxes, means with intervals, interaction plot, residual Q–Q) | hand-worked 2 × 2 × 2 case (SS 338/98/18/8, F 169/49/9); unbalanced design refused with the cell named | [x] |
| 5.3a | First-run install: the demonstration project is written into `Documents\Statistle` when it is not there (never overwritten), so the Store and zip builds both start with something to open (Ron, 2026-09-08) | `Core/Document/SampleProjects.cs`, `App.xaml.cs` | `PathsTests`: installed once, an edited copy survives; verified in the published exe | [x] |
| 5.3b | Inno Setup installer for local testers: per-user by default (no admin), Start-menu entry, optional desktop icon and `.statistle` association, uninstaller; the exe gets an icon from the tile art (Ron, 2026-09-08) | `packaging/installer/Statistle.iss`, `packaging/make-installer.ps1`, `src/Statistle.App/Statistle.ico` | `artifacts/Statistle-<ver>-setup.exe`; silent install, launch and silent uninstall verified on the dev box, Documents untouched | [x] |
| 5.3c | Instructions tab: a full manual page per rail item — a hand-written guide (when to use, steps, reading the results, pitfalls, worked example, plots, code) in `GuideCatalog`, plus every entry, result and method generated from the metadata; the overview of Statistle when nothing is selected; Open in browser / Save as HTML or text (Ron, 2026-09-08) | `Core/Instructions/`, `App/Reporting/InstructionsDocumentBuilder.cs`, the Instructions tab | `InstructionsTests`: every shipped item has a guide with all its parts, every label and method is on its page, the overview lists the analyses in course order | [x] |
| 5.3 | Code (`scipy.stats.f_oneway`, `tukey_hsd`, `studentized_range`, `ncf`, `bartlett`, `levene`, `kruskal`; `statsmodels` `anova_lm`, `pairwise_tukeyhsd`; R `aov`, `anova`, `TukeyHSD`, `ptukey`/`qtukey`, `bartlett.test`, `kruskal.test`) | each analysis writes both | cross-check: 68 configurations pass (6 ANOVA) | [x] |
| 5.4 | Store: Partner Center identity, listing, privacy statement (nothing leaves the machine), `msixupload` | `packaging/`, `resources/` | submitted | **Prepared 2026-09-09.** Identity `DeanEaglin.statistle` reserved and PFN-verified; privacy policy live at `punchmonkeyserver.com/privacy/statistle`; version 1.0.0; `make-store-assets.ps1` generates every tile and listing image from `resources/images/Statistle_logo2048x2048.jpg` under WACK's 204800-byte cap; `app.manifest` declares PerMonitorV2; `make-msixupload.ps1` and `run-wack.ps1` ported from SMADA; listing copy, age rating, declarations and the `runFullTrust` justification written. WACK PASS 23/24 (2026-09-09) — both of SMADA's findings pre-empted. Left: screenshots, submit |
| 5.5 | Help ▸ About Statistle (new Help menu, a small About window): Ron's picture beside "Created by Dr. Ron Eaglin", what Statistle is for in three plain sentences, and the version read from the assembly (`StatistleVersion`) (Ron, 2026-09-25: in every program's About) | `App/Views/AboutWindow.xaml(.cs)`, `App/Images/dr-ron-eaglin.jpg` (256 px), `MainWindow.xaml` Help menu | opens from Help ▸ About Statistle; shows Version 1.0.0; build clean, 330 tests green; photographed through UI Automation | ⚠️ built, waiting for Ron to look |
| 5.6 | Help ▸ About links to the Microsoft Store page, Statistle's page and all programs. Ron, 2026-10-30: *"The Help->About on all the apps should have a link to the location of the app on the Windows App Store. They should also have a link to their subdirectory on SoftwarePlus.ai and a link to all programs."* Under the "Created by" card: "More about Statistle, with help and examples: softwareplus.ai/statistle", "Statistle on the Microsoft Store (updates, ratings and reviews): apps.microsoft.com/detail/9MV608S7M9X6", "All of Dr. Ron Eaglin's programs: softwareplus.ai" (→ softwareplus.ai/#programs); each opens the browser (Statistle itself still connects to nothing); window widened 560 → 620 so the Store address stays on one line | `App/Views/AboutWindow.xaml(.cs)` | build clean, 330 tests green; captured through UI Automation + PrintWindow (620 × 486, nothing cut off); the three links are keyboard-focusable with names a screen reader reads ("Statistle on the Microsoft Store", "All of Dr. Ron Eaglin's programs, softwareplus.ai") | ⚠️ Ron to look; ships with the next Store release |
| 5.7 | Help ▸ Check for a new version… (Ron, 2026-10-30, for Accessibility+: *"Is it possible on the Help menu to add a 'check for new version?'"* then *"Add this to all the Windows Store Apps."*). Just above About; asks the Microsoft Store (`StoreContext`, tied to the window) whether a newer Statistle is published; "You have the latest version of Statistle (1.0.0)" (the same version About shows), or asks before letting the Store download and install it; a copy not installed from the Store (the hand-test exe, the zip, the Inno installer) says it cannot check and offers the Store page; every error in plain words with the Store page as the way out. Copied from Accessibility+ (`StoreUpdates.cs`). The App project now targets `net10.0-windows10.0.19041.0` (the Windows Store API needs a Windows version; the manifest's MinVersion is already 10.0.19041.0). **Note:** the privacy policy and listing say Statistle "makes no network connections … checks for updates" never; this asks the Store only when the person chooses the item, but the wording needs Ron's decision before the release that ships it | `App/StoreUpdates.cs`, `MainWindow.xaml(.cs)` Help menu, `Statistle.App.csproj` | build clean, 330 tests green; hand-test exe published to `manual-test\` | ⚠️ Ron: try it in the Store-installed copy once the next version is out; decide the privacy/listing wording |
| 5.8 | NuGet audit: PDFsharp 6.2.4 brings in `System.Security.Cryptography.Pkcs` 8.0.1 (NU1902/NU1903, fatal under TreatWarningsAsErrors; the runtime-specific publish failed). A direct reference to Pkcs 10.0.13 replaces it (the fix Retirement+ used) | `Core/Statistle.Core.csproj` | build, tests and the `-r win-x64` publish clean | ⚠️ Ron: nothing to look at; ships with the next release |

## Phase 6 — Usability round before 1.1.0 (UX review 2026-10-31)

The pre-release UX review (`docs/ux-reviews/2026-10-31-pre-release.md`: 2 Critical, 7 Major, 9 Minor) and the
portfolio usability checklist (`..\USABILITY-CHECKLIST.md`, Ron 2026-10-31). **The next release is 1.1.0** (Ron,
2026-10-31). Every finding was checked in the code first; all of them were real, so none was skipped.

How an item is marked in this phase: *(no icon)* to do · ⚠️ built and tested, waiting for Ron (the action is in
the row) · ✅ verified by Ron. Ron's hand-test steps are in `docs/MANUAL-TESTING.md`, round 2026-11, part B.

| # | Task | Done when | Status |
|---|---|---|---|
| 6.1 | **C1: work is never lost by surprise.** One `ConfirmDiscard` runs before New, Open, Open the sample project, closing the window and the Store install: "Save changes to {file} before {closing / starting a new project / opening another project / opening the sample project / installing the new version}? Yes saves them, No discards them, Cancel goes back." For an untitled project, Yes asks where to save; a cancelled or failed save stops. The autosave in `%LOCALAPPDATA%\Statistle` now remembers its file and is offered back on start ("Statistle closed with unsaved work in {file}. Restore it?"); it is deleted when the work is saved or discarded. If the copy is damaged, Statistle asks whether to delete it. The Store message no longer says "Your work is kept". UX review 2026-10-31 | `WorkspaceSafetyTests` (clean leave, Cancel, No, Yes with and without a path, Open, recovery offered and declined, save removes it) | ⚠️ Ron: hand-test B1, B2 |
| 6.2 | **C2: undo belongs to one project.** New and a successful Open clear Undo (Edit ▸ Undo goes grey). UX review 2026-10-31 | `New_clears_undo`, `Open_clears_undo_so_the_old_project_cannot_overwrite_the_new_file` | ⚠️ Ron: hand-test B3 |
| 6.3 | **M3: what is typed is what is saved.** The focused box is committed at the start of Save, Save As, the save question, Copy code, Save code as, the PDF/CSV/XML exports and the report. This includes File-menu clicks: a menu is its own focus scope, so the window's logical focus is read. Enter commits a value box (the focus stays), and Esc puts back the value in use. If what was typed was refused, Save says so. UX review 2026-10-31 | `Save_first_commits_…`, `Closing_commits_…` | ⚠️ Ron: hand-test B4, including **File ▸ Save from the menu** with a value still in the box |
| 6.4 | **M4: a rejected entry says why.** It appears in red under the box and at the top of the help panel: "'{text}' is not a number, so Statistle is still using {old}. Correct it or press Esc." Series and tables name the line, and for tables the column: "Line {n}, column '{name}': '{cell}' is not a number. The paste was not used; your earlier data is kept. …" (`ColumnTable.TryParse` and `DataSeries.TryParse*` gained a `problem` out parameter). UX review 2026-10-31 | `ParseProblemTests`, `A_rejected_number_…`, `A_rejected_table_paste_…` | ⚠️ Ron: hand-test B5 |
| 6.5 | **M5: open and save failures show a message box**, not only the status line. Open also catches XML, access and format errors (no crash) and keeps the project that was open. Failed export and undo also show a message box. UX review 2026-10-31 | `A_file_that_will_not_open_…` | ⚠️ Ron: hand-test B6 |
| 6.6 | **M6: the first start points to the sample.** File ▸ Open the sample project puts the sample back in Documents\Statistle if it was deleted. The empty Properties tab says what Statistle is for and has Open the sample project and + Data table buttons; the empty-state help starts "New here? …". UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test A1, A2, B7 |
| 6.7 | **M7: no dead end without tables.** One-way and Two-way ANOVA, Multiple regression, Confidence interval, and the One- and Two-sample tests say "This analysis reads a data table, and the project has none yet. Click + Data table on the left, paste or Import your data, then come back and choose it here." (`DataSources.ChooseOrAdd`). A deleted table is no longer named by its number: "The data table this analysis read from has been deleted, …". UX review 2026-10-31 | `An_analysis_in_a_project_without_tables_…` | ⚠️ Ron: hand-test B8 |
| 6.8 | **M8: Delete is only for data tables.** It is greyed for an analysis, with a tooltip that says why (shown while greyed). It asks "Delete the data table '{name}'? …" and no longer pushes an undo step for nothing. The button and the menu both read "Delete selected data table…". The rail lights the button of the analysis that is open. UX review 2026-10-31 | `Delete_is_offered_for_a_data_table_only_and_asks_first` | ⚠️ Ron: hand-test A3, B9 |
| 6.9 | **M9: table Import… shows a preview first.** The Import window has a whole-table mode with a Sheet drop-down. It reads "n columns will be imported (k columns with no numbers left out), replacing the n columns the table has now. Edit ▸ Undo (Ctrl+Z) puts the old data back." The status line then says "Imported {n} columns from {file}, sheet '{sheet}'. …", and for a series "Imported {n} values from {file}. …". UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B10 |
| 6.10 | **Minor fixes**: the Report tab and Code tab texts; "Export selected item as XML (advanced)…"; plain plurals ("1 data table", "3 analyses", "2 columns"); "Save report as PDF…" in the File menu and on the Report tab, one name for both (it replaces "Save as PDF…", which sat next to Save As… in the File menu); "Copy code", "Save code as…", "Save instructions as…", "Open report in browser". The privacy wording is unchanged: 5.7 stays Ron's decision. UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B11 |
| 6.11 | **Checklist 1: local help.** The Help menu is in the portfolio order. Help for this screen (F1) opens the Instructions tab for the selected item, which now links to that item's page in the web help ("More on the web: softwareplus.ai/statistle/help/{page}"). Then come Help on the web (softwareplus.ai/statistle), Check for a new version… and About Statistle…. F1 in the Import window opens the data-tables help page. The Project tree says what it holds. UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B12 |
| 6.12 | **Checklist 2: every control is explained.** Every menu item, button, tab, radio button and drop-down has a tooltip. Greyed items say what enables them (`ShowOnDisabled`). Value boxes explain Enter, Tab and Esc. The rail buttons, value boxes and drop-downs have accessible names. UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B13 |
| 6.13 | **Checklist 6: the same as every program.** Message box titles read "Statistle — {subject}". About gains the help and privacy-policy links (softwareplus.ai/statistle/privacy/). Check for a new version shows "Asking the Microsoft Store…" with a busy cursor, asks the save question before installing, says "Windows closes Statistle to install it; open it again from the Start menu.", and puts technical text after "Details for support:". UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B14, D2–D4 |
| 6.14 | **Instructions now match the program** (the web help found both errors). The data-table steps say that Import… shows a preview where you choose the sheet, and that every column with numbers replaces the table's columns. Descriptive statistics no longer tells the student to use a "Calculate…" link, which no analysis offers; it says to choose the same column in the other analysis, or type in the mean and SD. UX review 2026-10-31 | `InstructionsTests` green | ⚠️ Ron: hand-test B15 |
| 6.15 | **Version 1.1.0** (Ron, 2026-10-31): `StatistleVersion` in `Directory.Build.props` and `packaging/AppxManifest.xml` (the pack scripts rewrite the manifest's Version from the props). The hand-test exe was rebuilt in `manual-test\`. | About shows 1.1.0 | ⚠️ Ron: hand-test round 2026-11 (all of `docs/MANUAL-TESTING.md`) |
| 6.16 | **Walkthrough and heuristics on the changed screens** (ux-reviewer agent, inline). Its findings: 1 Major (File ▸ Save from the menu did not commit the box, fixed in 6.3) and 6 Minor (the damaged recovery copy, message box titles, the save question before the Open dialog, Save after a refused entry, the error wording for the sample/export/undo, text-only columns in a table import). All are fixed. Visual checks are left for the hand-test (part E). UX review 2026-10-31 | build clean, 351 tests green (333 engine + 18 window/view model) | ⚠️ Ron: part E of the hand-test (150 %, smallest window) |

## References

| Document | What it is for |
|---|---|
| `docs/PLAN.md` | The strategy: what the student does, the module-to-analysis map, the architecture |
| `docs/METHODS-AND-SOURCES.md` | Every statistic with its formula, source, Python and R |
| `docs/MANUAL-TESTING.md` | Ron's hand-tests, newest round first, and the publish command for `manual-test\Statistle.exe` |
| `docs/ux-reviews/2026-10-31-pre-release.md` | The pre-release UX review that phase 6 answers |
| `resources/STORE-SUBMISSION.md` | Everything Partner Center asks for (identity, listing, WACK) |
| `../USABILITY-CHECKLIST.md` | The portfolio's usability checklist (Ron, 2026-10-31) |
