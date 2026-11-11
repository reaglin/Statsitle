---
title: Parameters and statistics
---

# Parameters and statistics

- **Parameter:** a number that describes the whole population. Fixed, usually unknown.
- **Statistic:** a number calculated from a sample. Known, but different for every sample. Used to estimate a parameter.

| Measure | Parameter | Statistic |
|---|---|---|
| Mean | μ | x̄ |
| Standard deviation | σ (divide by N) | s (divide by n − 1) |
| Variance | σ² | s² |
| Proportion | p | p̂ |
| Size | N | n |

**Test:** can it be calculated from the data in hand? Then it is a statistic.

**Sampling error** = statistic − parameter (x̄ − μ for a mean). It comes from chance in which members were sampled, and shrinks on average as n grows (standard error σ/√n).

**Bias** is a systematic error: a miscalibrated instrument, a sample taken from one shift only, non-response. A larger sample does not reduce it; a better design does.

## Python and R

| Statistic | Python (`x` a numpy array) | R |
|---|---|---|
| x̄ | `x.mean()` | `mean(x)` |
| s | `x.std(ddof=1)` | `sd(x)` |
| s/√n | `stats.sem(x)` | `sd(x) / sqrt(length(x))` |
| p̂ | `np.mean(x == value)` | `mean(x == value)` |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 6-1 and 7-1 (statistics and parameters, point estimation).
- EGN3443 Module 6, Topic 2: https://reaglin.github.io/EGN3443/week-7module-6-sampling-distrbutions/
