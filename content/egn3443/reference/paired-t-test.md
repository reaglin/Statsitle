---
title: The paired t-test
---

# The paired t-test

Use it when each value in one sample is matched to one value in the other: the same unit measured twice (before and after, two gauges, two labs), or units matched in pairs.

| Step | |
|---|---|
| Differences | d = x₁ − x₂ for each pair (decide the order first) |
| Hypotheses | H₀: μ_d = Δ₀ (usually 0); H₁: μ_d ≠, < or > Δ₀ |
| Summaries | d̄ = mean of the d's; s_d = their standard deviation (divide by n − 1); n = number of pairs |
| Statistic | t = (d̄ − Δ₀) / (s_d / √n) |
| df | n − 1 |
| Interval for μ_d | d̄ ± t(α/2, n − 1) × s_d/√n |
| Assumes | pairs independent of each other; the **differences** roughly normal |

Pairing removes the unit-to-unit variation, so a small, steady change shows up that an independent-samples test would miss. Treating paired data as independent wastes this; pairing data that are not paired is meaningless.

Not normal, small n: the **Wilcoxon signed-rank test** (see *Rank tests*).

## Python and R

| Task | Python (scipy.stats) | R |
|---|---|---|
| Paired t | `stats.ttest_rel(x, y)` | `t.test(x, y, paired = TRUE)` |
| The same, on the differences | `stats.ttest_1samp(x - y, 0)` | `t.test(x - y, mu = 0)` |
| Normality of the differences | `stats.shapiro(x - y)` | `shapiro.test(x - y)` |

## In the Workbench

**Two-sample test**, Test *Paired samples (paired t)*. With columns, sample 1 is the "before" column and rows must line up. With summaries, type d̄, s_d and n in the sample 1 boxes.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 10.4 (paired t-test).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.3.1: https://www.itl.nist.gov/div898/handbook/prc/section3/prc31.htm
