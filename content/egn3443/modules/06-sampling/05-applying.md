---
id: st.m6.applying
title: Interpreting and applying sampling distributions
minutes: 45
objectives: [st.m6.o2, st.m6.o5]
builds-on: st.m6.sample-size
example: st.m6.applying.eg
exercise: st.m6.applying.ex
reference: [sampling-distribution-of-the-mean]
---

# Interpreting and applying sampling distributions

## Chance, or a real difference?

A process is meant to run at a target μ. A sample comes back with x̄ a little off target. Two explanations are possible:

1. **Sampling variability:** the process is still on target, and this sample just happened to come out a little high or low.
2. **A real difference:** the process has moved.

The sampling distribution of x̄, **assuming the process is on target**, settles which explanation is believable. If a sample mean like the one observed is common under that assumption, chance explains it. If it is rare, chance is a poor explanation, and the process has probably moved.

## How to read it

1. Assume the process is at its target: mean μ, standard deviation σ.
2. Work out the standard error, σ/√n, and check that x̄ is close to normal (item 6.3).
3. Find how many standard errors the observed x̄ is from μ: z = (x̄ − μ)/(σ/√n).
4. Find the probability of a sample mean at least that far out on that side: P(x̄ ≥ observed) = 1 − Φ(z) for a high mean, Φ(z) for a low one.

A rough guide:

| \|z\| | How often chance alone gives it (one side) | What it suggests |
|---|---|---|
| under 1 | more than 16 % | ordinary sampling variability |
| about 2 | about 2.3 % | unusual: look into it |
| 3 or more | 0.13 % or less | very unlikely by chance: the process has probably moved |

This is the reasoning of a **hypothesis test** (Module 8) and of a **control chart** (Module 1's quality control): the limits of an x̄ chart are μ ± 3σ/√n.

## A worked case by hand

A pump is rated to deliver μ = 120 L/min with σ = 6 L/min. A commissioning engineer takes n = 9 flow readings and gets x̄ = 116.2 L/min. Is the pump under-delivering, or is this chance?

1. **SE:** 6/√9 = **2 L/min**. Flow readings of a pump are close to normal, so x̄ is normal even with n = 9.
2. **z:** (116.2 − 120)/2 = **−1.90**.
3. **Probability of a mean this low or lower:** Φ(−1.90) = **0.0287**.
4. **Interpretation:** if the pump really delivers 120 L/min, only about 3 % of sets of 9 readings would average 116.2 or less. That is unusual enough to check the pump before accepting it, but not so rare that chance is ruled out.

Notice the role of n: a **single** reading of 116.2 would have z = −3.8/6 = −0.63, which is ordinary. The average of 9 readings is held much closer to μ, so the same 3.8 L/min gap means more.

## The Example: bolt lengths

A bolt machine set correctly cuts μ = 50.00 mm with σ = 0.20 mm. The inspector measures n = 25 bolts every hour, and this hour x̄ = 50.09 mm.

[[figure: st.m6.applying.eg 1]]

[[results: st.m6.applying.eg]]

The standard error is 0.20/√25 = 0.04 mm, so 50.09 is z = 0.09/0.04 = 2.25 standard errors above target. The probability of a mean of 50.09 or more is 1 − 0.9878 = **0.0122**, about 1 hour in 80 for a correctly set machine. That is rare enough to check the setting. The middle 95 % of hourly means, for a correct setting, is 49.92 to 50.08 mm, and 50.09 lies just outside it.

(The figure shades the area **at or below** the value; the probability asked here is the small area to its right. The **P(statistic ≥ value)** row gives it.)

## In Python and R

```python
import numpy as np
from scipy import stats
mu, sigma, n, xbar = 50.00, 0.20, 25, 50.09
se = sigma / np.sqrt(n)            # 0.04
z = (xbar - mu) / se               # 2.25
print(z, stats.norm.sf(z))         # P(x̄ ≥ 50.09) = 0.0122
```

```r
mu <- 50.00; sigma <- 0.20; n <- 25; xbar <- 50.09
se <- sigma / sqrt(n)
z <- (xbar - mu) / se
z; 1 - pnorm(z)                    # 2.25 and 0.0122
```

`stats.norm.sf(z)` is 1 − Φ(z), the area to the right ("survival function").

[[code: st.m6.applying.eg]]

## Common mistakes

- Judging a sample mean against σ instead of the standard error. A mean is held much closer to μ than a single value is.
- Reading a small probability as proof. 0.0122 says the result is rare **if** the machine is on target, not that the machine is certainly off. Module 8 makes the decision rule precise.
- Calling every difference from target "significant". With large n, a tiny shift becomes detectable; whether it matters is an engineering question (Module 8, practical significance).

## What to do next

On the **Exercise** tab, a technician weighs 16 bottles from a filler. Find how many standard errors their mean is above target and how likely a mean that high is by chance.
