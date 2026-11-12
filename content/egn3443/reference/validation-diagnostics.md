---
title: Multicollinearity, outliers and influence
---

# Multicollinearity, outliers and influence

| Diagnostic | Formula | Flag |
|---|---|---|
| Variance inflation factor | VIFⱼ = 1/(1 − Rⱼ²), Rⱼ² from predictor j on the other predictors | above 5 (some say 10) |
| Standardized residual | rᵢ = eᵢ/(σ̂√(1 − hᵢᵢ)) | beyond ±3: outlier (±2: look) |
| Leverage | hᵢᵢ, the diagonal of H = X(XᵀX)⁻¹Xᵀ; for a line 1/n + (xᵢ − x̄)²/S_xx | above 2p/n (some 3p/n) |
| Cook's distance | Dᵢ = (rᵢ²/p) · hᵢᵢ/(1 − hᵢᵢ) | above 1 (some 4/n) |

p = the number of coefficients, intercept included; n = the number of rows.

- **Multicollinearity** inflates the standard errors (by √VIF), makes single coefficients unstable and their t-tests weak while the F-test is strong. Predictions inside the data's range are still sound. Refine: drop one of the overlapping predictors, or combine them.
- **Leverage** depends only on the predictors. High leverage on the trend is harmless; high leverage off the trend is influential.
- **Influence** is shown by Cook's distance, not by the raw residual: an influential point pulls the fit towards itself and can have a small residual.
- Remove a point only for a documented reason; otherwise report the fit with and without it.

## A diagnostic workflow

1. Plot the data. 2. Fit. 3. Residuals against ŷ and each predictor. 4. Q–Q plot, Shapiro–Wilk; Durbin–Watson for time-ordered rows. 5. VIFs. 6. Standardized residuals, leverage, Cook's distance; check flagged records. 7. Cross-validate. 8. Refine and repeat.

## Python and R

| Task | Python (statsmodels) | R |
|---|---|---|
| VIF | `variance_inflation_factor(X_with_const.values, j)` | `car::vif(fit)` |
| Leverage | `fit.get_influence().hat_matrix_diag` | `hatvalues(fit)` |
| Cook's distance | `fit.get_influence().cooks_distance[0]` | `cooks.distance(fit)` |
| Residuals against leverage | `sm.graphics.influence_plot(fit)` | `plot(fit, which = 5)` |

## In the Workbench

**Multiple regression**: **Largest VIF**, **Predictor with the largest VIF** (each predictor's VIF ends its line of **Coefficients**), **Largest leverage**, **Largest Cook's distance**, **Most influential observation**. **Simple linear regression** gives the leverage and Cook's rows.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 12-5 (influential observations) and Section 12-6 (multicollinearity).
- Cook, R. D. (1977), Detection of influential observation in linear regression, *Technometrics* 19, 15–18.
