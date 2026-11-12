---
title: Multiple linear regression
---

# Multiple linear regression

**Model:** Y = β₀ + β₁X₁ + β₂X₂ + … + βₖXₖ + ε, with ε independent and normal, mean 0 and constant standard deviation σ (LINE: linearity, independence, normality, equal variance).

**Fitted equation:** ŷ = b₀ + b₁x₁ + … + bₖxₖ. Residual eᵢ = yᵢ − ŷᵢ.

| Symbol | Meaning |
|---|---|
| n | observations |
| k | predictors |
| p = k + 1 | coefficients, the intercept included |
| n − k − 1 | error degrees of freedom |

**Ordinary least squares:** the b's minimize SS_E = Σ(yᵢ − ŷᵢ)²; in matrix form b = (XᵀX)⁻¹Xᵀy, where X has a column of 1's and one column per predictor.

**Partial regression coefficient bⱼ:** the change in mean y for one unit more of xⱼ with every other predictor held fixed; units of y per unit of xⱼ.

**Coefficient test:** t = bⱼ / SE(bⱼ) on n − k − 1 df, for H₀: βⱼ = 0 with the other predictors in the model. Interval: bⱼ ± t(α/2, n − k − 1) × SE(bⱼ).

**Variance inflation factor:** VIFⱼ = 1/(1 − Rⱼ²), Rⱼ² from regressing xⱼ on the other predictors. 1 = no overlap; above about 5 (or 10), multicollinearity (Module 13).

## Fit and the overall F-test

| | Formula |
|---|---|
| Sums of squares | SS_T = Σ(yᵢ − ȳ)² = SS_R + SS_E |
| R² | SS_R/SS_T = 1 − SS_E/SS_T |
| Adjusted R² | 1 − (1 − R²)(n − 1)/(n − k − 1) |
| Residual standard error | σ̂ = √(SS_E/(n − k − 1)) = √MS_E |
| Mean squares | MS_R = SS_R/k, MS_E = SS_E/(n − k − 1) |
| Overall F | MS_R/MS_E = (R²/k)/[(1 − R²)/(n − k − 1)], on k and n − k − 1 df |

F-test hypotheses: H₀: β₁ = … = βₖ = 0 against H₁: at least one βⱼ ≠ 0. Reject when F > F(α; k, n − k − 1), or when the p-value < α.

## Python and R

| Task | Python (statsmodels) | R |
|---|---|---|
| Fit | `fit = sm.OLS(y, sm.add_constant(X)).fit()` or `smf.ols('y ~ x1 + x2', data=df).fit()` | `fit <- lm(y ~ x1 + x2, data = d)` |
| Coefficient table | `fit.summary()` | `summary(fit)` |
| Coefficients, SE, t, p | `fit.params`, `fit.bse`, `fit.tvalues`, `fit.pvalues` | `coef(summary(fit))` |
| Intervals | `fit.conf_int(alpha=0.05)` | `confint(fit)` |
| R², adjusted R² | `fit.rsquared`, `fit.rsquared_adj` | `summary(fit)$r.squared`, `$adj.r.squared` |
| F and its p | `fit.fvalue`, `fit.f_pvalue` | `summary(fit)$fstatistic`, then `pf(…, lower.tail = FALSE)` |
| Prediction | `fit.predict(new_X)` | `predict(fit, newdata)` |
| VIF | `variance_inflation_factor(Xc.values, j)` | `car::vif(fit)` |
| F critical value | `stats.f.ppf(0.95, k, n - k - 1)` | `qf(0.95, k, n - k - 1)` |

## In the Workbench

**Multiple regression**: **Data table**, **Response column (y)**, **Predictor columns** (names separated by commas; blank uses every other column), **Variable selection**, **Confidence level**, and **Coefficient to read** (a predictor's name, to show its line of the coefficient table as numbers). Results: Equation, Coefficients (estimate, se, t, p, CI lower, CI upper, VIF), R², Adjusted R², Residual standard error (σ̂), ANOVA F, ANOVA p-value, Regression and Error degrees of freedom, Largest VIF; then Module 13's diagnostics.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 12 (multiple linear regression).
- EGN3443 Module 12 on the course website (reaglin.github.io/EGN3443), "Multiple Regression".
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 4.1.4.1 (linear least squares regression): https://www.itl.nist.gov/div898/handbook/pmd/section1/pmd141.htm
