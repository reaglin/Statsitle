---
title: Choosing a two-sample test
---

# Choosing a two-sample test

Four questions, in order:

1. **What is compared?** Means of a measurement, spreads (variances) or proportions.
2. **Independent or paired?** The same units measured twice, or matched pairs: paired. Separate groups of units: independent.
3. **Is σ known?** Only from long process history: z. Otherwise t.
4. **Do the assumptions hold?** Normal (or large n) for the z and t tests; equal variances for the pooled t. If not: Welch, or a rank test.

| What you have | Test | Workbench "Test" |
|---|---|---|
| Independent, σ₁ and σ₂ known | Two-sample z | Two means, σ₁ and σ₂ known (z) |
| Independent, σ unknown, equal variances reasonable | Pooled t | Two means, equal variances (pooled t) |
| Independent, σ unknown, variances may differ | **Welch t** (the safe default) | Two means, unequal variances (Welch t) |
| Paired, differences roughly normal | Paired t | Paired samples (paired t) |
| Independent, far from normal, small n | Mann–Whitney U | Mann–Whitney U (non-parametric, independent) |
| Paired, differences far from normal, small n | Wilcoxon signed-rank | Wilcoxon signed-rank (non-parametric, paired) |
| Two variances | F-test | Two variances (F) |
| Two proportions | Two-proportion z (Module 10) | Two proportions (pooled z) |

**Sample size:** with each n above about 30, the t-tests are robust to non-normality (central limit theorem). The rank tests matter most for small, skewed samples or samples with extreme values.

**One- or two-sided:** two-sided unless the direction was fixed before the data were seen.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 10 (statistical inference for two samples).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.3 (comparisons based on data from two processes): https://www.itl.nist.gov/div898/handbook/prc/section3/prc3.htm
