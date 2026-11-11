---
title: Range, variance and standard deviation
---

# Range, variance and standard deviation

| Measure | Formula | Unit |
|---|---|---|
| Range | max − min | data unit |
| Sample variance | s² = Σ(xᵢ − x̄)² / (n − 1) | data unit squared |
| Sample standard deviation | s = √s² | data unit |
| Population variance | σ² = Σ(xᵢ − μ)² / N | data unit squared |
| Population standard deviation | σ = √σ² | data unit |
| Coefficient of variation | CV = 100 % × s / x̄ | % (no unit) |
| Standard error of the mean | SE = s / √n | data unit |

A shortcut for the sum of squares, useful by hand: Σ(xᵢ − x̄)² = Σxᵢ² − (Σxᵢ)² / n.

**n − 1:** use it whenever the data are a sample (nearly always). The deviations add to zero, so only n − 1 of them are free; dividing by n would make s² too small on average.

**Rough guide for bell-shaped data:** about 68 % of values lie within x̄ ± s, about 95 % within x̄ ± 2s, nearly all within x̄ ± 3s.

## Python and R

| Measure | Python | R |
|---|---|---|
| Range | `np.ptp(x)` | `max(x) - min(x)` (`range(x)` gives both ends) |
| Sample variance | `np.var(x, ddof=1)` | `var(x)` |
| Sample standard deviation | `np.std(x, ddof=1)` | `sd(x)` |
| Population standard deviation | `np.std(x)` | `sqrt(mean((x - mean(x))^2))` |
| Coefficient of variation | `100 * np.std(x, ddof=1) / np.mean(x)` | `100 * sd(x) / mean(x)` |
| Standard error | `stats.sem(x)` | `sd(x) / sqrt(length(x))` |

numpy divides by n unless `ddof=1` is given. R's `var` and `sd`, Excel's `VAR.S` and `STDEV.S` divide by n − 1.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 6.1 (sample variance, sample standard deviation, sample range).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.5.6, Measures of Scale: https://www.itl.nist.gov/div898/handbook/eda/section3/eda356.htm
