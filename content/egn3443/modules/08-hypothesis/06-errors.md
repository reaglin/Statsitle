---
id: st.m8.errors
title: Type I and Type II errors
minutes: 50
objectives: [st.m8.o5]
builds-on: st.m8.applying
example: st.m8.errors.eg
exercise: st.m8.errors.ex
reference: [type-i-ii-errors-power]
---

# Type I and Type II errors

## Two ways to be wrong

A test decides from a sample, so it can be wrong in two ways:

| | H₀ is true | H₀ is false |
|---|---|---|
| **Reject H₀** | **Type I error** (false positive). Probability α | correct. Probability 1 − β, the **power** |
| **Do not reject H₀** | correct. Probability 1 − α | **Type II error** (false negative). Probability β |

- **α = P(Type I error)** = P(reject H₀ when H₀ is true). You choose it.
- **β = P(Type II error)** = P(do not reject H₀ when H₀ is false). It depends on how false H₀ is.
- **Power = 1 − β**: the probability the test detects a real difference.

## What each error costs

Which error is worse depends on the engineering. Name the consequence of each before choosing α.

| Test | Type I error means | Type II error means |
|---|---|---|
| H₀: the weld batch meets strength | scrapping good welds | shipping weak welds into a structure |
| H₀: the machine is on target | stopping a good line to adjust it | running an off-target line and making scrap |
| H₀: the new alloy is no better | switching to an alloy that is not better | missing a real improvement |

When a Type II error is dangerous (weak welds), the test needs high power: a larger n, or a larger α.

## Power for a mean

Power is calculated for a particular true mean μ₁ worth detecting: the shift **δ = μ₁ − μ₀**. In standard errors the shift is

shift = δ √n / σ

and for a z-test with critical value z_α (one-sided) or z_{α/2} (two-sided), with Φ the standard normal area to the left:

| H₁ | Power at μ₁ |
|---|---|
| μ > μ₀ | 1 − Φ(z_α − shift) |
| μ < μ₀ | Φ(−z_α − shift) (shift is negative) |
| μ ≠ μ₀ | 1 − [Φ(z_{α/2} − shift) − Φ(−z_{α/2} − shift)] |

**The sample size for a target power** 1 − β (with z_β the z with area β above it: 1.2816 for power 0.90, 0.8416 for 0.80):

n = [(z_{α/2} + z_β) σ / δ]², rounded up (z_α in place of z_{α/2} for a one-sided test).

Montgomery and Runger (§9-2.2) use this normal approximation for t-tests too, with s in place of σ; so does the Workbench.

## The trade-off

- **Lower α** (fewer false alarms) moves the critical value out, so **β rises** and power falls, at the same n.
- **Larger n** shrinks the standard error, so the shift in standard errors grows: **power rises** at the same α.
- **A larger true shift δ** is easier to detect: power rises.
- **A larger σ** hides the shift: power falls.

The only way to lower both α and β is to take more data (or measure more precisely).

## A worked case by hand

Heat-treated pins should have mean hardness 100 HRB, σ = 10. A test of H₀: μ = 100 against H₁: μ > 100 uses n = 25 at α = 0.05 (z_α = 1.645). How likely is it to detect a true mean of 104?

1. **Shift:** δ√n/σ = 4 × 5/10 = **2.0** standard errors.
2. **Power:** 1 − Φ(1.645 − 2.0) = 1 − Φ(−0.355) = **0.639**.
3. **β** = 1 − 0.639 = **0.361**. More than a third of the time, a real 4-point rise would be missed.
4. **n for power 0.90:** [(1.645 + 1.2816) × 10/4]² = 7.316² = 53.5, so **54** pins.

## The Example: concrete strength

A mix specified at 4000 psi (σ = 300 psi) is tested with n = 9 cylinders, H₁: μ < 4000, α = 0.05. If the mix were really 3750 psi:

[[results: st.m8.errors.eg]]

The shift is −250 × 3/300 = −2.5 standard errors, so the power is Φ(−1.645 + 2.5) = Φ(0.855) = **0.804**: the test catches a 3750 psi mix four times in five, and misses it (β = 0.196) one time in five. Thirteen cylinders would raise the power to 0.913, past the target of 0.90.

The figure shows the test on this sample: the z curve under H₀, the rejection region z < −1.645, and the observed z. If the true mean were 3750 psi, z would instead centre on −2.5, and the power is the share of that shifted curve lying in the shaded region.

[[figure: st.m8.errors.eg 1]]

This sample's mean, 3870 psi, gives z = −1.30 and p = 0.0968: not rejected. Is the mix good? Not shown: with a power of only 0.80 against 3750, "do not reject" is weak reassurance. That is why a report gives the power for the shift that matters.

## In Python and R

```python
import numpy as np
from scipy import stats
mu0, mu1, sigma, n, alpha = 4000, 3750, 300, 9, 0.05
shift = (mu1 - mu0) * np.sqrt(n) / sigma                 # -2.5
power = stats.norm.cdf(stats.norm.ppf(alpha) - shift)    # lower-tailed: 0.804
z_a, z_b = stats.norm.ppf(1 - alpha), stats.norm.ppf(0.90)
n_needed = int(np.ceil(((z_a + z_b) * sigma / abs(mu1 - mu0)) ** 2))   # 13
print(power, 1 - power, n_needed)
```

```r
mu0 <- 4000; mu1 <- 3750; sigma <- 300; n <- 9; alpha <- 0.05
shift <- (mu1 - mu0) * sqrt(n) / sigma
power <- pnorm(qnorm(alpha) - shift)                      # 0.804
ceiling(((qnorm(1 - alpha) + qnorm(0.90)) * sigma / abs(mu1 - mu0))^2)   # 13
```

R's `power.t.test` and Python's `statsmodels` `TTestPower` compute power with the t distribution; they give a slightly lower power and a slightly larger n than the normal approximation.

[[code: st.m8.errors.eg]]

## Try it in the Workbench

Press **Open in the Workbench** on the Example tab. Change **Significance level (α)** to 0.01: the power at 3750 falls to 0.569. Then set **Sample size (n)** to 16: it climbs back to 0.843. Lower α costs power; more cylinders buy it back.

## Common mistakes

- Thinking α is the probability that H₀ is true after it is rejected. α is the probability of rejecting **if** H₀ is true.
- Reporting β as the power. Power = 1 − β.
- Using z_α for a two-sided test (or z_{α/2} for a one-sided one).
- Reading "do not reject" from a low-power test as evidence that nothing changed.

## What to do next

On the **Exercise** tab, relief valves should open at 150 kPa and a lot opening at 154 kPa is a safety concern. Find the power of the 10-valve test against 154 kPa, and the number of valves that gives a power of 0.90.
