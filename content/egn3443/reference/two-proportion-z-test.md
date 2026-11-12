---
title: Two-proportion z-test
---

# Two-proportion z-test

Two independent groups, each item a success or a failure (whatever outcome is counted). Group 1: x₁ successes in n₁; group 2: x₂ in n₂. H₀: p₁ = p₂ against H₁: p₁ ≠ p₂, p₁ > p₂ or p₁ < p₂.

| Quantity | Formula |
|---|---|
| Sample proportions | p̂₁ = x₁/n₁, p̂₂ = x₂/n₂ |
| Pooled proportion (under H₀) | p̂ = (x₁ + x₂)/(n₁ + n₂) |
| Test statistic | z = (p̂₁ − p̂₂) / √(p̂(1 − p̂)(1/n₁ + 1/n₂)) |
| p-value | two-sided 2 × P(Z > \|z\|); H₁ p₁ > p₂: P(Z > z); H₁ p₁ < p₂: P(Z < z) |
| (1 − α) interval for p₁ − p₂ (unpooled) | (p̂₁ − p̂₂) ± z(α/2) √(p̂₁(1 − p̂₁)/n₁ + p̂₂(1 − p̂₂)/n₂) |
| Sample size per group for a margin E | n = z(α/2)² [p₁(1 − p₁) + p₂(1 − p₂)] / E² |

**The test uses the pooled p̂** (it assumes H₀); **the interval uses p̂₁ and p̂₂** (it does not).

**Conditions:** independent groups of different items; random samples or random assignment; n₁p̂, n₁(1 − p̂), n₂p̂, n₂(1 − p̂) all at least 10 (some texts use 5). Too few: Fisher's exact test. The same items judged twice (paired proportions): McNemar's test.

**Link to chi-square:** for a 2 × 2 table, the chi-square test of independence without Yates' correction gives χ² = z² and the two-sided p-value.

## Python and R

| Task | Python | R |
|---|---|---|
| z and p by hand | `z = (p1 - p2) / np.sqrt(p * (1 - p) * (1/n1 + 1/n2))`; `2 * stats.norm.sf(abs(z))` | `z <- (p1 - p2) / sqrt(p * (1 - p) * (1/n1 + 1/n2))`; `2 * pnorm(abs(z), lower.tail = FALSE)` |
| Library test | `statsmodels.stats.proportion.proportions_ztest([x1, x2], [n1, n2])` | `prop.test(c(x1, x2), c(n1, n2), correct = FALSE)` (reports χ² = z²) |
| Fisher's exact test | `stats.fisher_exact([[x1, n1 - x1], [x2, n2 - x2]])` | `fisher.test(matrix(c(x1, n1 - x1, x2, n2 - x2), nrow = 2, byrow = TRUE))` |

## In the Workbench

**Two-sample test**, Test *Two proportions (pooled z)*, Data from *Summary statistics entered here*: Successes in sample 1 (x₁), Sample 1 size (n₁), Successes in sample 2 (x₂), Sample 2 size (n₂). With columns, each column holds 1 (success) or 0, one row per item.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 10.6 (inference on two population proportions).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.3.3, How can we determine whether two processes produce the same proportion of defectives?: https://www.itl.nist.gov/div898/handbook/prc/section3/prc33.htm
