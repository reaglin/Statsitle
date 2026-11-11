# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

> **Becoming Statistics+ (Ron, 2026-11-11).** The Statistics+ name goes on this Store product, and the program
> becomes a learning program like PLC+. It follows EGN3443 topic by topic, and each topic has Learn / Example /
> Exercise / Reference tabs. The calculator stays as the Workbench. Learners and reports come from PLC+'s
> `Learning.Core`, which becomes a shared package. Read `docs/LEARNING-PATH.md`, `docs/PLAN.md` (section
> *Statistics+*) and `docs/DEVELOPMENT-PLAN.md` phases 7–16. The code names stay Statistle.
>
> **Built 2026-11-11 (phases 7–11 to the pilot):** the name and 2.0.0; `src/Statistle.Learning` (tasks, `StatsTaskRunner`:
> answers calculated by the analysis, `StatsContent` + validator); `src/Statistle.App/Learning/` (Who is learning?,
> `CoursePage`, `TaskWorkspace`, Markdown and quiz views, ported from PLC+); the Course | Workbench strip; the Learner
> menu and idle lock; `content/egn3443` with the **Module 2 pilot** (`docs/CONTENT-FORMAT.md`). Ron approved the pilot
> 2026-11-11 (plan 11.1 ✅). **The same day, phase 11:** Start here (module 0) and Modules 1, 3 and 4 written; the
> **Simulation** analysis (seeded xoshiro256**, so a task's answers never change), control limits and the number of
> outliers in Descriptive statistics; recognized mistakes, `wrong`, `lookAgain`, figures in questions.
> **Phase 12 the same day:** Modules 5–9 written; **Chi-square tests** (new analysis), **Weibull** and a **fit to a
> data column** (Continuous distribution); every analysis's generated R is now run in R by the tests (it found the
> 'two-sided' and pstats bugs). ⚠️ Ron's hand-test round 2.0 (parts P, Q, R, S) covers all of it; modules 10–14 follow
> (phase 13). Figures of every task as PNGs: `STATISTLE_FIGURE_SHOTS=<folder> dotnet test tests/Statistle.App.Tests --filter FigureShots`.
> Pictures of the Course for checking by eye: `STATISTLE_COURSE_SHOTS=<folder> dotnet test tests/Statistle.App.Tests --filter CourseShots`.

**Statistle** — a Windows 10/11 desktop app that calculates the statistics of Ron's course
EGN3443 *Probability and Statistics for Engineers* (`github.com/reaglin/egn3443`), explains
every input and result for the student, and writes the **Python and R** that reproduce the data
and the analysis. WPF on .NET 10, free on the Microsoft Store. Started 2026-09-08 from a copy of
SMADA's core (`..\SMADA10`).

**Status: phases 0–5 built (except 5.4, the Store), 2026-09-08** — every rail item has an Instructions tab (`Core/Instructions/GuideCatalog.cs` holds the hand-written guide per item; the rest of the page is generated from the metadata, and `InstructionsTests` fails for an analysis without a guide). Descriptive statistics, the Code tab, the Module
3–6 analyses (Probability rules, Discrete distribution, Continuous distribution, Sampling
distribution), the Module 7–10 analyses (Confidence interval, Sample size, One-sample test,
Two-sample test — with Shapiro–Wilk, Levene, Mann–Whitney and Wilcoxon written in Core to
match scipy) the Module 11–13 regressions (Simple linear regression with transformations,
Multiple regression with forward/backward selection and k-fold CV, both over one OLS engine
`Methods/Regression/LinearModel.cs` on MathNet), and the Module 14 ANOVAs (One-way with Tukey
HSD, Bartlett, Levene, Kruskal–Wallis and power; Two-way for balanced designs) over
`Methods/Inference/Anova.cs` and a `StudentizedRange` written to match R's ptukey. 325 tests
green; the Python cross-check runs 68 configurations (needs numpy, scipy ≥ 1.11, statsmodels).
No GitHub remote yet.

**Store submission prepared 2026-09-09** (task 5.4): identity `DeanEaglin.statistle`, version 1.0.0, a live privacy page, the full listing copy, and a PFN-verified `.msixupload`. Everything Partner Center asks for is in `resources/` — start with `resources/STORE-SUBMISSION.md`. `packaging/` gained `make-store-assets.ps1` (all icons, tiles and listing images from `resources/images/Statistle_logo2048x2048.jpg`), `make-msixupload.ps1` and `run-wack.ps1`, all ported from SMADA. WACK passes (23/24; the one FAIL is the optional "Blocked executables", which SMADA also passed overall with). Left to do: capture the screenshots and submit.

### Read these first

| File | What it is |
|---|---|
| `docs/DEVELOPMENT-PLAN.md` | **The work breakdown** — numbered tasks per phase in course order, with acceptance criteria. Start here. |
| `docs/PLAN.md` | The strategy — what the student does, the module-to-analysis map, settled decisions, architecture. |
| `docs/METHODS-AND-SOURCES.md` | The register: every statistic, its formula, its source, the Python and R that reproduce it, the case that holds it. An analysis is not done until its rows are complete. |

## Solution structure

```
Statistle.sln
├── src/Statistle.Core/     net10.0. Never references a UI assembly (ArchitectureTests).
│   Persistence/   StatistleObject (reflection XML + change notification), StatistlePropertyAttribute
│                  (Label, ReportLabel, Decimals, Unit=ValueKind, Role, Min, Max, Help…), [StatistleHelp],
│                  DataSeries, ColumnTable (the data table's columns as text; ITextPersisted), TableImport,
│                  EnumLabel, IHasChoices, ILinksTo, IReportsProblem, LoadDiagnostics
│   Domain/        DataTable (the one artifact; also an ICodeSource) + DataSources (column pick-lists)
│   Document/      StatistleDocument (.statistle zip: project.xml, tables/, analyses/), StatistlePaths
│   Methods/       ICalculationMethod, MethodCatalog, MethodScope, IChoosesMethod;
│                  Descriptive/SummaryStatistics (Module 2); Distributions/DiscreteModels,
│                  ContinuousModels (over MathNet.Numerics); Inference/Inference.cs (Intervals,
│                  HypothesisTests, power) and Nonparametric.cs (rank tests, Shapiro–Wilk AS R94,
│                  Levene) — verified against M&R examples and scipy; Statistics/ (Regression)
│   Code/          CodeLanguage, ICodeSource, CodeText (identifiers, numbers, vectors, prints, imports),
│                  ScriptBuilder (one script in either language: Assign/Print/Either)
│   Analyses/      Analysis (base), AnalysisCatalog, DescriptiveStatistics, ProbabilityRules,
│                  DiscreteDistribution, ContinuousDistribution, SamplingDistribution,
│                  ConfidenceInterval, SampleSize, OneSampleTest, TwoSampleTest, SimpleRegression
│                  — each family/kind is a method class declaring its parameters; a DataSource
│                  (summary entered / data columns) method Generates + locks the summaries
│   Plotting/      IPlotSurface (PlotSpec, Line/Marker/Bar/Polygon series), IHasFigures,
│                  DescriptiveFigures (histogram + normal curve, box plot), DistributionFigures
│                  (PMF/CDF, density with shaded area, population vs sampling curves),
│                  InferenceFigures (interval bar, reference distribution with rejection region
│                  and observed statistic, two box plots), RegressionFigure
│   Reporting/     Report (+ Appendices), text/HTML/PDF writers, CsvExport
│   Units/         ValueKind (None, Percent, Probability, Count) — no conversions, labels only
├── src/Statistle.App/      net10.0-windows, WPF. Rail | tree | Properties / Report / Plots / Code
│   MainWindow     generic editor with the help panel (75/25), ResultsBeside two-column layout
│   ViewModels/    WorkspaceViewModel (incl. the Code tab: IsPython/IsR, CodeText, Copy, Save as),
│                  PropertyRow, RelayCommand, RowTemplateSelector, converters
│   Plotting/      ScottPlotSurface (all ScottPlot types), FigurePanel, ScottPlotFigureRenderer
│   Reporting/     ReportDocumentBuilder (flow document, code appendices in Consolas)
│   Views/         ImportDialog
├── tests/Statistle.Tests/  xUnit: framework, DataTable/ColumnTable, SummaryStatistics (Excel's
│                  documented SKEW/KURT example, R fivenum/quantile), StatisticsEngine (invariants),
│                  CodeGeneration (labels match, table variables, report appendices, Python cross-check
│                  when numpy+scipy are present), Harness/ (Invariants, Snapshot, WorkedCases)
├── samples/       demo-statistics.statistle — built by `Core/Document/SampleProjects.cs` (written here by
│                  `Statistle.exe --write-samples samples`; installed into Documents\Statistle on first run)
├── packaging/     AppxManifest.xml, pack.ps1, Assets (placeholder tiles) — the MSIX for the Store;
│                  make-tester-package.ps1 (zip) and make-installer.ps1 + installer/Statistle.iss
│                  (Inno Setup, per-user) for local testers; tester/README.txt goes in both
└── manual-test/   Statistle.exe, the hand-test build (gitignored)
```

## Build, test, run

```powershell
dotnet build Statistle.sln
dotnet test Statistle.sln
dotnet run --project src/Statistle.App
./packaging/pack.ps1                       # signed dev MSIX in artifacts/
./packaging/make-tester-package.ps1        # artifacts/Statistle-<version>-win-x64.zip for testers
./packaging/make-installer.ps1             # artifacts/Statistle-<version>-setup.exe (Inno Setup 6 required)
```

Hand-test build (framework-dependent; the machine has the .NET 10 runtime):

```powershell
dotnet publish src/Statistle.App/Statistle.App.csproj -c Release -r win-x64 `
    --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o manual-test
Move-Item manual-test/Statistle.App.exe manual-test/Statistle.exe -Force
```

The Python cross-check test (`CodeGenerationTests`) runs the generated script with `python`
if `import numpy, scipy` succeeds, and does nothing otherwise. The system Python here has
neither; a scratch venv was used to run it by hand.

## The ideas that carry the design

1. **Declare it on the property, never in a form.** `[StatistleProperty(Label, ReportLabel,
   Decimals, Unit, Role, Min, Max, Help)]` gives a property its editor row, its help-panel text,
   persistence, a report line and CSV. `[StatistleHelp]` on the class is the item's overview.
   Every property and class must have help — `LabelAndHelpTests` fails otherwise. Write it for
   an engineering student: what it is, when to use it, typical values, the Python/R that gives it.
2. **An analysis is a class; a method is a class.** Derive from `Analysis` and it is in the rail
   (`AnalysisCatalog`, by reflection). `CalculationMethod<T>` declares `Inputs`/`Outputs`
   (`nameof`), a `Citation`, `IsApplicableTo`; `MethodScope` shows only the fields in force;
   `MethodContract.Verify` checks the declarations. The report cites the active methods.
3. **One data artifact.** `DataTable` = named, ragged columns (`ColumnTable`). An analysis
   picks a column by a stored reference `"{tableId}␟{column}"` (`DataSources`), copies it into
   its own `DataSeries` (locked, with an Edit… link) via a method that `Generates` the series.
4. **The code is generated from the class that computed the numbers.** `ICodeSource.Code(language)`
   on the analysis prints every result under the **property's Label**, in the app's order, using
   the course's libraries (numpy, scipy.stats; base R without packages). `CodeGenerationTests`
   asserts every output label appears in both scripts; the Python cross-check asserts the
   printed numbers match the app to the displayed decimals. Explicit about conventions:
   `ddof=1`, `bias=False`, quantile type 7, `fivenum`.
5. **Blank is NaN.** A statistic that needs more values than it has is NaN, shown as "—", never
   0 and never an exception. `IReportsProblem.Problem` explains anything that stops a
   calculation.
6. **Verified, not compared.** Invariants (`Harness/Invariants`), worked cases from the course's
   sample problems and Montgomery & Runger, snapshots reviewed once, plus the Python cross-check.
   Published values beat re-derived ones: Excel's SKEW/KURT example, R's `fivenum`/`quantile`.

## Conventions

- `Directory.Build.props`: one version property (`StatistleVersion`), nullable, implicit usings,
  **TreatWarningsAsErrors** (xUnit analyzers included — use `Assert.Single(list, predicate)`).
- Third-party packages must be MIT/BSD/Apache: ScottPlot, PDFsharp, MathNet.Numerics (MIT;
  distributions now, linear algebra for multiple regression later). Parameterisations follow
  the course and R (rate for the exponential, meanlog/sdlog); the generated Python states
  scipy's (scale = 1/λ, s = σ with scale = e^μ) in a comment.
- Result labels must not contain ": " — the Python cross-check parses "Label: value".
- Core never references a UI assembly; all ScottPlot types stay in `ScottPlotSurface.cs`.
- Editor conventions from SMADA: aligned three-column grid, series one value per line, tables
  header + rows, red box on a bad paste keeps the old data, ranges under labels, click-to-select.
- UI capture for checking a build: never a full-screen grab or real input — see the memory note
  on PrintWindow + UI Automation (`winapi.ps1` pattern from the SMADA sessions).
- Hand-test at the end of every phase; samples under `samples\`, written by `--write-samples`.

## Sibling repos worth knowing

- `..\SMADA10` — the origin of the core, shell, harness and packaging scripts; its `CLAUDE.md`
  describes the same architecture in hydrology terms. Fixes to the shared framework should be
  considered for both, by hand — the copies are not linked.
- `..\EasyPeasyRetirement` — the other SMADA-core copy (finance), with `MoneyFormat` where this
  repo has `ValueKind`.
