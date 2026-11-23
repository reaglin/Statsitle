---
id: st.m21.trends
title: Extreme values and climate trends
minutes: 50
objectives: [st.m21.o2]
example: st.m21.trends.eg
exercise: st.m21.trends.ex
reference: [trend-tests]
---

# Extreme values and climate trends

Is a station getting warmer? Are heavy rains getting heavier, lakes freezing later, sea level rising? A **trend** is a steady change over time, hidden in year-to-year noise. Two tools find it: a **regression on time** (Module 11), which also says how fast; and the **Mann–Kendall test**, which asks only whether values tend to rise and needs no normal distribution.

## The Example: forty years at a weather station

The annual mean temperature at one station, as the difference from its 1984–2023 average (°C) (illustrative data, made up for this book):

[[figure: st.m21.trends.eg 1]]

[[results: st.m21.trends.eg]]

- The trend is **0.0224 °C a year**, or **0.224 °C a decade**, with a 95 % interval of 0.19 to 0.26 °C a decade (item 11.5).
- t = 12.1, p < 0.0001: far too steady a rise to be year-to-year noise.
- R² = 0.79: the trend explains most of the variation; the rest is weather.
- The Durbin–Watson statistic, **1.98**, is close to 2: the residuals do not run in long warm and cool spells, so the p-value can be trusted (item 13.2). Climate records often do have such spells (autocorrelation), and then an ordinary regression's p-value is too small; Part II's time-series methods correct it.

[[see-example: 1]]

## The Mann–Kendall test

The Mann–Kendall test counts, over every pair of years, whether the later value is higher (+1) or lower (−1), and adds them up:

S = Σ over i < j of sign(xⱼ − xᵢ).

With no trend, S is near 0; a rising series gives a large positive S. For n years without ties, Var(S) = n(n − 1)(2n + 5)/18, and Z = (S − 1)/√Var(S) for S > 0 is compared with the normal distribution.

Eight years of anomalies (°C): 0.12, −0.05, 0.20, 0.31, 0.18, 0.42, 0.37, 0.55.

- Of the 28 pairs, 24 rise and 4 fall: **S = 20**.
- Var(S) = 8 × 7 × 21 / 18 = 65.3; Z = 19/√65.3 = **2.35**; two-sided p = **0.019**: a significant upward trend.

The size of a Mann–Kendall trend is given by **Sen's slope**: the median of the slopes (xⱼ − xᵢ)/(j − i) over all pairs. It is robust to an odd year in the way the least-squares slope is not.

## Extremes in a changing climate

Frequency analysis (item 21.1) assumes the climate does not change: the 100-year flood of 1950 is the 100-year flood of 2050. If heavy rainfall is trending, that assumption (**stationarity**) fails, and design values must account for it. Hydrologists now test annual maxima for trends with these same tools before they fit a distribution.

Real data: NOAA's National Centers for Environmental Information publish station records and global temperature series.

## In Python and R

```python
from scipy import stats
fit = stats.linregress(year, anomaly)          # slope per year, p-value
tau, p = stats.kendalltau(year, anomaly)       # Mann–Kendall is Kendall's tau against time
sen = stats.theilslopes(anomaly, year)          # Sen's slope with an interval
```

```r
fit <- lm(anomaly ~ year); summary(fit)
cor.test(year, anomaly, method = "kendall")    # the Mann–Kendall test
# Sen's slope: median of all pairwise slopes, or trend::sens.slope(anomaly)
```

[[code: st.m21.trends.eg]]

## Common mistakes

- **A trend from a short record**: ten years of climate are mostly weather.
- **Ignoring autocorrelation**, which makes a trend look more certain than it is.
- **Extrapolating the line** decades past the data.
- **Cherry-picking the start year** (a cold or a hot year) to make the trend steeper or flatter.

## What to do next

On the **Exercise** tab, the ice-out day of a northern lake from 1990 to 2023: is it getting earlier, and by how much?

[[example: A trend: short records and one hot year]]

The station's 40 years. It starts at a trend of **0.0224 °C a year**, p < 0.0001, R² = **0.79**.

1. **One extreme year.** Change the last year's anomaly to 1.5 °C (a record heat year). The slope rises only a little: one year cannot make a 40-year trend, but it can dominate a short one.
2. **Put it back**, and keep only the first 10 years: delete the values from 1994 on in both columns. The trend's p-value is much larger and its interval is wide: ten years of weather hide a slow trend.

[[try: st.m21.warming.try @Station/Year "@Station/Anomaly (°C)" | Equation Slope SlopeP SlopeLower SlopeUpper RSquared DurbinWatson figure=1]]
