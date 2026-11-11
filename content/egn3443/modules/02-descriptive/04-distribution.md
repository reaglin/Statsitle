---
id: st.m2.distribution
title: Data distribution (symmetry and skewness)
minutes: 40
objectives: [st.m2.o3, st.m2.o4]
builds-on: st.m2.variability
example: st.m2.distribution.eg
quiz: quizzes/m2-distribution.json
reference: [skewness-and-shape, quartiles-and-outliers]
---

# Data distribution: symmetry and skewness

## What a distribution is

The **distribution** of a dataset is the pattern of its values: which values occur, and how often. The mean and the standard deviation give two numbers about it. A picture gives the whole pattern. The two pictures used in this course are:

- the **histogram**: the range of the data is cut into equal intervals (classes), and a bar shows how many values fall in each;
- the **box plot**: the box spans Q1 to Q3 with a line at the median, and the whiskers reach to the smallest and largest values that are not outliers.

**How many classes?** Too few bars hide the shape, and too many show random noise. The program uses **Sturges' rule** unless you set the number yourself: k = ⌈log₂ n + 1⌉ classes, which is 5 for 10 values, 6 for 20 values and 8 for 100 values. With fewer than about 20 values, any histogram is rough, so read its shape with care.

## Symmetric distributions

A distribution is **symmetric** when its left half is close to a mirror image of its right half. The mean and the median are about equal, and the box plot's median sits near the middle of the box, with whiskers of about the same length.

Measurements of a well-controlled process are often symmetric and bell-shaped, like these eighteen shaft diameters (nominal 25.00 mm):

[[figure: st.m2.distribution.symmetric 1]]

Mean 25.0006 mm, median 25.0000 mm, skewness −0.10.

## Skewness

A distribution is **skewed** when one tail is longer than the other. The side of the **long tail** names the skew:

- **Positive (right) skew**: a long tail of large values. The mean is pulled above the median. Typical of quantities that cannot go below zero but can be very large: repair times, waiting times, particle sizes, rainfall, response times.
- **Negative (left) skew**: a long tail of small values. The mean is pulled below the median. Typical of quantities with an upper limit that most values come close to: process yields near 100 %, scores near the maximum, the lifetimes of parts that mostly last until a scheduled replacement.

The **skewness** puts a number on it. The program, Excel's `SKEW`, and scipy with `bias=False` all use the adjusted sample skewness:

g₁ = [n / ((n − 1)(n − 2))] × Σ((xᵢ − x̄) / s)³

Each value's distance from the mean is measured in standard deviations and then **cubed**. Cubing keeps the sign and makes the far values count much more than the near ones. A long right tail gives large positive cubes, and the sum comes out positive.

| Skewness | Reading |
|---|---|
| between −0.5 and 0.5 | about symmetric |
| between 0.5 and 1 (or −1 and −0.5) | moderately skewed |
| above 1 (or below −1) | strongly skewed |

These limits are rules of thumb. With fewer than about 20 values, the skewness itself varies a lot from sample to sample, so look at the histogram as well.

## Positive (right) skew

The Example tab has the times to repair twenty breakdowns of a packaging machine, in hours:

[[figure: st.m2.distribution.eg 1]]

Most repairs take one to two hours, and a few take four to six. The mean is 2.00 h, the median is 1.55 h, and the skewness is 1.82: strongly positively skewed. In the box plot, the median (1.55 h) is closer to Q1 (1.08 h) than to Q3 (2.28 h), the whisker on the side of the long repairs is about four times as long as the other, and the two longest repairs (4.5 h and 6.2 h) show as outliers beyond the upper fence of 4.08 h:

[[figure: st.m2.distribution.eg 2]]

## Negative (left) skew

Eighteen batch yields of a chemical process, in percent. A yield cannot pass 100 %, and a few poor batches make a tail on the left:

[[figure: st.m2.distribution.left 1]]

The mean is 98.04 %, the median is 98.65 %, and the skewness is −1.76: strongly negatively skewed.

## Describing the spread of a distribution

A full description of a distribution gives its **center**, its **spread** and its **shape**, and names any **unusual values**:

- symmetric data: the mean and the standard deviation;
- skewed data: the median and the interquartile range (IQR = Q3 − Q1), and say which way it is skewed;
- outliers: values beyond the fences Q1 − 1.5 × IQR and Q3 + 1.5 × IQR. Look at each one. A recording error is corrected; a real extreme value is kept and reported.

The program also reports the **excess kurtosis**: near 0 for a normal distribution, positive when the tails are heavier (more extreme values than a normal curve would give), negative when they are lighter. It needs more data than skewness to mean much, so treat it as a secondary number.

## Recognizing a distribution in practice

1. Draw the histogram and the box plot. Look first, then calculate.
2. Compare the mean and the median. A large gap, relative to the standard deviation, means skew toward the side of the mean.
3. Read the skewness and its sign.
4. Ask whether the shape makes physical sense. Times and sizes bounded at zero are often right-skewed. Percentages near a limit are often left-skewed. Two peaks in a histogram often mean two machines, shifts or materials mixed together, and the data should be split.

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([1.2, 0.8, 2.5, 1.6, 0.9, 3.8, 1.1, 1.4, 6.2, 2.0,
              1.3, 0.7, 1.8, 2.9, 1.0, 4.5, 1.5, 0.9, 2.2, 1.7])
print(np.mean(x), np.median(x))        # 2.0 1.55
print(stats.skew(x, bias=False))       # 1.8218
print(stats.kurtosis(x, bias=False))   # excess kurtosis: 3.3125
```

```r
x <- c(1.2, 0.8, 2.5, 1.6, 0.9, 3.8, 1.1, 1.4, 6.2, 2.0,
       1.3, 0.7, 1.8, 2.9, 1.0, 4.5, 1.5, 0.9, 2.2, 1.7)
n <- length(x); m <- mean(x); s <- sd(x)
n / ((n - 1) * (n - 2)) * sum(((x - m) / s)^3)   # skewness: 1.8218
hist(x); boxplot(x, horizontal = TRUE)
```

Base R has no skewness function, so the formula is written out. The **e1071** and **moments** packages have one, but check which formula they use: scipy's default (`bias=True`) and e1071's default give a smaller value than Excel's `SKEW`.

## Common mistakes

- Naming the skew by where the **peak** is. The peak of a right-skewed histogram is on the left. The skew is named by the **tail**.
- Reading a histogram of 10 values as if its shape were certain.
- Removing outliers because they are inconvenient. An outlier is a value to look at, not a value to delete.

## What to do next

Open the **Example** tab and press **Open in the Workbench**. Change the 6.2 h repair to 2.0 h and watch the mean, the skewness and the box plot change. Then answer the questions on the **Exercise** tab.
