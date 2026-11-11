---
title: The sampling distribution of the mean and the Central Limit Theorem
---

# The sampling distribution of x̄

**Sampling distribution:** the distribution of a statistic over all possible samples of the same size n from the same population.

| | Single values x | Sample means x̄ |
|---|---|---|
| Mean | μ | μ_x̄ = μ |
| Standard deviation | σ | σ_x̄ = σ/√n (the **standard error**) |
| Shape | the population's | normal if the population is normal; close to normal for large n otherwise (CLT) |

When σ is unknown, the standard error is estimated by s/√n (Module 7).

## The Central Limit Theorem

For random, independent samples from any population with mean μ and finite standard deviation σ, the distribution of x̄ approaches Normal(μ, σ/√n) as n grows.

| Population | x̄ approximately normal when |
|---|---|
| Normal | any n |
| Roughly symmetric | n ≳ 15 |
| Skewed | n ≳ 30 |
| Very skewed or with extreme outliers | n well above 30 |

Sampling without replacement: keep n below about 10 % of the population.

## Probabilities about x̄

z = (x̄ − μ) / (σ/√n)  P(x̄ ≤ a) = Φ(z)  P(x̄ ≥ a) = 1 − Φ(z)  P(a ≤ x̄ ≤ b) = Φ(z_b) − Φ(z_a)

Middle 95 % of sample means: μ ± 1.96 σ/√n.

| \|z\| | One-side probability | Reading |
|---|---|---|
| 1 | 0.159 | ordinary |
| 2 | 0.023 | unusual |
| 3 | 0.0013 | very unlikely by chance |

## Sample size

- 4 × n halves the standard error; k² × n divides it by k.
- For a target standard error: n = (σ / SE)², rounded **up**.

## The sample proportion

p̂ has mean p and standard error √(p(1 − p)/n), and is close to normal when np ≥ 10 and n(1 − p) ≥ 10. The Workbench's **Sampling distribution** does both (Statistic: *Sample proportion p̂*); Module 7 uses it for intervals.

## Python and R

| Task | Python (`import numpy as np; from scipy import stats`) | R |
|---|---|---|
| Standard error | `sigma / np.sqrt(n)` | `sigma / sqrt(n)` |
| P(x̄ ≤ a) | `stats.norm(mu, se).cdf(a)` | `pnorm(a, mu, se)` |
| P(x̄ ≥ a) | `stats.norm(mu, se).sf(a)` | `1 - pnorm(a, mu, se)` |
| Middle 95 % | `stats.norm(mu, se).ppf([0.025, 0.975])` | `qnorm(c(0.025, 0.975), mu, se)` |
| n for a target SE | `math.ceil((sigma / target) ** 2)` | `ceiling((sigma / target)^2)` |
| Simulate means | `rng.exponential(4, (5000, 30)).mean(axis=1)` | `replicate(5000, mean(rexp(30, 0.25)))` |

numpy's `exponential` takes the scale 1/λ; R's `rexp` takes the rate λ.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 7-2 (sampling distributions and the Central Limit Theorem).
- EGN3443 Module 6: https://reaglin.github.io/EGN3443/week-7module-6-sampling-distrbutions/
