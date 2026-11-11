---
title: Quality control: outliers, control limits, acceptance sampling
---

# Quality control: outliers, control limits, acceptance sampling

## Outliers (Tukey's fences)

| | |
|---|---|
| Lower fence | Q1 − 1.5 × IQR |
| Upper fence | Q3 + 1.5 × IQR |
| Outlier | a value beyond a fence: investigate it |

The fences use quartiles, which an outlier hardly moves; the mean and standard deviation are pulled by it.

## Control charts (simple form)

| | |
|---|---|
| Center line | x̄ of the process while stable |
| Upper control limit | UCL = x̄ + 3s |
| Lower control limit | LCL = x̄ − 3s |
| Signal | a point beyond a limit: look for a special cause |

For a normal process, 99.7 % of values are within ±3σ. Limits come from a stable period of data, without the special causes. Quality-control texts also estimate the spread from moving ranges or subgroup ranges.

**Control limits** (from the process data) are not **specification limits** (from the design).

## Acceptance sampling

A plan "test n, accept the lot if at most c are defective" accepts a lot with defect rate p with probability P(X ≤ c), X binomial with n and p. Example: n = 50, c = 1, p = 0.02 gives 0.74.

## Python and R

| Task | Python | R |
|---|---|---|
| Fences | `q1, q3 = np.percentile(x, [25, 75])`, then `q1 - 1.5*(q3 - q1)` | `quantile(x, c(0.25, 0.75))`, then the same |
| Control limits | `np.mean(x) + 3*np.std(x, ddof=1)` | `mean(x) + 3*sd(x)` |
| Acceptance probability | `stats.binom.cdf(c, n, p)` | `pbinom(c, n, p)` |

## Sources

- Montgomery, D. C., *Introduction to Statistical Quality Control*, 8th ed., Wiley, Chapters 5 and 6 (control charts) and Chapter 15 (acceptance sampling).
- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 15, Statistical Quality Control.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Chapter 6, Process or Product Monitoring and Control: https://www.itl.nist.gov/div898/handbook/pmc/pmc.htm
