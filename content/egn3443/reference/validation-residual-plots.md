---
title: Residuals and residual plots
---

# Residuals and residual plots

## Definitions

| Quantity | Formula | Notes |
|---|---|---|
| Residual | eᵢ = yᵢ − ŷᵢ | observed minus fitted; positive means the model under-predicts |
| SS error | Σeᵢ² | the residuals themselves add up to 0 (least squares with an intercept) |
| Residual standard error | σ̂ = √(SS error/(n − p)) | p coefficients (p = 2 for a line) |
| Leverage (line) | hᵢᵢ = 1/n + (xᵢ − x̄)²/S_xx | average p/n |
| Standardized residual | rᵢ = eᵢ/(σ̂√(1 − hᵢᵢ)) | beyond ±2: look; beyond ±3: outlier |

## Reading the residual plot (residuals against ŷ)

| Pattern | Means | Try |
|---|---|---|
| Random horizontal band | model adequate | — |
| Curve, U or arch | wrong functional form (linearity) | transform x, or add x² |
| Funnel | unequal variance (heteroscedasticity) | transform y, often ln y |
| Runs of one sign, in collection order | dependent errors (independence) | add the time-related cause |
| One point far out | outlier | check the record; judge its influence |

Also plot the residuals against each predictor, and in collection order.

## Python and R

| Task | Python (statsmodels) | R |
|---|---|---|
| Residuals, fitted values | `fit.resid`, `fit.fittedvalues` | `residuals(fit)`, `fitted(fit)` |
| σ̂ | `np.sqrt(fit.mse_resid)` | `summary(fit)$sigma` |
| Leverages | `fit.get_influence().hat_matrix_diag` | `hatvalues(fit)` |
| Standardized residuals | `fit.get_influence().resid_studentized_internal` | `rstandard(fit)` |
| Residual plot | `plt.scatter(fit.fittedvalues, fit.resid)` | `plot(fit, which = 1)` |

## In the Workbench

**Simple linear regression** and **Multiple regression**: the Plots tab draws the residuals against the fitted values and the normal Q–Q plot of the standardized residuals.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 11-7 (adequacy of the regression model: residual analysis) and Section 12-5.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 4.4.4 (how can I tell if a model fits my data?): https://www.itl.nist.gov/div898/handbook/pmd/section4/pmd44.htm
