---
title: Type I and Type II errors and power
---

# Type I and Type II errors and power

| | H₀ true | H₀ false |
|---|---|---|
| Reject H₀ | Type I error, probability **α** | correct, probability **1 − β (power)** |
| Do not reject H₀ | correct, probability 1 − α | Type II error, probability **β** |

- α is chosen. β depends on the true value μ₁, on n, σ and α.
- Lower α → higher β at the same n. Larger n, larger shift δ = μ₁ − μ₀, or smaller σ → higher power.

## Power of a z-test on a mean

shift = δ√n / σ

| H₁ | Power at μ₁ |
|---|---|
| μ > μ₀ | 1 − Φ(z(α) − shift) |
| μ < μ₀ | Φ(−z(α) − shift) |
| μ ≠ μ₀ | 1 − [Φ(z(α/2) − shift) − Φ(−z(α/2) − shift)] |

Sample size for power 1 − β: n = [(z(α/2) + z(β)) σ / δ]², rounded up (z(α) for a one-sided test).

| | z |
|---|---|
| z(0.025) (two-sided α = 0.05) | 1.960 |
| z(0.05) (one-sided α = 0.05) | 1.645 |
| z(β) for power 0.80 | 0.8416 |
| z(β) for power 0.90 | 1.2816 |

For a t-test, Montgomery and Runger use the same approximation with s in place of σ; exact t-based power (R `power.t.test`) is slightly lower.

## Python and R

```python
from scipy import stats
power = 1 - (stats.norm.cdf(z_half - shift) - stats.norm.cdf(-z_half - shift))   # two-sided
```

```r
power <- 1 - (pnorm(z_half - shift) - pnorm(-z_half - shift))
power.t.test(n = 10, delta = 4, sd = 5, sig.level = 0.05)   # exact, t-based
```

## In the Workbench

**One-sample test**, z or t on a mean: fill **True mean worth detecting (μ₁)** and **Target power** to get **Power at μ₁** and **n for the target power**. Leave μ₁ blank to skip them.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 9-1.3 and 9-2.2 (Type II error and choice of sample size).
- NIST/SEMATECH *e-Handbook*, Section 7.2.2.2, Sample sizes required: https://www.itl.nist.gov/div898/handbook/prc/section2/prc222.htm
