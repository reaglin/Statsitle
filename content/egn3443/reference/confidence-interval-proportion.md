---
title: Confidence interval for a proportion
---

# Confidence interval for a proportion

| Quantity | Formula |
|---|---|
| Point estimate | p̂ = x / n |
| Standard error | √(p̂(1 − p̂) / n) |
| Interval (Wald, the course's hand formula) | p̂ ± z_{α/2} · √(p̂(1 − p̂) / n) |
| Margin of error | z_{α/2} · √(p̂(1 − p̂) / n) |
| Wilson score interval | [p̂ + z²/(2n) ± z·√(p̂(1 − p̂)/n + z²/(4n²))] / (1 + z²/n) |

Always z, never t. z: 1.645 (90 %), 1.960 (95 %), 2.576 (99 %).

**Conditions:** a random sample of independent items; n·p̂ ≥ 10 and n·(1 − p̂) ≥ 10. With fewer successes or failures, use Wilson's interval or an exact binomial interval.

## Python and R

| Quantity | Python | R |
|---|---|---|
| Wald interval | `p_hat + np.array([-1, 1]) * stats.norm.ppf(0.975) * np.sqrt(p_hat * (1 - p_hat) / n)` | `p_hat + c(-1, 1) * qnorm(0.975) * sqrt(p_hat * (1 - p_hat) / n)` |
| Wilson interval | `statsmodels.stats.proportion.proportion_confint(x, n, method="wilson")` | `prop.test(x, n, correct = FALSE)$conf.int` |
| Exact (Clopper–Pearson) | `proportion_confint(x, n, method="beta")` | `binom.test(x, n)$conf.int` |

R's `prop.test` uses a continuity correction unless `correct = FALSE`.

## In the Workbench

**Confidence interval**, *Interval for*: "A proportion (z)". Type the successes x and n, or choose a column of 0s and 1s. The Wilson limits are shown below the Wald limits.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 8-4 (large-sample confidence interval for a population proportion).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.2.4.1, Confidence intervals (for a proportion): https://www.itl.nist.gov/div898/handbook/prc/section2/prc241.htm
