---
title: Refining a regression model
---

# Refining a regression model

| Finding | Refinement |
|---|---|
| Curve against x, even spread | transform x: ln x (rise that keeps slowing), 1/x (fall towards a floor), √x (mild bend); or add x² |
| Funnel, spread grows with ŷ | ln y (or √y) |
| Exponential growth or decay | ln y against x |
| Power law y ∝ xᵇ | ln y against ln x |
| Non-significant term, low VIF, CV error not improved | remove it |
| VIFs above 5–10 | remove or combine one of the overlapping predictors |
| Influential point with a documented error | correct or remove it, and report it |
| CV RMSE well above in-sample RMSE | fewer terms |

## Comparing candidates

- **Same y:** R² (same number of terms), adjusted R², σ̂, cross-validated RMSE, residual plots.
- **Different y (y against ln y):** R² and σ̂ are on different scales and do not compare. Compare residual plots and prediction errors in y's own units (the *Fit for Purpose* activity's s), or cross-validate both.
- **Eliminate with a reason:** residual pattern; non-significant term; larger s (same scale); lower adjusted R² (same scale). Between near-equals, the simpler model.

## Good enough when

Residuals show no pattern; no unexplained influential point; predictors do not overlap badly (or only predictions are used); the cross-validated error is close to the in-sample error and small enough for the purpose. Use the model only inside the range of its data.

## Python and R

| Task | Python | R |
|---|---|---|
| ln x | `sm.OLS(y, sm.add_constant(np.log(x))).fit()` | `lm(y ~ log(x))` |
| 1/x | `sm.add_constant(1 / x)` | `lm(y ~ I(1 / x))` |
| ln y, back-transformed prediction | `np.exp(fit.predict(...))` | `exp(predict(fit, new))` |
| Quadratic | `np.column_stack([x, x**2])` | `lm(y ~ x + I(x^2))` |

## In the Workbench

**Simple linear regression**: **Transform x** and **Transform y** (*Natural log, ln*; *Square root, √*; *Reciprocal, 1/*); predictions are turned back into y's units. For x², add a column to the data table and use **Multiple regression**.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 11 (regression on transformed variables) and Section 12-6 (aspects of multiple regression modeling: polynomial terms, variable selection, multicollinearity).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 4.4.5 (if my current model does not fit the data well, how can I improve it?): https://www.itl.nist.gov/div898/handbook/pmd/section4/pmd45.htm
