---
id: st.m12.simple-to-multiple
title: From simple to multiple regression
minutes: 45
objectives: [st.m12.o1, st.m12.o2]
example: st.m12.simple-to-multiple.eg
exercise: st.m12.simple-to-multiple.ex
reference: [multiple-regression]
---

# From simple to multiple regression

## What one predictor cannot do

Module 11's simple linear regression explains a response y with one predictor x. That works when one thing drives the response. When two or more things drive it, and they happen to move together in the data, the one-predictor line gives a misleading answer.

Here is a case. A lab cast 12 concrete cylinders and tested them after 7, 14 or 28 days of curing. The mixes had water–cement (W/C) ratios from 0.40 to 0.62. Everyone knows two facts about concrete: it gains strength as it cures, and extra water weakens it. Yet look at the two simple regressions on these data (R², as in Module 11, is the share of the variation in strength the line explains; it is defined again below):

| Model | Slope | p-value of the slope | R² |
|---|---|---|---|
| Strength on Curing days alone | 0.121 MPa per day | 0.299 | 0.107 |
| Strength on W/C ratio alone | −10.4 MPa per unit of W/C | 0.509 | 0.045 |

Neither slope is significant, and the lines explain only about 11 % and 5 % of the variation. Read naively, curing time hardly matters.

The reason is in how the cylinders were made: the ones cured longest came from the wetter mixes. The correlation between Curing days and W/C ratio in this sample is 0.82. The simple regression on curing time compares long-cured wet cylinders with short-cured dry ones, so the strength gained from curing is cancelled by the strength lost to water. A predictor left out of the model does not go away: its effect is mixed into the slopes of the predictors that move with it.

## The multiple regression model

**Multiple regression** puts all the predictors into one model. With k predictors x₁, x₂, …, xₖ, the model for the population is

Y = β₀ + β₁X₁ + β₂X₂ + … + βₖXₖ + ε

- **Y** is the response, and **X₁ … Xₖ** are the **predictors** (also called independent or explanatory variables).
- **β₀** is the **intercept**, and **β₁ … βₖ** are the **regression coefficients**. Greek letters are the unknown population values; their estimates from a sample are written b₀, b₁, …, bₖ (some books write β̂).
- **ε** is the **error term**: everything about Y that the predictors do not explain (item 12.2).
- **k** is the number of predictors. The model has **p = k + 1** coefficients, the intercept included.

The fitted equation is ŷ = b₀ + b₁x₁ + … + bₖxₖ, where **ŷ** ("y-hat") is the predicted response. The **residual** of observation i is eᵢ = yᵢ − ŷᵢ, the observed value minus the predicted one.

## Fitting it: ordinary least squares

The coefficients are chosen exactly as in Module 11: **ordinary least squares (OLS)** picks the b's that make the sum of squared residuals

SS_E = Σ (yᵢ − ŷᵢ)²

as small as possible. With one predictor that gives the slope and intercept formulas of Module 11. With several, the answer is a matrix formula, b = (XᵀX)⁻¹Xᵀy, where the matrix X has a column of 1's (for the intercept) and one column per predictor. Nobody does that by hand: the Workbench, Python's statsmodels and R's `lm` all solve it, and they give the same numbers.

## The Example: concrete with both predictors

[[data: st.m12.simple-to-multiple.eg]]

[[results: st.m12.simple-to-multiple.eg]]

The fitted equation is

Strength = 62.767 + 0.5815·Curing days − 74.22·W/C ratio

With both predictors in the model, curing time matters a lot: **0.582 MPa per day** with the W/C ratio held fixed (p = 0.0001), almost five times the simple slope. The W/C coefficient, −74.2 MPa per unit of W/C ratio, is −0.742 MPa for each 0.01 of extra water at a fixed curing time. Together the two predictors explain **R² = 0.832** of the variation in strength, against 0.107 and 0.045 for each alone, and the typical miss is σ̂ = 1.53 MPa (σ̂, the residual standard error, is √(SS_E/(n − k − 1)); item 12.2).

To predict, put values into the equation. A cylinder with W/C = 0.50 cured for 21 days:

ŷ = 62.767 + 0.5815 × 21 − 74.22 × 0.50 = 62.767 + 12.212 − 37.111 = **37.87 MPa**

A mix with W/C = 0.55 cured 28 days instead of 14 is predicted 0.5815 × 14 = **8.14 MPa** stronger; the data have cylinders of that mix at both ages. That is what "holding the others fixed" means, and item 12.3 is about it. (No mix was tested at both 7 and 28 days, so a 7-against-28-day comparison at one W/C ratio would reach outside the data: item 12.3 says why that matters.)

[[figure: st.m12.simple-to-multiple.eg 1]]

The figure plots each cylinder's observed strength against the strength the model predicts. A perfect model would put every point on the diagonal line. Here most points lie within about 1.5 MPa of it; the largest miss is cylinder 2, 3.1 MPa stronger than predicted (the point well above the line near 35.7 MPa predicted).

## Controlling for other variables

"Controlling for" W/C ratio means comparing cylinders as if they all had the same W/C ratio. An experiment does this by design, by holding the other factors at fixed levels. Field data and production records usually cannot be designed, and that is where multiple regression earns its place: it adjusts for the other predictors in the arithmetic. It can only adjust for predictors that are **in the model**. A cause that was never measured still leaks into the coefficients of whatever moves with it.

## When to use multiple regression

- The response plausibly depends on **several** measured factors (strength on curing, water and cement content; fuel on load, speed and grade).
- You want the **separate effect** of one factor with the others accounted for.
- You want **better predictions** than one predictor gives.

## Continuity with simple regression

Everything from Module 11 carries over. The residuals, R² = 1 − SS_E/SS_T (SS_T = Σ(yᵢ − ȳ)² is the total variation of y around its mean ȳ), the residual standard error σ̂, the t-test of each coefficient and the assumptions on ε are the same ideas with k predictors in place of one. With k = 1, multiple regression *is* simple regression: fit the Workbench's **Multiple regression** with one predictor and you get Module 11's line. What changes: the degrees of freedom are n − k − 1 instead of n − 2 (item 12.2), R² needs an adjusted version (item 12.4), and the overall F-test is no longer the same as the slope's t-test (item 12.4).

## In Python and R

```python
import numpy as np, pandas as pd
import statsmodels.api as sm
days = np.array([7, 7, 7, 7, 14, 14, 14, 14, 28, 28, 28, 28])
wc = np.array([0.40, 0.42, 0.45, 0.48, 0.45, 0.50, 0.52, 0.55, 0.52, 0.55, 0.60, 0.62])
strength = np.array([35.6, 38.8, 32.9, 30.9, 35.2, 34.3, 32.8, 30.3, 40.8, 39.5, 34.2, 32.1])
X = pd.DataFrame({'days': days, 'wc': wc})
fit = sm.OLS(strength, sm.add_constant(X)).fit()
print(fit.params)        # const 62.767, days 0.5815, wc -74.2216
print(fit.rsquared)      # 0.8316
print(sm.OLS(strength, sm.add_constant(days)).fit().params)   # days alone: slope 0.1214
```

```r
days <- c(7, 7, 7, 7, 14, 14, 14, 14, 28, 28, 28, 28)
wc <- c(0.40, 0.42, 0.45, 0.48, 0.45, 0.50, 0.52, 0.55, 0.52, 0.55, 0.60, 0.62)
strength <- c(35.6, 38.8, 32.9, 30.9, 35.2, 34.3, 32.8, 30.3, 40.8, 39.5, 34.2, 32.1)
fit <- lm(strength ~ days + wc)
coef(fit)                 # 62.767, 0.5815, -74.2216
summary(fit)$r.squared    # 0.8316
coef(lm(strength ~ days)) # days alone: slope 0.1214
predict(fit, data.frame(days = 21, wc = 0.50))   # 37.87
```

The program's own code for the Example, with every result printed:

[[code: st.m12.simple-to-multiple.eg]]

## Common mistakes

- **Reading a simple slope as the effect of a factor** when other factors moved with it. The slope of Strength on Curing days alone (0.121) is not the effect of curing; the multiple-regression coefficient (0.582) is the effect with W/C held fixed.
- **Fitting one predictor at a time and adding the slopes up.** The coefficients of a multiple regression come from one fit with all the predictors together; they are not the simple slopes.
- **Believing the model has controlled for everything.** It adjusts only for the predictors in it.

## What to do next

On the **Exercise** tab, fit a two-predictor model to the fuel use of delivery trucks, and compare its Payload coefficient with what one predictor would give.
