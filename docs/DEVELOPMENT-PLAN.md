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
| 5.7 | Help ▸ Check for a new version… (Ron, 2026-10-30, for Accessibility+: *"Is it possible on the Help menu to add a 'check for new version?'"* then *"Add this to all the Windows Store Apps."*). Just above About; asks the Microsoft Store (`StoreContext`, tied to the window) whether a newer Statistle is published; "You have the latest version of Statistle (1.0.0)" (the same version About shows), or asks before letting the Store download and install it; a copy not installed from the Store (the hand-test exe, the zip, the Inno installer) says it cannot check and offers the Store page; every error in plain words with the Store page as the way out. Copied from Accessibility+ (`StoreUpdates.cs`). The App project now targets `net10.0-windows10.0.19041.0` (the Windows Store API needs a Windows version; the manifest's MinVersion is already 10.0.19041.0). **Note:** the privacy policy and listing say Statistle "makes no network connections … checks for updates" never; this asks the Store only when the person chooses the item, but the wording needs Ron's decision before the release that ships it | `App/StoreUpdates.cs`, `MainWindow.xaml(.cs)` Help menu, `Statistle.App.csproj` | build clean, 330 tests green; hand-test exe published to `manual-test\` | ⚠️ Ron: try it in the Store-installed copy once the next version is out; decide the privacy/listing wording. **Wording changed 2026-10-31 per Ron (reflect the reality):** the policy (softwareplus.ai/statistle/privacy/, effective 2026-10-31, covers 1.1.0; `resources/PRIVACY-POLICY.md` in step) lists the two ways Statistle goes online, each only when chosen (Check for a new version asks the Microsoft Store; Help on the web and the About links open the browser); the listing (Privacy paragraph, "no network use" feature, privacy URL now softwareplus.ai), submission notes and terms say the same. ⚠️ Ron to read the page |
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
| 6.10 | **Minor fixes**: the Report tab and Code tab texts; "Export selected item as XML (advanced)…"; plain plurals ("1 data table", "3 analyses", "2 columns"); "Save report as PDF…" in the File menu and on the Report tab, one name for both (it replaces "Save as PDF…", which sat next to Save As… in the File menu); "Copy code", "Save code as…", "Save instructions as…", "Open report in browser". The privacy wording was changed 2026-10-31 (see 5.7). UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B11 |
| 6.11 | **Checklist 1: local help.** The Help menu is in the portfolio order. Help for this screen (F1) opens the Instructions tab for the selected item, which now links to that item's page in the web help ("More on the web: softwareplus.ai/statistle/help/{page}"). Then come Help on the web (softwareplus.ai/statistle), Check for a new version… and About Statistle…. F1 in the Import window opens the data-tables help page. The Project tree says what it holds. UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B12 |
| 6.12 | **Checklist 2: every control is explained.** Every menu item, button, tab, radio button and drop-down has a tooltip. Greyed items say what enables them (`ShowOnDisabled`). Value boxes explain Enter, Tab and Esc. The rail buttons, value boxes and drop-downs have accessible names. UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B13 |
| 6.13 | **Checklist 6: the same as every program.** Message box titles read "Statistle — {subject}". About gains the help and privacy-policy links (softwareplus.ai/statistle/privacy/). Check for a new version shows "Asking the Microsoft Store…" with a busy cursor, asks the save question before installing, says "Windows closes Statistle to install it; open it again from the Start menu.", and puts technical text after "Details for support:". UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B14, D2–D4 |
| 6.14 | **Instructions now match the program** (the web help found both errors). The data-table steps say that Import… shows a preview where you choose the sheet, and that every column with numbers replaces the table's columns. Descriptive statistics no longer tells the student to use a "Calculate…" link, which no analysis offers; it says to choose the same column in the other analysis, or type in the mean and SD. UX review 2026-10-31 | `InstructionsTests` green | ⚠️ Ron: hand-test B15 |
| 6.15 | **Version 1.1.0** (Ron, 2026-10-31): `StatistleVersion` in `Directory.Build.props` and `packaging/AppxManifest.xml` (the pack scripts rewrite the manifest's Version from the props). The hand-test exe was rebuilt in `manual-test\`. | About shows 1.1.0 | ⚠️ Ron: hand-test round 2026-11 (all of `docs/MANUAL-TESTING.md`) |
| 6.16 | **Walkthrough and heuristics on the changed screens** (ux-reviewer agent, inline). Its findings: 1 Major (File ▸ Save from the menu did not commit the box, fixed in 6.3) and 6 Minor (the damaged recovery copy, message box titles, the save question before the Open dialog, Save after a refused entry, the error wording for the sample/export/undo, text-only columns in a table import). All are fixed. Visual checks are left for the hand-test (part E). UX review 2026-10-31 | build clean, 351 tests green (333 engine + 18 window/view model) | ⚠️ Ron: part E of the hand-test (150 %, smallest window) |
| 6.17 | **The hand-test build crashed on the first data table or analysis it drew** ("Cannot find resource named 'RejectionText'": the red under-the-box message's style was defined after the row templates that use it, and a StaticResource must come first). The style now sits above the row templates; every other StaticResource in the program's XAML was checked by script for the same mistake (none). `WindowLoadTests` missed it because it draws no row: the new `RowDrawingTests` shows the window off screen over its own autosave folder, opens the sample project, selects every data table and every analysis (all five kinds of row: value, list, choice, series, table) and every tab, and fails on any exception; it fails on the old XAML with this very message. The tests share one WPF thread (`WpfHost`), and `App.OnStartup` opens no window unless Statistle itself is running, so a test can never ask about or delete the real recovery copy. UX review 2026-10-31 | `RowDrawingTests` green, 352 tests green (333 engine + 19 window/view model); the rebuilt exe opens the sample, a table and an analysis without a crash | ⚠️ Ron: hand-test B16 |
| 6.18 | **The save question's title** reads "Statistle — Save changes?", like every other box (it was only "Statistle"). UX review 2026-10-31 | build clean; capture `save-question.png` | ⚠️ Ron: hand-test B17 |
| 6.19 | **No empty brackets in the help title**: a field whose unit has no symbol (a probability, a plain number) shows "Confidence level", not "Confidence level ()". The unit beside the box follows the same rule. UX review 2026-10-31 | `RowDrawingTests` checks every row's help title | ⚠️ Ron: hand-test B18 |
| 6.20 | **PDF, CSV, code and instructions that cannot be written show a message box** "Statistle — Could not export": the file, the likely cause (open in a PDF viewer or Excel) and what to do, then "Details for support:"; the status line still says it too. UX review 2026-10-31 | build clean | ⚠️ Ron: hand-test B19 |
| 6.21 | **Screen readers hear each data table's name** in the Project tree (they heard the class name). The rail's analysis buttons already had their names. UX review 2026-10-31 | `RowDrawingTests` checks each tree node's automation name | ⚠️ Ron: hand-test B20 (Narrator) |
| 6.22 | **The two-column forms fit the window**: the default window is 1440 × 820 (never larger than the screen), the Project column is a little narrower, the results column takes the width that is left, and a result in words (Mode, Decision, Interval, Hypotheses, Rejection region, Group summaries, Tukey HSD pairs) spans value and unit and wraps instead of being cut off. No horizontal scrollbar at the default size. The help pictures on SoftwarePlus.ai were retaken at the new size. UX review 2026-10-31 | `RowDrawingTests`: at 1440 px no two-column form is wider than the window and no box ends past it | ⚠️ Ron: hand-test B21 |

---

# Statistics+ (Ron, 2026-11-11)

Ron: *"I have reserved Statistics+ in the Windows App store. I want to convert Statistle to Statistics+ as a learning
software (similar to PLC+) that follows the outline of the course https://reaglin.github.io/EGN3443/ with the
learn/example/exercise/reference for the topics associated with the EGN3443 course (and the possibility of adding
topics in the future to the same architecture)."*

Ron's answers the same day (D1–D4 in `docs/PLAN.md`, section *Statistics+*):
- **D1 Store:** the Statistics+ name goes on the **existing Statistle product** (Store ID 9MV608S7M9X6; identity
  `DeanEaglin.statistle` unchanged; installs and reviews kept), as Assessment+ did. Code names, namespaces, the
  `.statistle` file type and `Documents\Statistle` stay ("the program is called X+ in front of a person, and keeps
  its code name in the code").
- **D2 Workbench:** the calculator stays, as the **Workbench** section beside the learning path. The same analyses
  drive the Example and Exercise tabs.
- **D3 Learners:** **the same as PLC+.** PLC+'s `Learning.Core` becomes the shared package (as PLC+'s plan D5 said it
  would once a second program used it). It provides named learners, PIN-locked progress files, mastery for each
  outcome, and signed reports and result lines.
- **D4 Granularity:** **one item per course topic**, and a module check at the end of each module. That makes about
  100 items over Start here and Modules 1–14 (`docs/LEARNING-PATH.md`).

How an item is marked from here on: *(no icon)* to do · ⚠️ built and tested, waiting for Ron (the action is in the
row) · ✅ verified by Ron · ❌ dropped (with the reason).

## Phase 7 — The plan and the name

| # | Task | Done when |
|---|---|---|
| 7.1 | The learning path: every course module, topic and outcome mapped to an item, and the analysis behind each item's Example and Exercise; the gaps in the engine named | `docs/LEARNING-PATH.md` written. ⚠️ Ron: read the map, especially the numbering (the site's "Week N / Module N−1") and the module 9/10 overlap |
| 7.2 | The decisions and the architecture written into `docs/PLAN.md` (section *Statistics+*), this plan's phases 7–16, `CLAUDE.md` | committed. ⚠️ Ron: answer the open questions below |
| 7.3 | **The name in front of a person:** window title, Help ▸ About (heading, "More about Statistics+…", "Statistics+ on the Microsoft Store…"), message box titles "Statistics+ — …", Instructions overview, report and code headers, installer and tester readme. The manifest's DisplayName and VisualElements are **Statistics+** (it must match the reserved name exactly). Version **2.0.0** | every visible "Statistle" is gone (a test scans the XAML and the user-facing strings), and About shows Statistics+ 2.0.0 · **Built 2026-11-11.** 373 tests green; `ProgramNameTests`. ⚠️ Ron: hand-test P1 |
| 7.4 | The icon carries the "+" (the suite's theme): the Statistle mark with a "+" on its colour; `make-store-assets.ps1` regenerates every tile | Ron chooses from two or three drafts |

## Phase 8 — Learning.Core as a shared package

PLC+ owns `Learning.Core` and `Learning.Reports.Pdf` today. They have no references to PLC. The leftovers are cosmetic: "PLC+" in error messages, and the PLC-flavoured `Fault` and `TruthTable` question kinds.

| # | Task | Done when |
|---|---|---|
| 8.1 | Move the two projects and their tests into their own repository, **`..\LearningCore\`** (as `..\AiManager\` is), with its own `CLAUDE.md`, `docs/DEVELOPMENT-PLAN.md` and `build\pack.ps1` → `C:\nuget-local` (`Eaglin.Learning.Core`, `Eaglin.Learning.Reports.Pdf`, version 1.0.0) | the packages are in `C:\nuget-local`; the repository is on GitHub (private) · **Built 2026-11-11.** `..\LearningCore` (`eca6439`), 107 tests; packages 1.0.0 in `C:
uget-local`. ⚠️ Ron: whether to create the GitHub repository |
| 8.2 | Make the core subject-neutral: the app name comes from the options in every message; `Fault`/`TruthTable` become subject question kinds registered by the program (PLC+ registers them), and the core keeps choice, multiple, numeric, match and text | `ArchitectureTests` in the core: no subject words; PLC+'s 374 tests are still green · **Built 2026-11-11.** `LearningApp.Name`; `Fault`/`TruthTable` stay in the core for now (harmless; Statistics+ shows "needs PLC+" for them). ⚠️ Ron: nothing to look at |
| 8.3 | **PLC+ moves to the packages** (a task in PLC+'s plan; it ships with PLC+'s next release) | PLC+ builds from `C:\nuget-local`, all its tests are green, and its hand-test exe runs unchanged · In PLC+ plan 13.1 (`6d39dd6`), with its next release |
| 8.4 | Statistics+ references the packages | it builds · **Built 2026-11-11.** ⚠️ Ron: nothing to look at |

## Phase 9 — The learning window

The family's tree-and-tabs window (PLC+ `Learning.Shell` is the model), with Statistle's Workbench inside it.

| # | Task | Done when |
|---|---|---|
| 9.1 | Start screen **"Who is learning?"**: learners in `%ProgramData%\Statistle\Learners`, `.statprogress` files, PIN, recovery code, import and merge, backup reminders (Learning.Core) | `LearnerScreen` works as in PLC+; tests for the options · **Built 2026-11-11.** Ported from PLC+; `%ProgramData%\Statistle\Learners`, `.statprogress`; idle lock; Learner menu. ⚠️ Ron: hand-test P1, P2, P9 |
| 9.2 | The tree: **My progress**, **Hand in a report**, **Start here**, Modules 1–14 → items (each with its mark ★ ✓ ↺ ◐ ○), then **Workbench** (the data tables and analyses of an open `.statistle` project, as today) | the tree loads the content package; selecting an item opens the item page and selecting a Workbench node opens today's editor · **Built 2026-11-11.** The Course | Workbench strip (Ctrl+L, Ctrl+B) instead of a Workbench node in the tree. ⚠️ Ron: hand-test P3, P4 |
| 9.3 | The item page: **Learn / Example / Exercise / Reference** tabs (Ctrl+1–4), Previous/Next, Pop out for Example and Exercise | `RowDrawingTests`-style test opens every item and every tab without an exception · **Built 2026-11-11.** `CourseDrawingTests`; Pop out not yet. ⚠️ Ron: hand-test P5–P8 |
| 9.4 | **My progress** dashboard: path map, module checks, review suggested, outcomes, time on task (Learning.Core `Dashboard`) | shown for a learner with progress · **Built 2026-11-11.** ⚠️ Ron: hand-test P8 |
| 9.5 | The Workbench without a learner: File ▸ New/Open still work; a student who only wants the calculator can choose "Just the Workbench" on the start screen | the 1.1 hand-tests (round 2026-11) still pass inside the Workbench · **Built 2026-11-11.** ⚠️ Ron: hand-test P10 |
| 9.6 | UX: walkthrough and heuristics (ux-reviewer) on the start screen, tree, item page and dashboard; `USABILITY-CHECKLIST.md` | review recorded, fixes made · **Built 2026-11-11.** `docs/ux-reviews/2026-11-11-course.md`: 5 Major, 8 Minor, all fixed. ⚠️ Ron: the visual checks in round 2.0 |

## Phase 10 — The statistics workspace and the engine gaps

| # | Task | Done when |
|---|---|---|
| 10.1 | **Content format** for Statistics+ (`docs/CONTENT-FORMAT.md`, from PLC+'s): `content/egn3443/manifest.json`, `modules/NN-slug/module.md` + items, `reference/`, `quizzes/`, and **`tasks/*.json`**. A task names an analysis, its settings, a dataset (inline columns or `datasets/*.csv`), what the student sees locked or open, and the **results asked** (by property name, with a tolerance taken from the property's Decimals) | the format is documented, and an example task loads · **Built 2026-11-11.** `docs/CONTENT-FORMAT.md`. ⚠️ Ron: nothing to look at |
| 10.2 | **`StatsTaskRunner : IChallengeRunner`** (domain `stats`): it builds the analysis from the task, calculates the expected results with the engine, and grades the student's typed results (or their settings: test chosen, tail, α) | `TaskRunnerTests`: a right answer passes, a rounding within tolerance passes, a wrong one fails with a sentence naming the result · **Built 2026-11-11.** `TaskRunnerTests` (10). ⚠️ Ron: nothing to look at |
| 10.3 | **Example workspace**: the analysis editor (entries beside results, Plots, Code) inside the Example tab, loaded from the task's example, editable, "Put it back" restores it | every Example in the package opens · **Built 2026-11-11.** Example as a worked page (not the editor itself); **Open in the Workbench** for changing inputs (`OpenTaskTests`). ⚠️ Ron: hand-test P5, P6 |
| 10.4 | **Exercise workspace**: the dataset and the analysis set up as the task says; boxes for the asked results; **Check my answers**; hints; after a pass, the Code tab and the Report show the full worked solution; the passing work is the report evidence (`ProgramEvidence`) | Exercise_Checked records progress as PLC+ does · **Built 2026-11-11.** ⚠️ Ron: hand-test P7 |
| 10.5 | Lesson widgets: `[[histogram: data]]`, `[[table: dataset]]`, `[[curve: normal 0 1 shade -1.96 1.96]]`, `[[code: task]]` (Python and R from the task, tabs) | rendered in Learn pages; the validator checks the arguments · **Built 2026-11-11.** `[[figure]]`, `[[data]]`, `[[results]]`, `[[code]]` (no `curve`: a figure of a task does it). ⚠️ Ron: hand-test P5 |
| 10.6 | Validator (`tools/ContentValidator`): Learning.Core's generic checks plus the subject hooks (the analysis exists, every asked result is an output of that analysis, datasets parse, every item's objectives exist) — run as a test on the shipped package | it fails on a broken package with one sentence per problem · **Built 2026-11-11.** As a test (`ContentTests`), not a separate tool yet |
| 10.7 | **New analysis: Simulation** — dice, coins, cards, darts and sampling from a distribution; the running proportion against theory (Law of Large Numbers), many samples' x̄ (CLT), 100 intervals' coverage, the familywise error of many t-tests; seeded, so a task is repeatable; Python (numpy.random) and R | `SimulationTests` (seeded results; the proportion converges); cross-check · **Built 2026-11-11.** `Simulation` (in the rail after Sampling distribution): coins, dice, cards (total with an ace 1 or 11 — the three-card 21 — hearts or aces), darts (π/4), sample means from a normal, uniform or exponential population (CLT), the coverage of t intervals and the familywise error of many t-tests; the running proportion against theory and the outcomes against theory (or the darts, or the first 100 intervals) in Plots. Seeded by the program's own xoshiro256** (`Core/Methods/Chance/RandomStream.cs`, checked against the reference outputs), so a task's numbers never change; random results carry `[SimulatedResult]`: the Python/R print them under the same labels with a comment that their counts differ, and the cross-check compares only the theory. `SimulationTests` (31), the Python cross-check (11 configurations) and the R scripts run by hand. ⚠️ Ron: open Simulation in the Workbench and try each experiment and its plots |
| 10.8 | **New analysis: Chi-square tests** — goodness of fit and independence (contingency table); expected counts, χ², df, p, the warning for expected counts < 5; `scipy.stats.chisquare`/`chi2_contingency`, R `chisq.test` | worked cases from M&R; cross-check  · **Built 2026-11-11.** `ChiSquareTest` ("Chi-square tests", in the rail after Two-sample tests): **goodness of fit** (observed counts typed or from a column; equal proportions or entered proportions/weights, always rescaled to add to 1, with a note when they did not add to 1; m estimated parameters for df = k − 1 − m; category names) and **independence/homogeneity** (a contingency table typed with a header line of column names — or the columns of a data table, `"CountsTable": "@Table/"` in a task — with row names; no continuity correction by default, Yates as a choice on a 2 × 2 table); results expected counts (with each (O − E)²/E), smallest expected count, the number below 5 and the expected-count check (a warning, not a refusal), χ², df, critical value, p, decision, Cramér's V. Figure 1 the χ² distribution with the rejection region and the observed χ²; figure 2 observed and expected counts side by side. Python (`chisquare`, `chi2_contingency`) and R (`chisq.test`) print every result. Tests: `ChiSquareTests` (15: M&R Ex. 9-12 χ² = 2.94 and Ex. 9-14 χ² = 49.63, invariants, Yates, problems, tables, figures, round-trip), `ChiSquareTaskTests` (4), 8 configurations in the Python cross-check; the R scripts run by hand (all agree). ⚠️ Ron: open Chi-square tests in the Workbench — both tests, typing a contingency table, and the two plots; whether the χ² should stay uncorrected by default (as now, matching the hand formula) |
| 10.9 | **Weibull** in Continuous distribution and a **fit check** on a data column (histogram with the fitted curve, Q–Q plot, the parameters by maximum likelihood) | worked case; cross-check against `scipy.stats.weibull_min.fit` |
| 10.10 | **Hold-out split** in Multiple regression's validation (seeded train/test share, test RMSE beside k-fold and LOOCV) | test; cross-check |
| 10.11 | **Control limits** (x̄ ± 3σ, individuals chart) as a figure in Descriptive statistics | figure test · **Built 2026-11-11.** Results Lower/Upper control limit (x̄ ∓ 3s) and Points outside the control limits; the control chart is **figure 3** (after the histogram and the box plot, so the pilot's figure numbers stand); Python and R lines. `DescriptiveAdditionsTests`, Python cross-check. ⚠️ Ron: whether s (as now) or the moving range / d₂ of quality-control texts should set the limits — the help names both |
| 10.12 | `METHODS-AND-SOURCES.md` rows for every new statistic | complete rows · **Built 2026-11-11.** Rows for the mode as a number, the number of outliers, the control limits, and a Simulation section (random stream, experimental probability, coins, dice, cards, darts, sample means, coverage, familywise error), each with the test that holds it |
| 10.13 | **Mistakes an exercise recognizes** (from writing the pilot): a task may name the result a common mistake gives, e.g. `"mistakes": { "Variance": { "result": "PopulationVariance", "says": "You divided by n; the sample variance divides by n − 1." } }` | a typed population variance gets that sentence · **Built 2026-11-11.** `mistakes` per asked result: one or a list of `{ result | scale | value, says }` (`docs/CONTENT-FORMAT.md`); a wrong answer within tolerance of a mistake's number reads "Label: x is not right. <says>"; the validator refuses an unknown result, a key not asked, and a mistake accepted as the right answer. Pilot: 2.2 (median for mean, mean for median, unsorted median) and 2.3 (÷ n for variance and SD, SD for variance and back). Tests: `TaskRunnerTests` (5), `ContentTests.Content_every_mistake_of_the_pilot_is_recognized`. ⚠️ Ron: hand-test P11 — are the sentences what you would say? |
| 10.14 | The validator warns when two asked results lie within each other's tolerance (the bolts' mean and median were 0.09 mm apart) | warning on such a task · **Built 2026-11-11.** The Learning Core's validator has only problems, so `StatsContent.Warnings()` is a separate list: the content test prints it and does not fail; the pilot has none. Test: `TaskRunnerTests.Two_asked_results_within_each_others_tolerance_give_a_warning`. ⚠️ Ron: nothing to look at (an author's tool) |
| 10.15 | Widgets in quiz prompts (`[[figure: task 1]]` in a question), so "classify the skew" can show the picture | a check question with a histogram · **Built 2026-11-11.** `[[figure/data/results/code: …]]` on its own line in a question's prompt is drawn in the question (and in its explanation, under the feedback); the validator checks them as in Learn pages. Pilot: `m2-distribution.json` q9 shows the sensor reply-time histogram (`st.m2.selecting.eg 1`), positively skewed. Tests: `ContentTests.Content_a_widget_in_a_question_is_checked_like_one_in_a_Learn_page`, `CourseDrawingTests.A_question_with_a_figure_draws_the_figure_in_the_quiz`. ⚠️ Ron: hand-test P12 |
| 10.16 | A numeric "number of outliers" result, so outliers and the mode can be asked in an exercise | result in Descriptive statistics · **Built 2026-11-11.** `OutlierCount` ("Number of outliers", the same type-7 fences as the Outliers list) and `ModeValue` ("Mode, when there is one": the single most frequent value; blank for no mode or a tie — the text `Mode` stays as it was, and as text it cannot be asked). `DescriptiveAdditionsTests`, Python cross-check. ⚠️ Ron: nothing to look at |

## Phase 11 — Content: Start here and Modules 1–4

Each module is written in the order of `docs/LEARNING-PATH.md`. For every topic: the Learn page, an Example on a dataset like the exercise's, an Exercise task or quiz, the Reference, the objectives from the course's MO list; then the module check. Datasets come from the course, from Montgomery & Runger, or are made for the task (and say so).

| # | Task | Done when |
|---|---|---|
| 11.1 | **Module 2 first, as the pilot** (Descriptive statistics; one analysis, five items, a check) | Ron reads it in the program and says whether the shape is right before the rest is written · **Written 2026-11-11** (2.1–2.6: 5 Learn pages, 6 reference pages, 9 tasks, 4 quizzes; every number checked). ✅ **Ron, 2026-11-11: "The pilot is good, we can now continue with the next Phase."** |
| 11.2 | Start here (3 items) | validator clean · **Written 2026-11-11** (module 0, `modules/00-start-here`: 0.1–0.3, 3 Learn pages, 2 tasks, 2 quizzes, 3 reference pages; every number checked; only what the program has is described, no reports to hand in). ⚠️ Ron: read 0.1–0.3 in the Course, especially whether "0. Start here" as module 0 reads well and whether its three outcomes (adapted from the course's Week 1 MOs, which are about the LMS) are right UX: walkthrough + heuristics (`docs/ux-reviews/2026-11-11-modules-0-4.md`), 7 Major and 11 Minor fixed. |
| 11.3 | Module 1 (6 items + check) | validator clean · **Written 2026-11-11** (1.1–1.7: 6 Learn pages and the check, 10 tasks, 5 quizzes, 6 new reference pages; every number checked; 1.4 and 1.5 are labeled previews; 1.6 works control limits x̄ ± 3s by hand, the program's control-limit figure to be added when it exists). ⚠️ Ron: read 1.1–1.7, the check's ten questions, and 1.6's quality-control content UX: walkthrough + heuristics (`docs/ux-reviews/2026-11-11-modules-0-4.md`), 7 Major and 11 Minor fixed. |
| 11.4 | Module 3 (5 + check) | validator clean · **Written 2026-11-11** (`modules/03-probability`: 5 items + check; 9 tasks — 2 exercises graded by the engine (3.3 two dice "sum ≥ 10": the theory by counting and the seed-1 simulation read in the Workbench; 3.4 bridge damage: P(A ∪ B), P(A ∣ B), P(B ∣ A)), both with recognized mistakes (only "= 10" counted, sums treated as equally likely, P(A) + P(B) without the overlap, the wrong denominator), and 7 examples/figures (Simulation: coins ×2, two dice, darts, three-card 21; Probability rules: joint known, Bayes); 4 quizzes, 31 questions (check: 10 over MO1–MO5, 80 %, the three-card 21 as Q6–Q7 with its running-proportion figure); 4 reference pages; every number in prose, hints, quiz answers and explanations checked (Python enumeration of all 22 100 three-card hands: 2012 total 21; the engine for the rest). Module 4 now needs `st.m3`. ⚠️ Ron: read the module, especially 3.3 (the exercise asks a seeded simulation count, read in the Workbench), 3.5 (Bayes, expected cost — the course's topic 5 names Bayes but doesn't work it) and the check's three-card 21 questions — they give the experimental 0.093 and the theory 2012/22 100, and the Workbench's Code tab for that simulation is a full answer to the Assignment 3 Monte Carlo program: say whether that is all right UX: walkthrough + heuristics (`docs/ux-reviews/2026-11-11-modules-0-4.md`), 7 Major and 11 Minor fixed. |
| 11.5 | Module 4 (6 + check) | validator clean · **Written 2026-11-11** (`modules/04-discrete`: 6 items + check; 9 tasks — 3 exercises graded by the engine (E and Var from a PMF, binomial, Poisson) and 6 examples (typed PMF ×3, binomial, Poisson, hypergeometric, geometric); 4 quizzes, 32 questions (check: 10 over MO1–MO5, 80 %); 5 reference pages; every number in the prose and the quiz answers checked against the engine's models; prerequisite `st.m2` until Module 3 exists). The data view now shows a typed table's x beside its P(x) (`TaskWorkspace.DataColumns`). ⚠️ Ron: read the module, especially 4.6 (six models, after the Distribution Detective game but with new cases) and the check; say whether the exercises may use the group assignment's numbers (they don't now) UX: walkthrough + heuristics (`docs/ux-reviews/2026-11-11-modules-0-4.md`), 7 Major and 11 Minor fixed. |

## Phase 12 — Content: Modules 5–9

| # | Task | Done when |
|---|---|---|
| 12.1 | Module 5 (6 + check) | validator clean |
| 12.2 | Module 6 (5 + check) | validator clean · **Written 2026-11-11** (`content/egn3443/modules/06-sampling`: 5 items + check; 11 tasks — 3 graded exercises (σ/√n and P(x̄ ≤ a) from a skewed population; SE and the middle 95 % at 4 × n; z and P(x̄ ≥ a) for "chance or a shift?") with 13 recognized mistakes, a Simulation of the CLT from an exponential population at n = 2, 5 and 30; quizzes 4 + 5 practice and a 10-question check, every question with lookAgain; 2 new reference pages; validator clean, no warnings; every number checked against the engine or Python). Prerequisite is `st.m4` until Module 5 exists. ⚠️ Ron: read 6.3 (the CLT pictures and the n ≥ 30 table) and 6.5 (the \|z\| guide: "about 2 unusual, 3 or more very unlikely"); "n to halve the SE" is asked in the check (q5, q6), not in an engine-graded exercise, because a task can only ask results and the given values show n |
| 12.3 | Module 7 (7 + check) | validator clean |
| 12.4 | Module 8 (7 + check) | validator clean |
| 12.5 | Module 9 (6 + check) | validator clean |

## Phase 13 — Content: Modules 10–14

| # | Task | Done when |
|---|---|---|
| 13.1 | Module 10 (6 + check) | validator clean |
| 13.2 | Module 11 (6 + check) | validator clean |
| 13.3 | Module 12 (5 + check) | validator clean |
| 13.4 | Module 13 (6 + check) | validator clean |
| 13.5 | Module 14 (6 + check) | validator clean |
| 13.6 | `docs/OUTCOMES.md`: every course outcome with the items and questions that assess it (as PLC+'s) | no outcome without an item |

## Phase 14 — Reports and handing in

| # | Task | Done when |
|---|---|---|
| 14.1 | **Hand in a report**: a signed, tagged PDF/A progress report per module check or of everything (Learning.Reports.Pdf); the evidence picture is the exercise's analysis (results table and figure) | a report opens in a PDF reader and File ▸ Check a report says it is genuine |
| 14.2 | The signed result line `STAT1|Name|Module 4 check|Complete, score 92%|date|…` and Send to my instructor; class codes | `ResultLine.Check` accepts it |
| 14.3 | Help page on softwareplus.ai for instructors (checking reports and lines) | deployed |

## Phase 15 — Release 2.0.0 as Statistics+

| # | Task | Done when |
|---|---|---|
| 15.1 | Full UX sweep (ux-reviewer, full-sweep mode) against `USABILITY-CHECKLIST.md`; fixes | review in `docs/ux-reviews/` |
| 15.2 | `docs/MANUAL-TESTING.md` round "2.0 — Statistics+" and the hand-test exe in `manual-test\` | ⚠️ Ron's hand-test |
| 15.3 | Store: manifest DisplayName Statistics+, listing text (a learning program for the course, with the Workbench), screenshots, privacy policy (learner files stay on the computer; reports are made only when asked), `.msixupload`, WACK; Partner Center: Product name Statistics+ | `resources/STORE-SUBMISSION.md` updated; WACK PASS |
| 15.4 | SoftwarePlus.ai: the program's page as Statistics+ (the folder stays `statistle/`, Q2), features, a sample item, the help, the privacy page, the home tile; `deploy.ps1 -Only …` | deployed |
| 15.5 | Tag `v2.0.0` once live; the portfolio `CLAUDE.md` updated | tagged |

## Phase 16 — Later topics on the same architecture

| # | Task | Done when |
|---|---|---|
| 16.1 | A second content package can be dropped beside `egn3443` and shows as more modules (the loader reads every package under `content\`) | a test package loads |
| 16.2 | First later topic (Ron to choose: Monte Carlo and risk, SPC control charts, design of experiments, reliability) | — |

## Open questions for Ron (Statistics+)

| # | Asked | Question | Answer |
|---|---|---|---|
| Q1 | 2026-11-11 | Has **Statistle 1.1.0** gone to the Store? If not, should it go first as a last Statistle release, or wait and ship everything as Statistics+ 2.0.0? | **Wait:** everything ships as Statistics+ 2.0.0; 1.1.0 is not submitted on its own (Ron, 2026-11-11) |
| Q2 | 2026-11-11 | The SoftwarePlus.ai folder: move to **`softwareplus.ai/statistics/`** with `statistle/` redirecting there (recommended), or keep `statistle/`? | **Keep `softwareplus.ai/statistle/`** (Ron, 2026-11-11); the page is retitled Statistics+ (15.4) |
| Q3 | 2026-11-11 | Where `Learning.Core` lives: its own repository `..\LearningCore\` packed to `C:\nuget-local`, as AI Manager is (recommended), or packed from inside PLC+? | **Own repository `..\LearningCore\`**, packed to `C:
uget-local` (Ron, 2026-11-11) → phase 8 |
| Q4 | 2026-11-11 | Should there be an **instructors' review site** like PLC+'s (softwareplus.ai/…/review/, a comment box per item) for colleagues teaching EGN3443? | **Not yet**; may be added as usage grows (Ron, 2026-11-11) |
| Q5 | 2026-11-11 | Datasets: may the Examples and Exercises use the course's assignment data and the datasets in its Colab notebooks, or only Montgomery & Runger and made-up data? | **Yes**, the course's assignment and Colab data may be used (Ron, 2026-11-11) |
| Q6 | 2026-11-11 | Is the course's "Additional Advanced Topics" module in the path at 2.0, or kept for later (phase 16)? | **Later** (phase 16), not in 2.0 (Ron, 2026-11-11) |
| Q8 | 2026-11-11 | The Workbench project belongs to the computer, as in 1.1, so after Sign out the next student sees it. Should Sign out offer to save and clear it (or should each learner have their own Workbench project)? | **2026-11-11, Ron:** "It is OK for students to see who else might have logged in, as long as they don't have the pin." Kept as is: the Workbench project stays the computer's; a learner's progress stays behind their PIN. |
| Q7 | 2026-11-11 | AI: PLC+ has none in its first release. Statistics+ has none either (the Workbench never had any). Is that right for 2.0? | **Correct, no AI (yet).** Once a use case is made for AI in either application (Statistics+ or PLC+) it might be added (Ron, 2026-11-11) |

## References

| Document | What it is for |
|---|---|
| `docs/LEARNING-PATH.md` | Statistics+: every course module, topic and outcome mapped to an item and its analysis |
| `docs/PLAN.md` | The strategy: what the student does, the module-to-analysis map, the architecture |
| `docs/METHODS-AND-SOURCES.md` | Every statistic with its formula, source, Python and R |
| `docs/MANUAL-TESTING.md` | Ron's hand-tests, newest round first, and the publish command for `manual-test\Statistle.exe` |
| `docs/CONTENT-FORMAT.md` | How Course content is written: modules, items, tasks, widgets, quizzes |
| `docs/ux-reviews/2026-11-11-course.md` | The UX review of the Course (plan 9.6) |
| `docs/ux-reviews/2026-10-31-pre-release.md` | The pre-release UX review that phase 6 answers |
| `resources/STORE-SUBMISSION.md` | Everything Partner Center asks for (identity, listing, WACK) |
| `../USABILITY-CHECKLIST.md` | The portfolio's usability checklist (Ron, 2026-10-31) |
