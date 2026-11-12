---
title: The familywise error rate
---

# The familywise error rate

When m tests are run, each at level α, the **familywise error rate** (FWER) is the probability that at least one of them rejects a null hypothesis that is true.

| | |
|---|---|
| m independent tests | FWER = 1 − (1 − α)ᵐ |
| Upper bound, any m tests | FWER ≤ m × α |
| Pairs among k groups | m = k(k − 1)/2 |
| Bonferroni correction | run each test at α/m; then FWER ≤ α |

| k groups | m pairs | 1 − 0.95ᵐ |
|---|---|---|
| 3 | 3 | 0.143 |
| 4 | 6 | 0.265 |
| 5 | 10 | 0.401 |
| 6 | 15 | 0.537 |

The pairwise t-tests among k groups share groups, so they are correlated and the true FWER is somewhat below the formula (about 0.12 for k = 3 and 0.28 for k = 5, from simulation with 10 per group), but still far above α.

## The ways out

- **One ANOVA F test** of H₀: all the means are equal, at level α.
- After a significant F, a **post-hoc procedure** that holds the FWER for the comparisons made: Tukey's HSD (all pairs), Bonferroni (a few planned comparisons), Scheffé (all contrasts), Dunnett (each group against a control).

## Python and R

| | Python | R |
|---|---|---|
| FWER | `1 - (1 - alpha) ** m` | `1 - (1 - alpha)^m` |
| Bonferroni-adjusted p-values | `statsmodels.stats.multitest.multipletests(p, method="bonferroni")` | `p.adjust(p, "bonferroni")` |

## In the Workbench

**Simulation**, Experiment *Many t-tests: the familywise error*: set the tests per trial, the sample size, α, the number of trials and the seed. It compares the share of trials with at least one rejection with 1 − (1 − α)ᵐ.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 13.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.4.7 (multiple comparisons): https://www.itl.nist.gov/div898/handbook/prc/section4/prc47.htm
