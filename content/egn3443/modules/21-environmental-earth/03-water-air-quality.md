---
id: st.m21.quality
title: Water and air quality monitoring
minutes: 50
objectives: [st.m21.o3]
example: st.m21.quality.eg
exercise: st.m21.quality.ex
reference: [environmental-monitoring]
---

# Water and air quality monitoring

Regulators set standards: nitrate in drinking water, ozone in air, a pollutant in a river. Monitoring checks them with samples, and the samples have a shape of their own. Concentrations are **positive and right-skewed**: mostly low, sometimes high. Many fall **below what the laboratory can measure**. And the question is rarely "what is the mean?" but "**how often is the standard exceeded?**"

## Concentrations are lognormal

A concentration is often the product of many independent factors (dilution, decay, sources), and a product of many factors tends to a **lognormal** distribution (item 5.6): ln(concentration) is roughly normal. In practice:

- fit μ and σ to ln(concentration);
- the **median** concentration is e^μ, the **geometric mean** of the data;
- the arithmetic mean, e^(μ + σ²/2), is larger and is pulled by the high values.

## The Example: nitrate in a river

Twenty-four monthly nitrate concentrations (mg/L as N) (illustrative data, made up for this book). The drinking-water standard used here is 10 mg/L.

[[figure: st.m21.quality.eg 1]]

[[results: st.m21.quality.eg]]

- The fit: μ = **1.245**, σ = **0.558**. The median concentration is e^1.245 = **3.47 mg/L**; the mean 4.06 mg/L.
- **P(X > 10) = 0.029**: about one sample in 34 would exceed the standard, though none of these 24 did.
- The 95th percentile is **8.70 mg/L**. Many regulations are written as a percentile ("the 95th percentile must not exceed …"), because a single high sample can always happen.

[[see-example: 1]]

## Compliance

A monitoring program answers "is the water compliant?" with a test or an interval, not a single sample:

- **Exceedance frequency**: the estimated share of time over the standard, P(X > standard), from the fitted model (here 2.9 %), against an allowed frequency.
- **A confidence interval for a percentile**: compliant if even its upper limit is below the standard.
- **Burden of proof**: H₀ "the water is compliant" protects the polluter; H₀ "it is not" protects the public. Which one a regulation uses decides how much data is needed (item 8.2).

## Values below the detection limit

A laboratory reports "< 0.5 mg/L" when a concentration is too low to measure: the value is **censored**, like the survival times of item 19.4.

- Replacing it with 0, the detection limit, or half of it (common, but crude) biases the mean and the standard deviation, badly when there are many.
- Better: fit the lognormal by **maximum likelihood with censoring**, which uses "below 0.5" as exactly that, or use **regression on order statistics**. Both are in Part II.

Real data: the US Water Quality Portal (waterqualitydata.us) and the EPA's air-quality data.

## In Python and R

```python
import numpy as np
from scipy import stats
ln_c = np.log(conc); mu, sigma = ln_c.mean(), ln_c.std(ddof=0)   # maximum likelihood, as the Workbench
p_exceed = stats.norm.sf((np.log(10) - mu) / sigma)      # P(X > 10)
p95 = np.exp(mu + stats.norm.ppf(0.95) * sigma)
```

```r
mu <- mean(log(conc)); sigma <- sqrt(mean((log(conc) - mu)^2))   # maximum likelihood, as the Workbench
plnorm(10, mu, sigma, lower.tail = FALSE)                # P(X > 10)
qlnorm(0.95, mu, sigma)
# censored data: NADA2 package (cenmle, cenros)
```

[[code: st.m21.quality.eg]]

## Common mistakes

- **The arithmetic mean** of skewed concentrations as "typical": the median (geometric mean) is.
- **Compliance from one sample**, high or low.
- **Substituting zero** for every value below the detection limit.
- **A normal model** for concentrations: it predicts negative values and too few high ones.

## What to do next

On the **Exercise** tab, arsenic in twenty wells: fit a lognormal and estimate the share of the county's wells over 10 µg/L.

[[example: Lognormal concentrations and a standard]]

A lognormal model of nitrate, μ = **1.15**, σ = **0.60** (median e^1.15 = 3.16 mg/L). P(X > 10) = **0.027** and the 95th percentile is **8.47 mg/L**.

1. **Raise μ to 1.5** (the median rises to 4.5 mg/L, as if a new source upstream). The exceedance probability rises several-fold.
2. **Back to 1.15, and raise σ to 0.9**: the same median, more variable. Exceedances rise too: variability matters as much as the typical level.
3. **Set x to 5** and read P(X > 5): how often half the standard is passed.

[[try: st.m21.nitrate.try LogMu=0..2.5 LogSigma=0.2..1.2 X=1..20 Probability | MoreThan Quantile Median Mean figure=1]]
