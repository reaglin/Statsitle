---
id: st.m12.fit
title: Assessing model fit
minutes: 50
objectives: [st.m12.o3, st.m12.o4]
builds-on: st.m12.coefficients
example: st.m12.fit.eg
exercise: st.m12.fit.ex
reference: [multiple-regression, model-building]
---

# Assessing model fit

## The concept of fit: splitting the variation

How well does the model explain the response? Start, as in Module 11, from the variation of y around its mean, and split it into the part the model explains and the part it leaves:

SS total = SS regression + SS error, written SS_T = SS_R + SS_E for short (the Workbench shows the long names)

| Sum of squares | Formula | What it is |
|---|---|---|
| **SS_T**, total | Σ (yᵢ − ȳ)² | the variation of the response, before any model |
| **SS_R**, regression | Σ (ŷᵢ − ȳ)² | the part the predictors explain |
| **SS_E**, error | Σ (yᵢ − ŷᵢ)² | the part left in the residuals |

For the two-predictor concrete model of item 12.1 (n = 12, k = 2): SS_T = 125.857, SS_E = 21.189, so SS_R = 104.668 (MPa²).

## R²: definition and interpretation

R² = SS_R / SS_T = 1 − SS_E / SS_T

the **coefficient of determination**: the fraction of the response's variation that the model explains. For the concrete, R² = 104.668 / 125.857 = **0.8316**: curing time and W/C ratio together explain 83 % of the cylinder-to-cylinder variation in strength. R² runs from 0 (the model explains nothing; ŷ = ȳ would do as well) to 1 (every point on the fitted plane). In multiple regression R² is also the square of the correlation between y and ŷ, which is why the observed-against-predicted plot of item 12.1 is a good picture of it.

## The limitation of R² with several predictors

**R² never falls when a predictor is added**, whatever the predictor. Least squares can always set the new coefficient to 0 and do no worse, and in a sample it almost always finds some small coefficient that does a little better. A column of random numbers raises R². With as many coefficients as observations (k = n − 1), R² = 1 exactly, whatever the data.

So R² cannot be used to decide whether a predictor belongs in the model, and a high R² is not proof of a good model. Nor does R² say whether the assumptions hold (Module 13), or how well the model will predict new data (Module 13's cross-validation).

## Adjusted R²: a charge for each predictor

**Adjusted R²** (item 11.4 gave its formula for one predictor) replaces the sums of squares by mean squares, each divided by its degrees of freedom:

adjusted R² = 1 − [SS_E/(n − k − 1)] / [SS_T/(n − 1)] = 1 − (1 − R²) (n − 1)/(n − k − 1)

Each added predictor costs a degree of freedom, so (n − 1)/(n − k − 1) grows. Adjusted R² rises only if the new predictor lowers SS_E by more than an average useless predictor would: exactly when the new coefficient's |t| is greater than 1. That is a much lower bar than p < 0.05, so adjusted R² can rise for a predictor that is not significant. Adjusted R² and σ̂ always move together (adjusted R² = 1 − σ̂²/s_y², with s_y the standard deviation of y), so count them as one piece of evidence, not two. It is always below R², and it can be negative for a useless model.

For the concrete: adjusted R² = 1 − (1 − 0.8316) × 11/9 = 1 − 0.16836 × 1.2222 = **0.7942**.

## The Example: a useless third predictor

The lab also logged the air temperature in the testing room (°C). The machine is in a climate-controlled room, so there is no reason the room temperature should change a cylinder's strength. Add it as a third predictor:

[[results: st.m12.fit.eg]]

| Model | R² | Adjusted R² | σ̂ (MPa) | F | p of F |
|---|---|---|---|---|---|
| Curing days, W/C ratio (item 12.1) | 0.8316 | 0.7942 | 1.534 | 22.23 | 0.00033 |
| + Lab temp | 0.8318 | 0.7687 | 1.627 | 13.19 | 0.0018 |

R² went **up**, by 0.0002, as it always does. Adjusted R² went **down**, from 0.7942 to 0.7687, and the residual standard error went up: the extra coefficient cost a degree of freedom and bought almost nothing. Lab temp's own line says the same: b = 0.041 MPa per °C with t = 0.09 and p = 0.93. The two-predictor model is the better one.

## Comparing R² and adjusted R²

| | R² | Adjusted R² |
|---|---|---|
| Measures | the share of the variation explained in **this** sample | the same, charged for the number of predictors |
| When a predictor is added | never falls | falls unless the new coefficient's t is beyond ±1 |
| Use it to | describe how much the model explains | compare models with different numbers of predictors |

Report R² to say how much the model explains; use adjusted R² (with σ̂, the coefficients' tests and the residual checks) to choose between models.

## Is the model significant at all? The overall F-test

Before reading individual coefficients, ask whether the predictors together explain anything:

H₀: β₁ = β₂ = … = βₖ = 0 (no predictor is related to y)
H₁: at least one βⱼ ≠ 0

The test statistic compares the explained variation per regression degree of freedom with the unexplained variation per error degree of freedom:

F = MS_R / MS_E = [SS_R / k] / [SS_E / (n − k − 1)] = (R² / k) / [(1 − R²)/(n − k − 1)]

If H₀ is true, F follows an **F distribution** with k and n − k − 1 degrees of freedom (the ratio of two independent variance estimates; Module 9 met it). Large F is evidence against H₀; the p-value is the area above F. The pieces go in an **ANOVA table**:

| Source | df | SS | MS | F | p |
|---|---|---|---|---|---|
| Regression | k = 2 | 104.668 | 52.334 | 22.23 | 0.00033 |
| Error | n − k − 1 = 9 | 21.189 | 2.354 | | |
| Total | n − 1 = 11 | 125.857 | | | |

The critical value is F(0.05; 2, 9) = 4.256 (the F value with 5 % above it, on 2 and 9 degrees of freedom), and 22.23 is far beyond it: the concrete model explains a significant share of the variation in strength. The Workbench reports F as **ANOVA F**, its p-value as **ANOVA p-value**, and the two degrees of freedom as **Regression degrees of freedom** and **Error degrees of freedom**.

How the F-test and the coefficients' t-tests relate:

- With **one** predictor, F = t² and they are the same test (Module 11).
- **F significant, no single t significant:** the predictors together explain y, but they overlap so much (high VIF) that the data cannot say which one does it.
- **F not significant, one t significant:** with many predictors, one small p-value can turn up by chance (the familywise error rate of item 14.1). The F-test guards against that, so look at it first.

## Using fit statistics to refine a model

- Compare models with **adjusted R²** and **σ̂**, never with R².
- Check that the **overall F** is significant before reading single coefficients.
- A predictor with a large p-value **and** a fall in adjusted R² when it is added has earned no place, unless theory says it must be there (item 12.5).
- In-sample fit is optimistic. How the model predicts **new** data is measured by cross-validation, in Module 13.

## In Python and R

```python
print(fit.rsquared, fit.rsquared_adj)        # R², adjusted R²
print(fit.fvalue, fit.f_pvalue)              # overall F and its p-value
print(fit.df_model, fit.df_resid)            # k and n − k − 1
```

```r
s <- summary(fit)
s$r.squared; s$adj.r.squared
s$fstatistic                                  # F, k, n − k − 1
pf(s$fstatistic[1], s$fstatistic[2], s$fstatistic[3], lower.tail = FALSE)
qf(0.95, 2, 9)                                # the critical value F(0.05; 2, 9) = 4.256
```

R's `anova(fit)`, and statsmodels' `sm.stats.anova_lm(fit)` for a model fitted with the formula interface (`smf.ols('strength ~ days + wc', data=df).fit()`), split SS_R into one line per predictor, in the order they were entered; add the lines to get the table above. The program's code for the Example:

[[code: st.m12.fit.eg]]

## Common mistakes

- **Choosing the model with the higher R².** It is always the one with more predictors.
- **The wrong degrees of freedom.** The divisor is n − k − 1, not n − k: the intercept is a coefficient too.
- **Reading a significant F as "every predictor matters".** It says at least one does.
- **Treating a high R² as proof the model is right.** Check the residuals (Module 13).

## What to do next

On the **Exercise** tab, fit a three-predictor model to the power drawn by a pump, and find R², adjusted R², F and its error degrees of freedom.
