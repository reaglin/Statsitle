---
title: Skewness, kurtosis and the histogram
---

# Skewness, kurtosis and the histogram

| Measure | Formula |
|---|---|
| Skewness (adjusted, as Excel's `SKEW`) | g₁ = [n / ((n − 1)(n − 2))] × Σ((xᵢ − x̄) / s)³ |
| Excess kurtosis (adjusted, as Excel's `KURT`) | [n(n + 1) / ((n − 1)(n − 2)(n − 3))] × Σ((xᵢ − x̄) / s)⁴ − 3(n − 1)² / ((n − 2)(n − 3)) |
| Histogram classes (Sturges' rule) | k = ⌈log₂ n + 1⌉ |

Skewness needs at least 3 values, excess kurtosis at least 4. Both vary a lot in samples under about 20.

| Skewness | Shape | Mean and median |
|---|---|---|
| −0.5 to 0.5 | about symmetric | mean ≈ median |
| above 0.5 (strong above 1) | positively (right) skewed: long tail of large values | mean > median |
| below −0.5 (strong below −1) | negatively (left) skewed: long tail of small values | mean < median |

Skew is named by the side of the **long tail**, not the side of the peak.

**Excess kurtosis:** about 0 for a normal distribution; positive for heavier tails (more extreme values); negative for lighter tails (flat or bounded data).

## Python and R

| Measure | Python | R |
|---|---|---|
| Skewness | `stats.skew(x, bias=False)` | `n/((n-1)*(n-2)) * sum(((x - mean(x))/sd(x))^3)` |
| Excess kurtosis | `stats.kurtosis(x, bias=False)` | the formula above, written out |
| Histogram | `plt.hist(x, bins=k)` | `hist(x, breaks = k)` |

scipy's default `bias=True`, and the default of R's e1071 package, give a smaller skewness than Excel and this program for small samples. Write `bias=False` to match.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 6.3, Frequency Distributions and Histograms (shape, symmetry and skew).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.5.11, Measures of Skewness and Kurtosis: https://www.itl.nist.gov/div898/handbook/eda/section3/eda35b.htm, and Section 1.3.3.14, Histogram.
- Joanes, D. N., and Gill, C. A. (1998), "Comparing measures of sample skewness and kurtosis", *The Statistician* 47(1), 183–189 (the adjusted formulas).
