---
id: st.m21.spatial
title: Spatial statistics
minutes: 50
objectives: [st.m21.o4]
example: st.m21.spatial.eg
quiz: quizzes/m21-spatial.json
reference: [spatial-statistics]
---

# Spatial statistics

Groundwater levels are measured at wells, rainfall at gauges, soil contamination at boreholes. Between them, nothing is measured, yet the map must be filled in. **Spatial statistics** uses the fact that **nearby places are alike**: the closer two wells, the more similar their water levels. This page shows the two ideas behind every contour map of environmental data: **interpolation** and the **variogram**. Kriging, which combines them, is Part II.

## Interpolation: inverse distance weighting

The simplest estimate at an unmeasured point x₀ is a weighted average of the measured values, with near points weighted more:

ẑ(x₀) = Σ wᵢ zᵢ / Σ wᵢ, with wᵢ = 1/dᵢ² (dᵢ the distance to measured point i).

Three wells around a site, with water levels (m above datum) and distances:

| Well | Level zᵢ (m) | Distance dᵢ (km) | wᵢ = 1/dᵢ² |
|---|---|---|---|
| A | 24.0 | 1.0 | 1.000 |
| B | 21.0 | 2.0 | 0.250 |
| C | 27.0 | 3.0 | 0.111 |

ẑ = (24.0 × 1.000 + 21.0 × 0.250 + 27.0 × 0.111) / (1.000 + 0.250 + 0.111) = 32.25 / 1.361 = **23.7 m**: close to well A, the nearest.

Inverse distance weighting is quick, but it has no model of *how* alike places are, and it gives no error for its estimate.

## The variogram

The **semivariance** at a separation h measures how different values are at that distance:

γ(h) = ½ · average of (z(x) − z(x + h))² over all pairs about h apart.

Plotted against h, it is the **variogram**:

- **Nugget**: γ near h = 0. Measurement error and variation on scales smaller than the spacing of the wells.
- **Slope**: how fast values become less alike with distance.
- **Sill**: the level γ reaches when places are so far apart they are no more alike than any two random places.
- **Range**: the distance at which it reaches the sill: beyond it, a measurement says nothing about another.

## The Example: groundwater levels

The empirical semivariance of groundwater levels (m²) between pairs of wells, by separation distance (km) (illustrative data, made up for this book). Within this range the variogram rises nearly as a straight line, so a regression (Module 11) estimates its nugget (the intercept) and slope.

[[figure: st.m21.spatial.eg 1]]

[[results: st.m21.spatial.eg]]

- **Nugget ≈ 1.59 m²**: even wells next to each other differ by about √(2 × 1.59) = 1.8 m in level, from measurement and very local effects.
- **Slope ≈ 1.35 m² per km**: wells become less alike steadily with distance over these 4 km. The sill and range lie further out.
- **Kriging** uses a variogram model fitted like this to choose the weights of the interpolation, and gives each estimate a **kriging variance**: a map of the uncertainty as well as of the values.

[[see-example: 1]]

## In Python and R

```python
import numpy as np
w = 1 / d**2; z_hat = (w * z).sum() / w.sum()      # inverse distance weighting
# variogram and kriging: pip install scikit-gstat pykrige
```

```r
w <- 1 / d^2; sum(w * z) / sum(w)                   # inverse distance weighting
# variogram and kriging: library(gstat); variogram(level ~ 1, wells); krige(...)
```

[[code: st.m21.spatial.eg]]

## Common mistakes

- **Treating nearby measurements as independent** in a test or an interval: they are not, and the effective sample size is smaller.
- **Interpolating beyond the range**, where the data say nothing.
- **A contour map without its uncertainty.**

## What to do next

On the **Exercise** tab, practice questions on interpolation and the variogram.

[[example: The variogram: nugget and slope]]

The groundwater semivariance by distance: nugget (intercept) **1.59 m²**, slope **1.35 m²/km**.

1. **Change the first value** (the semivariance at 0.5 km) to 0.4. The intercept, the nugget, falls: the closest wells now agree much better, so less of the variation is noise.
2. **Change the last value** (at 4 km) to 8. The line steepens: the variogram is still rising at 4 km, so the range lies further out.

[[try: st.m21.variogram.try "@Variogram/Semivariance (m2)" | Equation Intercept Slope RSquared figure=1]]
