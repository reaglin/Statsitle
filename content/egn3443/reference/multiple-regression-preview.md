---
title: Simple and multiple regression
---

# Simple and multiple regression

| | Simple | Multiple |
|---|---|---|
| Predictors | one, x | two or more, x₁ … x_k |
| Model | ŷ = b₀ + b₁x | ŷ = b₀ + b₁x₁ + … + b_kx_k |
| A slope means | change in ŷ per unit of x | change in ŷ per unit of xⱼ, **the other predictors held constant** |
| Picture | scatter plot with a line | observed against predicted; residual plots |
| Error df | n − 2 | n − k − 1 |
| Compare models by | | adjusted R², σ̂, and engineering sense |

**Choose by the question.** One predictor answers "how does y change with x?" when nothing else varies with x. Use several when y has several known causes, when an effect must be read with others held constant, or when a predictor of interest moves together with another. Keep at least about 10 observations per predictor.

**R² never falls when a predictor is added.** Adjusted R² = 1 − (1 − R²)(n − 1)/(n − k − 1) falls when the new predictor does not earn its place.

Module 12 teaches multiple regression in full: the coefficient table, the F-test of the whole model, and choosing the predictors.

## Python and R

| Task | Python | R |
|---|---|---|
| Fit | `smf.ols("y ~ x1 + x2", data).fit()` | `lm(y ~ x1 + x2, data = d)` |
| Output | `fit.summary()` | `summary(fit)` |

## In the Workbench

**Multiple regression**: **Data table**, **Response column (y)**, **Predictor columns** (names separated by commas), **Variable selection** *All the predictors listed*.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 12-1 and 12-2.
- Brownlee, K. A. (1965), *Statistical Theory and Methodology in Science and Engineering*, 2nd ed., Wiley, p. 454 (the stack-loss data; R's `stackloss`).
