---
title: Reading and reporting regression output
---

# Reading and reporting regression output

## R's `summary(lm(y ~ x))`, line by line

| Line | What it is | Statistics+ label |
|---|---|---|
| `(Intercept)` Estimate | b₀ | Intercept (b₀) |
| `x` Estimate | b₁, the slope | Slope (b₁) |
| Std. Error | SE(b₀), SE(b₁) | Intercept / Slope standard error |
| t value | estimate / standard error | Slope t |
| Pr(>\|t\|) | two-sided p-value of H₀: coefficient = 0 | Intercept / Slope p-value |
| Residual standard error, on n − 2 degrees of freedom | σ̂ | Residual standard error (σ̂) |
| Multiple R-squared | R² | R² |
| Adjusted R-squared | adjusted R² | Adjusted R² |
| F-statistic on 1 and n − 2 DF, p-value | the overall F-test | ANOVA F, ANOVA p-value |

statsmodels' `fit.summary()` gives the same numbers: `coef`, `std err`, `t`, `P>|t|`, `[0.025 0.975]`, `R-squared`, `Adj. R-squared`, `F-statistic`, `Prob (F-statistic)`.

## What each number answers

| Question | Read |
|---|---|
| Which way, and how much per unit of x? | the slope, with its confidence interval |
| Is there a relation at all? | the slope's p-value (= the F-test's in simple regression) |
| How much of the variation is explained? | R² |
| How far off is a prediction typically? | σ̂ |
| What will one new unit at x₀ give? | the prediction interval at x₀ |
| What is the average at x₀? | the confidence interval on the mean response at x₀ |

## Reporting to a non-technical reader

1. The finding, in everyday words, with its direction.
2. The size, in the reader's units, with a plain range ("likely between …").
3. How much it accounts for (R² as "about three quarters of the differences").
4. A prediction for one case, with its spread.
5. The limits: the range and source of the data; "associated with", not "causes", unless x was set in an experiment; any failed residual check.

Leave out p-values and symbols, or put them in a technical appendix.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 11-4 to 11-6.
- R Core Team, `summary.lm` in the R documentation: https://stat.ethz.ch/R-manual/R-devel/library/stats/html/summary.lm.html
