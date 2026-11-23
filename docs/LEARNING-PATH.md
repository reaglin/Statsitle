# Statistics+ — the learning path (EGN3443, module by module)

The course at https://reaglin.github.io/EGN3443/ (read 2026-11-11), mapped onto the learning path.
Decided by Ron on 2026-11-11: **one item for each course topic**, and each module ends with a **module check**.
Every item has the four tabs PLC+ has:

| Tab | What it holds in Statistics+ |
|---|---|
| **Learn** | The topic in Markdown: the idea, the formula, a small worked case, and the Python and R. Widgets draw a histogram, a table, a distribution curve or a code block in the page. |
| **Example** | A worked analysis that is already filled in. A dataset from the course or Montgomery & Runger is loaded into the analysis the topic uses, and its results, plots and code are shown. The student can change the inputs and watch the results move. It is never the exercise's answer (PLC+ D21): it uses a similar dataset. |
| **Exercise** | A task on a new dataset. The student either works the analysis and types the asked results (graded with a tolerance), or answers questions: choose the test, read the p-value, state the conclusion. **The correct numbers are calculated by the same analysis class that the Workbench uses**, so the answer key cannot drift from the engine. The item is complete when the student passes. |
| **Reference** | The formulas, the assumptions, the Python and R functions, and the sources (`METHODS-AND-SOURCES.md` rows). |

Ids are stable: module `st.mN`, outcome `st.mN.oK` (the course's MO numbers), item `st.mN.<slug>`.
The analysis names are the existing Workbench analyses (`src/Statistle.Core/Analyses/`).
**New** marks something the engine does not have yet (phase 10 builds it).

**Numbering.** The course site labels pages as both "Week N" and "Module N−1". The path uses the module numbers (Module 1 = Introduction to Statistics for Engineers, retitled 2026-11-23). Week 1 orientation becomes **Start here**.

---

## Start here (Week 1)

| Item | Learn | Example / Exercise |
|---|---|---|
| How Statistics+ works | the four tabs, the Workbench, progress, reports | quiz: navigation |
| Python and R for this course | Colab, RStudio, the Code tab | Example: a Code tab from Descriptive statistics; Exercise: run it and type a printed value |
| The report template | the course's report sections and how the Report tab fills them | quiz |

## Module 1 — Statistics for Engineers

Outcomes: MO1 descriptive statistics (Apply) · MO2 choose normal/binomial/Poisson (Analyze) · MO3 interpret CIs and tests (Evaluate) · MO4 frequency table, histogram, box plot (Apply) · MO5 sources of variability (Analyze).

| # | Item (course topic) | Analysis behind Example / Exercise |
|---|---|---|
| 1.1 | Introduction to statistics in engineering | quiz (vocabulary: population, sample, parameter, statistic) |
| 1.2 | Descriptive statistics | Descriptive statistics |
| 1.3 | Data variability and distribution shape | Descriptive statistics (histogram, box plot) |
| 1.4 | Probability distributions | Discrete distribution, Continuous distribution (preview) |
| 1.5 | Statistical inference and interpretation | Confidence interval, One-sample test (preview) |
| 1.6 | Applications in engineering analysis and quality control | Descriptive statistics (outlier fences); **New:** control limits (x̄ ± 3σ) as a widget |
| 1.7 | Module check | covers MO1–MO5 |

## Module 2 — Descriptive Statistics

Outcomes: MO1 mean/median/mode · MO2 range/variance/SD · MO3 classify skew · MO4 choose the measure of centre · MO5 interpret for an audience.

| # | Item | Analysis |
|---|---|---|
| 2.1 | Introduction to descriptive statistics | Descriptive statistics |
| 2.2 | Measures of central tendency | Descriptive statistics |
| 2.3 | Measures of variability | Descriptive statistics |
| 2.4 | Data distribution (symmetry, skewness) | Descriptive statistics (skewness, histogram) |
| 2.5 | Selecting and applying the right measures | quiz on scenarios + Descriptive statistics |
| 2.6 | Module check | MO1–MO5 |

## Module 3 — Probability Concepts

Outcomes: MO1 sample spaces · MO2 complement, addition, multiplication · MO3 theoretical vs experimental, Law of Large Numbers · MO4 plain-language interpretation · MO5 choose the rule.

| # | Item | Analysis |
|---|---|---|
| 3.1 | Introduction to probability | quiz |
| 3.2 | Sample spaces and events | Probability rules (counting); quiz on listing outcomes |
| 3.3 | Theoretical vs experimental probability | **New: Simulation** (dice, cards, darts; the running proportion against the theoretical value) |
| 3.4 | Core probability rules | Probability rules |
| 3.5 | Interpreting probability in context | Probability rules (conditional, Bayes); quiz |
| 3.6 | Module check | MO1–MO5 (the course's three-card 21 question as a simulation exercise) |

## Module 4 — Discrete Probability Distributions

Outcomes: MO1 valid distribution · MO2 E and Var from a PMF · MO3 binomial vs Poisson · MO4 exact probabilities · MO5 interpret.

| # | Item | Analysis |
|---|---|---|
| 4.1 | Introduction to discrete distributions | Discrete distribution (typed PMF) |
| 4.2 | Probability mass functions | Discrete distribution (typed PMF, validity) |
| 4.3 | Expected value and variance | Discrete distribution (typed PMF) |
| 4.4 | The binomial distribution | Discrete distribution (binomial) |
| 4.5 | The Poisson distribution | Discrete distribution (Poisson) |
| 4.6 | Applying discrete distributions | quiz "which distribution?" (as the course's Distribution Detective: Bernoulli, binomial, Poisson, geometric, Pascal, hypergeometric) + Discrete distribution |
| 4.7 | Module check | MO1–MO5 |

## Module 5 — Continuous Probability Distributions

Outcomes: MO1 PDF and CDF · MO2 uniform, normal, exponential probabilities · MO3 choose the model · MO4 interpret.

| # | Item | Analysis |
|---|---|---|
| 5.1 | Introduction to continuous distributions | Continuous distribution |
| 5.2 | The uniform distribution | Continuous distribution (uniform) |
| 5.3 | The normal distribution | Continuous distribution (normal, z-scores) |
| 5.4 | The exponential distribution | Continuous distribution (exponential, memoryless) |
| 5.5 | Calculating and interpreting probabilities | Continuous distribution |
| 5.6 | Applying continuous distributions | Continuous distribution; **New:** Weibull (the course reading uses it) and a fit check on data (histogram with the fitted curve, Q–Q) |
| 5.7 | Module check | MO1–MO4 |

## Module 6 — Sampling Distributions

Outcomes: MO1 parameter vs statistic · MO2 mean and standard error · MO3 effect of n · MO4 when the CLT applies · MO5 sampling variability vs real difference.

| # | Item | Analysis |
|---|---|---|
| 6.1 | Introduction to sampling distributions | Sampling distribution; **New: Simulation** (draw many samples, histogram of x̄) |
| 6.2 | Population parameters vs sample statistics | quiz + Descriptive statistics |
| 6.3 | The Central Limit Theorem | Sampling distribution + Simulation from a skewed population |
| 6.4 | The effect of sample size on variability | Sampling distribution |
| 6.5 | Interpreting and applying sampling distributions | Sampling distribution |
| 6.6 | Module check | MO1–MO5 |

## Module 7 — Point Estimation and Confidence Intervals

Outcomes: MO1 unbiased vs efficient · MO2 CLT for inference · MO3 CIs for means and proportions · MO4 what changes the width · MO5 interpret a CI.

| # | Item | Analysis |
|---|---|---|
| 7.1 | Introduction to point estimation | Descriptive statistics; Simulation (bias of s with n vs n−1) |
| 7.2 | Sampling distributions and the CLT | Sampling distribution |
| 7.3 | Confidence intervals for means | Confidence interval (z and t) |
| 7.4 | Confidence intervals for proportions | Confidence interval (Wald, Wilson) |
| 7.5 | Interpreting confidence intervals | Simulation (100 intervals, how many cover μ); quiz on wording |
| 7.6 | Precision and margin of error | Sample size |
| 7.7 | Applying estimation to real data | Confidence interval from a data column (the course's battery example) |
| 7.8 | Module check | MO1–MO5 |

## Module 8 — Hypothesis Testing

Outcomes: MO1 write H0/Ha · MO2 select the test · MO3 statistic and p-value · MO4 p vs α, practical significance · MO5 Type I/II errors.

| # | Item | Analysis |
|---|---|---|
| 8.1 | Foundations of hypothesis testing | quiz |
| 8.2 | Formulating the null and alternative | quiz (claims → hypotheses, tails) |
| 8.3 | Test statistics and sampling distributions | One-sample test (z, t) |
| 8.4 | P-values and significance levels | One-sample test (the reference curve with the rejection region) |
| 8.5 | Applying common tests | "choose the test" quiz; One-sample, Two-sample, One-way ANOVA; **New: Chi-square tests** (goodness of fit, independence in a contingency table) |
| 8.6 | Type I and Type II errors | One-sample test (power for an alternative) |
| 8.7 | Interpreting and communicating results | One-sample test; quiz on wording |
| 8.8 | Module check | MO1–MO5 |

## Module 9 — Hypothesis Testing II

Outcomes: MO1 choose two-sample z / t / paired t · MO2 compute the statistic and df · MO3 check assumptions · MO4 interpret p and CI · MO5 non-parametric alternative.

| # | Item | Analysis |
|---|---|---|
| 9.1 | Two-sample hypothesis tests | Two-sample test (z, pooled t, Welch) |
| 9.2 | Paired sample comparisons | Two-sample test (paired t) |
| 9.3 | Selecting the appropriate test | "choose the test" decision quiz |
| 9.4 | Assumptions and conditions | Two-sample test (Shapiro–Wilk, Levene, F) |
| 9.5 | Introduction to non-parametric methods | Two-sample test (Mann–Whitney, Wilcoxon) |
| 9.6 | Interpreting and communicating results | Two-sample test |
| 9.7 | Module check | MO1–MO5 |

## Module 10 — Two-Sample Hypothesis Testing

Outcomes: MO1 independent vs paired · MO2 verify assumptions · MO3 two means · MO4 two proportions (pooled z) · MO5 interpret in context.
This module revisits module 9 with new data; its items say so and link back (`builds-on`).

| # | Item | Analysis |
|---|---|---|
| 10.1 | Foundations of two-sample testing | quiz |
| 10.2 | Independent vs paired samples | quiz on designs + Two-sample test |
| 10.3 | Assumptions and conditions | Two-sample test |
| 10.4 | Comparing two means | Two-sample test |
| 10.5 | Comparing two proportions | Two-sample test (two proportions, pooled) |
| 10.6 | Interpreting results and decisions | Two-sample test |
| 10.7 | Module check | MO1–MO5 |

## Module 11 — Regression Analysis

Outcomes: MO1 least-squares line · MO2 coefficients, R², adjusted R², p, F · MO3 simple vs multiple · MO4 residual checks · MO5 communicate findings.

| # | Item | Analysis |
|---|---|---|
| 11.1 | Foundations of regression | Simple linear regression |
| 11.2 | Simple linear regression | Simple linear regression |
| 11.3 | Multiple regression models | Multiple regression (preview) |
| 11.4 | Evaluating model performance | Simple linear regression (R², RMSE, F, residuals) |
| 11.5 | Interpreting regression results | Simple linear regression |
| 11.6 | Applying regression to real data | Simple linear regression on a data table |
| 11.7 | Module check | MO1–MO5 |

## Module 12 — Multiple Regression

Outcomes: MO1 build the model by OLS · MO2 partial coefficients · MO3 R² vs adjusted R² · MO4 overall F-test · MO5 justify predictors.

| # | Item | Analysis |
|---|---|---|
| 12.1 | From simple to multiple regression | Multiple regression |
| 12.2 | Model specification and structure | Multiple regression |
| 12.3 | Interpreting regression coefficients | Multiple regression |
| 12.4 | Assessing model fit | Multiple regression (R², adjusted R²) |
| 12.5 | Building and evaluating models | Multiple regression (forward/backward selection) |
| 12.6 | Module check | MO1–MO5 |

## Module 13 — Validating Regression

Outcomes: MO1 assumption violations from residual plots · MO2 compute and read residuals · MO3 hold-out vs k-fold vs LOOCV · MO4 multicollinearity, outliers, influence · MO5 recommend refinements.

| # | Item | Analysis |
|---|---|---|
| 13.1 | Foundations of regression validation | Simple linear regression (residual plots) |
| 13.2 | Assessing model assumptions | Simple / Multiple regression (Durbin–Watson, Q–Q) |
| 13.3 | Residual analysis | Simple linear regression |
| 13.4 | Cross-validation techniques | Multiple regression (k-fold, LOOCV); **New:** hold-out split |
| 13.5 | Diagnosing model weaknesses | Multiple regression (VIF, leverage, Cook's distance) |
| 13.6 | Model refinement and decisions | Simple linear regression (transformations compared) |
| 13.7 | Module check | MO1–MO5 (as the course's "Fit for Purpose" activity) |

## Module 14 — ANOVA

Outcomes: MO1 why not many t-tests · MO2 verify assumptions · MO3 one-way F and the ANOVA table · MO4 main effects vs interaction · MO5 post-hoc tests.

| # | Item | Analysis |
|---|---|---|
| 14.1 | Introduction to ANOVA | Simulation (familywise Type I error of many t-tests) |
| 14.2 | Assumptions underlying ANOVA | One-way ANOVA (Bartlett, Levene) |
| 14.3 | The F-statistic and ANOVA logic | One-way ANOVA |
| 14.4 | One-way ANOVA | One-way ANOVA |
| 14.5 | Multi-factor designs | Two-way ANOVA |
| 14.6 | Post-hoc testing and interpreting results | One-way ANOVA (Tukey HSD) |
| 14.7 | Module check | MO1–MO5 |

## The book in three parts (Ron, 2026-11-23)

Ron, 2026-11-23: *"(1) Fundamental Statistics for Engineers which will still keep the module 1 statistics for engineers and
change it to introduction to Statistics for Engineers. The next part is "Advanced Statistics" … and the third part will be
"Additional Applications of Statistics" that will cover how Statistics is used in other fields (Biology, Business, etc..)"*
(he wrote "2 parts" and described three; three are built). Each part has a page in `content/egn3443/parts/`; a module names
its part in its front matter (`part:`). Start here (Module 0) stays above the parts. Parts II and III are **in development**:
each module's `module.md` (status `coming-later`) lists its outcomes and topics, with no items yet (plan phase P).

| Part | Modules | Status |
|---|---|---|
| **Part I · Fundamental Statistics for Engineers** | 1 Introduction to Statistics for Engineers (was Statistics for Engineers) … 14 ANOVA, above | written |
| **Part II · Advanced Statistics** | 15–18, below | in development |
| **Part III · Additional Applications of Statistics** | 19–23, below (proposed, ⚠️ Ron: plan Q19) | in development |

### Part II · Advanced Statistics (one module per bullet of Ron's)

| Module | Topics (as Ron's bullet gives them) | Outcomes (`st.mN.oK`) | Workbench pieces it may need |
|---|---|---|---|
| 15 Advanced Regression and Experimental Design | Multiple linear regression in depth; polynomial regression; nonlinear regression; factorial designs (2^k, fractional); response surfaces | o1 fit and compare multiple, polynomial and nonlinear models; o2 plan a factorial experiment, estimate effects and interactions, explore a response surface | **New**: nonlinear least squares, 2^k factorial analysis, response-surface fit (multiple regression covers the linear part) |
| 16 Stochastic Processes | Markov chains; Poisson processes; Wiener processes (Brownian motion); stochastic calculus for systems evolving over time | o1 model a system as a Markov chain or Poisson process and find its long-run behaviour; o2 describe the Wiener process and the idea of stochastic calculus | **New**: Markov chain (transition matrix, steady state), Poisson process and random-walk simulation (Simulation has the generator) |
| 17 Multivariate and Reliability Analysis | Multivariate models; quality control; system reliability; uncertainty propagation | o1 analyse correlated measurements together and monitor a process with control charts; o2 find a system's reliability from its components and propagate input uncertainty | **New**: covariance matrix / PCA, control charts (control limits exist in Descriptive statistics), series–parallel reliability, Monte Carlo propagation (Weibull exists) |
| 18 Bayesian and Computational Statistics | Bayesian inference (prior information); parameter estimation (maximum likelihood); Monte Carlo simulation; bootstrapping | o1 update a prior with data and estimate parameters by maximum likelihood; o2 answer by Monte Carlo simulation and bootstrap an interval | **New**: conjugate-prior updates, MLE (the distribution fit has some), bootstrap (Simulation has the seeded generator) |

### Part III · Additional Applications of Statistics (proposed fields)

| Module | Topics | Outcomes |
|---|---|---|
| 19 Biology and Medicine | Clinical trials and their design; risk, odds and relative risk; survival analysis; epidemiology and screening tests | o1 read the design and statistics of a trial or study; o2 choose a test for survival times, rates and risks |
| 20 Business and Economics | Time series and forecasting; risk and return; Six Sigma and quality in business; A/B tests and decisions | o1 forecast from a time series with its uncertainty; o2 decide under risk with quality, finance and A/B tests |
| 21 Environmental and Earth Sciences | Extreme values and return periods; hydrology and climate records; trends in monitoring data; values below a detection limit | o1 size a rare event with extreme-value statistics; o2 analyse monitoring data, trends and censored values |
| 22 Social Sciences and Psychology | Survey sampling and margins of error; measurement and reliability; experiments with people; reading published results | o1 design a survey sample and judge its margin of error; o2 judge a measure's reliability |
| 23 Data Science and Computing | Statistics behind machine learning; model evaluation and cross-validation; many tests at once; big data, sampling and bias | o1 evaluate a model with a hold-out set and cross-validation; o2 handle many tests, sampling and bias in large data |

Each module, when it is written, follows Part I's pattern: Learn / Example (examples to try, each with a figure) / Exercise /
Reference, a module check, outcomes in `docs/OUTCOMES.md`, and its answers calculated by a Workbench analysis.

## Counts

Start here + 14 modules in Part I, about **100 items**: 82 topics, 14 module checks, 3 start items. Parts II and III: 9 modules in development (2026-11-23).
New engine pieces: Simulation, Chi-square tests, Weibull + distribution fit, hold-out split, control limits.
