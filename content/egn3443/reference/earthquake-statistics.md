---
title: Earthquake statistics: Gutenberg–Richter and Poisson occurrence
---

# Earthquake statistics: Gutenberg–Richter and Poisson occurrence

| Quantity | Formula |
|---|---|
| Gutenberg–Richter law | log₁₀ N(≥ M) = a − b M; b ≈ 1 in most regions |
| Yearly rate of M ≥ m | 10^(a − b m) |
| Number in t years | Poisson with mean λt (item 4.5) |
| At least one in t years | 1 − e^(−λt) |
| Return period for a probability P in t years | T = −t / ln(1 − P); 10 % in 50 years → T ≈ 475 years |

A Poisson process has no memory: time since the last event does not change the chance of the next.

**Real data:** the USGS earthquake catalogue (earthquake.usgs.gov/earthquakes/search).

## Python and R

| Task | Python | R |
|---|---|---|
| Fit the law | `stats.linregress(M, np.log10(N))` | `lm(log10(N) ~ M)` |
| At least one | `1 - np.exp(-rate * t)` or `stats.poisson.sf(0, rate * t)` | `ppois(0, rate * t, lower.tail = FALSE)` |

**Read more:** Gutenberg and Richter (1944), "Frequency of earthquakes in California", *Bulletin of the Seismological Society
of America* 34, 185–188.
