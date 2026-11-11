---
title: The normal distribution
---

# The normal distribution

X ~ N(μ, σ²): the symmetric bell curve. **The second parameter is the variance**; σ = √σ².

| Quantity | Formula |
|---|---|
| Density | f(x) = (1/(σ√(2π))) e^(−(x − μ)²/(2σ²)) |
| z-score | z = (x − μ)/σ |
| P(X ≤ x) | Φ(z) |
| P(X > x) | 1 − Φ(z) |
| P(x₁ ≤ X ≤ x₂) | Φ(z₂) − Φ(z₁) |
| Percentile for q | x = μ + zσ, where Φ(z) = q |
| Symmetry | Φ(−z) = 1 − Φ(z) |
| Mean, median | μ |
| Variance | σ² |

**Empirical rule:** 68.3 % within μ ± σ, 95.4 % within μ ± 2σ, 99.7 % within μ ± 3σ.

## Values of Φ to know

| z | −2.326 | −1.960 | −1.645 | −1 | 0 | 1 | 1.645 | 1.960 | 2.326 |
|---|---|---|---|---|---|---|---|---|---|
| Φ(z) | 0.01 | 0.025 | 0.05 | 0.1587 | 0.5 | 0.8413 | 0.95 | 0.975 | 0.99 |

## Python and R

| Quantity | Python (`X = stats.norm(mu, sigma)`) | R |
|---|---|---|
| Φ(z) | `stats.norm.cdf(z)` | `pnorm(z)` |
| P(X ≤ x) | `X.cdf(x)` | `pnorm(x, mu, sigma)` |
| P(X > x) | `X.sf(x)` | `pnorm(x, mu, sigma, lower.tail = FALSE)` |
| Percentile for q | `X.ppf(q)` | `qnorm(q, mu, sigma)` |
| z for q | `stats.norm.ppf(q)` | `qnorm(q)` |

Both take **σ**, the standard deviation, not the variance.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 4-6 (normal distribution) and Appendix Table III (cumulative standard normal distribution).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.6.6.1, Normal Distribution: https://www.itl.nist.gov/div898/handbook/eda/section3/eda3661.htm
