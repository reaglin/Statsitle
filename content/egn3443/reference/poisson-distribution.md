---
title: The Poisson distribution
---

# The Poisson distribution

X = the number of events in an interval of time, length, area or volume, X ~ Poisson(λ).

**Conditions:** events happen one at a time, independently, at a constant average rate. No fixed number of trials; no upper limit on X.

| Quantity | Formula |
|---|---|
| PMF | P(X = x) = e^(−λ)·λˣ / x!, x = 0, 1, 2, … |
| Mean | μ = λ |
| Variance | σ² = λ |
| Standard deviation | σ = √λ |
| λ for the interval asked about | λ = rate per unit × length of the interval |

**Scale first:** 2 flaws per 10 m is λ = 1 for 5 m and λ = 5 for 25 m.

**At least k:** P(X ≥ k) = 1 − P(X ≤ k − 1). There is no end to add up to.

**Approximation to the binomial:** for large n and small p, Binomial(n, p) ≈ Poisson(np).

## Python and R

| Quantity | Python (`X = stats.poisson(lam)`) | R |
|---|---|---|
| P(X = k) | `X.pmf(k)` | `dpois(k, lam)` |
| P(X ≤ k) | `X.cdf(k)` | `ppois(k, lam)` |
| P(X > k) | `X.sf(k)` | `1 - ppois(k, lam)` |
| P(X ≥ k) | `X.sf(k - 1)` | `1 - ppois(k - 1, lam)` |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 3-9 (Poisson distribution).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.6.6.19, Poisson Distribution: https://www.itl.nist.gov/div898/handbook/eda/section3/eda366j.htm
