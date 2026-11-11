---
title: Two-sample z and t tests
---

# Two-sample z and t tests

Independent samples: n₁ values from population 1, n₂ from population 2. H₀: μ₁ − μ₂ = Δ₀ (usually 0) against H₁: ≠, < or >. Every statistic is (x̄₁ − x̄₂ − Δ₀) / SE.

| Test | Use when | SE | Reference | df |
|---|---|---|---|---|
| Two-sample z | σ₁, σ₂ known | √(σ₁²/n₁ + σ₂²/n₂) | Z | — |
| Pooled t | σ unknown, σ₁ = σ₂ reasonable | sₚ √(1/n₁ + 1/n₂) | t | n₁ + n₂ − 2 |
| Welch t | σ unknown, variances may differ (default) | √(s₁²/n₁ + s₂²/n₂) | t | ν (below) |

**Pooled standard deviation:** sₚ = √{[(n₁ − 1)s₁² + (n₂ − 1)s₂²] / (n₁ + n₂ − 2)}

**Welch–Satterthwaite df:** with a = s₁²/n₁ and b = s₂²/n₂, ν = (a + b)² / [a²/(n₁ − 1) + b²/(n₂ − 1)]

**p-value:** two-sided 2 × P(T > |t|); H₁ "<": P(T < t); H₁ ">": P(T > t).

**Confidence interval for μ₁ − μ₂:** (x̄₁ − x̄₂) ± t(α/2, df) × SE (z(α/2) for the z-test). A two-sided test at α rejects H₀: μ₁ − μ₂ = 0 exactly when 0 is outside the (1 − α) interval.

**Effect size:** Cohen's d = (x̄₁ − x̄₂) / sₚ. About 0.2 small, 0.5 medium, 0.8 large (Cohen, 1988).

## Python and R

| Task | Python (scipy.stats) | R |
|---|---|---|
| Welch t from data | `stats.ttest_ind(x, y, equal_var=False)` | `t.test(x, y)` |
| Pooled t from data | `stats.ttest_ind(x, y, equal_var=True)` | `t.test(x, y, var.equal = TRUE)` |
| From summaries | `stats.ttest_ind_from_stats(m1, s1, n1, m2, s2, n2, equal_var=False)` | the arithmetic, with `pt` and `qt` |
| One-sided | `alternative='less'` or `'greater'` | `alternative = "less"` or `"greater"` |
| t critical value | `stats.t.ppf(1 - alpha/2, df)` | `qt(1 - alpha/2, df)` |
| z-test p-value | `2 * stats.norm.sf(abs(z))` | `2 * pnorm(abs(z), lower.tail = FALSE)` |

## In the Workbench

**Two-sample test**, Test *Two means, σ₁ and σ₂ known (z)*, *Two means, equal variances (pooled t)* or *Two means, unequal variances (Welch t)*; Data from summary statistics or two columns.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 10.1 (variances known) and 10.2 (variances unknown: pooled and Welch).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.3.1, Do two processes have the same mean?: https://www.itl.nist.gov/div898/handbook/prc/section3/prc31.htm
