---
title: Online experiments at scale
---

# Online experiments at scale

| Quantity | Formula |
|---|---|
| Test of two conversion rates | two-proportion z-test (item 10.5) |
| Absolute and relative lift | p_B − p_A; (p_B − p_A)/p_A |
| False winners among m metrics | 1 − (1 − α)^m when independent |
| Bonferroni for m metrics | α/m each |
| Sample ratio check | chi-square goodness of fit of the arm sizes to the planned split (item 8.5) |

**Practice:** one primary metric named in advance; guardrail metrics; fixed sample size or a sequential design; A/A tests;
check the sample ratio; run long enough for novelty effects to fade.

## Python and R

| Task | Python | R |
|---|---|---|
| Test | `statsmodels.stats.proportion.proportions_ztest([x1, x2], [n1, n2])` | `prop.test(c(x1, x2), c(n1, n2), correct = FALSE)` |
| Sample size | `NormalIndPower().solve_power(proportion_effectsize(p1, p2), power=0.8)` | `power.prop.test(p1 = p1, p2 = p2, power = 0.8)` |

**Read more:** Kohavi, Tang and Xu, *Trustworthy Online Controlled Experiments* (Cambridge University Press, 2020).
