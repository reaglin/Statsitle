---
id: st.m1.variability
title: Data variability and distribution shape
minutes: 45
objectives: [st.m1.o4, st.m1.o5, st.m1.o1]
builds-on: st.m1.descriptive
example: st.m1.variability.eg
quiz: quizzes/m1-variability.json
reference: [frequency-and-histograms, quartiles-and-outliers, skewness-and-shape, sources-of-variability]
---

# Data variability and distribution shape

Item 1.2 described data with numbers. This item draws them. A picture of the data shows its **distribution**: which values occur and how often. It shows at a glance what the numbers can hide: a long tail, two groups mixed together, a value far from the rest.

## The frequency distribution

A **frequency distribution** (or frequency table) groups the data into classes of equal width and counts the values in each.

1. **Choose the number of classes, k.** Too few hide the shape; too many show noise. The program uses **Sturges' rule**, k = ⌈log₂ n + 1⌉, unless you set k yourself: 5 classes for 10 values, 6 for 20, 7 for 50, 8 for 100.
2. **Find the class width.** Width = range / k, rounded up to a convenient number.
3. **Count.** Each class includes its lower limit and not its upper one, so a value on a boundary goes in the higher class. (The last class also includes its upper limit, so the maximum is counted.)
4. **Add the relative frequency** (count / n) and the **cumulative frequency** (the running total), if they help the reader.

## A worked case by hand

The Example has twenty cycle times of an assembly station, in seconds (s). Sorted:

40.0, 41.5, 42.3, 42.8, 43.1, 43.6, 43.9, 44.2, 44.4, 44.8, 45.1, 45.5, 45.9, 46.4, 46.8, 47.5, 48.3, 49.1, 50.6, 52.0

- n = 20, so k = ⌈log₂ 20 + 1⌉ = ⌈4.32 + 1⌉ = 6 classes.
- The range is 52.0 − 40.0 = 12.0 s, so the width is 12.0 / 6 = 2.0 s.

| Class (s) | Count | Relative frequency | Cumulative |
|---|---|---|---|
| 40 to under 42 | 2 | 10 % | 2 |
| 42 to under 44 | 5 | 25 % | 7 |
| 44 to under 46 | 6 | 30 % | 13 |
| 46 to under 48 | 3 | 15 % | 16 |
| 48 to under 50 | 2 | 10 % | 18 |
| 50 to 52 | 2 | 10 % | 20 |

Check: the counts add to n = 20, and the relative frequencies to 100 %. Reading it: 13 of the 20 cycles (65 %) took less than 46 s.

## The histogram

The **histogram** draws the frequency table: one bar per class, its height the count. The program also draws a normal curve with the same mean and standard deviation, for comparison.

[[figure: st.m1.variability.eg 1]]

The bars rise to a peak at 44 to 46 s and fall away more slowly on the right than on the left. A few slow cycles make a short tail to the right.

## The box plot

The **box plot** draws five numbers. The box runs from Q1 to Q3, so it holds the middle half of the data; the line inside is the median. The whiskers reach the smallest and largest values inside the **fences**, Q1 − 1.5 × IQR and Q3 + 1.5 × IQR. A value beyond a fence is drawn as a separate point: an **outlier**, to be looked at.

[[figure: st.m1.variability.eg 2]]

Here Q1 = 43.475 s, the median is 44.95 s and Q3 = 46.975 s, so the IQR is 3.5 s. The fences are 38.225 s and 52.225 s, and every cycle is inside them: no outliers. The median sits a little left of the middle of the box, and the right whisker is longer: a mild right skew.

## The measures of variability, side by side

| Measure | Cycle times | What it uses |
|---|---|---|
| Range | 12.0 s | the two extremes only |
| Variance s² | 9.20 s² | every value, squared units |
| Standard deviation s | 3.03 s | every value, the data's units |
| IQR | 3.50 s | the middle half only |

For data with a bell-shaped histogram, about 68 % of the values lie within one standard deviation of the mean. Here x̄ ± s = 45.39 ± 3.03, which is 42.36 to 48.42 s, and 14 of the 20 cycles (70 %) are inside it.

## Shapes you will see

| Shape | Histogram | Mean and median | Skewness |
|---|---|---|---|
| Symmetric | mirror image about the center | about equal | near 0 |
| Positively (right) skewed | long tail of large values | mean > median | positive |
| Negatively (left) skewed | long tail of small values | mean < median | negative |
| Bimodal | two peaks | either | either |

The cycle times have mean 45.39 s, median 44.95 s and skewness 0.49: close to symmetric, with a slight right tail. Times are often right-skewed: a cycle cannot be much faster than the machine allows, but a jam can make it much slower.

**Two peaks usually mean two processes.** If the histogram of a day's parts has two peaks, look for two machines, two shifts or two batches of material mixed together, and split the data.

## Variability, process consistency and quality

A **consistent** process makes nearly the same result every time: its standard deviation is small compared with the tolerance. The variability comes from two kinds of cause:

- **Common causes**: many small effects that are always present (small differences in material, vibration, temperature). They make the steady, random spread of a stable process. Reducing them means changing the process itself: a better machine, a tighter material specification.
- **Special (assignable) causes**: a specific event that is not part of the normal process: a worn tool, a wrong setting, a bad batch of material, a new operator. They show up as outliers, sudden shifts or trends. They are found and removed.

Why it matters for quality: a part is defective when it falls outside its **tolerance**, and that depends on the spread as well as the mean. If the cycle-time target were "under 50 s", the 2 cycles of 20 (10 %) at 50 s or more would miss it, although the mean, 45.39 s, is well under. Reducing the spread brings those slow cycles in. Item 1.6 shows how quality control watches for special causes.

## In Python and R

```python
import numpy as np
import matplotlib.pyplot as plt
x = np.array([43.1, 45.5, 40.0, 44.4, 47.5, 42.8, 46.4, 44.2, 49.1, 43.6,
              45.9, 41.5, 52.0, 44.8, 42.3, 46.8, 43.9, 48.3, 45.1, 50.6])
counts, edges = np.histogram(x, bins=6)
print(counts)                 # [2 5 6 3 2 2]
print(edges)                  # [40. 42. 44. 46. 48. 50. 52.]
plt.hist(x, bins=6); plt.show()
plt.boxplot(x, vert=False); plt.show()
```

```r
x <- c(43.1, 45.5, 40.0, 44.4, 47.5, 42.8, 46.4, 44.2, 49.1, 43.6,
       45.9, 41.5, 52.0, 44.8, 42.3, 46.8, 43.9, 48.3, 45.1, 50.6)
h <- hist(x, breaks = seq(40, 52, by = 2), right = FALSE, include.lowest = TRUE)
h$counts                      # 2 5 6 3 2 2
boxplot(x, horizontal = TRUE)
```

In R, `right = FALSE` makes each class include its lower limit, as the program does. R's default (`right = TRUE`) includes the upper limit instead, which can move a value that sits on a boundary into the class below.

## Common mistakes

- Classes of unequal width, which make the bars' heights misleading.
- Too many classes for a small sample: with 20 values, 15 classes show only noise.
- Calling a distribution "skewed left" because its peak is on the left. The skew is named by the **tail**.
- Deleting an outlier without finding out why it is there.
- Judging a process by its mean alone, when the tolerance is set on individual parts.

## What to do next

Open the **Example** tab and press **Open in the Workbench**. Set **Histogram classes** to 12, then to 3, and watch the histogram change. Then answer the questions on the **Exercise** tab: you will build a small frequency table by hand.
