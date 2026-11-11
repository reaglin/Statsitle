---
id: st.m6.parameters-statistics
title: Population parameters vs sample statistics
minutes: 35
objectives: [st.m6.o1]
builds-on: st.m6.introduction
example: st.m6.parameters-statistics.eg
quiz: quizzes/m6-parameters.json
reference: [parameters-and-statistics, mean-median-mode, variance-standard-deviation]
---

# Population parameters vs sample statistics

## Two kinds of number

- A **parameter** describes the whole **population**: every resistor the line makes, every bridge deck in the state. It is one fixed number, but usually unknown, because the whole population is seldom measured.
- A **statistic** describes a **sample** taken from the population. It is calculated from the data, so it is known, but it changes from sample to sample.

A statistic is used to **estimate** a parameter. Greek letters are kept for parameters and Latin letters (or a hat) for statistics:

| What it measures | Parameter (population) | Statistic (sample) |
|---|---|---|
| Mean | μ | x̄ |
| Standard deviation | σ | s |
| Variance | σ² | s² |
| Proportion | p | p̂ ("p-hat") |
| Size | N | n |

A quick test: **could you work it out from the data you have?** If yes, it is a statistic. If it needs every member of the population, it is a parameter.

## Sampling error

The difference between a statistic and the parameter it estimates is the **sampling error**:

sampling error = x̄ − μ

It is not a mistake. It comes from chance alone: which members happened to be in the sample. A bigger sample makes it smaller on average (item 6.4), but it never goes away. Mistakes, such as a miscalibrated gauge or a sample taken only from the morning shift, are a different thing: they are **bias**, and a bigger sample does not cure them.

## A worked case by hand

A state has 2400 bridges on its inspection list. Records of every bridge give a mean deck age of 41.0 years. An engineer picks 30 bridges at random, visits them, and finds a mean deck age of 43.5 years, with 8 of the 30 needing deck repair.

1. **41.0 years** comes from every bridge in the population: a parameter, μ = 41.0.
2. **43.5 years** comes from the 30 bridges visited: a statistic, x̄ = 43.5.
3. **8 of 30** is a sample proportion: p̂ = 8/30 = 0.267. The proportion of all 2400 bridges needing repair, p, is a parameter, and it is unknown.
4. **Sampling error of the mean:** x̄ − μ = 43.5 − 41.0 = **2.5 years**. Another 30 bridges would give another x̄ and another error.

## The Example: one sample of ten resistors

The resistor line of item 6.1 has μ = 100 Ω and σ = 2 Ω. These are parameters; here they are known because the line has a long record. An inspector measured one sample of 10:

[[data: st.m6.parameters-statistics.eg]]

[[results: st.m6.parameters-statistics.eg]]

- The **mean** x̄ = 100.16 Ω is a statistic. It estimates μ = 100 Ω, with a sampling error of 100.16 − 100 = 0.16 Ω.
- The **standard deviation** s = 1.3583 Ω is a statistic. It estimates σ = 2 Ω. With only 10 values, s can easily be this far from σ.
- The **standard error of the mean**, s/√n = 1.3583/√10 = 0.4295 Ω, estimates σ/√n = 0.6325 Ω, the spread of x̄ in item 6.1. When σ is unknown, which is usual, s/√n is what an engineer uses (Module 7).

[[figure: st.m6.parameters-statistics.eg 1]]

## In Python and R

```python
import numpy as np
x = np.array([101.2, 98.7, 100.4, 99.1, 102.3, 100.8, 97.9, 100.1, 99.6, 101.5])
mu = 100                              # the parameter, known from the line's record
xbar = x.mean()                       # the statistic, 100.16
print(xbar, x.std(ddof=1))            # x̄ and s
print(xbar - mu)                      # sampling error, 0.16
print(x.std(ddof=1) / np.sqrt(len(x)))  # s/√n, 0.4295
```

```r
x <- c(101.2, 98.7, 100.4, 99.1, 102.3, 100.8, 97.9, 100.1, 99.6, 101.5)
mu <- 100
mean(x); sd(x)                        # x̄ and s
mean(x) - mu                          # sampling error
sd(x) / sqrt(length(x))               # s/√n
```

`ddof=1` in Python and `sd()` in R divide by n − 1, as a sample standard deviation should (Module 2).

[[code: st.m6.parameters-statistics.eg]]

## Common mistakes

- Calling a sample mean "the mean" in a report as if it were μ. Say "the sample mean of 10 resistors was 100.16 Ω".
- Thinking a parameter changes from sample to sample. It is fixed; only the statistic moves.
- Treating sampling error as a mistake to be fixed. It is chance, and it is measured by the standard error.

## What to do next

The **Exercise** tab has five questions: tell parameters from statistics, and work out a sampling error.
