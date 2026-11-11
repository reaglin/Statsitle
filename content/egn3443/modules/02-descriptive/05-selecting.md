---
id: st.m2.selecting
title: Selecting and applying the right measures
minutes: 40
objectives: [st.m2.o4, st.m2.o5]
builds-on: st.m2.distribution
example: st.m2.selecting.eg
quiz: quizzes/m2-selecting.json
reference: [choosing-measures, mean-median-mode, quartiles-and-outliers]
---

# Selecting and applying the right measures

The program calculates every measure for any column of numbers. It cannot tell which ones mean something for your data, or which ones your reader needs. That choice is yours, and this item is about making it.

## Start with the type of data

| Type | What the values are | Examples | Center | Spread |
|---|---|---|---|---|
| Nominal (categorical) | Names, with no order | failure cause, material supplier, defect type | mode | (counts or percentages of each category) |
| Ordinal | Categories with an order, but unequal steps | condition rating 1 to 5, survey answers, hardness grade | median (or mode) | range of categories, IQR |
| Interval or ratio (numerical) | Measured or counted numbers with equal steps | length, strength, flow, time, temperature, counts | mean or median | standard deviation or IQR |

Codes are not numbers. If supplier A is entered as 1 and supplier B as 2, the "mean supplier" of 1.4 means nothing, even though the program will calculate it.

Numerical data can be **continuous** (any value in a range: 50.13 mm) or **discrete** (counts: 3 rejected parts). Both are summarized the same way in this module.

## Choose the center

1. Categorical data: the **mode**.
2. Ordinal data: the **median**.
3. Numerical data: look at the histogram and compare the mean and the median.
   - Roughly symmetric, no extreme outliers: the **mean**.
   - Skewed, or with outliers you have checked and kept: the **median**.
   - When a total matters (energy used, volume pumped, cost), the **mean**, even for skewed data, because the total is n × x̄.

## Choose the spread to go with it

- With the mean, report the **standard deviation**.
- With the median, report the **IQR** (or the quartiles Q1 and Q3).
- Add the **range** (or the minimum and maximum) when the extremes matter: the lowest strength in a batch, the longest outage.
- Use the **coefficient of variation** to compare spread between things measured on different scales.

## Consider the context and the audience

The same data can need different summaries for different readers.

- **A manager or a client** wants a typical value and how often things go wrong, in plain words and units: "a typical repair takes about 1.5 hours; one repair in ten takes more than 4 hours".
- **An engineer** checking a design wants the numbers behind a calculation: the mean and the standard deviation, n, and whether the data are skewed.
- **A specification or a standard** may name the statistic for you. Concrete acceptance, for example, is written in terms of averages of tests and individual low results, so report what the specification asks for.

## Apply it to a real dataset

The Example tab has twenty response times of a plant's sensor network, in milliseconds (ms):

[[data: st.m2.selecting.eg]]

[[figure: st.m2.selecting.eg 2]]

[[results: st.m2.selecting.eg]]

Work through the choice:

1. **Type:** numerical (times).
2. **Shape:** the box plot shows two outliers, 410 ms and 1240 ms. The mean (252.7 ms) is far above the median (188.5 ms), and the skewness is 4.16. Strongly right-skewed.
3. **Center:** the median, 188.5 ms. The mean is larger than eighteen of the twenty replies, so it is not typical.
4. **Spread:** the IQR, 24.25 ms (Q1 = 178.5 ms, Q3 = 202.75 ms). The standard deviation, 238 ms, is larger than most of the replies themselves, which is a sign it is the wrong measure here.
5. **Unusual values:** report the two slow replies separately. They may be the most important finding: a control system that waits 1.2 s for a sensor can miss an alarm.

## Communicate it clearly

A good statement of descriptive statistics:

- names the **quantity and its unit**;
- gives **n**;
- gives a **center and a spread that match** each other and the shape;
- rounds sensibly: one or two more digits than the data were measured to, not ten;
- says what is **unusual**, and what it means for the reader.

For the response times, written for the plant's operations manager:

*"Over 20 polls, the median response time was 189 ms, and the middle half of replies took between 179 and 203 ms. Two replies were much slower, 410 ms and 1240 ms, probably when the network was busy. These slow replies, not the typical one, should decide the alarm timeout."*

The same data written for an engineer would add n = 20, Q1, Q3, the skewness of 4.2 and the outlier fences (142 and 239 ms), and would say that the mean and standard deviation were not used because of the two outliers.

## In Python and R

```python
import numpy as np
x = np.array([182, 175, 210, 168, 195, 1240, 188, 172, 205, 179,
              191, 233, 186, 170, 198, 410, 184, 177, 202, 189])
q1, med, q3 = np.percentile(x, [25, 50, 75])
iqr = q3 - q1
print(med, q1, q3, iqr)                          # 188.5 178.5 202.75 24.25
print(x[(x < q1 - 1.5*iqr) | (x > q3 + 1.5*iqr)])  # outliers: [1240  410]
```

```r
x <- c(182, 175, 210, 168, 195, 1240, 188, 172, 205, 179,
       191, 233, 186, 170, 198, 410, 184, 177, 202, 189)
quantile(x, c(0.25, 0.5, 0.75))   # 178.5 188.5 202.75 (type 7, R's default)
IQR(x)                            # 24.25
boxplot.stats(x)$out              # 1240 410 (uses Tukey's hinges, so the fences can differ slightly)
```

## Common mistakes

- Reporting the mean and standard deviation of skewed data because they are the first numbers on the screen.
- Pairing the median with the standard deviation, or the mean with the IQR.
- Calculating a mean of codes for categories.
- Giving more digits than the data support: 252.7000 ms from times measured to 1 ms.
- Dropping outliers from the summary without saying so.

## What to do next

Open the **Example** tab and press **Open in the Workbench**. Delete the two slow replies and compare the mean with the median again. Then answer the scenario questions on the **Exercise** tab.
