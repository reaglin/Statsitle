---
title: Descriptive statistics: the vocabulary
---

# Descriptive statistics: the vocabulary

| Term | Meaning | Symbol |
|---|---|---|
| Population | Every item of interest | size N |
| Sample | The items actually measured | size n |
| Parameter | A number describing the population (usually unknown) | μ, σ, σ² |
| Statistic | A number calculated from the sample | x̄, s, s² |
| Descriptive statistics | Summaries of the data in hand | |
| Inferential statistics | Conclusions about the population from a sample (Module 7 on) | |

## The four kinds of summary

| Kind | Measures |
|---|---|
| Center | mean x̄, median, mode |
| Spread | range, variance s², standard deviation s, interquartile range, coefficient of variation |
| Position | minimum, maximum, quartiles Q1 and Q3 |
| Shape | skewness, excess kurtosis, histogram, box plot |

## Python and R

| Task | Python (numpy, scipy.stats) | R (base) |
|---|---|---|
| Many statistics at once | `stats.describe(x)` | `summary(x)` |
| Histogram | `matplotlib.pyplot.hist(x)` | `hist(x)` |
| Box plot | `matplotlib.pyplot.boxplot(x, vert=False)` | `boxplot(x, horizontal = TRUE)` |

`stats.describe` reports the variance with n − 1, but its skewness and kurtosis use `bias=True` by default, so they differ from the program's (see Skewness and shape).

## Sources

- Montgomery, D. C., and Runger, G. C., *Applied Statistics and Probability for Engineers*, 7th ed., Wiley, Chapter 6, Descriptive Statistics (Section 6.1, Numerical summaries of data).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.5, Quantitative Techniques, and Section 1.3.3, Graphical Techniques: https://www.itl.nist.gov/div898/handbook/
