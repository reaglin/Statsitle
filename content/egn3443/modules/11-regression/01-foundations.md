---
id: st.m11.foundations
title: Foundations of regression
minutes: 40
objectives: [st.m11.o1, st.m11.o3]
example: st.m11.foundations.eg
exercise: st.m11.foundations.ex
reference: [correlation, simple-linear-regression]
---

# Foundations of regression

## What regression is for

Regression models how one variable changes with another. One variable is the **response**, y: the quantity you want to predict or explain, such as concrete strength, fuel use or pavement condition. The other is the **predictor**, x: a quantity you set or can measure first, such as the water–cement ratio, the payload or the pavement's age. (Other names: dependent and independent variable; outcome and explanatory variable.)

Engineers use regression to:

- **predict**: the strength of a mix before it is cast, the timber in a tree before it is cut;
- **quantify an effect**: how many MPa each extra 0.01 of water–cement ratio costs;
- **calibrate**: turn a sensor's millivolts into a temperature;
- **decide**: when a road needs resurfacing, from how fast its condition falls.

A physical law such as Hooke's law (F = kx) is **deterministic**: the same x always gives the same y. Measured data are not like that. Two batches with the same water–cement ratio differ in strength because of the cement, the curing, the test. Regression therefore models y as a straight line **plus random error**:

Y = β₀ + β₁x + ε

β₀ (the **intercept**) and β₁ (the **slope**) are the population's true values, which we never know. ε is the random error, assumed to have mean 0 and the same standard deviation σ at every x. From the data we estimate them by b₀ and b₁ and write the **fitted line** ŷ = b₀ + b₁x. The hat (ˆ) on y marks a value predicted by the line, not a measured one. Item 11.2 shows how b₀ and b₁ are found.

This module is about **one** predictor: **simple** linear regression. When the response depends on several predictors at once (strength on the water–cement ratio *and* the curing time), the model is a **multiple** regression. Item 11.3 previews it, and Module 12 teaches it.

## Always plot first

Before any number, draw the **scatter plot**: x on the horizontal axis, y on the vertical, one point per observation. It shows at a glance:

- the **direction**: does y rise or fall as x rises?
- the **form**: do the points follow a straight line, or a curve?
- the **strength**: do they hug the line, or scatter widely?
- **unusual points**: one far from the rest, which may be an error or may tell you something.

The Example: ten batches of concrete, with the 28-day compressive strength (MPa) against the water–cement ratio.

[[data: st.m11.foundations.eg]]

[[figure: st.m11.foundations.eg 1]]

The points fall from left to right: more water, weaker concrete. They follow a straight line fairly closely. The red line is the least-squares line, which item 11.2 explains.

## The correlation coefficient r

The **correlation coefficient** r measures how closely the points follow a straight line, in one number. It uses three sums over the n pairs:

- S_xx = Σ(x − x̄)², the spread of x;
- S_yy = Σ(y − ȳ)², the spread of y;
- S_xy = Σ(x − x̄)(y − ȳ), how x and y move together. A point above both means, or below both, adds a positive product; a point above one mean and below the other adds a negative one.

r = S_xy / √(S_xx · S_yy)

r is always between −1 and +1:

| r | Means |
|---|---|
| +1 | every point exactly on a rising line |
| about +0.8 to +1 | a strong rising linear relation |
| about 0 | no **linear** relation |
| about −0.8 to −1 | a strong falling linear relation |
| −1 | every point exactly on a falling line |

r has no units, and it does not change if you change the units of x or y (MPa to psi), or swap x and y. For the concrete, r = −0.92: a strong falling relation.

### A small case by hand

Four points, from the course's reading on regression: (1, 2), (2, 3), (3, 5), (4, 4).

1. x̄ = 10/4 = 2.5 and ȳ = 14/4 = 3.5.
2. The deviations x − x̄ are −1.5, −0.5, 0.5, 1.5, and y − ȳ are −1.5, −0.5, 1.5, 0.5.
3. S_xx = 2.25 + 0.25 + 0.25 + 2.25 = 5. S_yy = 2.25 + 0.25 + 2.25 + 0.25 = 5.
4. S_xy = (−1.5)(−1.5) + (−0.5)(−0.5) + (0.5)(1.5) + (1.5)(0.5) = 2.25 + 0.25 + 0.75 + 0.75 = 4.
5. r = 4 / √(5 × 5) = 4/5 = **0.80**.

### R², the coefficient of determination

The square of r, **R²**, is the share of the variation in y that the straight line explains. For the four points, R² = 0.80² = 0.64: the line explains 64 % of the variation in y, and 36 % is left over as scatter around it. For the concrete, R² = 0.851. Item 11.4 shows where R² comes from and why it is read this way.

## What r cannot tell you

**r measures straight-line relations only.** A strong curved relation can have r near 0. Here is a centrifugal pump's efficiency at flow rates from 10 to 110 L/s. It peaks near 60 L/s and falls off on both sides:

[[figure: st.m11.foundations.curved 1]]

r = −0.01 and R² = 0.0001, yet the efficiency clearly depends on the flow. A straight line is the wrong model here, and only the plot shows it. Compare twelve operators' assembly times against their age, where the points are a shapeless cloud:

[[figure: st.m11.foundations.weak 1]]

Here r = −0.23 and R² = 0.05: there is no useful relation.

**Correlation is not causation.** A strong r says x and y move together, not that x causes y. Both may follow a third variable: across a city's summer days, ice-cream sales and pump failures both rise with the temperature. To show that x *causes* a change in y, set x yourself in a designed experiment, as the concrete supplier did with the water–cement ratio.

**One unusual point can make or break r.** Always look at the scatter plot before trusting the number.

## In Python and R

```python
import numpy as np
x = np.array([0.40, 0.42, 0.45, 0.48, 0.50, 0.53, 0.55, 0.58, 0.60, 0.65])
y = np.array([48.7, 48.7, 43.2, 41.4, 45.6, 38.8, 42.4, 38.8, 35.9, 30.3])
r = np.corrcoef(x, y)[0, 1]        # -0.9223
print(r, r**2)                     # r and R² = 0.8506
# from scipy import stats; stats.pearsonr(x, y) also gives a p-value
```

```r
x <- c(0.40, 0.42, 0.45, 0.48, 0.50, 0.53, 0.55, 0.58, 0.60, 0.65)
y <- c(48.7, 48.7, 43.2, 41.4, 45.6, 38.8, 42.4, 38.8, 35.9, 30.3)
cor(x, y)          # -0.9223
cor(x, y)^2        # 0.8506
plot(x, y)         # always look first
```

The Workbench writes the whole regression in both languages, under the results of the Example:

[[code: st.m11.foundations.eg]]

## Common mistakes

- **Trusting r without the plot.** r near 0 can hide a strong curve, and a high r can come from one far-off point.
- **Reading r as a slope.** r = −0.92 does not mean "strength falls 0.92 MPa per unit". The slope has units (item 11.2); r has none.
- **Mixing up r and R².** R² = r² is the share of variation explained. r = 0.7 explains only 49 %.
- **Reading cause into correlation.** Only a designed experiment shows cause and effect.

## What to do next

On the **Exercise** tab, find r and R² for a building's electricity use against the outdoor temperature, and press **Check my answers**.
