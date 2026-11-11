---
title: The exponential distribution
---

# The exponential distribution

X ~ Exp(λ): the waiting time to the next random event, or the life of a part that fails at random. λ is the **rate** (events per unit time); **the mean is 1/λ**.

| Quantity | Formula |
|---|---|
| Density | f(x) = λ e^(−λx), x ≥ 0 |
| P(X ≤ x) | 1 − e^(−λx) |
| P(X > x), reliability R(x) | e^(−λx) |
| Percentile for q | −ln(1 − q)/λ |
| Median | ln 2/λ = 0.693/λ |
| Mean | 1/λ |
| Variance | 1/λ² |
| Standard deviation | 1/λ |
| Hazard rate | λ at every age: it does not wear out |
| Memoryless | P(X > s + t \| X > s) = P(X > t) |

**Link to the Poisson:** if the number of events in time t is Poisson with mean λt, the time between events is exponential with rate λ. P(no event in t) = e^(−λt) both ways.

**Rate from a mean:** a mean time between failures of 400 h gives λ = 1/400 = 0.0025 per hour.

## Python and R

| Quantity | Python (`X = stats.expon(scale=1/lam)`) | R |
|---|---|---|
| P(X ≤ x) | `X.cdf(x)` | `pexp(x, rate = lam)` |
| P(X > x) | `X.sf(x)` | `1 - pexp(x, rate = lam)` |
| Percentile for q | `X.ppf(q)` | `qexp(q, rate = lam)` |

scipy takes `scale = 1/λ` (the mean); R takes the `rate` λ.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 4-8 (exponential distribution).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.6.6.7, Exponential Distribution: https://www.itl.nist.gov/div898/handbook/eda/section3/eda3667.htm
