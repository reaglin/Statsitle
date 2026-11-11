---
title: Point estimators
---

# Point estimators

| Parameter | Estimator | Unbiased? | Standard error |
|---|---|---|---|
| mean μ | x̄ = Σx / n | yes: E[X̄] = μ | σ/√n, estimated by s/√n |
| proportion p | p̂ = x / n | yes: E[P̂] = p | √(p(1 − p)/n), estimated by √(p̂(1 − p̂)/n) |
| variance σ² | s² = Σ(x − x̄)² / (n − 1) | yes: E[S²] = σ² | |
| variance σ² | Σ(x − x̄)² / n | no: averages (n − 1)/n × σ² | |
| standard deviation σ | s = √s² | slightly biased low; used anyway | |

## The properties

| Property | Means | Compare by |
|---|---|---|
| **Unbiased** | E[estimator] = parameter: right on average, over all samples | the mean of the sampling distribution |
| **Efficient** | among unbiased estimators, the one with the smallest variance | the variance (standard error) |
| **Consistent** | gets closer to the parameter as n grows | the standard error → 0 |

For a normal population, Var(median) ≈ (π/2)·σ²/n ≈ 1.57 × Var(x̄): the mean is the more efficient estimator of μ.

**Estimator** = the rule (a random variable before sampling). **Estimate** = the number it gives for one sample.

## Python and R

| Quantity | Python (numpy) | R |
|---|---|---|
| x̄ | `x.mean()` | `mean(x)` |
| s² (n − 1) | `x.var(ddof=1)` | `var(x)` |
| s | `x.std(ddof=1)` | `sd(x)` |
| s/√n | `x.std(ddof=1) / np.sqrt(len(x))` | `sd(x) / sqrt(length(x))` |
| p̂ from 0/1 data | `x.mean()` | `mean(x)` |

numpy divides by n unless `ddof=1`; R always divides by n − 1.

## In the Workbench

**Descriptive statistics** shows x̄, s, s², the standard error, and the population (÷ n) forms. **Simulation** ▸ *Sample means* shows the sampling distribution of x̄.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 7-3 (general concepts of point estimation: bias, variance, standard error, mean squared error).
- EGN3443 Module 7 on the course website (reaglin.github.io/EGN3443), topic 1, Introduction to Point Estimation.
