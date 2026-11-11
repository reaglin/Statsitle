---
title: The uniform distribution
---

# The uniform distribution

X ~ U(a, b): every value between a and b equally likely.

| Quantity | Formula |
|---|---|
| Density | f(x) = 1/(b − a) for a ≤ x ≤ b, 0 outside |
| CDF | F(x) = (x − a)/(b − a) for a ≤ x ≤ b |
| P(c ≤ X ≤ d) | (d − c)/(b − a): width × height |
| Percentile for q | a + q(b − a) |
| Mean | (a + b)/2 |
| Variance | (b − a)²/12 |
| Standard deviation | (b − a)/√12 |

**Uses:** a random moment within a cycle, a rounding error (U(−0.5, 0.5) units, σ = 0.289 units), limits known and nothing else, random numbers for simulation.

## Python and R

| Quantity | Python (`X = stats.uniform(loc=a, scale=b - a)`) | R |
|---|---|---|
| P(X ≤ x) | `X.cdf(x)` | `punif(x, a, b)` |
| P(c ≤ X ≤ d) | `X.cdf(d) - X.cdf(c)` | `punif(d, a, b) - punif(c, a, b)` |
| Percentile | `X.ppf(q)` | `qunif(q, a, b)` |
| Mean, standard deviation | `X.mean()`, `X.std()` | `(a + b) / 2`, `(b - a) / sqrt(12)` |

scipy's `scale` is the **width** b − a, not the upper limit.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 4-5 (continuous uniform distribution).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.6.6.2, Uniform Distribution: https://www.itl.nist.gov/div898/handbook/eda/section3/eda3662.htm
