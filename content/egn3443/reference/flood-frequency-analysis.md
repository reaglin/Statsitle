---
title: Flood and rainfall frequency analysis
---

# Flood and rainfall frequency analysis

| Quantity | Formula |
|---|---|
| Exceedance probability | p = P(X > Q), X the annual maximum |
| Return period | T = 1/p; the T-year event is the (1 − 1/T) quantile |
| At least one T-year event in n years | 1 − (1 − 1/T)ⁿ |
| Frequency factor form | Q_T = x̄ + K_T · s |
| Gumbel (EV I) | K_T = −(√6/π)[0.5772 + ln(ln(T/(T − 1)))]; K₁₀₀ = 3.137 |
| Lognormal | Q_T = exp(μ + z₁₋₁/T · σ), μ and σ of ln Q (item 5.6) |
| Log-Pearson III | log Q_T = mean(log Q) + K(T, skew) · sd(log Q); K from tables (Bulletin 17C) |

**Data:** one maximum per year (independent values); check for trends first (item 21.2). Real data: USGS National Water
Information System (waterdata.usgs.gov); rainfall frequency: NOAA Atlas 14.

## Python and R

| Task | Python | R |
|---|---|---|
| Lognormal quantile | `np.exp(mu + stats.norm.ppf(0.99) * sigma)` | `qlnorm(0.99, mu, sigma)` |
| Gumbel fit | `stats.gumbel_r.fit(peaks)`, `.ppf(0.99, *params)` | `evd::fgev` or by the frequency factor |
| Log-Pearson III | `stats.pearson3.ppf(0.99, skew, loc, scale)` on log₁₀ Q | `PearsonDS` or frequency-factor tables |

**Read more:** Wanielista, Kersten and Eaglin, *Hydrology: Water Quantity and Quality Control* (Wiley), the chapter on
frequency analysis; England et al. (2019), *Guidelines for Determining Flood Flow Frequency — Bulletin 17C*, USGS
Techniques and Methods 4-B5.
