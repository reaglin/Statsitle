---
id: st.m11.simple
title: Simple linear regression
minutes: 50
objectives: [st.m11.o1]
builds-on: st.m11.foundations
example: st.m11.simple.eg
exercise: st.m11.simple.ex
reference: [simple-linear-regression, correlation]
---

# Simple linear regression

## The line that fits best

Many lines pass near a cloud of points. Which one is "the" line? For each observation, the **residual** is how far the point is above or below the line, measured vertically:

eᵢ = yᵢ − ŷᵢ, where ŷᵢ = b₀ + b₁xᵢ is the line's value at xᵢ

A positive residual means the point is above the line; a negative one, below. The **least-squares line** is the line that makes the **sum of squared residuals** as small as possible:

SS_E = Σ(yᵢ − ŷᵢ)²

(E for error.) Squaring stops positive and negative misses from cancelling, and it punishes a big miss more than several small ones. Calculus gives the line that minimizes SS_E, with S_xx and S_xy as in item 11.1:

- **slope** b₁ = S_xy / S_xx
- **intercept** b₀ = ȳ − b₁x̄

The second formula says the least-squares line always passes through the point of means (x̄, ȳ). The residuals of a least-squares line always add up to 0.

The slope can also be written b₁ = r · s_y / s_x, where s_x and s_y are the standard deviations of x and y. So the slope has the sign of r, and r = 0 means a flat line.

## A worked case by hand

The four points of item 11.1: (1, 2), (2, 3), (3, 5), (4, 4), with x̄ = 2.5, ȳ = 3.5, S_xx = 5 and S_xy = 4.

1. b₁ = S_xy / S_xx = 4/5 = **0.8**.
2. b₀ = ȳ − b₁x̄ = 3.5 − 0.8 × 2.5 = 3.5 − 2.0 = **1.5**.
3. The line: ŷ = 1.5 + 0.8x.

| x | y | ŷ = 1.5 + 0.8x | residual e = y − ŷ | e² |
|---|---|---|---|---|
| 1 | 2 | 2.3 | −0.3 | 0.09 |
| 2 | 3 | 3.1 | −0.1 | 0.01 |
| 3 | 5 | 3.9 | 1.1 | 1.21 |
| 4 | 4 | 4.7 | −0.7 | 0.49 |
| | | | sum 0 | SS_E = 1.80 |

Try another line through the same points, ŷ = 1 + x. Its residuals are 0, 0, 1 and −1, and its SS_E is 0 + 0 + 1 + 1 = 2.00. That is more than 1.80. No line has a smaller SS_E than the least-squares line.

## Reading the slope and the intercept

**The slope** is the change in the predicted y for **one unit more of x**, in y's units per x's unit. Say it in those units: "the roughness rises 9.77 µm per mm/rev of feed" (the Example below). When one unit of x is a large step, scale it: "0.098 µm for every 0.01 mm/rev more feed".

**The intercept** is the predicted y at x = 0. It often has no physical meaning: no shaft is turned at zero feed, and no concrete has a water–cement ratio of 0. It is then just the constant that places the line, and you should not read it as a fact about the process.

## Predicting with the line

To predict, put x into the line: ŷ = b₀ + b₁x. Two rules:

- **Interpolate, do not extrapolate.** The line is fitted over the x values in the data. Inside that range it is supported by the data; outside it, nothing says the relation stays straight. Tool wear, fatigue and fuel use all bend at the extremes.
- **Keep the decimals.** Round b₀ and b₁ only at the end. A slope rounded too early is then multiplied by x, and the error grows with it.

## The Example: feed rate and surface roughness

A machinist turned eight shafts at feed rates from 0.10 to 0.28 mm/rev and measured the roughness Ra (µm).

[[data: st.m11.simple.eg]]

[[figure: st.m11.simple.eg 1]]

From the data: x̄ = 0.1875, ȳ = 1.99375, S_xx = 0.02735 and S_xy = 0.267275. So:

- b₁ = 0.267275 / 0.02735 = **9.7724** µm per mm/rev: each 0.01 mm/rev more feed adds about 0.098 µm of roughness;
- b₀ = 1.99375 − 9.7724 × 0.1875 = **0.1614** µm, a fitting constant: zero feed is far outside the data;
- the line: ŷ = 0.1614 + 9.7724x.

At a feed of 0.16 mm/rev, inside the tested range, the predicted roughness is ŷ = 0.1614 + 9.7724 × 0.16 = **1.725** µm.

[[results: st.m11.simple.eg]]

The results list holds much more than the line: the slope's test and confidence interval, R², the ANOVA table, the intervals at x₀ = 0.16 and some diagnostics. Items 11.4 and 11.5 explain them. Module 13 explains the diagnostics at the end (Durbin–Watson, Cook's distance, leverage).

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([0.10, 0.12, 0.15, 0.18, 0.20, 0.22, 0.25, 0.28])
y = np.array([1.26, 1.30, 1.36, 2.02, 2.19, 2.32, 2.67, 2.83])
fit = stats.linregress(x, y)
print(fit.intercept, fit.slope)          # 0.1614, 9.7724
print(fit.intercept + fit.slope * 0.16)  # 1.7250
# np.polyfit(x, y, 1) gives [slope, intercept]; statsmodels' OLS gives the full table
```

```r
x <- c(0.10, 0.12, 0.15, 0.18, 0.20, 0.22, 0.25, 0.28)
y <- c(1.26, 1.30, 1.36, 2.02, 2.19, 2.32, 2.67, 2.83)
fit <- lm(y ~ x)            # y "is modeled by" x
coef(fit)                   # 0.1614, 9.7724
predict(fit, data.frame(x = 0.16))   # 1.7250
```

In R's formula `y ~ x` the response comes first. In both languages, swapping x and y gives a different line (below).

[[code: st.m11.simple.eg]]

## Common mistakes

- **Swapping x and y.** Regressing x on y minimizes the horizontal misses, not the vertical ones, and gives a different line. Its slope S_xy / S_yy is not 1/b₁ unless r = ±1. Put the variable you want to predict on the y side.
- **A sign slip in the intercept.** b₀ = ȳ − b₁x̄. With a negative slope, −b₁x̄ is positive.
- **Rounding the slope early.** Keep at least four significant figures until the prediction is done.
- **Extrapolating.** A prediction outside the range of x in the data is a guess, however good R² is.
- **Reading the intercept as physics** when x = 0 is far from the data.

## What to do next

On the **Exercise** tab, fit the line for weld penetration against current, predict the depth at 175 A, and press **Check my answers**.
