---
title: Binomial, Poisson and normal at a glance
---

# Binomial, Poisson and normal at a glance

| | Binomial | Poisson | Normal |
|---|---|---|---|
| Random variable | successes in n trials | events in an interval | a continuous measurement |
| Values | 0, 1, …, n | 0, 1, 2, … | any real number |
| Parameters | n, p | λ (mean rate per interval) | μ, σ |
| Probability | P(X = k) = C(n, k) pᵏ(1 − p)ⁿ⁻ᵏ | P(X = k) = e^(−λ) λᵏ / k! | area under the bell curve |
| Mean | np | λ | μ |
| Variance | np(1 − p) | λ | σ² |
| Conditions | fixed n; independent trials; same p | events at random, independently, at a constant rate | symmetric, many small effects |

**z-score:** z = (x − μ) / σ. **Empirical rule:** 68 % within μ ± σ, 95 % within μ ± 2σ, 99.7 % within μ ± 3σ.

## Python and R

| Task | Python (scipy.stats) | R |
|---|---|---|
| Binomial P(X = k), P(X ≤ k) | `stats.binom.pmf(k, n, p)`, `stats.binom.cdf(k, n, p)` | `dbinom(k, n, p)`, `pbinom(k, n, p)` |
| Poisson P(X = k), P(X ≤ k) | `stats.poisson.pmf(k, lam)`, `stats.poisson.cdf(k, lam)` | `dpois(k, lam)`, `ppois(k, lam)` |
| Normal P(X ≤ x) | `stats.norm.cdf(x, mu, sigma)` | `pnorm(x, mu, sigma)` |
| Normal value with P(X ≤ x) = q | `stats.norm.ppf(q, mu, sigma)` | `qnorm(q, mu, sigma)` |

## In the Workbench

**Discrete distribution** (binomial, Poisson and more) and **Continuous distribution** (normal and more). Modules 4 and 5 use them fully.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 3.6 (binomial), 3.9 (Poisson) and 4.6 (normal).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.6.6, Gallery of Distributions: https://www.itl.nist.gov/div898/handbook/eda/section3/eda366.htm
