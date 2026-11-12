---
title: Simple linear regression
---

# Simple linear regression

**Model:** Y = β₀ + β₁x + ε, with errors ε independent, normal, mean 0 and the same standard deviation σ at every x.

**Fitted line (least squares):** ŷ = b₀ + b₁x, the line that minimizes SS_E = Σ(y − ŷ)².

| Quantity | Formula |
|---|---|
| Slope | b₁ = S_xy / S_xx = r · s_y / s_x |
| Intercept | b₀ = ȳ − b₁x̄ (the line passes through (x̄, ȳ)) |
| Residual | eᵢ = yᵢ − ŷᵢ (they sum to 0) |
| Residual standard error | σ̂ = √(SS_E / (n − 2)) |
| Slope's standard error | SE(b₁) = σ̂ / √S_xx |
| Intercept's standard error | SE(b₀) = σ̂ √(1/n + x̄²/S_xx) |
| Test of H₀: β₁ = 0 | t = b₁ / SE(b₁), n − 2 df |
| Interval for the slope | b₁ ± t(α/2, n − 2) SE(b₁) |
| Mean response at x₀ | ŷ₀ ± t(α/2, n − 2) σ̂ √(1/n + (x₀ − x̄)²/S_xx) |
| One new observation at x₀ | ŷ₀ ± t(α/2, n − 2) σ̂ √(1 + 1/n + (x₀ − x̄)²/S_xx) |

S_xx, S_yy and S_xy are on the Correlation page. t(α/2, n − 2) is the t value with n − 2 degrees of freedom and α/2 above it.

**Reading:** the slope is the change in ŷ per one unit of x, in y's units per x's unit. The intercept is ŷ at x = 0, often outside the data and then only a fitting constant. Predict only inside the range of x in the data.

## Python and R

| Task | Python | R |
|---|---|---|
| Line only | `stats.linregress(x, y)` or `np.polyfit(x, y, 1)` | `coef(lm(y ~ x))` |
| Full output | `sm.OLS(y, sm.add_constant(x)).fit().summary()` | `summary(lm(y ~ x))` |
| Intervals for b₀, b₁ | `fit.conf_int(0.05)` | `confint(fit)` |
| Prediction at x₀ | `fit.get_prediction([[1, x0]]).summary_frame(alpha=0.05)` | `predict(fit, data.frame(x = x0), interval = "prediction")` |

In statsmodels' prediction frame, `mean_ci_*` is the interval on the mean response and `obs_ci_*` the prediction interval. In R, `interval = "confidence"` gives the first and `"prediction"` the second.

## In the Workbench

**Simple linear regression**: **x column** and **y column** from a data table (or pairs typed in **Data (x, y)**), **Confidence level**, **Predict at x₀, optional**. Results: **Intercept (b₀)**, **Slope (b₁)**, **Slope standard error**, **Slope t**, **Slope p-value**, **Slope confidence interval lower/upper**, **Fitted y at x₀**, **Mean response lower/upper**, **New observation lower/upper**.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 11 (Sections 11-2 to 11-6).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 4.1.4.1, Linear least squares regression: https://www.itl.nist.gov/div898/handbook/pmd/section1/pmd141.htm
