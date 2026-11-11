---
id: st.m4.binomial
title: The binomial distribution
minutes: 45
objectives: [st.m4.o3, st.m4.o4, st.m4.o5]
builds-on: st.m4.expected-value
example: st.m4.binomial.eg
exercise: st.m4.binomial.ex
reference: [binomial-distribution, which-distribution]
---

# The binomial distribution

## When it applies

The binomial distribution counts **successes in a fixed number of trials**. A "success" is just the outcome being counted. It is often a bad one: a defective part, a failed test, a leak. Four conditions must all hold (remember them as **BINS**):

1. **Binary:** each trial has two outcomes, success or failure.
2. **Independent:** the outcome of one trial does not change the others.
3. **Number fixed:** the number of trials n is set before you start.
4. **Same p:** every trial has the same probability of success p.

Then X, the number of successes in n trials, is binomial: X ~ Binomial(n, p). X can be 0, 1, …, n.

A single trial (n = 1) is a **Bernoulli trial**: X is 1 with probability p and 0 with probability 1 − p. A binomial X is the sum of n independent Bernoulli trials.

## The formula

P(X = x) = C(n, x) · pˣ · (1 − p)ⁿ⁻ˣ, for x = 0, 1, …, n

- pˣ(1 − p)ⁿ⁻ˣ is the probability of one particular order of x successes and n − x failures.
- C(n, x) = n! / (x!(n − x)!), "n choose x", counts how many orders there are.

The mean and variance have short formulas:

μ = np, σ² = np(1 − p), σ = √(np(1 − p))

## A worked case by hand

A supplier's O-rings are defective with probability p = 0.1, independently. You inspect n = 5. Let X be the number defective.

1. **None defective:** P(X = 0) = C(5, 0) × 0.1⁰ × 0.9⁵ = 1 × 1 × 0.59049 = **0.5905**.
2. **Exactly one:** P(X = 1) = C(5, 1) × 0.1¹ × 0.9⁴ = 5 × 0.1 × 0.6561 = **0.3281**.
3. **At most one:** P(X ≤ 1) = 0.59049 + 0.32805 = **0.9185**.
4. **Two or more:** P(X ≥ 2) = 1 − 0.91854 = **0.0815**.
5. **Mean and spread:** μ = 5 × 0.1 = **0.5** defective, σ² = 5 × 0.1 × 0.9 = 0.45, σ = **0.671**.

So an inspection of five O-rings finds two or more defective only about 8 % of the time, when the supplier's rate really is 10 %.

## The shape

The binomial is symmetric when p = 0.5. With a small p, as in most defect counts, the bars pile up near 0 and the distribution is skewed to the right. With a large p it is skewed to the left. As n grows, it looks more and more like a bell curve (Module 5).

## The Example: sensors that need recalibration

A vendor's pressure sensors need recalibration on arrival 20 % of the time. A shipment has 10 sensors, and X is the number that need it.

[[figure: st.m4.binomial.eg 1]]

[[results: st.m4.binomial.eg]]

The most likely count is 2, with P(X = 2) = 0.3020, and the mean is np = 10 × 0.2 = 2 sensors. The chance that at most 2 need recalibration is 0.6778, so about one shipment in three has three or more.

[[figure: st.m4.binomial.eg 2]]

## In Python and R

```python
from scipy import stats
n, p = 10, 0.2
X = stats.binom(n, p)
print(X.pmf(2))       # P(X = 2) = 0.3020
print(X.cdf(2))       # P(X ≤ 2) = 0.6778
print(X.sf(2))        # P(X > 2) = 1 − P(X ≤ 2) = 0.3222
print(X.mean(), X.var())   # mean 2, variance 1.6
```

```r
n <- 10; p <- 0.2
dbinom(2, n, p)       # P(X = 2) = 0.3020
pbinom(2, n, p)       # P(X ≤ 2) = 0.6778
1 - pbinom(2, n, p)   # P(X > 2) = 0.3222
n * p; n * p * (1 - p)   # 2 and 1.6
```

`pmf` and `dbinom` give "exactly"; `cdf` and `pbinom` give "at most". For "at least k", use `X.sf(k - 1)` or `1 - pbinom(k - 1, n, p)`.

[[code: st.m4.binomial.eg]]

## Common mistakes

- Leaving out C(n, x). Without it you have the probability of one particular order only.
- Using the binomial without a fixed n: "test chips until one fails" has no fixed number of trials (item 4.6, geometric).
- Using it when p changes from trial to trial, such as drawing from a small lot without putting parts back (item 4.6, hypergeometric).
- Reading "at least 2" as 1 − P(X ≤ 2). It is 1 − P(X ≤ 1).

## What to do next

On the **Exercise** tab, fifteen sensors are installed and each can fail in its first year. Find the probabilities asked, the mean and the standard deviation, and press **Check my answers**.
