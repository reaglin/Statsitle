---
title: Independent and paired designs
---

# Independent and paired designs

**The question:** does each observation in one group have one particular partner in the other?

| Design | What links the values | Test on a measurement | Rank test |
|---|---|---|---|
| **Independent samples** | nothing; sizes may differ | Welch t (or pooled t) | Mann–Whitney U |
| **Repeated measures** | the same unit measured twice (before/after, two conditions) | paired t | Wilcoxon signed-rank |
| **Matched pairs** | two units matched on what affects the outcome; treatment assigned within the pair | paired t | Wilcoxon signed-rank |
| **Natural pairs** | two units that come as a pair (two halves, two sides, neighbors) | paired t | Wilcoxon signed-rank |

**Paired:** work with the differences d = (group 1) − (group 2); n pairs, df = n − 1; drop a pair with a missing value.

**Misreading the design:**

- Paired data as independent: the unit-to-unit variation stays in the SE; the test loses power and may miss a real effect.
- Independent data as paired: the pairs are invented, and the p-value is wrong.
- Pairing on something that does not affect the outcome: no variation removed, degrees of freedom lost.

## Python and R

| Test | Python (scipy.stats) | R |
|---|---|---|
| Paired t | `stats.ttest_rel(x, y)` | `t.test(x, y, paired = TRUE)` |
| Welch t | `stats.ttest_ind(x, y, equal_var=False)` | `t.test(x, y)` |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 10.4 (paired t-test).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.3.1: https://www.itl.nist.gov/div898/handbook/prc/section3/prc31.htm
