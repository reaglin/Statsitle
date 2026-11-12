---
id: st.m13.refinement
title: Model refinement and decisions
minutes: 50
objectives: [st.m13.o5, st.m13.o1]
builds-on: st.m13.weaknesses
example: st.m13.refinement.eg
exercise: st.m13.refinement.ex
reference: [validation-refinement, validation-diagnostics]
---

# Model refinement and decisions

Items 13.1 to 13.5 find what is wrong with a model. This item decides what to do about it, compares the candidate models fairly, and says when to stop.

## From finding to refinement

| Validation finding | Refinement to try |
|---|---|
| Curve in the residuals against x | Transform x (ln x, √x, 1/x), or add a curved term such as x² |
| Funnel: spread grows with ŷ | Transform y (ln y, sometimes √y) |
| Exponential growth or decay in y | ln y against x |
| A power law (y ∝ xᵇ) | ln y against ln x |
| A predictor with a large p-value, low VIF, and a CV error that rises with it | Remove it (variable removal) |
| Two predictors with VIFs above 5 to 10 | Remove one, or combine them |
| An influential point with a documented error | Correct or remove it, and say so |
| Runs in time order | Find the time-related cause and add it as a predictor |
| Cross-validated error well above in-sample | Simplify: fewer terms |

Each refinement is followed by the checks again: a transformation that straightens the curve can create a funnel, and dropping a predictor can expose a pattern it was hiding.

## Transformations and functional form

A **transformation** replaces x or y by a function of it, and the model is fitted as a straight line in the new variables. It is still least squares; only the columns change.

- **Transform x** (ln x, √x, 1/x) changes the **shape** of the curve but keeps y's scale, so the spread of the residuals is unchanged. Use it when the relation bends but the scatter is even. Which one: ln x for a rise that keeps slowing (strength gain, learning curves), 1/x for a fall towards a floor (cost per part with a setup cost, a heat sink's resistance against fin area), √x for a milder bend.
- **Transform y** (ln y) changes both the shape and the **spread**: it shrinks large values more than small ones, so it can turn a funnel into a band and exponential growth into a line. Predictions are turned back by exp(), and then estimate the **median** of y rather than its mean.
- **Functional form** can also change by adding terms: a **quadratic** term (a column x² in Multiple regression) for a curve with a peak or a valley, which no single transform can follow.

Prefer the form the engineering suggests. A model whose shape follows the physics (y ∝ v² for braking, a fixed cost shared over a batch) is more likely to hold for new cases, and its coefficients mean something.

## Comparing competing models fairly

| Models being compared | Compare by |
|---|---|
| Same y, same number of terms (line in x against line in ln x) | R², σ̂, residual plots, cross-validated RMSE |
| Same y, different numbers of terms | adjusted R², cross-validated RMSE, the p-value of the added term (not R², which never falls) |
| **Different y** (y against ln y) | Residual plots, and the errors of the predictions **in y's own units** |

The last row matters. With ln y, R² measures how much of the variation **of ln y** is explained and σ̂ is in units of ln y, so neither can be set beside the R² and σ̂ of a model for y. The course's *Fit for Purpose* activity compares every model by s computed from residuals in the original units of y for that reason. In Statistics+, σ̂ of a model with Transform y is on the transformed scale; to compare it with a model for y itself, compare the residual plots and the predictions in the original units, or cross-validate both.

A model is **eliminated** for a reason you can state: a pattern in its residuals, a term that is not significant, a larger σ̂ (on the same y scale) or a larger cross-validated error, a lower adjusted R² (on the same y scale). The survivor with the smallest error is the candidate; between two that are nearly equal, choose the simpler one.

## The Example: concrete strength gain

The strength of one concrete mix, tested at ages from 1 to 90 days.

[[data: st.m13.refinement.eg]]

**A straight line in age** is the first try:

[[figure: st.m13.refinement.line 2]]

The residuals arch: −11.0 MPa at 1 day, positive from 5 to 56 days (up to +7.0 at 28 days), −6.3 at 90 days. R² = 0.739, σ̂ = 5.92 MPa, and the 90-day cylinder has a Cook's distance of 2.68, not because it is wrong but because the line is: a wrong form makes the end points look influential.

Strength rises quickly and then ever more slowly, the shape of a logarithm. With **Transform x** set to *Natural log, ln*:

[[figure: st.m13.refinement.eg 1]]

[[results: st.m13.refinement.eg]]

[[figure: st.m13.refinement.eg 2]]

| Model | R² | σ̂ (MPa) | Residuals | Largest Cook's D | Decision |
|---|---|---|---|---|---|
| Strength against age | 0.739 | 5.92 | arch | 2.68 | eliminate: residual pattern |
| Strength against √age | 0.903 | 3.60 | still an arch | 1.18 | eliminate: residual pattern, larger σ̂ |
| Strength against ln(age) | 0.989 | 1.24 | random, within ±2.4 | 0.18 | **keep** |

All three models have the same y, so R² and σ̂ compare directly. The log model's residuals form a band, Shapiro–Wilk p = 0.800, and its σ̂ is a fifth of the line's. Its equation is Strength = 7.06 + 7.87 ln(age): each doubling of the age adds 7.87 × ln 2 = 5.5 MPa. At 60 days it predicts 39.3 MPa, with a 95 % prediction interval of 36.2 to 42.3 MPa for one cylinder.

## Balancing statistics and practical use

The best-fitting model is not always the one to use:

- **Purpose.** A calibration curve must have small, patternless residuals across its whole range. An early design estimate can accept a larger σ̂ for a simpler model engineers can check by hand.
- **Range.** Every model is validated only inside the data. The ln(age) model keeps rising for ever, while real concrete levels off: do not use it far beyond 90 days.
- **Simplicity.** Between two models with nearly the same cross-validated error, choose the one with fewer terms and coefficients that mean something.
- **Cost of data.** A predictor that costs money to measure must earn its place in prediction error.

## When a model is good enough

Stop refining when:

1. the residual plots show no pattern and the assumptions hold well enough for how the model will be used;
2. no observation is unexplained and influential;
3. the predictors do not overlap badly, or only predictions (not single coefficients) are used;
4. the cross-validated error is close to the in-sample error, and small enough for the purpose.

Then report the model with its validation: the residual plots, the diagnostics, the cross-validated RMSE, the range of the data, and what was changed on the way and why.

## In Python and R

```python
import numpy as np
import statsmodels.api as sm
age = np.array([1, 2, 3, 5, 7, 10, 14, 21, 28, 42, 56, 90])
st = np.array([7.3, 12.3, 15.0, 20.6, 21.9, 27.5, 25.6, 30.2, 34.7, 36.0, 38.3, 43.0])
for name, u in [("age", age), ("sqrt", np.sqrt(age)), ("ln", np.log(age))]:
    fit = sm.OLS(st, sm.add_constant(u)).fit()
    print(name, fit.rsquared, np.sqrt(fit.mse_resid))
```

```r
age <- c(1, 2, 3, 5, 7, 10, 14, 21, 28, 42, 56, 90)
st <- c(7.3, 12.3, 15.0, 20.6, 21.9, 27.5, 25.6, 30.2, 34.7, 36.0, 38.3, 43.0)
fit <- lm(st ~ log(age))
summary(fit); plot(fit, which = 1)
predict(fit, data.frame(age = 60), interval = "prediction")
```

[[code: st.m13.refinement.eg]]

## In the Workbench

In **Simple linear regression**, change **Transform x** or **Transform y** and watch R², σ̂ and the residual plot. Keep a note of each model's numbers, or add the analysis twice and compare. For a quadratic term, add a column x² to the data table and use **Multiple regression**.

## Common mistakes

- **Comparing R² across different y's.** The R² of ln y and of y answer different questions.
- **Choosing by R² alone.** Look at the residuals of every candidate.
- **Transforming without a reason.** Try the form the physics suggests first.
- **Extrapolating a transformed model.** Its curve beyond the data is a guess.

## What to do next

On the **Exercise** tab, refine a cost-per-part model with the reciprocal of the batch size: find its R², σ̂ and the fitted cost for a batch of 120, and press **Check my answers**.
