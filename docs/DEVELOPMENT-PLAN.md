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

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 3.1 | `ConfidenceIntervals`: mean (z, t), proportion (Wald, Wilson), variance and SD (χ²), difference of means (pooled, Welch, paired), difference of proportions; **Sample size** for a margin of error (mean, proportion) | `Core/Methods/Inference/ConfidenceIntervals.cs`, `Analyses/ConfidenceInterval.cs`, `SampleSize.cs` | M&R Ch. 8 examples; invariant: the interval contains the point estimate and widens with confidence | [ ] |
| 3.2 | `OneSampleTests`: z and t for a mean, z for a proportion, χ² for a variance; tail choice; statistic, critical value, p, decision, CI, power for a stated alternative and the n for a target power | `Inference/OneSampleTests.cs`, `Analyses/OneSampleTest.cs` | Module 9 problem 1: z = 2.40, p = 0.0164; Module 8 problem 1 (proportion) | [ ] |
| 3.3 | `TwoSampleTests`: independent t pooled / Welch, paired t, two-sample z, two proportions pooled, F for variances; Cohen's d; Shapiro–Wilk, Levene; Mann–Whitney U, Wilcoxon signed-rank | `Inference/TwoSampleTests.cs`, `Analyses/TwoSampleTest.cs` | Module 10 problems; invariant: pooled = Welch when s₁ = s₂ and n₁ = n₂ | [ ] |
| 3.4 | Figures: sampling distribution with rejection region and the observed statistic; two-group box plots; paired differences | `Plotting/` | | [ ] |
| 3.5 | Code (`scipy.stats.ttest_1samp/ttest_ind/ttest_rel/norm`, `statsmodels.stats.proportion`; R `t.test`, `prop.test`, `var.test`, `wilcox.test`, `shapiro.test`) | `Core/Code/` | cross-check | [ ] |

## Phase 4 — Regression (Modules 11–13)

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 4.1 | Simple regression completed: SEs, t and p per coefficient, CIs, ANOVA table, F, residual SE, r, prediction with confidence and prediction intervals at x₀, residual plots | `Methods/Regression/SimpleRegression.cs`, `Analyses/SimpleRegression.cs` | M&R Ch. 11 example; Module 11 problem 1 | [ ] |
| 4.2 | Multiple regression over MathNet linear algebra: coefficient table, R², adjusted R², F, VIF, forward / backward selection | `Regression/MultipleRegression.cs`, `Analyses/MultipleRegression.cs` | M&R Ch. 12 example; Module 12 problem 1 | [ ] |
| 4.3 | Validation: residuals vs fitted, Q–Q, Durbin–Watson, leverage, Cook's D, transformations compared (log, √, 1/x), hold-out and k-fold RMSE | `Regression/Diagnostics.cs`, `CrossValidation.cs`, `Analyses/RegressionValidation.cs` | Module 13 problems | [ ] |
| 4.4 | Code (`statsmodels.api.OLS`, `sklearn.model_selection`; R `lm`, `summary`, `predict`, `plot(lm)`) | `Core/Code/` | cross-check | [ ] |

## Phase 5 — ANOVA (Module 14) and release

| # | Task | Targets | Acceptance | Status |
|---|---|---|---|---|
| 5.1 | One-way ANOVA: SS/df/MS table, F, p, η²; Tukey HSD; Bartlett and Levene; Kruskal–Wallis | `Methods/Anova/`, `Analyses/OneWayAnova.cs` | Module 14 problem 1 alloys; M&R Ch. 13 | [ ] |
| 5.2 | Two-way ANOVA from a table with two factor columns and a response | `Anova/TwoWayAnova.cs`, `Analyses/TwoWayAnova.cs` | M&R Ch. 14 example | [ ] |
| 5.3 | Code (`scipy.stats.f_oneway`, `statsmodels` `anova_lm`, `pairwise_tukeyhsd`; R `aov`, `TukeyHSD`, `kruskal.test`) | `Core/Code/` | cross-check | [ ] |
| 5.4 | Store: Partner Center identity, listing, privacy statement (nothing leaves the machine), `msixupload` | `packaging/` | submitted | [ ] |
