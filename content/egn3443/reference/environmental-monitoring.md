---
title: Environmental monitoring: lognormal data, standards, detection limits
---

# Environmental monitoring: lognormal data, standards, detection limits

| Quantity | Formula |
|---|---|
| Lognormal fit | μ, σ = mean and standard deviation of ln(concentration) |
| Median (geometric mean) | e^μ |
| Arithmetic mean | e^(μ + σ²/2) |
| Exceedance of a standard S | P(X > S) = P(Z > (ln S − μ)/σ) |
| p-th percentile | e^(μ + z_p σ) |

**Below the detection limit (censored values):** avoid substituting 0, the limit or half of it; use maximum likelihood with
censoring or regression on order statistics (Part II).

**Real data:** the Water Quality Portal (waterqualitydata.us); EPA air-quality data (epa.gov/outdoor-air-quality-data).

## Python and R

| Task | Python | R |
|---|---|---|
| Exceedance | `stats.norm.sf((np.log(S) - mu) / sigma)` | `plnorm(S, mu, sigma, lower.tail = FALSE)` |
| Percentile | `np.exp(mu + stats.norm.ppf(0.95) * sigma)` | `qlnorm(0.95, mu, sigma)` |
| Censored data | `statsmodels` survival tools, or by maximum likelihood | `NADA2::cenmle`, `NADA2::cenros` |

**Read more:** Helsel, *Statistics for Censored Environmental Data Using Minitab and R* (Wiley).
