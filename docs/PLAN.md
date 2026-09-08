# Statistle — plan

**What it is.** A Windows 10/11 desktop app that calculates the statistics taught in EGN3443
*Probability and Statistics for Engineers* (Ron's course, published at
`github.com/reaglin/egn3443`), explains every input and result in the student's terms, and
writes the **Python and R** code that reproduces the data and the analysis. Free on the
Microsoft Store. Education first: every parameter carries what it is, when to use it, typical
values, and its source, exactly as SMADA does for hydrology.

Ron's brief (2026-09-08):

> Statistics calculation and learning software that follows the EGN3443 class. It will
> calculate the specific statistics in the course from an interface similar to SMADA, with
> items on the left side (data, analysis types) and information on the right side. A major
> feature will be able to produce Python and R versions of variables and also Python/R code
> to perform the analyses. Targeted at a free release on the Windows App Store and education
> focused, so all parameters will include full explanations, uses, and sources.

`DEVELOPMENT-PLAN.md` is the numbered work breakdown. `METHODS-AND-SOURCES.md` is the register
of every statistic the app computes: formula, source, the Python and R that do the same, and
the worked case that holds it. This file is the strategy.

---

## What the student does with it

1. **Enters data** — a *data table*: named columns of numbers, typed, pasted from a spreadsheet,
   or imported from text / CSV / Excel. One column is a sample; two are x–y pairs; several are
   groups, or predictors and a response. Columns may differ in length (ANOVA groups usually do).
2. **Picks an analysis** from the rail, chooses the column(s), sets the options the method
   needs (confidence level, hypothesised mean, tail, equal variances or not) and reads the
   results beside the entries — every number with its meaning in the help panel below.
3. **Reads the report** (Report tab): entries, results, the method's citation, plots — the
   *methodology* and *conclusions* sections of the course's report template, ready to paste.
4. **Takes the code** (Code tab): Python and R that declare the same data as variables and run
   the same analysis with the standard libraries the course uses — `numpy`, `scipy.stats`,
   `pandas`, `statsmodels`; base R (`t.test`, `lm`, `aov`, `TukeyHSD`). The student runs it in
   Colab or R and sees the same numbers; the code goes in the report's appendix.

The three things a student must be able to say after using it: *which* statistic answers the
question, *why* that one (assumptions, conditions), and *how* to get it in code.

## The course, module by module, and what the app computes

| Module | Course topic | Analysis in the rail | Phase |
|---|---|---|---|
| 1–2 | Statistics for engineers; descriptive statistics | **Descriptive statistics** — mean, median, mode, range, variance, standard deviation, coefficient of variation, quartiles and IQR, percentiles, skewness, kurtosis, five-number summary, frequency table and histogram, box plot, outlier fences | 1 |
| 3 | Probability concepts | **Probability rules** — complement, addition (mutually exclusive / general), multiplication (independent / conditional), Bayes for two events; counting (permutations, combinations) | 2 |
| 4 | Discrete distributions | **Discrete distribution** — binomial, Poisson, geometric, negative binomial, hypergeometric, and a typed PMF table: P(X = k), P(X ≤ k), P(X ≥ k), mean, variance, PMF/CDF plot | 2 |
| 5 | Continuous distributions | **Continuous distribution** — uniform, normal (z-scores, empirical rule), exponential, plus t, χ², F for later modules: P(X ≤ x), P(a < X < b), quantiles (inverse), density and CDF plots | 2 |
| 6 | Sampling distributions | **Sampling distribution** — mean and standard error of x̄ and p̂ for a sample size, CLT probabilities, the effect of n on the spread, with a simulation plot | 2 |
| 7 | Point estimation and confidence intervals | **Confidence interval** — mean (z with σ known, t with s), proportion (Wald, Wilson), variance/standard deviation (χ²), difference of means and proportions; **Sample size** for a margin of error | 3 |
| 8–9 | Hypothesis testing I and II (one sample) | **One-sample test** — z-test and t-test for a mean, z-test for a proportion, χ² test for a variance; one- or two-tailed; test statistic, critical value, p-value, decision at α, confidence interval, Type I / II errors and power for a given alternative | 3 |
| 9–10 | Two-sample tests | **Two-sample test** — independent t (pooled and Welch), paired t, two-sample z, two proportions (pooled), F-test for two variances; effect size (Cohen's d); assumption checks (Shapiro–Wilk normality, Levene / F for variances); non-parametric alternatives (Mann–Whitney U, Wilcoxon signed-rank) | 3 |
| 11 | Regression analysis | **Simple linear regression** — least squares, slope and intercept with standard errors, t-tests and confidence intervals, R², r, ANOVA table and F, residual standard error, prediction and confidence bands for a new x, residual plots | 4 |
| 12 | Multiple regression | **Multiple regression** — k predictors, coefficient table, R² and adjusted R², F-test, VIFs for multicollinearity, forward / backward selection by adjusted R² or p-value | 4 |
| 13 | Validating regression | **Regression validation** — residuals vs fitted, normal Q–Q, Durbin–Watson, leverage and Cook's distance, transformations (log, square root, inverse) compared, hold-out and k-fold cross-validation with RMSE | 4 |
| 14 | ANOVA | **One-way ANOVA** — SS between / within, F, p, η²; Tukey HSD post-hoc; Bartlett / Levene; Kruskal–Wallis alternative. **Two-way ANOVA** — main effects and interaction from a table with two factors | 5 |
| 15 | Advanced topics | Out of scope for v1 (Monte Carlo simulation is a possible later analysis) | — |

## Settled decisions

| | Decision | Why |
|---|---|---|
| Platform | **WPF on .NET 10**, Windows 10/11, MSIX by script for the **Microsoft Store**, free. | Same toolchain and release routine as SMADA, AuthorPlus and EasyPeasyRetirement. |
| Reuse | SMADA's core is **copied** and renamed (`StatistleObject`, `StatistlePropertyAttribute`, `[StatistleHelp]`, `Analysis` for `SmadaTool`, `AnalysisCatalog`, `MethodScope`, the generic editor with the help panel, Report/Plots tabs, HTML/PDF/CSV writers, `TableImport`, the tester zip script). Not referenced across repos. | Two release cadences. The core carries 360+ tests in SMADA and its ideas — declare on the property, get the editor, the help panel, the report and the export for free — are exactly what a many-analysis teaching app needs. |
| Data | One artifact type, the **data table** (named columns, ragged allowed). Analyses choose columns by name. Project = one `.statistle` file (a zip of readable XML parts, the `.smada` shape) under `Documents\Statistle\`. | Every module's data is columns: a sample, pairs, groups, predictors. One editor, one importer, one persistence path. |
| Numerics | **MathNet.Numerics** (MIT) for distribution functions (normal, t, χ², F, binomial, Poisson…) and linear algebra (multiple regression). Everything else — the statistics themselves — is written in Core, in the course's notation, so the formula the student sees is the formula that runs. | Writing inverse-t and regularised-beta functions ourselves buys nothing; writing the mean, the pooled variance and the F ratio ourselves is the point. MIT is Store-safe. |
| Code generation | Each analysis method implements `ICodeSource`: `PythonCode(context)` and `RCode(context)`. A data table renders as `numpy` arrays / a `pandas` DataFrame and as R vectors / a `data.frame`. The Code tab shows both, with Copy and Save as `.py` / `.R`. The code prints the same results the app shows, in the same order, so the student can check them line by line. | This is the course's own workflow: solve it, then show the code in the appendix. Generating it from the same method class that computed the numbers keeps the two from drifting. |
| Verification | SMADA's three tiers: **invariants** (variance ≥ 0, R² in [0, 1], CI contains the estimate, p in [0, 1], one-tailed p = half of two-tailed at the same t, pooled t equals Welch t when variances are equal…), **worked cases** from the course's sample problems and Montgomery & Runger examples, **snapshots** reviewed once. Plus a **cross-check tier**: for every analysis the generated Python is run under `pytest`-style harness *if* a Python with numpy/scipy is on the machine, and the printed numbers must match the app's to 6 significant figures (skipped, not failed, when Python is absent). | The code the student is handed must agree with the screen. Running it in CI is the only honest check. |
| Explanations | Every property: `Help` — what it is, when to use it, typical values, pitfalls. Every analysis: `[StatistleHelp]` — the question it answers, assumptions, how to read the result. Every method: `Citation`. Sources are the course modules, Montgomery & Runger (*Applied Statistics and Probability for Engineers*, 7th ed.), and the NIST/SEMATECH *e-Handbook of Statistical Methods* (free, citable by section). | The app is a teacher. A number without its meaning is a calculator. |
| Not a grader | The app computes and explains; it does not tell the student the answer to an assignment is right. Sample problems from the course ship as sample projects so the student can compare. | It supports the course; it does not replace doing the work. |

## Architecture

```
Statistle.sln
├── src/Statistle.Core/     net10.0. Never references a UI assembly (architecture test).
│   Persistence/     StatistleObject, StatistlePropertyAttribute (+Help), StatistleHelp, EnumLabel,
│                    DataSeries, ColumnTable (the data table's columns as text), TableImport,
│                    IHasChoices, ILinksTo, IReportsProblem, LoadDiagnostics
│   Domain/          DataTable (the one artifact), DataSources (column pick-lists for analyses)
│   Document/        StatistleDocument (.statistle zip: project.xml, tables/, analyses/), StatistlePaths
│   Methods/         ICalculationMethod, MethodCatalog, MethodScope, IChoosesMethod
│   Methods/Descriptive/   SummaryStatistics, Quantiles, FrequencyTable
│   Methods/Distributions/ Discrete and continuous distributions over MathNet (PMF/PDF/CDF/quantile)
│   Methods/Inference/     ConfidenceIntervals, OneSampleTests, TwoSampleTests, SampleSize, Power
│   Methods/Regression/    SimpleRegression, MultipleRegression, Diagnostics, CrossValidation
│   Methods/Anova/         OneWayAnova, TwoWayAnova, TukeyHsd
│   Code/            ICodeSource, PythonWriter, RWriter (variables + analysis; shared formatting)
│   Analyses/        Analysis (base), AnalysisCatalog, one class per rail entry
│   Plotting/        IPlotSurface, PlotSpec, IHasFigures, the figures (histogram, box, PMF/PDF,
│                    scatter + fit, residual, Q–Q, group means)
│   Reporting/       Report + text/HTML/PDF writers, CsvExport
├── src/Statistle.App/      net10.0-windows, WPF. Rail | tree | Properties / Report / Plots / Code
│   ViewModels/      WorkspaceViewModel, PropertyRow (generic editor, help panel), CodeViewModel
│   Plotting/        ScottPlotSurface (all ScottPlot types), FigurePanel, ScottPlotFigureRenderer
│   Views/           ImportDialog, CodeView
├── tests/Statistle.Tests/  xUnit: framework, engine invariants, worked cases, snapshots, code cross-check
├── samples/         demo-statistics.statistle (course sample problems as tables + analyses)
├── packaging/       AppxManifest.xml, pack.ps1, Assets, make-tester-package.ps1, tester/README.txt
└── manual-test/     Statistle.exe, the hand-test build (gitignored)
```

## Risks

- **Scope.** Fourteen modules is a lot of analyses. The phases below ship them in course order,
  each phase a hand-testable build, so the app is useful from phase 1 and complete by phase 5.
- **Agreement between app and code.** Held by the cross-check tier and by generating code from
  the same method class. Where scipy and R differ by default (e.g. `ddof`, Welch vs pooled,
  continuity corrections) the generated code sets the option explicitly and the help says so.
- **Numerical edge cases** — tiny p-values, perfect fits, constant columns, n = 1. Invariants
  sweep them; `IReportsProblem` explains instead of crashing.
