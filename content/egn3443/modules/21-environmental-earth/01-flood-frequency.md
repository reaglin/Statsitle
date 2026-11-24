---
id: st.m21.floods
title: Frequency analysis of floods and rainfall
minutes: 55
objectives: [st.m21.o1]
example: st.m21.floods.eg
exercise: st.m21.floods.ex
reference: [flood-frequency-analysis]
---

# Frequency analysis of floods and rainfall

A bridge, a culvert, a levee or a storm sewer is designed for a flood it should survive. Not the largest flood possible, which has no limit, but one with a stated **chance of being exceeded** in any year. **Frequency analysis** fits a probability distribution to the largest flow (or rainfall) of each year and reads the design value from it. It is Module 5's distributions, used at their extreme end.

## Annual maxima, exceedance and the return period

- The data: one value per year, the **annual maximum** (peak flow, or the largest one-day rainfall). Using one value a year keeps the values independent.
- The **exceedance probability** p of a flow Q: the chance that a year's maximum exceeds Q, p = P(X > Q).
- The **return period** (recurrence interval) T = 1/p. The **100-year flood** has p = 0.01: it is the 0.99 quantile of the annual-maximum distribution.

The 100-year flood is **not** a flood that comes once every hundred years. It has a 1 % chance every year. Over a 30-year mortgage the chance of at least one is 1 − 0.99³⁰ = **26 %**.

## The Example: 25 years at a river gauge

The annual peak flows (m³/s) at a gauge for 25 years (illustrative data, made up for this book):

[[data: st.m21.floods.eg]]

Floods are skewed: most years are modest and a few are very large, so a symmetric normal model fits badly. A **lognormal** model (ln of the peak is normal, item 5.6) is a common choice.

[[figure: st.m21.floods.eg 1]]

[[results: st.m21.floods.eg]]

- The fit: μ = **5.991**, σ = **0.513** (the mean and standard deviation of ln Q).
- The **100-year flood**, the 0.99 quantile: e^(5.991 + 2.326 × 0.513) = **1,319 m³/s**.
- A flow of 800 m³/s is exceeded with probability **0.088** a year: its return period is 1/0.088 ≈ **11 years**.
- The Q–Q correlation is 0.961: the lognormal fits these 25 values reasonably, though the plot shows the largest years a little off the line.

[[see-example: 1]]

## Gumbel and log-Pearson Type III

Hydrology uses several distributions, through **frequency factors**: Q_T = x̄ + K_T · s, with K_T depending on the distribution and T.

- **Gumbel** (Extreme Value Type I), the classic for annual maxima: K_T = −(√6/π)[0.5772 + ln(ln(T/(T − 1)))]. For T = 100, K = 3.137. With the gauge's mean **452.0** and standard deviation **218.3 m³/s**: Q₁₀₀ = 452.0 + 3.137 × 218.3 = **1,137 m³/s**.
- **Log-Pearson Type III**: the logs of the flows follow a Pearson III distribution, whose K_T depends on the **skew** of the logs as well. It is the standard for flood frequency in the United States (Bulletin 17C, USGS, 2019), and the method of the hydrology text and SMADA.

The two models give 1,137 and 1,319 m³/s for the same 25 years. **The choice of distribution matters most where it is least checkable**: at return periods longer than the record. Design rules fix the method so that answers are consistent, and engineers report the uncertainty too.

## Design storms

Rainfall works the same way. The annual maximum rainfall of each **duration** (15 minutes, 1 hour, 24 hours) is fitted, and the T-year depths for every duration make the **intensity–duration–frequency (IDF)** curves a storm sewer is designed from. In the US the published IDF values are NOAA Atlas 14. Real annual peak flows are in the USGS National Water Information System (waterdata.usgs.gov).

## In Python and R

The Workbench fits the lognormal by **maximum likelihood**: σ is the standard deviation of the logs **dividing by n**, not n − 1 (with 30 years the two differ by under 2 %). The code below does the same, so its numbers match the Workbench's.

```python
import numpy as np
from scipy import stats
ln_q = np.log(peaks); mu, sigma = ln_q.mean(), ln_q.std(ddof=0)   # maximum likelihood: divide by n
q100 = np.exp(mu + stats.norm.ppf(0.99) * sigma)          # lognormal 100-year flood
K = -np.sqrt(6)/np.pi * (0.5772 + np.log(np.log(100/99)))  # Gumbel frequency factor
q100_gumbel = peaks.mean() + K * peaks.std(ddof=1)
```

```r
mu <- mean(log(peaks)); sigma <- sqrt(mean((log(peaks) - mu)^2))   # maximum likelihood: divide by n
qlnorm(0.99, mu, sigma)                                     # lognormal 100-year flood
K <- -sqrt(6)/pi * (0.5772 + log(log(100/99)))
mean(peaks) + K * sd(peaks)                                 # Gumbel
```

[[code: st.m21.floods.eg]]

## Common mistakes

- **"Once in a hundred years"**: it is a 1 % chance every year.
- **Every flood of every year** rather than one maximum per year: the values are then not independent.
- **A normal model** for skewed peaks.
- **Extrapolating far beyond the record** without saying how uncertain it is: 25 years say little about the 500-year flood.

## What to do next

On the **Exercise** tab, thirty years at a small river: fit a lognormal, give the 50-year flood, and the exceedance probability of 300 m³/s.

[[example: Design floods from a lognormal model]]

A lognormal model of the gauge's annual peaks, μ = **5.99**, σ = **0.51** (the fit, rounded). It starts at the 0.99 quantile: the **100-year flood, about 1,308 m³/s**.

1. **Set the probability to 0.98**: the 50-year flood, about **1,138 m³/s**. **0.90**: the 10-year flood, about **768 m³/s**.
2. **Back to 0.99, and move σ to 0.70.** A more variable river: the 100-year flood grows far more than the median does. The spread, not the average, decides the extremes.
3. **Move x to 1,500** and read P(X > x): the annual exceedance probability of a flow above the 100-year flood.

[[try: st.m21.design-flood.try LogMu=5..7 LogSigma=0.2..1 X=100..3000 Probability | Quantile MoreThan Median figure=1]]
