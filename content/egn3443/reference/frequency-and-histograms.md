---
title: Frequency tables and histograms
---

# Frequency tables and histograms

| Step | Rule |
|---|---|
| Number of classes | Sturges: k = ⌈log₂ n + 1⌉ (the program's default); or the square-root rule, ⌈√n⌉ |
| Class width | range / k, rounded up to a convenient number |
| Class limits | each class includes its lower limit, not its upper; the last class includes both |
| Frequency | the count of values in the class |
| Relative frequency | count / n |
| Cumulative frequency | the running total of the counts |

| n | Sturges' k |
|---|---|
| 10 | 5 |
| 20 | 6 |
| 50 | 7 |
| 100 | 8 |

**Histogram:** one bar per class, touching, height = frequency (or relative frequency). **Box plot:** box from Q1 to Q3 with the median, whiskers to the most extreme values inside the fences Q1 − 1.5 × IQR and Q3 + 1.5 × IQR, outliers as points.

## Python and R

| Task | Python | R |
|---|---|---|
| Counts in k equal classes | `np.histogram(x, bins=k)` | `hist(x, breaks = k, plot = FALSE)$counts` (R treats k as a suggestion) |
| Counts with given limits | `np.histogram(x, bins=[40, 42, 44])` | `hist(x, breaks = c(40, 42, 44), right = FALSE, plot = FALSE)$counts` |
| Histogram | `plt.hist(x, bins=k)` | `hist(x)` |
| Box plot | `plt.boxplot(x, vert=False)` | `boxplot(x, horizontal = TRUE)` |

numpy and the program count a value on a class boundary in the higher class; R does the same only with `right = FALSE`.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 6.3, Frequency Distributions and Histograms, and Section 6.4, Box Plots.
- Sturges, H. A. (1926), "The choice of a class interval", *Journal of the American Statistical Association* 21(153), 65–66.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.3.14, Histogram: https://www.itl.nist.gov/div898/handbook/eda/section3/histogra.htm
