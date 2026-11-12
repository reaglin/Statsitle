---
id: st.m13.weaknesses
title: Diagnosing model weaknesses
minutes: 50
objectives: [st.m13.o4]
builds-on: st.m13.cross-validation
example: st.m13.weaknesses.eg
exercise: st.m13.weaknesses.ex
reference: [validation-diagnostics]
---

# Diagnosing model weaknesses

A model can pass the residual checks and still be weak in three ways that the residual plot hides: its predictors overlap (**multicollinearity**), one observation is far from the rest in y (an **outlier**), or one observation is steering the fit (an **influential observation**). This item measures each.

## Multicollinearity

Predictors are **collinear** when one can be predicted from the others: the flow of chilled water set in proportion to the cooling load, the number of parts machined and the cutting time, a floor area in m² and in ft². The model then cannot tell which of them is doing the work. The data hold the information about their **sum**, not about each one separately.

The **variance inflation factor** of predictor j measures it:

VIFⱼ = 1 / (1 − Rⱼ²)

where Rⱼ² is the R² from regressing predictor j on **the other predictors** (not on y). With no overlap, Rⱼ² = 0 and VIF = 1. The name says what it does: the variance of bⱼ is VIFⱼ times what it would be without the overlap, so its standard error is √VIFⱼ times larger.

| VIF | Reading |
|---|---|
| 1 | no overlap |
| up to about 5 | moderate; usually harmless |
| above 5 (some say 10) | serious: the coefficients cannot be trusted one by one |

Symptoms: large standard errors; a strong overall F-test with no individually significant predictor; coefficients that change a lot, even change sign, when a row or a predictor is added or removed; a sign that contradicts the engineering. What multicollinearity does **not** spoil is prediction **inside the range of the data**: the combined effect is well estimated. It is the interpretation of single coefficients that fails.

## The Example: a chiller

[[data: st.m13.weaknesses.eg]]

[[results: st.m13.weaknesses.eg]]

R² = 0.991 and F is huge, yet the flow's coefficient has p = 0.54. Engineering says the flow certainly matters. The **Largest VIF** is 107.5 (Load), and the coefficient table shows Flow's VIF as 105.1: load and flow have a correlation of 0.995. Alone with the wet-bulb temperature, flow is highly significant (5.21 kW per L/s, p < 0.0001). Together with load, neither can be separated from the other.

**Refinement:** drop one of the pair. With **Predictor columns** set to *Load, WetBulb*:

| Model | se of the load coefficient | Largest VIF | Adjusted R² | σ̂ (kW) | 5-fold CV RMSE (kW) |
|---|---|---|---|---|---|
| Load, Flow, WetBulb | 0.1996 | 107.5 | 0.9883 | 5.99 | 7.17 |
| Load, WetBulb | 0.0220 | 1.38 | 0.9889 | 5.84 | 6.79 |

The load coefficient's standard error falls ninefold (√107.5 ≈ 10), and the simpler model predicts unseen afternoons better. Flow lost nothing: its information is in load. (Keeping both and reporting only their combined effect is the other choice; ridge regression, beyond this course, is a third.)

## Outliers

An **outlier** is an observation whose y is far from the model's prediction: a standardized residual (item 13.3) beyond about ±3. It may be a recording error, a failed test, a different regime (a machine in its warm-up), or simply the rare large error. Check the record first.

## Leverage

**Leverage** hᵢᵢ measures how far observation i's **predictor values** are from the centre of all of them; it ignores y. For a line, hᵢᵢ = 1/n + (xᵢ − x̄)²/S_xx. The leverages always average p/n (p coefficients, n rows), so:

- hᵢᵢ above **2p/n** is **high leverage** (some use 3p/n).

A high-leverage point is not bad in itself; a point far out on x that lies on the trend helps, by pinning the line down. It is dangerous when its y is also off, because the fit bends to reach it. In the chiller data, the afternoon with the largest load (row 10, 440 tons) has the largest leverage, 0.61, above 2p/n = 2 × 4/15 = 0.53, but it lies on the trend: its Cook's distance is only 0.02.

## Influence: Cook's distance

An observation is **influential** if leaving it out changes the fit a lot. **Cook's distance** measures how far all the fitted values move when observation i is left out, and it combines the two ingredients:

Dᵢ = (rᵢ² / p) × hᵢᵢ / (1 − hᵢᵢ)

with rᵢ the standardized residual. A large residual at low leverage, or high leverage on the trend, gives a small Dᵢ; a large residual **at** high leverage gives a large one.

- Dᵢ above **1**: influential; look at it closely.
- Some use **4/n** as a lower flag in larger data sets.

## An influential point: a saturated amplifier

[[data: st.m13.weaknesses.influence]]

[[figure: st.m13.weaknesses.influence 1]]

The last reading, at 25 kN, is far from the other loads: its leverage is 0.86 (2p/n = 2 × 2/10 = 0.40). The amplifier had saturated, so the strain recorded, 390 µε, is too low, and the line tilts down to reach it.

[[results: st.m13.weaknesses.influence]]

Its Cook's distance is **24.1**, beyond any threshold. Notice what the plain residuals would have said: at 25 kN the residual is only −15.3 µε, smaller than the +23.8 µε at 10 kN, because the line has moved to meet the point. Its standardized residual, −2.80, and above all its Cook's distance, give it away. A raw residual never shows influence.

Without the 25 kN reading, the slope is 19.93 µε per kN instead of 15.08, σ̂ falls from 14.6 to 2.07 µε, and the line predicts about 499 µε at 25 kN. Here there is a documented reason to drop the point (the amplifier's range); without one, report the fit with and without it.

## A systematic diagnostic workflow

1. **Plot the data** (scatter plots, each predictor against y and against each other).
2. **Fit** the model the engineering suggests.
3. **Residuals against ŷ and against each predictor**: form and equal variance (13.1, 13.2).
4. **Normal Q–Q plot and Shapiro–Wilk** of the residuals; **Durbin–Watson** if the rows are in time order (13.2).
5. **VIF** of each predictor (multicollinearity).
6. **Standardized residuals, leverage, Cook's distance**: outliers and influence. Check every flagged row's record.
7. **Cross-validate** (13.4): does the model predict unseen rows nearly as well as it fits?
8. **Refine** what the checks found (13.6) and go round again.

Ignoring these costs real money: a coefficient with the wrong sign used to set a process, a calibration curve dragged by one bad reading, a model that looked excellent and predicted next month's data badly.

## In Python and R

```python
import statsmodels.api as sm
from statsmodels.stats.outliers_influence import variance_inflation_factor
X = sm.add_constant(df[["Load", "Flow", "WetBulb"]]); fit = sm.OLS(df["Power"], X).fit()
print([variance_inflation_factor(X.values, j) for j in range(1, X.shape[1])])
infl = fit.get_influence()
print(infl.hat_matrix_diag, infl.resid_studentized_internal, infl.cooks_distance[0])
```

```r
fit <- lm(Power ~ Load + Flow + WetBulb, data = chiller)
car::vif(fit)                       # or 1/(1 - R²) of each predictor on the others
hatvalues(fit); rstandard(fit); cooks.distance(fit)
plot(fit, which = 5)                # standardized residuals against leverage, with Cook's contours
```

[[code: st.m13.weaknesses.eg]]

## In the Workbench

**Multiple regression** reports the **Largest VIF** and the **Predictor with the largest VIF** (every predictor's VIF is the last number on its line of **Coefficients**), the **Largest leverage**, the **Largest Cook's distance** and the **Most influential observation**. **Simple linear regression** reports the same leverage and Cook's distance rows. Try a model without a suspect row by deleting it from a copy of the data table.

## Common mistakes

- **Dropping a predictor because its p-value is large, without checking VIF.** It may matter a great deal and only overlap another.
- **Reading VIF as a relation with y.** It is about the predictors among themselves.
- **Hunting influential points by their raw residuals.** The most influential point often has a small residual.
- **Deleting influential points silently.** Fix a documented error; otherwise report the model with and without the point.

## What to do next

On the **Exercise** tab, diagnose a tool-wear model: find its largest VIF, leverage and Cook's distance, and press **Check my answers**.
