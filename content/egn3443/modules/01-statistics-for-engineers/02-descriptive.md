---
id: st.m1.descriptive
title: Descriptive statistics
minutes: 45
objectives: [st.m1.o1]
builds-on: st.m1.introduction
example: st.m1.descriptive.eg
exercise: st.m1.descriptive.ex
reference: [descriptive-statistics, mean-median-mode, variance-standard-deviation, quartiles-and-outliers]
---

# Descriptive statistics

Descriptive statistics turn a column of measurements into a few numbers that say what the data look like. There are four kinds:

| Kind | Question | Measures |
|---|---|---|
| **Center** | What is a typical value? | mean, median, mode |
| **Spread** | How much do the values differ? | range, variance, standard deviation, interquartile range |
| **Position** | Where does a value stand among the others? | minimum, maximum, quartiles, percentiles |
| **Shape** | Is the data symmetric, or does it have a long tail? | skewness, the histogram, the box plot |

This item shows each kind once. Module 2 spends a full item on each.

## Center: mean and median

- The **mean** is the arithmetic average: x̄ = Σxᵢ / n. It uses every value, so one extreme value pulls it.
- The **median** is the middle value once the data are sorted. With an even n, it is the average of the two middle values. Extreme values do not move it.
- The **mode** is the most frequent value. For measured data, values rarely repeat, so it is mostly used for counts and categories.

## Spread: range, variance, standard deviation

- The **range** is maximum − minimum.
- The **sample variance** is the average squared distance from the mean, dividing by n − 1 for a sample: s² = Σ(xᵢ − x̄)² / (n − 1).
- The **standard deviation** s = √s² is in the data's own unit. Read it as the typical distance of a value from the mean.
- The **coefficient of variation**, CV = 100 % × s / x̄, gives the spread relative to the mean.

## Position: quartiles and percentiles

The **p-th percentile** is the value with p % of the data below it. The **first quartile** Q1 is the 25th percentile, the **median** the 50th, and the **third quartile** Q3 the 75th. The **interquartile range**, IQR = Q3 − Q1, is the width of the middle half of the data.

The program, R and numpy find a quartile the same way: sort the values; the p-th quantile is at position h = (n − 1)p + 1, and when h is not a whole number it is that far between the two values on either side.

## Shape: skewness

**Skewness** is near 0 for symmetric data, positive when there is a long tail of large values, and negative for a long tail of small values. Beyond about ±1 the data are strongly skewed. Item 1.3 shows the shapes as pictures.

## A worked case by hand

A concrete technician measured the slump of five batches, in millimeters (mm): 75, 90, 80, 95, 85.

1. **Mean.** Σxᵢ = 75 + 90 + 80 + 95 + 85 = 425, so x̄ = 425 / 5 = **85 mm**.
2. **Median.** Sorted: 75, 80, 85, 90, 95. The middle (3rd) value is **85 mm**.
3. **Range.** 95 − 75 = **20 mm**.
4. **Variance.** The deviations from 85 are −10, 5, −5, 10, 0. Their squares are 100, 25, 25, 100, 0, which add to 250. s² = 250 / (5 − 1) = **62.5 mm²**.
5. **Standard deviation.** s = √62.5 = **7.91 mm**.
6. **Quartiles.** For Q1, h = (5 − 1) × 0.25 + 1 = 2, so Q1 is the 2nd sorted value, **80 mm**. For Q3, h = (5 − 1) × 0.75 + 1 = 4: **90 mm**. IQR = 90 − 80 = **10 mm**.
7. **Shape.** The values are spaced evenly round 85 mm, so the mean equals the median and the skewness is 0: symmetric.
8. **CV.** 100 % × 7.91 / 85 = **9.3 %**.

A summary for a report: *"The five batches had a mean slump of 85 mm (median 85 mm) with a standard deviation of 7.9 mm, from 75 to 95 mm."*

## The Example

Fifteen aluminum test bars were pulled to failure. Their tensile strengths, in megapascals (MPa):

[[data: st.m1.descriptive.eg]]

[[results: st.m1.descriptive.eg]]

Reading the results:

- **Center.** The mean is 313.07 MPa and the median 313.00 MPa. They agree, so no extreme value is pulling the mean.
- **Spread.** The standard deviation is 6.15 MPa: a typical bar is within about 6 MPa of the mean. The range is 22 MPa (303 to 325 MPa), and the CV is 1.96 %, a small spread for a metal property.
- **Position.** Q1 = 308.5 MPa and Q3 = 317.0 MPa, so the middle half of the bars lie within an IQR of 8.5 MPa.
- **Shape.** The skewness is 0.21: about symmetric. There are no outliers.

[[figure: st.m1.descriptive.eg 2]]

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([312, 305, 318, 309, 321, 314, 307, 316, 311, 325, 303, 313, 319, 308, 315])
print(np.mean(x), np.median(x))       # 313.0667 313.0
print(np.std(x, ddof=1))              # 6.1466
print(np.percentile(x, [25, 75]))     # [308.5 317. ]
print(stats.skew(x, bias=False))      # 0.2077
```

```r
x <- c(312, 305, 318, 309, 321, 314, 307, 316, 311, 325, 303, 313, 319, 308, 315)
mean(x); median(x)              # 313.0667 313
sd(x)                           # 6.1466
quantile(x, c(0.25, 0.75))      # 308.5 317
summary(x)                      # minimum, quartiles, median, mean, maximum
```

The full script, which prints every result with the program's labels:

[[code: st.m1.descriptive.eg]]

## Common mistakes

- Taking the median without sorting first.
- Dividing by n instead of n − 1 for a sample's variance (in numpy, forgetting `ddof=1`).
- Reporting the variance as if it were the standard deviation. The variance is in squared units.
- Reporting a center without a spread, or numbers without units.

## What to do next

On the **Exercise** tab, twelve spot welds were pulled apart, and one is much stronger than the rest. Calculate the mean, the median, the standard deviation and the IQR, and press **Check my answers**. Then notice which of the four the strong weld moved most.
