---
title: The binomial distribution
---

# The binomial distribution

X = the number of successes in n trials, X ~ Binomial(n, p).

**Conditions (BINS):** Binary outcomes; Independent trials; Number of trials n fixed; Same p on every trial.

| Quantity | Formula |
|---|---|
| PMF | P(X = x) = C(n, x)·pˣ·(1 − p)ⁿ⁻ˣ, x = 0, 1, …, n |
| Binomial coefficient | C(n, x) = n! / (x!(n − x)!) |
| Mean | μ = np |
| Variance | σ² = np(1 − p) |
| Standard deviation | σ = √(np(1 − p)) |

**Special case:** n = 1 is a Bernoulli trial: P(X = 1) = p, P(X = 0) = 1 − p.

**Approximations:** when n is large and p small (n ≥ 20, p ≤ 0.05), Poisson with λ = np. Drawing from a finite lot without replacement is hypergeometric; the binomial with p = K/N is close when n is under about a tenth of N.

## Python and R

| Quantity | Python (`X = stats.binom(n, p)`) | R |
|---|---|---|
| P(X = k) | `X.pmf(k)` | `dbinom(k, n, p)` |
| P(X ≤ k) | `X.cdf(k)` | `pbinom(k, n, p)` |
| P(X > k) | `X.sf(k)` | `1 - pbinom(k, n, p)` |
| P(X ≥ k) | `X.sf(k - 1)` | `1 - pbinom(k - 1, n, p)` |
| Mean, variance | `X.mean()`, `X.var()` | `n * p`, `n * p * (1 - p)` |
| C(n, x) | `math.comb(n, x)` | `choose(n, x)` |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 3-6 (binomial distribution).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.6.6.18, Binomial Distribution: https://www.itl.nist.gov/div898/handbook/eda/section3/eda366i.htm
