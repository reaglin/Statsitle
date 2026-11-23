---
id: st.m21.quakes
title: Earthquakes and natural hazards
minutes: 45
objectives: [st.m21.o5]
example: st.m21.quakes.eg
exercise: st.m21.quakes.ex
reference: [earthquake-statistics]
---

# Earthquakes and natural hazards

Nobody can predict the day of an earthquake. What seismologists and engineers can say is **how often** earthquakes of each size happen in a region, and from that the **chance of at least one** strong enough to matter during a building's life. Two pieces of statistics do it: a straight line on a log scale, and the Poisson distribution (item 4.5).

## The Gutenberg–Richter law

Small earthquakes are common and large ones rare, in a regular way. If N is the yearly number of earthquakes of magnitude **at least M**,

**log₁₀ N = a − b M.**

- **a** sets the region's overall activity.
- **b**, the **b-value**, is close to 1 in most regions: each step of one magnitude makes earthquakes about **ten times rarer**.

The law is a regression of log₁₀ N on M (Module 11), and the fitted line, carried past the largest magnitudes in the catalogue with care, estimates how often a large earthquake happens.

## The Example: a regional catalogue

log₁₀ of the yearly number of earthquakes of at least each magnitude, for one region (illustrative data, made up for this book):

[[figure: st.m21.quakes.eg 1]]

[[results: st.m21.quakes.eg]]

- log₁₀ N = 4.93 − **0.989** M: the b-value is 0.99, typical.
- For M ≥ 7: log₁₀ N = −1.995, N = 10^(−1.995) = **0.0101 a year**: about one such earthquake every **99 years** on average. The catalogue reaches only M 6, so this is an extrapolation, and the interval around it should be read with that in mind.

[[see-example: 1]]

## The chance of at least one: Poisson

Large earthquakes in a region occur roughly independently at a steady rate, so their number in t years is **Poisson** with mean λt (item 4.5). The chance of at least one is

P(at least one) = 1 − e^(−λt).

For M ≥ 7 at 0.0101 a year, over a 50-year design life: λt = 0.505 and P = 1 − e^(−0.505) = **0.40**. Building codes are written this way: the design ground motion has a **10 % chance of being exceeded in 50 years**, which is a return period of about 475 years (1 − e^(−50/475) = 0.10).

The same Poisson reasoning is used for other hazards: hurricanes making landfall, landslides, volcanic eruptions.

Real data: the USGS earthquake catalogue (earthquake.usgs.gov) lists every recorded earthquake with its magnitude, time and place.

## In Python and R

```python
import numpy as np
from scipy import stats
fit = stats.linregress(mag, np.log10(n_per_year))   # slope = -b
rate7 = 10 ** (fit.intercept + fit.slope * 7)
p_one = 1 - np.exp(-rate7 * 50)                     # at least one in 50 years
```

```r
fit <- lm(log10(n_per_year) ~ mag)
rate7 <- 10^predict(fit, data.frame(mag = 7))
1 - exp(-rate7 * 50)                                # or ppois(0, rate7 * 50, lower.tail = FALSE)
```

[[code: st.m21.quakes.eg]]

## Common mistakes

- **N as the number of exactly magnitude M** rather than at least M (the law is cumulative).
- **"Overdue"**: in a Poisson process, the years since the last earthquake do not make the next one more likely.
- **Extrapolating the line** far past the catalogue's largest magnitudes without saying so.
- **Adding yearly probabilities**: 0.01 a year is not 0.50 in 50 years, it is 1 − e^(−0.5) = 0.39.

## What to do next

On the **Exercise** tab, another region's catalogue: fit the Gutenberg–Richter line and find the yearly rate of M ≥ 6.5 earthquakes.

[[example: At least one large earthquake: a Poisson model]]

A Poisson model of large earthquakes in 50 years at 0.02 a year: the mean is **λ = 1.0**. With k = 1, P(at least one) is **AtLeast**: 1 − e^(−1) = **0.632**; P(none) = **0.368**.

1. **Move the mean to 0.1** (a 500-year return period, over 50 years): the chance of at least one falls to about 0.10, the building-code level.
2. **Move it to 3** (a very active region): at least one is almost certain, and P(at least 3), with k = 3, is about **0.58**.

[[try: st.m21.occurrence.try Rate=0.05..5 K | Exactly AtLeast AtMost Mean figure=1]]
