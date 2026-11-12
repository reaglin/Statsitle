---
title: The assumptions of a regression model
---

# The assumptions of a regression model

y = β₀ + β₁x₁ + … + β_kx_k + ε, with the errors ε:

| Assumption (LINE) | How to check | If it fails |
|---|---|---|
| **L**inearity of the mean | residuals against ŷ and each predictor: no curve | coefficients biased; predictions wrong in parts of the range |
| **I**ndependence | residuals in collection order; Durbin–Watson near 2 | standard errors too small; p-values and intervals too optimistic |
| **N**ormality | normal Q–Q plot of the residuals; Shapiro–Wilk of the residuals (not of y) | matters little for coefficients when n is large; prediction intervals off |
| **E**qual variance | residuals against ŷ: even band, no funnel | standard errors wrong; intervals wrong where the spread differs |

## Durbin–Watson

d = Σᵢ₌₂ⁿ (eᵢ − eᵢ₋₁)² / Σᵢ₌₁ⁿ eᵢ², between 0 and 4, about 2(1 − r₁) with r₁ the lag-one correlation of the residuals. Near 2: independent; well below 2: positive autocorrelation; well above: negative. Bounds at α = 0.05 with one predictor: n = 20, d_L = 1.20, d_U = 1.41; n = 30, d_L = 1.35, d_U = 1.49. Only meaningful for rows in time (collection) order. A curved relation on rows sorted by x also lowers d.

## Shapiro–Wilk of the residuals

H₀: the errors are normal. p < 0.05: evidence against normality. Low power for small n: read the Q–Q plot too.

## Python and R

| Check | Python | R |
|---|---|---|
| Durbin–Watson | `statsmodels.stats.stattools.durbin_watson(fit.resid)` | `sum(diff(e)^2) / sum(e^2)`; `car::durbinWatsonTest(fit)` for a p-value |
| Shapiro–Wilk | `scipy.stats.shapiro(fit.resid)` | `shapiro.test(residuals(fit))` |
| Q–Q plot | `sm.qqplot(fit.get_influence().resid_studentized_internal, line="45")` | `plot(fit, which = 2)` |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 11-7 and 12-5.
- Durbin, J. and Watson, G. S. (1951), Testing for serial correlation in least squares regression II, *Biometrika* 38, 159–178. Bounds from Savin, N. E. and White, K. J. (1977), *Econometrica* 45, 1989–1996.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 4.4.4: https://www.itl.nist.gov/div898/handbook/pmd/section4/pmd44.htm
