---
id: st.m13.foundations
title: Foundations of regression validation
minutes: 40
objectives: [st.m13.o1, st.m13.o2]
example: st.m13.foundations.eg
quiz: quizzes/m13-foundations.json
reference: [validation-residual-plots, validation-assumptions]
---

# Foundations of regression validation

**Review and new.** Item 11.4 (*Evaluating model performance*) introduced the four assumptions (LINE), the residual plot against the fitted values with its two classic failures (the tool-wear curve and the office-cost funnel), the normal Q–Q plot and the Shapiro–Wilk test of the residuals. This item gathers them into **validation**: what it is for, its two halves, and the cycle of checking and refining. Items 13.2 to 13.6 then add what is new: the Durbin–Watson test, standardized residuals, leverage and Cook's distance, cross-validation and refinement.

## What validation is, and why a good R² is not enough

Least squares always finds *a* line. It finds one through data that curve, through data whose scatter grows tenfold from one end to the other, and through data with a typing error in them, and it reports an R² and a slope p-value for each. Those numbers answer "how much of y's variation does this line explain?" and "is there any relation at all?". They do not answer the questions an engineer needs answered before using the model:

- Is the model the **right shape** for the data?
- Are the **assumptions** behind its p-values and intervals met?
- Does it **predict new observations** about as well as it fits the old ones?
- Is it resting on **one or two observations**?

**Regression validation** is the work of answering these. It has two halves:

- **Reliability**: is the fitted model a sound description of *these* data? Its form is right, its assumptions hold, and no single point is steering it. The residual checks of items 13.1 to 13.3 and the diagnostics of item 13.5 answer this.
- **Generalizability**: will it work on *new* data from the same process? A model can describe its own data well and still predict poorly, especially when it has many terms. Cross-validation (item 13.4) answers this.

## The assumptions behind a regression

The model of Modules 11 and 12 is y = β₀ + β₁x + ε (with more β's and x's for multiple regression), where ε is the random error of each observation. The t-tests, F-test and intervals are calculated as if:

| Assumption | Says | Broken when, for example |
|---|---|---|
| **L**inearity | The mean of y really is a straight-line function of x (or of the predictors) | Braking distance grows with the square of speed |
| **I**ndependence | The errors of different observations are unrelated | Readings logged hour by hour from an instrument that drifts |
| **N**ormality | The errors follow a normal distribution | A few large positive errors, from occasional delays |
| **E**qual variance | The errors have the same spread at every x (**homoscedasticity**) | Measurement error proportional to the size of the part |

The four initials spell **LINE**. The errors ε cannot be seen, because the true β's are not known. What can be seen are the **residuals**, eᵢ = yᵢ − ŷᵢ: each observation minus the model's fitted value for it (ŷ, "y-hat", is the fitted value). The residuals are the errors' stand-ins, so every check of the assumptions is a check of the residuals. Item 13.3 computes them by hand.

## The residual plot

The most useful picture in regression is the **residual plot**: the residuals (vertical) against the fitted values ŷ (horizontal), with a line at zero. If the model is right, the residuals are what is left over at random, so the plot should be a **horizontal band of points with no pattern**: about as many above zero as below, at every fitted value, with the same spread from left to right.

Any pattern is the model telling you what it missed:

| What you see | What it means | Assumption |
|---|---|---|
| A random horizontal band | Nothing missed | All four look fine |
| A curve (a U, an arch) | The relation bends and the line does not | Linearity |
| A funnel (spread grows or shrinks along ŷ) | The error depends on the size of y | Equal variance |
| One point far from the band | An outlier: an error, or a case the model does not cover | Check the point (item 13.5) |
| Long runs of the same sign, in time order | Successive errors are related | Independence (item 13.2) |

With one predictor, the residuals against x look the same as the residuals against ŷ (flipped left to right when the slope is negative). With several predictors, plot against ŷ, and against each predictor in turn.

## The Example: braking distance

[[data: st.m13.foundations.eg]]

[[figure: st.m13.foundations.eg 1]]

At first sight the line is excellent:

[[results: st.m13.foundations.eg]]

R² = 0.9815, and the slope's p-value is far below 0.0001. Now look at the residuals:

[[figure: st.m13.foundations.eg 2]]

They are not a random band. At 20 to 30 km/h the residuals are positive (+2.6, +3.3, +0.8 m), from 35 to 65 km/h they are all negative (the largest −4.1 m at 40 km/h), and from 70 to 80 km/h positive again, +4.1 m at 80 km/h. That **U shape** says the relation curves upward and the line cuts across it: too long a stopping distance in the middle, too short at both ends. Physics agrees: the kinetic energy a brake must absorb grows with the square of speed. The line under-predicts the stopping distance at the highest speed by about 4 m, which is exactly where it matters most.

The R² of 0.98 did not warn of any of this. **Always look at the residual plot before you believe a fit.** The cure (a transformation, or a curved model) is item 13.6; with ln y fitted against ln x, for example, the U disappears.

## Validation is a cycle

Validation is not a single test at the end. It is a loop:

1. **Fit** a model suggested by the engineering and by a scatter plot.
2. **Check** it: the residual plot, the assumptions (13.2, 13.3), the influential points (13.5), and its prediction error on new data (13.4).
3. **Refine** what the checks find: change the form, transform a variable, drop or add a predictor, correct or explain a point (13.6).
4. **Check again**, and stop when the model is good enough **for its purpose**: a calibration curve needs tiny, patternless residuals; a rough planning estimate can live with more.

## In Python and R

```python
import numpy as np
import statsmodels.api as sm
import matplotlib.pyplot as plt
speed = np.array([20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80])
dist = np.array([5.9, 11.1, 13.0, 16.0, 17.1, 24.8, 26.6, 33.5, 37.9, 43.2, 48.1, 53.3, 61.0])
fit = sm.OLS(dist, sm.add_constant(speed)).fit()
print(fit.rsquared, fit.resid)            # R² and the residuals
plt.scatter(fit.fittedvalues, fit.resid); plt.axhline(0); plt.xlabel("fitted"); plt.ylabel("residual"); plt.show()
```

```r
speed <- c(20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80)
dist <- c(5.9, 11.1, 13.0, 16.0, 17.1, 24.8, 26.6, 33.5, 37.9, 43.2, 48.1, 53.3, 61.0)
fit <- lm(dist ~ speed)
summary(fit)$r.squared; residuals(fit)
plot(fitted(fit), residuals(fit)); abline(h = 0)    # or plot(fit, which = 1)
```

[[code: st.m13.foundations.eg]]

## In the Workbench

Choose **Simple linear regression**, set **x column** and **y column** (or type the pairs into **Data (x, y)**). The Plots tab shows the fit, the residuals against the fitted values, and the normal Q–Q plot of the residuals (item 13.2).

## What to do next

On the **Exercise** tab, six practice questions, three of them with residual plots, ask what the plots and the numbers say. After a wrong try the explanation shows. You need 80 % to complete the item.
