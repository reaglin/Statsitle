---
id: st.m4.introduction
title: Introduction to discrete distributions
minutes: 30
objectives: [st.m4.o1, st.m4.o5]
example: st.m4.introduction.eg
quiz: quizzes/m4-introduction.json
reference: [probability-mass-function]
---

# Introduction to discrete distributions

## Random variables

Before a shift starts, nobody knows how many trucks will be waiting at the loading dock at 8:00. It could be 0, 1, 2, 3 or 4. A quantity whose value is decided by chance is a **random variable**. It is written with a capital letter, X, and a value it takes with a small letter, x.

- "X = the number of trucks waiting at 8:00" is the random variable.
- "X = 2" is an event: two trucks are waiting.
- P(X = 2) is the probability of that event.

A random variable turns the outcomes of an experiment into numbers, so we can calculate with them: an average, a spread, a chance of exceeding a limit.

## Discrete and continuous

| Kind | Values it can take | Examples |
|---|---|---|
| **Discrete** | Separate values you can list, usually counts 0, 1, 2, … | defective boards in a batch, calls per hour, cracks per meter of weld, tests until the first failure |
| **Continuous** | Any value in an interval | the strength of a cylinder, the time to repair a machine, the diameter of a shaft |

A quick test: if you **count** it, it is discrete; if you **measure** it, it is continuous. This module is about discrete random variables. Module 5 covers continuous ones.

## The probability distribution

The **probability distribution** of a discrete random variable lists every value it can take with its probability. Written as a function, P(X = x) = f(x), it is called the **probability mass function (PMF)**. Item 4.2 covers its rules. For now, two things are true of every PMF:

1. No probability is negative: f(x) ≥ 0.
2. The probabilities add to 1: Σ f(x) = 1. Something always happens.

## A worked case by hand

A pump station has two pumps. Each one is working at any moment with probability 0.9, independently of the other. Let X be the number of pumps working. X can be 0, 1 or 2.

1. **X = 0:** both are down. P = 0.1 × 0.1 = **0.01**.
2. **X = 2:** both are working. P = 0.9 × 0.9 = **0.81**.
3. **X = 1:** one working and one down. That can happen two ways (pump A up and B down, or A down and B up), each with probability 0.9 × 0.1 = 0.09. P = 2 × 0.09 = **0.18**.

| x | 0 | 1 | 2 |
|---|---|---|---|
| P(X = x) | 0.01 | 0.18 | 0.81 |

Check: 0.01 + 0.18 + 0.81 = 1. The station has at least one pump working with probability 0.18 + 0.81 = 0.99. This is a binomial distribution with n = 2 and p = 0.9, which item 4.4 makes general.

## The cumulative distribution function

The **cumulative distribution function (CDF)** adds the PMF up to a value:

F(x) = P(X ≤ x) = the sum of f(t) for every t ≤ x

For the pumps: F(0) = 0.01, F(1) = 0.01 + 0.18 = 0.19, F(2) = 1. The CDF answers "at most" questions directly: the chance that at most one pump is working is F(1) = 0.19.

## The Example: trucks at a loading dock

A distribution center counted the trucks waiting at 8:00 on many mornings and turned the counts into probabilities:

[[data: st.m4.introduction.eg]]

The **bar chart of the PMF** has one bar per value. Its height is the probability. The dark bar is the value asked about, k = 2:

[[figure: st.m4.introduction.eg 1]]

The **CDF** is a staircase. It steps up at each value by that value's probability and reaches 1 at the largest value:

[[figure: st.m4.introduction.eg 2]]

And the numbers the program calculates:

[[results: st.m4.introduction.eg]]

Two trucks is the most likely count, P(X = 2) = 0.35. The chance of at most two is P(X ≤ 2) = 0.10 + 0.25 + 0.35 = 0.70, so on about 30 % of mornings three or more trucks are waiting. The mean, 1.95 trucks, is the long-run average count. Item 4.3 explains it.

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([0, 1, 2, 3, 4])
p = np.array([0.10, 0.25, 0.35, 0.20, 0.10])
X = stats.rv_discrete(values=(x, p))
print(X.pmf(2), X.cdf(2))   # P(X = 2) = 0.35, P(X ≤ 2) = 0.70
```

```r
x <- c(0, 1, 2, 3, 4)
p <- c(0.10, 0.25, 0.35, 0.20, 0.10)
sum(p[x == 2]); sum(p[x <= 2])   # 0.35, then 0.70
barplot(p, names.arg = x)        # the PMF as bars
```

The Example's full script, which prints every result with its label:

[[code: st.m4.introduction.eg]]

## Common mistakes

- Treating a measured quantity (a time, a length) as discrete because it was rounded. A repair time of "2 hours" is still continuous.
- Forgetting a value. If the table leaves out X = 0, the probabilities will not add to 1.
- Mixing up the random variable and its value: X is "the number of trucks", x = 2 is one number it can be.

## What to do next

Look at the **Example** tab, and press **Open in the Workbench** to change a probability (keep the total at 1). Then answer the questions on the **Exercise** tab.
