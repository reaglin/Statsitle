---
id: st.m5.introduction
title: Introduction to continuous distributions
minutes: 40
objectives: [st.m5.o1]
example: st.m5.introduction.eg
quiz: quizzes/m5-introduction.json
reference: [probability-density-function]
---

# Introduction to continuous distributions

## Counting and measuring

Module 4's random variables were **counts**: 0, 1, 2, … defects. You could list every value and give each one a probability.

A **continuous** random variable is **measured**: the time a board takes to heat up, the diameter of a shaft, the strength of a weld. It can take any value in an interval, such as 89.7 s, 89.73 s or 89.7314 s. There are too many values to list, so the distribution cannot be a table of probabilities.

| | Discrete (Module 4) | Continuous (this module) |
|---|---|---|
| Values | separate values you can list: 0, 1, 2, … | any value in an interval |
| Described by | the PMF, f(x) = P(X = x), drawn as bars | the **density** f(x), drawn as a curve |
| A probability is | the height of a bar, or a sum of bars | an **area** under the curve |
| P(X = one exact value) | can be large | **0** |

A rounded measurement is still continuous. A repair time recorded as "2 hours" was really 1.9 or 2.07 hours.

## The probability density function

The **probability density function (PDF)** f(x) of a continuous random variable is a curve with two properties:

1. It is never negative: f(x) ≥ 0.
2. The total area under it is 1. Something always happens.

The probability that X falls in an interval is the **area under the curve** over that interval:

P(a ≤ X ≤ b) = the area under f(x) from a to b (in calculus, the integral of f(x) from a to b)

## Why a single value has probability zero

The area over a single point is a line with no width, so its area is 0: **P(X = x) = 0** for every x. A shaft is never exactly 25.000000… mm; only intervals have probability. Two things follow:

- P(X ≤ x) and P(X < x) are the **same number** for a continuous variable. The "≤ or <" question of Module 4 does not matter here.
- The **height** f(x) is **not a probability**. It is a probability per unit of x (per second, per mm), so it can be larger than 1 when the values are packed into a narrow range. Item 5.5 comes back to this.

## The cumulative distribution function

The **cumulative distribution function (CDF)** is the area to the left of x:

F(x) = P(X ≤ x)

It rises from 0 to 1 as x moves from the smallest value to the largest. It answers every probability question by subtraction:

| Question | From the CDF |
|---|---|
| P(X ≤ x), "at most x", "below x" | F(x) |
| P(X > x), "more than x", "above x" | 1 − F(x) |
| P(a ≤ X ≤ b), "between a and b" | F(b) − F(a) |

## A worked case by hand

A maintenance task takes X hours, with density f(x) = x/2 for 0 ≤ x ≤ 2 (a ramp: long times are more likely than short ones) and 0 elsewhere.

1. **Is it a valid density?** f(x) is never negative. The area under it is a triangle with base 2 and height f(2) = 1: ½ × 2 × 1 = **1**. Yes.
2. **The CDF:** the area to the left of x is a smaller triangle with base x and height x/2, so F(x) = ½ × x × x/2 = **x²/4**.
3. **At most 1 hour:** P(X ≤ 1) = F(1) = 1/4 = **0.25**.
4. **More than 1 hour:** P(X > 1) = 1 − 0.25 = **0.75**.
5. **Between 0.5 and 1.5 hours:** F(1.5) − F(0.5) = 2.25/4 − 0.25/4 = **0.50**.
6. **Exactly 1 hour:** P(X = 1) = **0**, although f(1) = 0.5. The height is not the probability.

The **mean** of a continuous X is its balance point, E[X] = the integral of x·f(x), the continuous version of Module 4's Σ x·f(x). For this ramp it is 4/3 = **1.33 hours**. The variance is defined the same way. You will not need to integrate in this module: each distribution has a formula for its mean and variance.

## The Example: time to reach reflow temperature

In a reflow oven, the time for a circuit board to reach soldering temperature is continuous. Over many boards it follows a bell-shaped density with mean 90 s and standard deviation 6 s (the **normal** distribution, item 5.3). The shaded area is the probability that a board takes between 84 and 96 s:

[[figure: st.m5.introduction.eg 1]]

The **CDF** is a smooth S-shaped curve. At 84 s it has climbed to 0.1587: about 16 % of boards take 84 s or less.

[[figure: st.m5.introduction.eg 2]]

[[results: st.m5.introduction.eg]]

The area between 84 and 96 s is F(96) − F(84) = 0.8413 − 0.1587 = **0.6827**: about 68 % of boards take between 84 and 96 s. The density at 84 s, 0.0403 per second, is a height, not a probability.

## In Python and R

```python
from scipy import stats
X = stats.norm(90, 6)         # a continuous random variable
print(X.pdf(84))              # density f(84) = 0.0403, a height
print(X.cdf(84))              # F(84) = P(X ≤ 84) = 0.1587
print(X.cdf(96) - X.cdf(84))  # P(84 ≤ X ≤ 96) = 0.6827
```

```r
dnorm(84, 90, 6)                      # density f(84) = 0.0403
pnorm(84, 90, 6)                      # F(84) = 0.1587
pnorm(96, 90, 6) - pnorm(84, 90, 6)   # 0.6827
```

The pattern is the same for every family: `pdf` / `d…` is the density, `cdf` / `p…` the area to the left, `ppf` / `q…` the value for a given area (the quantile).

[[code: st.m5.introduction.eg]]

## Common mistakes

- Reading the density f(x) as a probability. Only areas are probabilities.
- Asking for P(X = x) of a continuous variable. It is 0; ask for an interval.
- Using the discrete "≤ or <" rules of Module 4. For a continuous X, P(X < x) = P(X ≤ x).
- Treating a measured quantity as discrete because it was rounded.

## What to do next

Look at the **Example** tab and press **Open in the Workbench**. Change the **Upper value (x₂)** and watch the shaded area change. Then answer the questions on the **Exercise** tab.
