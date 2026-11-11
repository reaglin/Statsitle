---
title: Quartiles, IQR, outliers and the box plot
---

# Quartiles, IQR, outliers and the box plot

| Measure | Formula or rule |
|---|---|
| First quartile Q1 | the value with 25 % of the data below it |
| Third quartile Q3 | the value with 75 % of the data below it |
| Interquartile range | IQR = Q3 − Q1, the width of the middle half |
| Lower fence | Q1 − 1.5 × IQR |
| Upper fence | Q3 + 1.5 × IQR |
| Outlier (Tukey's rule) | a value below the lower fence or above the upper fence |

**How the program finds a quartile (type 7, linear interpolation):** sort the n values. For the p-th quantile, the position is h = (n − 1)p + 1. If h is a whole number, the quantile is the value in that position. Otherwise it is part way between the values on either side: x₍ⱼ₎ + (h − j)(x₍ⱼ₊₁₎ − x₍ⱼ₎), where j is the whole part of h. This is R's default, numpy's default and Excel's `QUARTILE.INC`. Other conventions give slightly different quartiles for small samples; the program also reports Tukey's hinges (R's `fivenum`).

**The box plot:** the box runs from Q1 to Q3, with a line at the median. The whiskers reach to the smallest and largest values inside the fences. Values beyond the fences are drawn as separate points.

**An outlier is a flag, not a verdict.** Check each one: a recording or measuring error is corrected or removed (and the removal reported); a real extreme value is kept.

## Python and R

| Task | Python | R |
|---|---|---|
| Quartiles | `np.percentile(x, [25, 75])` | `quantile(x, c(0.25, 0.75))` |
| IQR | `stats.iqr(x)` | `IQR(x)` |
| Five-number summary | `np.percentile(x, [0, 25, 50, 75, 100])` | `fivenum(x)` (Tukey's hinges) or `summary(x)` |
| Box plot | `plt.boxplot(x, vert=False)` | `boxplot(x, horizontal = TRUE)` |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 6.4, Box Plots (quartiles, IQR, outliers).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.3.7, Box Plot: https://www.itl.nist.gov/div898/handbook/eda/section3/boxplot.htm, and Section 7.1.6, What are outliers in the data?
- Hyndman, R. J., and Fan, Y. (1996), "Sample quantiles in statistical packages", *The American Statistician* 50(4), 361–365 (the quantile types).
