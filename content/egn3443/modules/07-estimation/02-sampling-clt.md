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

If you have done Module 6, this is a recap; the new part is the proportion (the sampling distribution of p̂, below), which the Exercise asks about.

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

## The sampling distribution of p̂

A **sample proportion** p̂ = x/n (x items of n have the property: defective, failed, late) is the mean of 0s and 1s, one per item, so the CLT applies to it too. p̂ is approximately normal with

- **center** p, the population proportion (p̂ is unbiased), and
- **standard error** √(p(1 − p)/n),

when np ≥ 10 and n(1 − p) ≥ 10. A single item is 0 or 1, with standard deviation √(p(1 − p)); the proportion of n items varies √n times less, exactly as x̄ does.

## Why this matters for estimation

The CLT is what lets us use the normal (and later the t) distribution to say how far x̄ is likely to be from μ, even when we do not know the population's shape. 95 % of a normal distribution lies within 1.96 standard deviations of its mean, so

P(μ − 1.96·σ/√n ≤ X̄ ≤ μ + 1.96·σ/√n) = 0.95

Read the same inequality from the other side: the distance between X̄ and μ is at most 1.96·σ/√n, 95 % of the time. So

X̄ − 1.96·σ/√n ≤ μ ≤ X̄ + 1.96·σ/√n

holds for 95 % of samples. That is the **95 % confidence interval** for μ, x̄ ± 1.96·σ/√n. Item 7.3 builds it, and replaces σ by s when σ is unknown.

## A worked case by hand: a proportion

5 % of the connectors from a supplier are defective (p = 0.05). An inspector checks lots of n = 400 and finds p̂, the proportion defective in the lot.

1. **Shape:** np = 400 × 0.05 = 20 and n(1 − p) = 380 are both at least 10, so p̂ is close to normal.
2. **Standard error:** √(0.05 × 0.95 / 400) = √0.00011875 = **0.0109**.
3. **P(p̂ ≥ 0.07):** z = (0.07 − 0.05) / 0.0109 = 1.84, and P(Z ≥ 1.84) = 1 − 0.9671 = **0.033**. About one lot in thirty shows 7 % defective or more when the true rate is 5 %.
4. **The middle 95 % of p̂:** 0.05 ± 1.96 × 0.0109 = 0.05 ± 0.0214, from **0.0286 to 0.0714**.

## The Example: wire tensile strength

A wire's tensile strength has μ = 520 MPa and σ = 24 MPa. Samples of n = 36 pieces are tested. The figure draws the population (wide) and the sampling distribution of x̄ (narrow) on one axis. The shaded area is P(512 ≤ x̄ ≤ 528).

[[figure: st.m7.sampling-clt.eg 1]]

[[results: st.m7.sampling-clt.eg]]

The standard error is 24 / √36 = 4 MPa, six times smaller than σ. The range 512 to 528 MPa is μ ± 2 standard errors, so it holds the sample mean with probability 0.9545. The middle 95 % is μ ± 1.96 × 4, from 512.16 to 527.84 MPa. With n = 36 ≥ 30, the CLT makes these probabilities good even if single strengths are not normal.

## Seeing the CLT: means of a skewed population

Repair times are often exponential: most repairs are quick, a few take very long. Here the population has a mean of 2 hours (rate λ = 0.5 per hour, so σ = 2 hours too) and is strongly skewed. The Simulation drew 2000 samples of n = 30 and plotted their means:

[[figure: st.m7.sampling-clt.sim 1]]

[[results: st.m7.sampling-clt.sim]]

The means pile up in a bell shape around 2 hours, close to the normal curve the CLT predicts, although single repair times are nothing like normal. Their standard deviation, 0.3586 h, is close to σ/√n = 2 / √30 = 0.3651 h. Open it in the Workbench and set the sample size to 2: the histogram becomes skewed again. It may not look wider, because the horizontal axis rescales to fit the bars; read the axis: at n = 30 the means spread from about 1 to 3.5 hours, at n = 2 from 0 to about 9 hours.

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

- Using σ instead of σ/√n for a question about a sample mean. σ describes single items; σ/√n describes averages. For a proportion the same mistake is using √(p(1 − p)) instead of √(p(1 − p)/n).
- Dividing by n instead of √n.
- Saying the CLT makes the **data** normal. It makes the **mean** of many data approximately normal; the data keep their shape.
- Relying on the CLT with a small sample from a skewed population.

## What to do next

On the **Exercise** tab, a known share of a supplier's cable assemblies fail a test. Find the standard error of the sample proportion p̂, the probability that a sample's p̂ is 0.10 or more, and the middle 95 % of p̂, as in the worked case above.
