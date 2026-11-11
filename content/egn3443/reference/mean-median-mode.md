---
title: Mean, median and mode
---

# Mean, median and mode

| Measure | Formula or rule | Resistant to outliers? |
|---|---|---|
| Sample mean | x̄ = Σxᵢ / n | No |
| Population mean | μ = Σxᵢ / N | No |
| Median, n odd | the sorted value in position (n + 1) / 2 | Yes |
| Median, n even | the average of the sorted values in positions n / 2 and n / 2 + 1 | Yes |
| Mode | the most frequent value; there may be several, or none | Yes |

**Shape and the three measures:** symmetric, mean ≈ median; positively (right) skewed, mean > median; negatively (left) skewed, mean < median.

**Which to use:** mean for symmetric numerical data, and whenever a total matters (total = n × x̄); median for skewed numerical data, data with outliers, and ordinal data; mode for categorical data.

## Python and R

| Measure | Python | R |
|---|---|---|
| Mean | `np.mean(x)` | `mean(x)` |
| Median | `np.median(x)` | `median(x)` |
| Mode | `stats.mode(x, keepdims=False).mode` (the smallest, if several) | `tab <- table(x); names(tab)[tab == max(tab)]` |

R's `mode(x)` is not the statistical mode: it returns the storage type, such as "numeric". `stats.mode` returns a value even when every value occurs once, so check its `count`.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 6.1 (sample mean, sample median, mode).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.5.1, Measures of Location: https://www.itl.nist.gov/div898/handbook/eda/section3/eda351.htm
