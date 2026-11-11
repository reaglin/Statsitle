---
id: st.m2.variability
title: Measures of variability
minutes: 45
objectives: [st.m2.o2, st.m2.o5]
builds-on: st.m2.central-tendency
example: st.m2.variability.eg
exercise: st.m2.variability.ex
reference: [variance-standard-deviation, quartiles-and-outliers]
---

# Measures of variability

## Why spread matters

Two filling machines both put a mean of 500 g in each package. Machine A fills between 498 g and 502 g. Machine B fills between 470 g and 530 g. Their means are equal, but machine B underfills customers and wastes product. The center alone hides the difference. A **measure of variability** (or spread) says how far the values typically are from each other or from the center.

In engineering, variability is often the number that matters most: tolerances, safety factors and quality limits are all set by it.

## Range: the simplest measure

Range = maximum − minimum

It is quick and easy to explain. But it uses only the two most extreme values, so one unusual value changes it completely, and it tends to grow as you take more measurements. Use it for a quick look, or for very small samples, such as the five parts in a quality-control subgroup.

## Variance: the average squared deviation

Each value's **deviation** is its distance from the mean, xᵢ − x̄. Some are positive and some negative, and they always add to zero, so their plain average is no use. Square them first, then average:

s² = Σ(xᵢ − x̄)² / (n − 1)

This is the **sample variance**. Its unit is the square of the data's unit: for lengths in mm, s² is in mm².

**Why n − 1?** The deviations are measured from x̄, which was calculated from the same data, so x̄ sits closer to these values than the true mean μ does. Dividing by n would make the variance too small on average. Dividing by n − 1 corrects that. Another way to see it: the n deviations must add to zero, so once n − 1 of them are known the last is fixed. Only n − 1 are free to vary: these are the **degrees of freedom**.

When the data really are the whole population (every unit made, not a sample of them), divide by n instead:

σ² = Σ(xᵢ − μ)² / n

## Standard deviation: the most used measure

The standard deviation is the square root of the variance:

s = √s²

It is back in the data's own unit, so it can be compared with the mean and with tolerances. Read it as "the typical distance of a value from the mean". For data with a roughly bell-shaped histogram, about 68 % of the values lie within one standard deviation of the mean, and about 95 % within two.

## A worked case by hand

The five daily reject counts from item 2.2: 4, 7, 5, 9, 5. The mean is x̄ = 6.

| xᵢ | xᵢ − x̄ | (xᵢ − x̄)² |
|---|---|---|
| 4 | −2 | 4 |
| 7 | 1 | 1 |
| 5 | −1 | 1 |
| 9 | 3 | 9 |
| 5 | −1 | 1 |
| Sum | 0 | 16 |

1. **Range** = 9 − 4 = **5 parts**.
2. **Sample variance** s² = 16 / (5 − 1) = **4 parts²**.
3. **Sample standard deviation** s = √4 = **2 parts**.
4. For comparison, the population variance would be 16 / 5 = 3.2 and σ = 1.79. That is smaller, as expected.

Check: the deviations column adds to 0. If yours does not, the mean is wrong.

## Two more measures you will see

- The **interquartile range**, IQR = Q3 − Q1, is the width of the middle half of the data. Like the median, it ignores the extremes, so it is the measure of spread to use with skewed data. The box in a box plot is the IQR.
- The **coefficient of variation**, CV = 100 % × s / x̄, gives the spread as a percentage of the mean. It compares spread between things on different scales: a CV of 1 % on bolt lengths and 5 % on concrete strengths says the concrete varies more, relative to its size.

## Choosing and interpreting

| Use | When |
|---|---|
| Standard deviation | Numerical data, roughly symmetric. The usual choice, and what later modules need |
| Variance | In calculations (variances add; standard deviations do not). Rarely reported alone, since its unit is squared |
| Range | A quick look, or very small samples |
| IQR | Skewed data or data with outliers; report it with the median |
| CV | Comparing spread between datasets with different means or units |

A standard deviation is large or small only in context. A standard deviation of 3.8 L/min is small for a pump delivering 121 L/min (CV ≈ 3 %), and it would be huge for a dosing pump delivering 5 L/min.

## The Example

Ten runs of a pump at one setting, flow in liters per minute (L/min):

[[data: st.m2.variability.eg]]

[[figure: st.m2.variability.eg 2]]

The range is 12 L/min (115 to 127). The sample standard deviation is 3.81 L/min, so a typical run is within about 4 L/min of the mean of 121.1 L/min. The variance is 14.54 (L/min)².

## In Python and R

```python
import numpy as np
x = np.array([118, 124, 121, 115, 127, 119, 122, 125, 117, 123])
print(np.ptp(x))            # range: 12
print(np.var(x, ddof=1))    # sample variance: 14.5444
print(np.std(x, ddof=1))    # sample standard deviation: 3.8137
print(np.std(x))            # population form (divides by n): 3.6180
```

```r
x <- c(118, 124, 121, 115, 127, 119, 122, 125, 117, 123)
max(x) - min(x)   # range: 12 (R's range(x) gives the two ends, 115 and 127)
var(x)            # sample variance: 14.5444
sd(x)             # sample standard deviation: 3.8137
```

**Watch the default.** numpy's `np.std` and `np.var` divide by n unless you write `ddof=1`. R's `sd` and `var`, Excel's `STDEV.S` and scipy's `stats.tstd` divide by n − 1.

## Common mistakes

- Dividing by n for a sample. In Python this happens when `ddof=1` is left out.
- Forgetting the square root, and reporting the variance as the standard deviation.
- Reporting the variance without its squared unit, or comparing it with the mean.
- Using the range to compare two samples of different sizes. The larger sample usually has the larger range.

## What to do next

On the **Exercise** tab, twelve rebar yield strengths are waiting. Calculate the range, the sample variance and the sample standard deviation, type them in, and press **Check my answers**. The hints help if your answer is not accepted.
