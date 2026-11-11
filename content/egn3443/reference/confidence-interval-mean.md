---
title: Confidence interval for a mean
---

# Confidence interval for a mean

estimate ± critical value × standard error

| Case | Interval | Critical value |
|---|---|---|
| σ known | x̄ ± z_{α/2} · σ/√n | z: 1.645 (90 %), 1.960 (95 %), 2.576 (99 %) |
| σ unknown (the usual case) | x̄ ± t_{α/2, n−1} · s/√n | t with n − 1 degrees of freedom |
| One-sided upper bound, σ unknown | μ ≤ x̄ + t_{α, n−1} · s/√n | all of α in one tail |
| One-sided lower bound, σ unknown | μ ≥ x̄ − t_{α, n−1} · s/√n | all of α in one tail |

Margin of error E = critical value × standard error. Width = 2E.

**Conditions:** a random sample of independent observations; a normal population, or n ≥ 30 (look at a histogram or box plot for a small sample).

## t critical values, two-sided

| df | 90 % | 95 % | 99 % |
|---|---|---|---|
| 5 | 2.015 | 2.571 | 4.032 |
| 7 | 1.895 | 2.365 | 3.499 |
| 9 | 1.833 | 2.262 | 3.250 |
| 11 | 1.796 | 2.201 | 3.106 |
| 14 | 1.761 | 2.145 | 2.977 |
| 19 | 1.729 | 2.093 | 2.861 |
| 29 | 1.699 | 2.045 | 2.756 |
| ∞ (z) | 1.645 | 1.960 | 2.576 |

## Python and R

| Quantity | Python (`from scipy import stats`) | R |
|---|---|---|
| z_{α/2} | `stats.norm.ppf(1 - alpha / 2)` | `qnorm(1 - alpha / 2)` |
| t_{α/2, n−1} | `stats.t.ppf(1 - alpha / 2, n - 1)` | `qt(1 - alpha / 2, n - 1)` |
| t interval from data | `stats.t.interval(0.95, n - 1, loc=x.mean(), scale=x.std(ddof=1) / np.sqrt(n))` | `t.test(x, conf.level = 0.95)$conf.int` |

## In the Workbench

**Confidence interval**, *Interval for*: "A mean, σ known (z)" or "A mean, σ unknown (t)". *Data from*: summary statistics typed in, or a column of a data table. A one-sided 95 % bound is the matching end of the 90 % two-sided interval.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 8-1 (σ known) and 8-2 (σ unknown, the t distribution).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.5.2, Confidence Limits for the Mean: https://www.itl.nist.gov/div898/handbook/eda/section3/eda352.htm
