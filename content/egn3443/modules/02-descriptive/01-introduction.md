---
id: st.m2.introduction
title: Introduction to descriptive statistics
minutes: 25
objectives: [st.m2.o1, st.m2.o2, st.m2.o5]
example: st.m2.introduction.eg
quiz: quizzes/m2-introduction.json
reference: [descriptive-statistics]
---

# Introduction to descriptive statistics

## What descriptive statistics are

An engineer who tests twenty concrete cylinders gets twenty numbers. Nobody can read twenty numbers and see what they mean. **Descriptive statistics** summarize them in a few numbers and pictures:

- where the values are centered;
- how far they spread out;
- what shape they make;
- whether any value is unusual.

Descriptive statistics only describe the data you have. They do not, by themselves, say anything about the concrete you did not test. Drawing conclusions about the larger group is **inferential statistics**, which starts in Module 7. Inference always starts from the descriptive numbers you learn here.

## Population and sample

- The **population** is every item you care about: all the concrete poured that day.
- A **sample** is the part you measured: the twenty cylinders.
- A number that describes a population is a **parameter**. It is written with a Greek letter: the population mean μ, the population standard deviation σ. It is usually unknown.
- A number calculated from a sample is a **statistic**: the sample mean x̄, the sample standard deviation s. It is our estimate of the parameter.

In engineering the data are nearly always a sample. That matters in Module 2 because the sample variance divides by n − 1, not n (item 2.3 explains why).

## The four kinds of summary

| Kind | Question it answers | Measures |
|---|---|---|
| Center (central tendency) | What is a typical value? | mean, median, mode |
| Spread (variability) | How much do the values differ? | range, variance, standard deviation, interquartile range |
| Position | Where does a value stand among the others? | minimum, maximum, quartiles |
| Shape | Is the data symmetric, or does it have a long tail? | skewness, the histogram, the box plot |

A good summary reports at least one number of each kind, with units. "The mean strength was 32.3 MPa" is half a summary. "The mean strength was 32.3 MPa with a standard deviation of 1.6 MPa, and eighteen of the twenty cylinders reached the 30 MPa design strength" tells the reader what they need to know.

## A first look at a dataset

Here are the twenty cylinder strengths from the Example tab, in megapascals (MPa):

[[data: st.m2.introduction.eg]]

The **histogram** counts how many values fall in each interval (class) and draws a bar for each. The curve is a normal curve with the same mean and standard deviation, for comparison. These strengths pile up around 32 MPa and fall off evenly on both sides.

[[figure: st.m2.introduction.eg 1]]

The **box plot** shows the same data in five numbers. The box runs from the first quartile Q1 to the third quartile Q3, so it holds the middle half of the data. The line in the box is the median. The whiskers reach to the most extreme values that are not outliers, and an outlier would show as a separate point.

[[figure: st.m2.introduction.eg 2]]

And these are the numbers the program calculates from them:

[[results: st.m2.introduction.eg]]

Do not try to learn all of these numbers now. Items 2.2 to 2.4 explain them one group at a time.

## Descriptive statistics in Python and R

The course uses Python (numpy and scipy) and R. Both can calculate every number above. The Code tab of the Example shows the full script. The core of it is short:

```python
import numpy as np
x = np.array([31.2, 33.8, 29.6, 32.4, 34.1])   # first five values
print(np.mean(x), np.median(x), np.std(x, ddof=1))
```

```r
x <- c(31.2, 33.8, 29.6, 32.4, 34.1)   # first five values
mean(x); median(x); sd(x)
summary(x)   # minimum, quartiles, median, mean, maximum
```

## Common mistakes

- Reporting a number without its unit. A standard deviation of 1.6 means nothing until you say 1.6 MPa.
- Reporting only the center. Two machines can have the same mean and very different spread.
- Treating the sample's numbers as if they were the population's. x̄ is an estimate of μ, not μ itself.

## What to do next

Open the **Example** tab to see the full analysis of the cylinders, and press **Open in the Workbench** to try changing a value. Then answer the questions on the **Exercise** tab.
