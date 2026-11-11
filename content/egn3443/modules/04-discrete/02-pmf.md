---
id: st.m4.pmf
title: Probability mass functions
minutes: 40
objectives: [st.m4.o1, st.m4.o5]
builds-on: st.m4.introduction
example: st.m4.pmf.eg
quiz: quizzes/m4-pmf.json
reference: [probability-mass-function]
---

# Probability mass functions

## What a PMF is

The **probability mass function** of a discrete random variable X gives the probability of each value:

f(x) = P(X = x)

It can be a table (as in the Example), a formula (as for the binomial and Poisson in items 4.4 and 4.5), or a bar chart. All three say the same thing.

## The two conditions for a valid PMF

A list of numbers is a probability distribution only if:

1. **Non-negative:** f(x) ≥ 0 for every x. (And so no f(x) is above 1.)
2. **Sums to one:** Σ f(x) = 1, over every value X can take.

Check both every time you build or are given a distribution. A table that adds to 0.95 has left out an outcome, or one of its numbers is wrong. A negative "probability" is always an error.

The Workbench checks these for a typed table: if the probabilities are negative or do not add to 1, it says so and calculates nothing.

## Constructing a distribution

Sometimes all the probabilities but one are known. The last one is whatever makes the total 1:

| x | 0 | 1 | 2 | 3 |
|---|---|---|---|---|
| f(x) | 0.15 | 0.35 | ? | 0.10 |

f(2) = 1 − (0.15 + 0.35 + 0.10) = 1 − 0.60 = **0.40**.

Sometimes the PMF is a formula with a constant to find. Suppose f(x) = c·x for x = 1, 2, 3, 4. The sum must be 1:

c·1 + c·2 + c·3 + c·4 = 10c = 1, so c = **0.1**, and f(1) = 0.1, f(2) = 0.2, f(3) = 0.3, f(4) = 0.4.

## Probabilities of events from the PMF

Every question about a discrete X is answered by adding the right bars. Read the words carefully, because with whole numbers "less than" and "at most" are different:

| Words | Symbol | How to calculate |
|---|---|---|
| exactly k | P(X = k) | f(k) |
| at most k, no more than k | P(X ≤ k) | f(smallest) + … + f(k) = F(k) |
| fewer than k, less than k | P(X < k) | P(X ≤ k − 1) = F(k − 1) |
| at least k, k or more | P(X ≥ k) | 1 − P(X ≤ k − 1) |
| more than k | P(X > k) | 1 − P(X ≤ k) |
| from k to k₂ inclusive | P(k ≤ X ≤ k₂) | F(k₂) − F(k − 1) |

The **complement rule**, P(X ≥ k) = 1 − P(X ≤ k − 1), saves work when the other side has fewer bars, and is the only way when X has no upper limit (Poisson).

## A worked case by hand

The number of defects X on a circuit board has this PMF (the Example's data):

| x | 0 | 1 | 2 | 3 | 4 | 5 |
|---|---|---|---|---|---|---|
| f(x) | 0.40 | 0.30 | 0.15 | 0.08 | 0.05 | 0.02 |

1. **Valid?** Every value is positive, and 0.40 + 0.30 + 0.15 + 0.08 + 0.05 + 0.02 = 1.00. Yes.
2. **At least one defect:** P(X ≥ 1) = 1 − P(X = 0) = 1 − 0.40 = **0.60**.
3. **More than one defect:** P(X > 1) = 1 − P(X ≤ 1) = 1 − (0.40 + 0.30) = **0.30**.
4. **From one to three defects:** P(1 ≤ X ≤ 3) = 0.30 + 0.15 + 0.08 = **0.53**.
5. **The CDF:** F(0) = 0.40, F(1) = 0.70, F(2) = 0.85, F(3) = 0.93, F(4) = 0.98, F(5) = 1.00.

Going back from the CDF to the PMF: each probability is a step of the staircase, f(x) = F(x) − F(x − 1). For example f(2) = 0.85 − 0.70 = 0.15.

## The Example

[[data: st.m4.pmf.eg]]

The Example sets k = 1 and k₂ = 3, so the bars from 1 to 3 are dark: their total is P(1 ≤ X ≤ 3) = 0.53.

[[figure: st.m4.pmf.eg 1]]

[[figure: st.m4.pmf.eg 2]]

[[results: st.m4.pmf.eg]]

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([0, 1, 2, 3, 4, 5])
p = np.array([0.40, 0.30, 0.15, 0.08, 0.05, 0.02])
print(p.min() >= 0, np.isclose(p.sum(), 1))   # True True: a valid PMF
X = stats.rv_discrete(values=(x, p))
print(1 - X.cdf(0))          # P(X ≥ 1) = 0.60
print(X.cdf(3) - X.cdf(0))   # P(1 ≤ X ≤ 3) = 0.53
```

```r
x <- 0:5
p <- c(0.40, 0.30, 0.15, 0.08, 0.05, 0.02)
all(p >= 0); isTRUE(all.equal(sum(p), 1))   # TRUE TRUE
1 - sum(p[x <= 0])             # P(X ≥ 1) = 0.60
sum(p[x >= 1 & x <= 3])        # P(1 ≤ X ≤ 3) = 0.53
cumsum(p)                      # the CDF: 0.40 0.70 0.85 0.93 0.98 1.00
```

`rv_discrete` refuses a table whose probabilities do not add to 1. Comparing a sum of decimals with 1 needs `np.isclose` or `all.equal`, because 0.1 + 0.2 is not exactly 0.3 in a computer.

[[code: st.m4.pmf.eg]]

## Common mistakes

- Reading "fewer than 3" as P(X ≤ 3). It is P(X ≤ 2).
- For "at least 1", adding every bar from 1 up instead of using 1 − P(X = 0). Both are right, but the long way invites slips, and with no upper limit it is impossible.
- Accepting a table without checking that it adds to 1.
- Treating the CDF value F(x) as the probability of exactly x.

## What to do next

Answer the questions on the **Exercise** tab. They ask you to check tables, complete one, and find probabilities of events.
