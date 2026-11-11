---
title: The Weibull distribution and fitting to data
---

# The Weibull distribution and fitting to data

## The Weibull

Shape β, scale η (the characteristic life, in the units of X).

| Quantity | Formula |
|---|---|
| P(X ≤ x) | 1 − e^(−(x/η)^β) |
| Reliability R(x) = P(X > x) | e^(−(x/η)^β) |
| Hazard rate h(x) | (β/η)(x/η)^(β−1) |
| Percentile for q | η(−ln(1 − q))^(1/β); B10 life: q = 0.10 |
| R(η) | e⁻¹ = 0.368: 63.2 % have failed by η |
| Mean | η Γ(1 + 1/β), where Γ is the gamma function (Python `scipy.special.gamma`, R `gamma`) |

| β | Failure rate | Meaning |
|---|---|---|
| < 1 | falls with age | early failures |
| = 1 | constant | the exponential, λ = 1/η |
| > 1 | rises with age | wear-out |

## Fitting by maximum likelihood

| Family | Fitted parameters |
|---|---|
| Normal | μ̂ = x̄; σ̂ = √(Σ(x − x̄)²/n), dividing by **n** (as scipy's `norm.fit`), so σ̂ = s·√((n − 1)/n) |
| Exponential | λ̂ = 1/x̄ |
| Uniform | â = min, b̂ = max |
| Weibull | β̂ solves Σx^β ln x / Σx^β − 1/β − mean(ln x) = 0; then η̂ = (Σx^β̂/n)^(1/β̂) |

## Judging the fit

| Check | Good fit |
|---|---|
| Histogram with the fitted density | the curve follows the bars, also in the tails |
| Q–Q plot: sorted data against the model's quantiles at (i − 0.5)/n | points near a straight line |
| Q–Q correlation r | near 1 (0.98 or more for 20 to 50 values); the larger r of two families fits better |
| Kolmogorov–Smirnov D | the largest gap between the data's cumulative proportion and the fitted CDF; smaller is better |

## Python and R

| Quantity | Python | R |
|---|---|---|
| Model | `X = stats.weibull_min(beta, scale=eta)` | `pweibull(x, beta, eta)` |
| R(x) | `X.sf(x)` | `1 - pweibull(x, beta, eta)` |
| Percentile | `X.ppf(q)` | `qweibull(q, beta, eta)` |
| Fit | `stats.weibull_min.fit(data, floc=0)` | `MASS::fitdistr(data, "weibull")` |
| Q–Q r | `np.corrcoef(X.ppf(p), np.sort(data))[0, 1]` | `cor(qweibull(p, beta, eta), sort(data))` |
| K–S D | `stats.kstest(data, X.cdf).statistic` | `ks.test(data, "pweibull", beta, eta)$statistic` |

with p = (i − 0.5)/n for i = 1, …, n.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 4-10 (Weibull distribution), 7-4 (maximum likelihood) and 6-7 (probability plots).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.6.6.8, Weibull Distribution: https://www.itl.nist.gov/div898/handbook/eda/section3/eda3668.htm, and Section 1.3.3.21, Normal Probability Plot.
