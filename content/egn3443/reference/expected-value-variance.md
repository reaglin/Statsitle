---
title: Expected value and variance of a discrete random variable
---

# Expected value and variance of a discrete random variable

| Quantity | Formula | Unit |
|---|---|---|
| Expected value (mean) | μ = E[X] = Σ x·f(x) | units of X |
| Variance | σ² = V[X] = Σ (x − μ)²·f(x) | units of X, squared |
| Variance, shortcut | σ² = E[X²] − μ², with E[X²] = Σ x²·f(x) | units of X, squared |
| Standard deviation | σ = √σ² | units of X |
| Linear function Y = aX + b | E[Y] = a·E[X] + b; V[Y] = a²·V[X]; σ_Y = abs(a)·σ_X | units of Y |

No n or n − 1: the probabilities do the weighting. A PMF describes all the outcomes, so these are population values (μ and σ, not x̄ and s).

**Means of the common models:** Bernoulli p; binomial np; Poisson λ; geometric 1/p; Pascal r/p; hypergeometric nK/N.
**Variances:** Bernoulli p(1 − p); binomial np(1 − p); Poisson λ; geometric (1 − p)/p²; Pascal r(1 − p)/p²; hypergeometric n(K/N)(1 − K/N)(N − n)/(N − 1).

## Python and R

| Quantity | Python | R |
|---|---|---|
| Mean | `np.sum(x * p)` or `X.mean()` | `sum(x * p)` |
| Variance | `np.sum((x - mu)**2 * p)` or `X.var()` | `sum((x - mu)^2 * p)` |
| Standard deviation | `np.sqrt(var)` or `X.std()` | `sqrt(v)` |

Not `np.var(x)` or `var(x)`: those treat the values as an unweighted dataset.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 3-4 (mean and variance of a discrete random variable).
- EGN3443 Module 4, Topic 3: https://reaglin.github.io/EGN3443/week-5module-4-discrete-probability-distributions/
