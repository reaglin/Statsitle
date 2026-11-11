---
title: Test statistics for one sample
---

# Test statistics for one sample

| Test | Statistic | Reference distribution under H₀ | Conditions |
|---|---|---|---|
| Mean, σ known | z = (x̄ − μ₀) / (σ/√n) | standard normal | normal population, or n large (CLT) |
| Mean, σ unknown | t = (x̄ − μ₀) / (s/√n) | t, df = n − 1 | normal population, or n large |
| Proportion | z = (p̂ − p₀) / √(p₀(1 − p₀)/n) | standard normal (approx.) | np₀ ≥ 10 and n(1 − p₀) ≥ 10 |
| Variance | χ² = (n − 1)s² / σ₀² | χ², df = n − 1 | normal population (sensitive to it) |

- x̄ sample mean, s sample standard deviation (divides by n − 1), p̂ = x/n sample proportion, μ₀, p₀, σ₀ the values in H₀.
- The proportion test's standard error uses **p₀**, because the test assumes H₀.
- **Degrees of freedom:** n − 1 for a one-sample t; the deviations from x̄ add to zero, so one is not free.

## t against z

| df | 4 | 11 | 15 | 30 | ∞ (z) |
|---|---|---|---|---|---|
| t_{0.025, df} (two-sided α = 0.05) | 2.776 | 2.201 | 2.131 | 2.042 | 1.960 |

Use t whenever the standard deviation comes from the sample. t has heavier tails, so the same value of the statistic gives a larger p-value than z.

## Python and R

| Task | Python | R |
|---|---|---|
| t-test from data | `stats.ttest_1samp(x, mu0, alternative='two-sided')` | `t.test(x, mu = mu0)` |
| p from t | `2 * stats.t.sf(abs(t), df)` | `2 * pt(abs(t), df, lower.tail = FALSE)` |
| Proportion test | `statsmodels.stats.proportion.proportions_ztest(x, n, p0)` | `prop.test(x, n, p = p0, correct = FALSE)` |
| p from z | `2 * stats.norm.sf(abs(z))` | `2 * pnorm(abs(z), lower.tail = FALSE)` |

`proportions_ztest` uses p̂ in its standard error unless given `prop_var=p0`; `prop.test` uses p₀ and reports χ² = z².

## In the Workbench

**One-sample test**, with **Test** set to *Mean, σ known (z-test)*, *Mean, σ unknown (t-test)*, *Proportion (z-test)* or *Variance (χ²-test)*. **Data from** a column of a data table computes x̄, s and n for you.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 9-2 (z), 9-3 (t), 9-4 (variance), 9-5 (proportion).
- NIST/SEMATECH *e-Handbook*, Section 7.2.2: https://www.itl.nist.gov/div898/handbook/prc/section2/prc22.htm
