---
title: Checking the assumptions of two-sample tests
---

# Checking the assumptions of two-sample tests

| Assumption | Needed by | How to check |
|---|---|---|
| Random samples, independent observations | all tests | from how the data were collected (no test can check it) |
| The two samples independent of each other | z, pooled, Welch, Mann–Whitney, two-proportion z | from the design (otherwise paired) |
| Normal populations (or n large) | z, pooled, Welch; the **differences** for paired t | box plot or histogram; Shapiro–Wilk |
| Equal variances | pooled t | the course's rule: larger s² over smaller s² above about 2 to 3 → treat as unequal; Levene's test |
| Enough successes and failures | two-proportion z | n₁p̂, n₁(1 − p̂), n₂p̂, n₂(1 − p̂) all at least 10, with the pooled p̂; otherwise Fisher's exact test |
| Normal populations | F-test for two variances | Shapiro–Wilk; the F-test is very sensitive to non-normality |

## Shapiro–Wilk test of normality

H₀: the sample comes from a normal population. W near 1 looks normal. **p < 0.05: evidence against normality.** p ≥ 0.05: no evidence against it (not proof). Low power for small n; flags trivial departures for large n. Look at the plot too.

## Levene's test of equal variances

H₀: σ₁² = σ₂². Compares the mean absolute deviation from each sample's median (the Brown–Forsythe form, the default in scipy and R's car package). Robust to non-normality. **p < 0.05: variances differ, use Welch.**

## F-test for two variances

F = s₁²/s₂², with n₁ − 1 and n₂ − 1 df. Two-sided p = 2 × the smaller tail. Interval for σ₁²/σ₂²: [F / F(α/2; n₁ − 1, n₂ − 1), F / F(1 − α/2; n₁ − 1, n₂ − 1)], with F(α/2; …) the upper α/2 point. Use it when the variance is the question, with normal data.

## Python and R

| Check | Python (scipy.stats) | R |
|---|---|---|
| Shapiro–Wilk | `stats.shapiro(x)` | `shapiro.test(x)` |
| Levene (median) | `stats.levene(x, y)` | `car::leveneTest(value ~ group)` |
| F-test | `F = np.var(x, ddof=1) / np.var(y, ddof=1)`; `stats.f.sf(F, n1 - 1, n2 - 1)` | `var.test(x, y)` |

## In the Workbench

**Two-sample test** with Data from two columns and a mean test: the Shapiro–Wilk and Levene rows appear with the results. Test *Two variances (F)* for the F-test.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 10.5 (two variances) and Section 6.7 (probability plots).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Sections 1.3.5.9 (F-test for equality of two variances) and 1.3.5.10 (Levene test): https://www.itl.nist.gov/div898/handbook/eda/section3/eda35a.htm
- Shapiro, S. S. and Wilk, M. B. (1965), An analysis of variance test for normality, *Biometrika* 52, 591–611.
