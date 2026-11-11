---
id: st.m1.introduction
title: Introduction to statistics in engineering
minutes: 30
objectives: [st.m1.o1, st.m1.o5]
quiz: quizzes/m1-introduction.json
reference: [engineering-statistics, descriptive-statistics]
---

# Introduction to statistics in engineering

## Why statistics matters in engineering

An engineer designs a beam for a steel whose yield strength is "350 MPa". But the steel delivered to the site is not one number. Every bar is slightly different. Some are 362 MPa, some 371, now and then one is 349. The design has to work for the bars that will actually arrive, including the weak ones.

That is the problem statistics solves. **Statistics is the science of collecting, describing and drawing conclusions from data that vary.** Engineers use it to:

- decide whether a batch of material meets its specification, from tests on a few pieces;
- tell whether a process has changed, or only looks different by chance;
- predict how often a part will fail, or a flood will reach a given level;
- compare two designs, suppliers or methods fairly;
- set tolerances and safety factors.

## Data-driven decisions

Without statistics, decisions are made on the last measurement, or on whoever argues hardest. With statistics, a decision rests on all the data, and says how sure it is.

Suppose a concrete supplier's last cylinder test came in at 28 MPa, below the 30 MPa design strength. Is the concrete bad? One low result is not enough to say. The question becomes statistical: across all the tests, what is the typical strength, how much do the results vary, and how often does a test fall below 30 MPa? Module 2 gives the numbers that answer the first two questions, and Modules 4 and 5 the third.

## The vocabulary

| Term | Meaning | Engineering example |
|---|---|---|
| **Population** | Every item you want to know about | all the concrete poured on a bridge deck |
| **Sample** | The items you actually measured | the 20 cylinders cast and tested |
| **Observation** | One measured item, or one measurement | one cylinder's strength, 31.2 MPa |
| **Variable** | The quantity that is measured, and varies | compressive strength |
| **Parameter** | A number describing the population, usually unknown; Greek letters | the mean strength of the whole deck, μ |
| **Statistic** | A number calculated from the sample; Latin letters | the mean of the 20 cylinders, x̄ |

A statistic **estimates** a parameter. x̄ = 32.3 MPa from 20 cylinders is our best estimate of μ, but μ is not exactly 32.3 MPa: another 20 cylinders would give a slightly different x̄.

**Types of data.** A variable is either

- **numerical**: measured (continuous: 31.2 MPa, 50.13 mm) or counted (discrete: 3 defects); or
- **categorical**: names or groups, such as the supplier, the defect type or the shift. When the categories have an order (a condition rating of 1 to 5) the data are **ordinal**.

The type decides which statistics make sense. A mean strength is useful; a "mean supplier" is not.

## Two branches of statistics

- **Descriptive statistics** summarize the data you have: the mean, the standard deviation, a histogram. Items 1.2 and 1.3, and all of Module 2.
- **Inferential statistics** draw conclusions about the population from the sample, and say how sure they are: confidence intervals and hypothesis tests. Item 1.5 previews them; Modules 7 to 10 teach them.

**Probability** connects the two. It describes how samples behave when we know the population; inference turns that round, to learn about the population from a sample. Item 1.4 previews the probability models engineers use most.

## Understanding variability

**Variability** is the spread of values you get when you measure the same kind of thing more than once. It is not a mistake to be removed; it is a property of every real process. It comes from many sources:

| Source | Example |
|---|---|
| Material | two coils of steel from the same mill differ slightly |
| Machine | a cutting tool wears, a spindle runs a little warm |
| Method | two operators set up the same job in different ways |
| Measurement | a caliper read twice gives two values |
| Environment | temperature and humidity change through the day |

Engineers care about variability as much as about the average. A process whose mean is on target but which varies widely makes many parts out of tolerance. Item 1.3 measures variability, and item 1.6 shows how quality control watches it.

## Statistical thinking

Statistical thinking is an engineering habit with three parts:

1. All work happens in a **process**, and every process varies.
2. **Understanding and reducing variability** is the key to quality.
3. **Decisions use data**, and say how sure they are.

When you see a number in this course, ask: what was measured, on how many items, how much would it change with another sample, and what decision does it support?

## In Python and R

Data are stored as a list of numbers. The rest of the course builds on these two lines:

```python
import numpy as np
strength = np.array([31.2, 33.8, 29.6, 32.4, 34.1])   # a sample of five cylinders (MPa)
print(len(strength), np.mean(strength))               # n = 5 and x̄ ≈ 32.22
```

```r
strength <- c(31.2, 33.8, 29.6, 32.4, 34.1)   # a sample of five cylinders (MPa)
length(strength); mean(strength)              # n = 5 and x̄ = 32.22
```

## Common mistakes

- Treating a sample statistic as if it were the population parameter. x̄ estimates μ; it is not μ.
- Judging a process on one measurement.
- Treating codes for categories (supplier 1, 2, 3) as numbers.
- Thinking variability means someone made an error. Variability is part of every process; the question is how much, and from where.

## What to do next

There is no Example for this item. Answer the vocabulary questions on the **Exercise** tab, then press **Next ▸** for descriptive statistics.
