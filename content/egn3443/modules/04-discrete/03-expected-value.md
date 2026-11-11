---
id: st.m4.expected-value
title: Expected value and variance
minutes: 45
objectives: [st.m4.o2, st.m4.o5]
builds-on: st.m4.pmf
example: st.m4.expected-value.eg
exercise: st.m4.expected-value.ex
reference: [expected-value-variance, probability-mass-function]
---

# Expected value and variance

A PMF lists every value and its probability. Like a dataset in Module 2, it has a center and a spread. For a distribution they are called the **expected value** (or mean) μ and the **variance** σ².

## The expected value: the long-run average

μ = E[X] = Σ x·f(x)

Multiply each value by its probability and add. The result is the **balance point** of the bar chart, and the average you would get over many, many repetitions. It does not have to be a value X can take: a mean of 1.8 service calls a day is fine, though no day has 1.8 calls.

This is the same idea as the sample mean x̄ in Module 2. There, each value counted once in n; here, each value counts in proportion to its probability.

## The variance and standard deviation

σ² = V[X] = Σ (x − μ)²·f(x)

The variance is the probability-weighted average of the squared distance from the mean. A shortcut that is easier by hand:

σ² = E[X²] − μ², where E[X²] = Σ x²·f(x)

The **standard deviation** σ = √σ² is in the units of X, so it is the number to report: how far a typical outcome is from the mean.

There is no n − 1 here. A PMF describes the whole population of outcomes, so this is the population form of Module 2's variance.

## A worked case by hand

The two-pump station from item 4.1: X is the number of pumps working.

| x | f(x) | x·f(x) | x²·f(x) | (x − μ)²·f(x) |
|---|---|---|---|---|
| 0 | 0.01 | 0 | 0 | (0 − 1.8)² × 0.01 = 0.0324 |
| 1 | 0.18 | 0.18 | 0.18 | (1 − 1.8)² × 0.18 = 0.1152 |
| 2 | 0.81 | 1.62 | 3.24 | (2 − 1.8)² × 0.81 = 0.0324 |
| Sum | 1.00 | 1.80 | 3.42 | 0.1800 |

1. **Mean:** μ = 1.80 pumps.
2. **Variance by the definition:** σ² = 0.0324 + 0.1152 + 0.0324 = **0.18 pumps²**.
3. **Variance by the shortcut:** E[X²] = 3.42, so σ² = 3.42 − 1.80² = 3.42 − 3.24 = **0.18**. The same.
4. **Standard deviation:** σ = √0.18 = **0.424 pumps**.

On average 1.8 of the 2 pumps are working, and the count rarely strays from that.

## Costs and other linear functions

Engineers often need the mean of something that depends on X: a cost, a time, a load. For Y = a·X + b, with constants a and b:

- E[aX + b] = a·E[X] + b
- V[aX + b] = a²·V[X], so σ_Y = |a|·σ_X. Adding a constant b shifts every outcome, so it does not change the spread.

The Example's service calls: each call costs $80 of a technician's time, and the van costs $50 a day whether it goes out or not. The daily cost is Y = 80X + 50, so E[Y] = 80 × 1.80 + 50 = **$194** a day, and σ_Y = 80 × 1.2083 = **$96.66**.

## The Example: service calls per day

A field-service team logged the calls it receives per day:

[[data: st.m4.expected-value.eg]]

[[figure: st.m4.expected-value.eg 1]]

[[results: st.m4.expected-value.eg]]

E[X] = 0 × 0.15 + 1 × 0.30 + 2 × 0.25 + 3 × 0.20 + 4 × 0.10 = 1.80 calls a day. E[X²] = 0.30 + 1.00 + 1.80 + 1.60 = 4.70, so σ² = 4.70 − 1.80² = 1.46 calls² and σ = 1.21 calls. The team should plan for about 1.8 calls a day, and expect most days to fall within about 1.2 calls of that.

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([0, 1, 2, 3, 4])
p = np.array([0.15, 0.30, 0.25, 0.20, 0.10])
mu = np.sum(x * p)                    # 1.80
var = np.sum((x - mu)**2 * p)         # 1.46
print(mu, var, np.sqrt(var))          # σ = 1.2083
X = stats.rv_discrete(values=(x, p))  # or let scipy do it:
print(X.mean(), X.var(), X.std())
```

```r
x <- 0:4
p <- c(0.15, 0.30, 0.25, 0.20, 0.10)
mu <- sum(x * p)              # 1.80
v  <- sum((x - mu)^2 * p)     # 1.46
c(mu, v, sqrt(v))             # σ = 1.2083
```

Do not use `np.var(x)` or `var(x)` here. They treat the five values as a dataset with equal weights and ignore the probabilities.

[[code: st.m4.expected-value.eg]]

## Interpreting the numbers

- The expected value is a **long-run average**, not a prediction for one day. "We expect 1.8 calls" means the average over many days is 1.8.
- Use it to plan totals: over a 20-day month, expect about 20 × 1.8 = 36 calls.
- Use σ for the day-to-day swing. A large σ compared with μ means the workload is unpredictable, and a plan built only on the mean will often be wrong.

## Common mistakes

- Averaging the x values (0, 1, 2, 3, 4 → 2) without weighting by the probabilities.
- Writing σ² = E[X²] − μ but forgetting to square μ.
- Reporting the variance as the spread. Its unit is squared; the standard deviation is in the units of X.
- Dividing by n or n − 1. A PMF's mean and variance are sums weighted by probabilities; there is nothing to divide by.

## What to do next

On the **Exercise** tab, a plant's weekly machine breakdowns have a PMF. Calculate the mean, the variance and the standard deviation, type them in, and press **Check my answers**.
