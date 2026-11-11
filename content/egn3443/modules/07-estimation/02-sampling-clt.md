---
id: st.m7.sampling-clt
title: Sampling distributions and the central limit theorem
minutes: 45
objectives: [st.m7.o2]
builds-on: st.m7.introduction
example: st.m7.sampling-clt.eg
exercise: st.m7.sampling-clt.ex
reference: [clt-for-inference]
---

# Sampling distributions and the central limit theorem

## The sampling distribution of x̄

Take a sample of n parts and calculate x̄. Take another sample: a different x̄. The values x̄ takes from sample to sample have a distribution of their own, the **sampling distribution** of the sample mean. Module 6 studies it in detail. This item uses three facts from it to build confidence intervals.

1. **Its center is μ.** E[X̄] = μ: the sample mean is unbiased (item 7.1).
2. **Its spread is the standard error** σ/√n. It shrinks as n grows, but only with the square root: four times the data halves the standard error.
3. **Its shape is close to normal** when n is large enough, whatever the population's shape. That is the central limit theorem.

## The central limit theorem (CLT)

If X̄ is the mean of n independent observations from any population with mean μ and standard deviation σ, then for large n

X̄ is approximately normal, with mean μ and standard deviation σ/√n.

So the standardized mean

Z = (X̄ − μ) / (σ/√n)

is approximately standard normal. Two rules of thumb say when "approximately" is good enough:

- If the **population is normal**, X̄ is normal for **any** n.
- Otherwise, **n ≥ 30** is usually enough. A strongly skewed population (repair times, waiting times) needs more; a nearly symmetric one needs fewer.

For a **proportion**, p̂ is the mean of 0s and 1s, so the CLT applies too: p̂ is approximately normal with mean p and standard error √(p(1 − p)/n), when np ≥ 10 and n(1 − p) ≥ 10.

## Why this matters for estimation

The CLT is what lets us use the normal (and later the t) distribution to say how far x̄ is likely to be from μ, even when we do not know the population's shape. 95 % of a normal distribution lies within 1.96 standard deviations of its mean, so

P(μ − 1.96·σ/√n ≤ X̄ ≤ μ + 1.96·σ/√n) = 0.95

Read the same inequality from the other side: the distance between X̄ and μ is at most 1.96·σ/√n, 95 % of the time. So

X̄ − 1.96·σ/√n ≤ μ ≤ X̄ + 1.96·σ/√n

holds for 95 % of samples. That is the **95 % confidence interval** for μ, x̄ ± 1.96·σ/√n. Item 7.3 builds it, and replaces σ by s when σ is unknown.

## A worked case by hand

Resistors from a reel have μ = 100 Ω and σ = 2 Ω. A technician measures n = 25 of them.

1. **Standard error:** σ/√n = 2 / √25 = 2 / 5 = **0.4 Ω**.
2. **Shape:** n = 25 is under 30, so we need the population to be close to normal. Resistances usually are, so X̄ is normal.
3. **P(x̄ > 100.5 Ω):** z = (100.5 − 100) / 0.4 = 1.25, and P(Z > 1.25) = 1 − 0.8944 = **0.1056**. About one sample in ten averages more than 100.5 Ω, although single resistors above 100.5 Ω are far more common (P(Z > 0.25) = 0.40).
4. **The middle 95 % of x̄:** 100 ± 1.96 × 0.4 = 100 ± 0.784, from **99.216 to 100.784 Ω**.

## The Example: wire tensile strength

A wire's tensile strength has μ = 520 MPa and σ = 24 MPa. Samples of n = 36 pieces are tested. The figure draws the population (wide) and the sampling distribution of x̄ (narrow) on one axis. The shaded area is P(512 ≤ x̄ ≤ 528).

[[figure: st.m7.sampling-clt.eg 1]]

[[results: st.m7.sampling-clt.eg]]

The standard error is 24 / √36 = 4 MPa, six times smaller than σ. The range 512 to 528 MPa is μ ± 2 standard errors, so it holds the sample mean with probability 0.9545. The middle 95 % is μ ± 1.96 × 4, from 512.16 to 527.84 MPa. With n = 36 ≥ 30, the CLT makes these probabilities good even if single strengths are not normal.

## Seeing the CLT: means of a skewed population

Repair times are often exponential: most repairs are quick, a few take very long. Here the population has a mean of 2 hours (rate λ = 0.5 per hour, so σ = 2 hours too) and is strongly skewed. The Simulation drew 2000 samples of n = 30 and plotted their means:

[[figure: st.m7.sampling-clt.sim 1]]

[[results: st.m7.sampling-clt.sim]]

The means pile up in a bell shape around 2 hours, close to the normal curve the CLT predicts, although single repair times are nothing like normal. Their standard deviation, 0.3586 h, is close to σ/√n = 2 / √30 = 0.3651 h. Open it in the Workbench and set the sample size to 2: the histogram becomes skewed again.

## In Python and R

```python
import numpy as np
from scipy import stats
mu, sigma, n = 520, 24, 36
se = sigma / np.sqrt(n)                       # 4.0
xbar = stats.norm(mu, se)                     # the sampling distribution of x̄
print(xbar.cdf(528) - xbar.cdf(512))          # 0.9545
print(xbar.ppf(0.025), xbar.ppf(0.975))       # middle 95 %: 512.16 527.84
```

```r
mu <- 520; sigma <- 24; n <- 36
se <- sigma / sqrt(n)                          # 4
pnorm(528, mu, se) - pnorm(512, mu, se)        # 0.9545
qnorm(c(0.025, 0.975), mu, se)                 # 512.16 527.84
```

The one thing to remember: the standard deviation passed to `norm` or `pnorm` is the **standard error** σ/√n, not σ.

[[code: st.m7.sampling-clt.eg]]

## Common mistakes

- Using σ instead of σ/√n for a question about a sample mean. σ describes single items; σ/√n describes averages.
- Dividing by n instead of √n.
- Saying the CLT makes the **data** normal. It makes the **mean** of many data approximately normal; the data keep their shape.
- Relying on the CLT with a small sample from a skewed population.

## What to do next

On the **Exercise** tab, bottles are filled by a machine with a known mean and standard deviation. Find the standard error, a probability for the sample mean, and the middle 95 % of x̄.
