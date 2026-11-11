---
title: The central limit theorem for inference
---

# The central limit theorem for inference

## The sampling distribution of x̄

| | |
|---|---|
| Mean | E[X̄] = μ |
| Standard error | σ/√n |
| Shape | normal if the population is normal (any n); approximately normal for n ≥ 30 otherwise (more for strong skew) |
| Standardized | Z = (X̄ − μ) / (σ/√n) ≈ standard normal |

## The sampling distribution of p̂

Mean p, standard error √(p(1 − p)/n), approximately normal when np ≥ 10 and n(1 − p) ≥ 10.

## From the CLT to an interval

P(μ − 1.96·σ/√n ≤ X̄ ≤ μ + 1.96·σ/√n) = 0.95

so, for 95 % of samples,

X̄ − 1.96·σ/√n ≤ μ ≤ X̄ + 1.96·σ/√n

which is the 95 % confidence interval x̄ ± 1.96·σ/√n. With σ unknown, s replaces σ and t replaces z.

## Python and R

| Quantity | Python (`from scipy import stats`) | R |
|---|---|---|
| P(x̄ ≤ a) | `stats.norm.cdf(a, mu, sigma / np.sqrt(n))` | `pnorm(a, mu, sigma / sqrt(n))` |
| Middle 95 % of x̄ | `stats.norm.ppf([0.025, 0.975], mu, sigma / np.sqrt(n))` | `qnorm(c(0.025, 0.975), mu, sigma / sqrt(n))` |

## In the Workbench

**Sampling distribution** (μ, σ, n, a value of x̄) and **Simulation** ▸ *Sample means* (try an exponential population with n = 2, then n = 30).

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 7-2 (sampling distributions and the central limit theorem).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.5.2, Confidence Limits for the Mean: https://www.itl.nist.gov/div898/handbook/eda/section3/eda352.htm
