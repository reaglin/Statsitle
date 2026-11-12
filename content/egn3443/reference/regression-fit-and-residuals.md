---
title: Regression fit and residual checks
---

# Regression fit and residual checks

## Sums of squares and the ANOVA table

SS total = Σ(y − ȳ)² = SS regression + SS error, with SS regression = Σ(ŷ − ȳ)² and SS error = Σ(y − ŷ)².

| Source | df | SS | MS | F | p |
|---|---|---|---|---|---|
| Regression | k | SS regression | SS regression / k | MS_R / MS_E | P(F > F observed) |
| Error | n − k − 1 | SS error | SS error / (n − k − 1) | | |
| Total | n − 1 | SS total | | | |

k is the number of predictors (1 in simple regression). F tests H₀: all slopes are 0, on k and n − k − 1 degrees of freedom. In simple regression F = t² of the slope.

## Measures of fit

| Measure | Formula | Reading |
|---|---|---|
| R² | SS regression / SS total | share of the variation in y explained; 0 to 1 |
| Adjusted R² | 1 − (1 − R²)(n − 1)/(n − k − 1) | R² with a charge per predictor; for comparing models |
| Residual standard error σ̂ | √(SS error / (n − k − 1)) = √MS_E | typical miss, in y's units |
| RMSE | √(SS error / n) | typical miss, dividing by n (used to compare models' predictions) |

## The assumptions (LINE) and their checks

| Assumption | Check | A failure looks like |
|---|---|---|
| **L**inearity | residuals against fitted values | a curve (U or arch) |
| **I**ndependence | residuals in time order; Durbin–Watson (Module 13) | long runs of one sign, waves |
| **N**ormality | normal Q–Q plot of residuals; Shapiro–Wilk p | points bending away from the line; small p |
| **E**qual variance (homoscedasticity) | residuals against fitted values | a funnel: spread growing with ŷ |

A good residual plot is a shapeless horizontal band around 0. A failed check makes the p-values and intervals untrustworthy. Remedies (Module 13): a transformation of x or y, a curve, another predictor, or a different model.

## Python and R

| Task | Python (statsmodels) | R |
|---|---|---|
| R², adjusted R² | `fit.rsquared`, `fit.rsquared_adj` | `summary(fit)$r.squared`, `$adj.r.squared` |
| σ̂ | `np.sqrt(fit.mse_resid)` | `summary(fit)$sigma` |
| F and its p | `fit.fvalue`, `fit.f_pvalue` | `summary(fit)$fstatistic`, `anova(fit)` |
| Residuals, fitted | `fit.resid`, `fit.fittedvalues` | `residuals(fit)`, `fitted(fit)` |
| Residual and Q–Q plots | `plt.scatter(fit.fittedvalues, fit.resid)`; `sm.qqplot(fit.resid, line="s")` | `plot(fit, which = 1:2)` |
| Shapiro–Wilk | `stats.shapiro(fit.resid)` | `shapiro.test(residuals(fit))` |

## In the Workbench

**Simple linear regression** gives **R²**, **Adjusted R²**, **Residual standard error (σ̂)**, **SS regression**, **SS error**, **SS total**, **ANOVA F**, **ANOVA p-value** and **Shapiro–Wilk p of residuals**. Its figures are the scatter plot with the line, the residuals against fitted values, and the normal Q–Q plot.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 11-4, 11-7 (adequacy of the model, residual analysis) and 12-2.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 4.4.4, How can I tell if a model fits my data?: https://www.itl.nist.gov/div898/handbook/pmd/section4/pmd44.htm
