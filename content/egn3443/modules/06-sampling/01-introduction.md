---
id: st.m6.introduction
title: Introduction to sampling distributions
minutes: 40
objectives: [st.m6.o1, st.m6.o2]
example: st.m6.introduction.eg
quiz: quizzes/m6-introduction.json
reference: [sampling-distribution-of-the-mean, law-of-large-numbers]
---

# Introduction to sampling distributions

## One sample, many possible samples

A line makes thousands of resistors a day. To check them, an inspector measures 10 and works out their mean. That mean, x̄, is one number from one sample. Had she picked 10 other resistors, x̄ would have come out a little different.

So x̄ is a **random variable**: before the sample is taken, its value is uncertain, and it has a distribution of its own. That distribution is called the **sampling distribution** of x̄.

**Sampling distribution:** the distribution of a statistic (such as the sample mean x̄) over all the possible samples of the same size n from the same population.

Why it matters: the population mean μ is what the engineer wants, but x̄ is what she has. The sampling distribution says how far x̄ is likely to be from μ. Every confidence interval (Module 7) and every hypothesis test (Module 8) is built on it.

## Two different spreads

Keep two distributions apart:

| | The population | The sampling distribution of x̄ |
|---|---|---|
| What varies | single values x, one resistor at a time | sample means x̄, one sample of n at a time |
| Centre | μ | μ (the same) |
| Spread | σ | σ/√n, the **standard error** |

The sample mean averages out the high and low values in the sample, so it varies **less** than single values do. Item 6.3 proves the σ/√n; here it is enough to see it happen.

## A worked case by hand

Take a tiny population: the four numbers **2, 4, 6, 8**. Its mean is μ = (2 + 4 + 6 + 8)/4 = **5**, and its standard deviation (dividing by N = 4, since this is the whole population) is

σ = √[((2 − 5)² + (4 − 5)² + (6 − 5)² + (8 − 5)²) / 4] = √(20/4) = √5 = **2.236**.

Now draw a sample of n = 2, with replacement, so each draw can be any of the four. There are 4 × 4 = 16 equally likely samples. Their means are:

| | 2 | 4 | 6 | 8 |
|---|---|---|---|---|
| **2** | 2 | 3 | 4 | 5 |
| **4** | 3 | 4 | 5 | 6 |
| **6** | 4 | 5 | 6 | 7 |
| **8** | 5 | 6 | 7 | 8 |

Count each mean to get the sampling distribution of x̄:

| x̄ | 2 | 3 | 4 | 5 | 6 | 7 | 8 |
|---|---|---|---|---|---|---|---|
| P(x̄) | 1/16 | 2/16 | 3/16 | 4/16 | 3/16 | 2/16 | 1/16 |

1. **Shape:** the population was flat (each value 1/4), but the means pile up in the middle.
2. **Centre:** the mean of x̄ is Σ x̄ · P(x̄) = 80/16 = **5**, the same as μ.
3. **Spread:** the variance of x̄ is Σ (x̄ − 5)² · P(x̄) = (9 + 8 + 3 + 0 + 3 + 8 + 9)/16 = 40/16 = 2.5, so its standard deviation is √2.5 = **1.581**. That is σ/√n = 2.236/√2 = 1.581.

The means are centred where the population is, and they are less spread out by a factor √n.

## The Example: means of ten resistors

With a real population, listing every sample is impossible, but a computer can draw thousands of them. The resistors have μ = 100 Ω and σ = 2 Ω. The computer drew 2000 samples of 10 resistors and plotted the 2000 sample means:

[[figure: st.m6.introduction.eg 1]]

[[results: st.m6.introduction.eg]]

The means centre on 100.02 Ω, close to μ = 100. Their standard deviation is 0.6349 Ω, close to σ/√n = 2/√10 = 0.6325 Ω and less than a third of σ = 2 Ω. Single resistors range over about 100 ± 4 Ω; means of ten stay within about 100 ± 1.3 Ω (95 % of the 2000 means lay within 2 standard errors of μ).

## In Python and R

```python
import numpy as np
rng = np.random.default_rng(1)
samples = rng.normal(100, 2, size=(2000, 10))   # 2000 samples of n = 10
means = samples.mean(axis=1)
print(means.mean(), means.std(ddof=1))           # about 100 and 0.63
print(2 / np.sqrt(10))                           # σ/√n = 0.6325
```

```r
set.seed(1)
means <- replicate(2000, mean(rnorm(10, 100, 2)))
mean(means); sd(means)                            # about 100 and 0.63
2 / sqrt(10)                                      # σ/√n = 0.6325
```

Each program draws its own random numbers, so your means differ a little from the program's. σ/√n is the same everywhere.

[[code: st.m6.introduction.eg]]

## Try it in the Workbench

Press **Open in the Workbench** on the Example tab and change **Sample size (n)** to 40. The histogram narrows to half its width: four times the sample, half the spread (item 6.4).

## Common mistakes

- Mixing up σ (spread of single values) with σ/√n (spread of sample means).
- Thinking a sampling distribution is the distribution of the data in one sample. It is the distribution of the **statistic** over many samples.
- Expecting x̄ to equal μ. It almost never does exactly; the sampling distribution says by how much it typically misses.

## What to do next

The **Exercise** tab has four questions on what a sampling distribution is and what it shows.
