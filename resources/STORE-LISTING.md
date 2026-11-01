# Statistle — Microsoft Store listing copy

Paste-ready text for Partner Center → *Store listings → English (United States)*.
Character limits in brackets are Partner Center's. Keep this file in step with what is actually
submitted.

Images for the listing are in `resources/images/store/` — see the README there for which file
goes in which slot. Screenshots are Ron's to capture (list at the end).

---

## Product name  [256]

```
Statistle
```

Reserved in Partner Center; identity `DeanEaglin.statistle` (see `STORE-IDENTITY.md`).

## Short title  [50]

```
Statistle
```

## Short description  [1000]

```
Statistle is free statistics software for students. Enter a data table, choose an analysis, and
it computes the result, explains every input and every number it reports, and writes the Python
and R that reproduce the same analysis - so you can check the work, not just copy the answer.
```

## Description  [10000]

```
Statistle is a free statistics package for the Windows desktop, written for students working
through a first course in probability and statistics for engineers. It follows the syllabus of
EGN3443 module by module, from descriptive statistics to two-way ANOVA.

It is built around one idea: a statistics answer is only useful if you can see where it came
from. So every analysis names the method it used, explains what each input means and where each
result comes from, and writes out the Python and the R that reproduce it.


WHAT IT DOES

Describing data
  - Descriptive statistics for one sample: count, mean, median and mode, variance, standard
    deviation, quartiles and the five-number summary
  - Histogram with a fitted normal curve, and a box plot

Probability and distributions
  - The probability rules for two events - independent, mutually exclusive, or given the
    intersection, a conditional probability, or the union
  - Discrete distributions: read P(X = k), the cumulative probability and a range, with the
    mean and variance of the family
  - Continuous distributions: density, cumulative probability, a range, and the quantile for a
    given probability, with the region shaded on the plot
  - The sampling distribution of the mean or a proportion, drawn against the population it
    came from

Estimation and testing
  - Confidence intervals for a mean, a proportion or a variance
  - The sample size needed for a given margin of error and confidence
  - One-sample tests of a mean, a proportion or a variance
  - Two-sample tests: pooled t when the variances can be assumed equal, Welch t when not,
    paired t, and the proportion and variance tests
  - The assumption checks that belong with them - Shapiro-Wilk for normality, Levene for equal
    variance - and the nonparametric alternatives, Mann-Whitney and Wilcoxon signed-rank

Regression
  - Simple linear regression: the least-squares line, slope and intercept with their standard
    errors and tests, r and r-squared, confidence and prediction intervals, and the common
    transformations when the straight line does not fit
  - Multiple regression: several predictors at once, with forward and backward selection and
    k-fold cross-validation

Analysis of variance
  - One-way ANOVA, with Tukey HSD for which pairs differ, Bartlett and Levene for equal
    variance, Kruskal-Wallis as the nonparametric alternative, and the power of the test
  - Two-way ANOVA for balanced designs


THE CODE TAB

Every analysis writes the Python and the R that reproduce it - the data, the call, and the
printing of the result - in ordinary, readable style using numpy, scipy and statsmodels, or
base R. Copy it into your own notebook and run it. Statistle does not run the code and does not
need Python or R installed; the code is there so that what the app did is never a black box, and
so the step from a teaching tool to real analysis is a short one.


HOW IT WORKS

One project file. Data tables and every analysis you set up are saved together in a .statistle
file, so a whole assignment comes back exactly as you left it.

Every entry explains itself. Each field knows its meaning, its valid range and its units; the
help panel under the form describes what to put there and why.

Every tool has a manual page. The Instructions tab explains what the analysis is for, what each
entry means, what it produces, and which method and source stand behind the result - written to
be read by someone learning the material.

Report, plot and export. Any analysis produces a formatted report as text, HTML or PDF, the
plots that belong with it, and a CSV of its values.


PRIVACY

Statistle runs entirely on your computer. It collects nothing and sends none of your work
anywhere. It goes online only when you ask: Help > Check for a new version asks the Microsoft
Store, and Help on the web opens a page in your browser. Your work is saved in Documents\Statistle or wherever you choose.


FREE

Statistle is free, with no advertising, no accounts and no in-app purchases. It is published to
support the course it was written for.
```

## Product features  [up to 20, 200 each]

```
Descriptive statistics with histogram, fitted normal curve and box plot
Probability rules for two events - independent, exclusive, conditional or joint
Discrete and continuous distributions, with the region shaded on the plot
The sampling distribution of a mean or proportion, drawn against its population
Confidence intervals for a mean, a proportion or a variance
The sample size needed for a given margin of error and confidence
One-sample tests of a mean, a proportion or a variance
Two-sample tests - pooled t, Welch t, paired t, proportions and variances
Shapiro-Wilk and Levene assumption checks, with Mann-Whitney and Wilcoxon alternatives
Simple linear regression with transformations, confidence and prediction intervals
Multiple regression with forward and backward selection and k-fold cross-validation
One-way ANOVA with Tukey HSD, Bartlett, Levene, Kruskal-Wallis and power
Two-way ANOVA for balanced designs
Writes the Python and R that reproduce every analysis
A manual page for every analysis, written for someone learning the material
Reports as text, HTML or PDF; plots and CSV export from any analysis
One .statistle project file holds the whole assignment
Works offline: no account, no telemetry; online only when you ask (new version, web help)
```

## What's new in this version  [1500]

```
First public release.

Statistle covers a first course in probability and statistics for engineers: descriptive
statistics, probability rules, discrete and continuous distributions, sampling distributions,
confidence intervals and sample size, one- and two-sample tests with their assumption checks and
nonparametric alternatives, simple and multiple regression, and one- and two-way ANOVA - each
with a manual page, an explanation of every number, and the Python and R that reproduce it.
```

## Search terms  [up to 7, 30 each]

```
statistics
hypothesis test
confidence interval
regression
ANOVA
probability distribution
statistics homework
```

## Copyright and trademark info  [200]

```
Copyright (c) 2026 Dean Eaglin. Statistle follows the syllabus of EGN3443, Probability and
Statistics for Engineers.
```

The Store account publishes as **Dean Eaglin**, which is what `PublisherDisplayName` in the
manifest and this copyright line must say. The author credit inside the app remains Ron Eaglin.

## Developed by  [255]

```
Ron Eaglin
```

(*Developed by* is free text shown on the listing; it does not have to equal the account's
publisher display name.)

## Additional license terms

Use the Standard Application License Terms **plus** the text of `resources/TERMS-OF-USE.md`
pasted into *Additional license terms*.

## Category

**Education → Instructional tools.**
(Next best fit if that reads wrong at submission: *Productivity*. Education is the intended
audience.)

## Support and contact

| Field | Value |
|---|---|
| Privacy policy URL | **From 1.1.0:** `https://softwareplus.ai/statistle/privacy/` (the policies moved to SoftwarePlus.ai; revised 2026-10-31 for Check for a new version and Help on the web). 1.0 used `https://punchmonkeyserver.com/privacy/statistle`, which stays up for older installs. |
| Support contact info | ron.eaglin@gmail.com |
| Website | *(optional)* |

---

## Screenshots — Ron to capture

Requirements: **.png**, at least **1366 × 768**, up to 10 (Microsoft recommends 4).

**Capture wider than 1366.** SMADA's first set came out 1341–1347 px and had to be padded before
Partner Center would take them. A maximised window on a 1920-wide display is comfortably clear.

Capture with a real worked example loaded — a sample from `samples/` — not an empty form.
Suggested set, in listing order; the first is what appears in search results:

1. **A two-sample test** — the result, the assumption checks, and the reference distribution with
   the rejection region shaded. The clearest picture of what Statistle is for.
2. **The Code tab** — the Python (or R) written for that same analysis. This is the
   differentiator; it belongs high in the list.
3. **Descriptive statistics** — the summary with the histogram and fitted normal curve.
4. **Confidence interval** — with the interval bar plot.
5. **Simple regression** — the fitted line with its scatter, slope and r-squared.
6. **One-way ANOVA** — the table with Tukey HSD.
7. **Instructions tab** — a full manual page for one analysis.
8. **A data table** — the left-hand table with columns entered, showing where the data lives.

Captions are optional; if used, keep them to one plain sentence each.
