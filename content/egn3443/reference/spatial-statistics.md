---
title: Spatial statistics: interpolation and the variogram
---

# Spatial statistics: interpolation and the variogram

| Quantity | Formula |
|---|---|
| Inverse distance weighting | ẑ(x₀) = Σ wᵢ zᵢ / Σ wᵢ, wᵢ = 1/dᵢᵖ (p = 2 usual) |
| Semivariance | γ(h) = ½ · mean of (z(x) − z(x + h))² over pairs about h apart |
| Nugget | γ as h → 0: measurement error and very short-range variation |
| Sill | the level γ reaches; about the variance of the data |
| Range | the distance at which γ reaches the sill |
| Kriging | weights from a fitted variogram model; gives a kriging variance at every point |

Nearby measurements are not independent: an interval that treats them as independent is too narrow.

## Python and R

| Task | Python | R |
|---|---|---|
| Inverse distance weighting | `w = 1/d**2; (w*z).sum()/w.sum()` | `sum(w*z)/sum(w)` or `gstat::idw` |
| Empirical variogram | `skgstat.Variogram(coords, values)` | `gstat::variogram(z ~ 1, data)` |
| Ordinary kriging | `pykrige.OrdinaryKriging(x, y, z)` | `gstat::krige(z ~ 1, data, grid, model)` |

**Read more:** Isaaks and Srivastava, *An Introduction to Applied Geostatistics* (Oxford University Press).
