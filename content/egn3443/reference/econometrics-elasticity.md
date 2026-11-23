---
title: Econometrics: elasticity and omitted variables
---

# Econometrics: elasticity and omitted variables

**Econometrics** is regression on economic data, usually observational. Method: Module 11 (simple regression), Module 12
(multiple regression), item 13.6 (transformations).

| Quantity | Formula | Meaning |
|---|---|---|
| Price elasticity of demand | ε = (%Δ quantity)/(%Δ price) | % change in quantity for a 1 % change in price |
| Log–log model | ln q = b₀ + b₁ ln p | b₁ = ε; the demand curve is q = e^b₀ p^b₁ |
| Log–linear model | ln y = b₀ + b₁ x | 100·b₁ ≈ % change in y per unit of x |
| Effect of a price change | q₁/q₀ = (p₁/p₀)^b₁ | exact, for any size of change |
| Revenue | R = p·q, R₁/R₀ = (p₁/p₀)^(1 + b₁) | revenue rises with price when −1 < b₁ < 0 (inelastic) |

**Omitted-variable bias.** If z affects y and moves with x, a regression of y on x alone gives x the credit for z. The bias
has the sign of (effect of z on y) × (correlation of z with x). Remedy: include z (multiple regression), or an experiment.

## Python and R

| Task | Python | R |
|---|---|---|
| Log–log fit | `sm.OLS(np.log(q), sm.add_constant(np.log(p))).fit()` | `lm(log(q) ~ log(p))` |
| Interval for the elasticity | `fit.conf_int()` | `confint(fit)` |
| With a second variable | `smf.ols("np.log(q) ~ np.log(p) + np.log(income)", d).fit()` | `lm(log(q) ~ log(p) + log(income))` |

**Read more:** Wooldridge, *Introductory Econometrics: A Modern Approach* (Cengage), chapters 2–3 (simple and multiple
regression) and 6 (logarithmic forms).
