---
id: st.m2.central-tendency
title: Measures of central tendency
minutes: 40
objectives: [st.m2.o1, st.m2.o4]
builds-on: st.m2.introduction
example: st.m2.central-tendency.eg
exercise: st.m2.central-tendency.ex
reference: [mean-median-mode]
---

# Measures of central tendency

A measure of central tendency is one number that stands for a typical value of the data. There are three: the **mean**, the **median** and the **mode**. Each answers a slightly different question, and they agree only when the data are symmetric.

## The mean: the arithmetic average

Add the values and divide by how many there are:

x̄ = Σxᵢ / n

- x̄ ("x-bar") is the sample mean.
- xᵢ is the i-th value, and Σ means "add them all up".
- n is the number of values.

The mean is the **balance point** of the data: if each value were a weight on a ruler, the ruler would balance at x̄. It uses every value, which makes it the best estimate of the population mean μ when the data are roughly symmetric. It also means a single extreme value can pull it a long way.

## The median: the middle value

Sort the values from smallest to largest. The median is the one in the middle.

- If n is **odd**, the median is the value in position (n + 1) / 2.
- If n is **even**, there are two middle values, in positions n / 2 and n / 2 + 1. The median is their average.

Half the values are at or below the median, and half are at or above it. The median only cares about the order of the values, not how far out the extreme ones are. That makes it **resistant** to outliers.

## The mode: the most frequent value

The mode is the value that occurs most often. A dataset can have one mode, several modes (each occurring equally often), or none, when every value occurs once.

With measured data such as lengths or strengths, values rarely repeat exactly, so the mode is often not useful. It is the only measure of center for **categorical** data: if a plant logs the cause of each breakdown (electrical, hydraulic, mechanical), the mode is the most common cause. A mean of "hydraulic" makes no sense.

## A worked case by hand

A line inspector counts rejected parts on five days: 4, 7, 5, 9, 5.

1. **Mean.** Σxᵢ = 4 + 7 + 5 + 9 + 5 = 30, and n = 5, so x̄ = 30 / 5 = **6 parts**.
2. **Median.** Sorted: 4, 5, 5, 7, 9. n = 5 is odd, so the median is in position (5 + 1) / 2 = 3: **5 parts**.
3. **Mode.** 5 occurs twice and every other value once: the mode is **5 parts**.

Now suppose the 9 had been a bad day with 29 rejects: 4, 7, 5, 29, 5.

- The mean becomes 50 / 5 = **10 parts**. It moved by 4.
- The median is still **5 parts** (sorted: 4, 5, 5, 7, 29).

One unusual day moved the mean far above four of the five values. The median did not move.

## Comparing mean, median and mode

| Shape of the data | How they compare |
|---|---|
| Symmetric | mean ≈ median (≈ mode) |
| Long tail of large values (positively skewed) | mean > median |
| Long tail of small values (negatively skewed) | mean < median |

So comparing the mean and the median is a quick test of shape. Item 2.4 makes this precise.

## Choosing the measure

- **Mean**: numerical data that are roughly symmetric, with no extreme outliers. Also when the total matters: the mean flow times the hours gives the total volume pumped.
- **Median**: numerical data that are skewed or have outliers, such as repair times, incomes or response times. Also ordinal data (ratings such as poor, fair, good).
- **Mode**: categorical data, or to name the most common value.

When in doubt, report both the mean and the median. If they differ a lot, say why.

## The Example

The Example tab weighs ten cement bags filled to 25 kg:

[[data: st.m2.central-tendency.eg]]

[[figure: st.m2.central-tendency.eg 1]]

The mean is 25.03 kg, the median is 25.00 kg and the mode is 25.0 kg. They are close together because the weights are spread evenly on both sides of 25 kg.

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([25.1, 24.9, 25.0, 25.2, 24.8, 25.0, 25.3, 25.0, 24.9, 25.1])
print(np.mean(x))                          # 25.03
print(np.median(x))                        # 25.0
print(stats.mode(x, keepdims=False).mode)  # 25.0
```

```r
x <- c(25.1, 24.9, 25.0, 25.2, 24.8, 25.0, 25.3, 25.0, 24.9, 25.1)
mean(x)     # 25.03
median(x)   # 25
tab <- table(x); names(tab)[tab == max(tab)]   # "25": base R has no mode function
```

The full script for the Example, which prints every result with its label, is on the Example's Code tab:

[[code: st.m2.central-tendency.eg]]

## Common mistakes

- Taking the middle value of the data **before sorting** it.
- With an even n, taking one of the two middle values instead of their average.
- Reporting the mean of skewed data, such as repair times, as the "typical" value. It is pulled toward the long tail.
- Using R's `mode(x)`. It does not give the statistical mode: it says how R stores x ("numeric").

## What to do next

On the **Exercise** tab, twelve bolts were measured and one is longer than the rest. Calculate the mean and the median, type them in, and press **Check my answers**. Then see which one the long bolt moved.
