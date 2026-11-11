---
title: Confidence intervals and hypothesis tests at a glance
---

# Confidence intervals and hypothesis tests at a glance

## Confidence interval for a mean (σ unknown)

x̄ ± t(α/2, n − 1) × s / √n

| Piece | Meaning |
|---|---|
| x̄ | point estimate of μ |
| s / √n | standard error of the mean |
| t(α/2, n − 1) | critical value: 2.131 for 95 % and n = 16; 2.064 for 95 % and n = 25 |
| t × s / √n | margin of error |

Reading: "We are 95 % confident that μ is between L and U." The confidence is in the method: about 95 % of such intervals contain μ.

## Hypothesis test for a mean (one-sample t-test)

| Step | |
|---|---|
| Hypotheses | H₀: μ = μ₀; H₁: μ ≠ μ₀ (or < or >) |
| Significance level | α, usually 0.05, chosen first |
| Test statistic | t = (x̄ − μ₀) / (s / √n), with n − 1 degrees of freedom |
| p-value | the probability, if H₀ is true, of a statistic at least this far from μ₀ |
| Decision | p < α: reject H₀. p ≥ α: do not reject H₀ |

A two-sided test at level α rejects μ₀ exactly when μ₀ is outside the (1 − α) confidence interval.

## Interpreting

- Not rejecting H₀ is not proof of H₀.
- Statistical significance is not practical significance: compare the size of the effect with the tolerance.
- A confidence interval for a mean is not a range for individual parts.
- A biased sample gives biased conclusions.

## Python and R

| Task | Python (scipy.stats) | R |
|---|---|---|
| t critical value | `stats.t.ppf(0.975, df)` | `qt(0.975, df)` |
| Two-sided p-value from t | `2 * stats.t.sf(abs(t), df)` | `2 * pt(abs(t), df, lower.tail = FALSE)` |
| Test and interval from data | `stats.ttest_1samp(x, mu0)` | `t.test(x, mu = mu0)` |

## In the Workbench

**Confidence interval** and **One-sample test**. Modules 7 and 8 use them fully.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 8.2 (confidence interval on a mean, variance unknown) and Section 9.3 (tests on a mean, variance unknown).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.2.2, Are the data consistent with the assumed process mean?: https://www.itl.nist.gov/div898/handbook/prc/section2/prc22.htm
