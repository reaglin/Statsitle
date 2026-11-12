---
id: st.m13.residuals
title: Residual analysis
minutes: 45
objectives: [st.m13.o2, st.m13.o1]
builds-on: st.m13.assumptions
example: st.m13.residuals.eg
exercise: st.m13.residuals.ex
reference: [validation-residual-plots]
---

# Residual analysis

The two items before this one read residual plots. Item 11.2 computed residuals by hand for the least-squares line (e = y − ŷ, and their squares, SS error); this item computes them for any model, scales them so that "large" has a meaning, and reads what their pattern says.

## What a residual is

For each observation the model gives a **fitted value** ŷᵢ, and the **residual** is what is left over:

eᵢ = yᵢ − ŷᵢ   (observed minus fitted)

A positive residual means the observation lies **above** the line (the model under-predicts it); a negative one, **below** (the model over-predicts). Residuals are in y's units.

Two facts hold for every least-squares fit with an intercept:

- The residuals **add up to 0**, so their average says nothing; their sizes and pattern do.
- Their squares add up to **SS error** = Σeᵢ², the quantity least squares makes as small as possible.

From SS error comes the **residual standard error**, the typical size of a residual:

σ̂ = √(SS error / (n − p))

where p is the number of coefficients the model estimated (p = 2 for a line: the intercept and the slope). Dividing by n − p rather than n allows for the fact that the line was fitted to these same points; it is the regression version of the n − 1 in a sample standard deviation.

## Computing residuals: the Example

A spring was loaded with six weights and its extension measured.

[[data: st.m13.residuals.eg]]

Least squares gives x̄ = 35 N, ȳ = 7.1333 mm, S_xx = Σ(xᵢ − x̄)² = 1750 and S_xy = Σ(xᵢ − x̄)(yᵢ − ȳ) = 350, so b₁ = 350/1750 = 0.2000 mm/N and b₀ = 7.1333 − 0.2000 × 35 = 0.1333 mm. The line is ŷ = 0.1333 + 0.2000x.

| Load x (N) | Extension y (mm) | Fitted ŷ | Residual e = y − ŷ | e² |
|---|---|---|---|---|
| 10 | 2.1 | 2.1333 | −0.0333 | 0.0011 |
| 20 | 4.3 | 4.1333 | 0.1667 | 0.0278 |
| 30 | 5.9 | 6.1333 | −0.2333 | 0.0544 |
| 40 | 8.4 | 8.1333 | 0.2667 | 0.0711 |
| 50 | 9.8 | 10.1333 | −0.3333 | 0.1111 |
| 60 | 12.3 | 12.1333 | 0.1667 | 0.0278 |
| **Sum** | | | **0** | **0.2933** |

So SS error = 0.2933 mm² and σ̂ = √(0.2933/4) = √0.07333 = **0.2708 mm**. The typical extension is within about 0.27 mm of the line.

[[results: st.m13.residuals.eg]]

[[figure: st.m13.residuals.eg 2]]

The residuals alternate in sign, with no curve and no funnel, and the largest, −0.33 mm at 50 N, is only 1.2 σ̂ from zero. Six points cannot show much, but nothing here argues against the line.

## How large is large? Standardized residuals

A residual of 0.33 mm is small for the spring and would be huge for a micrometer, so residuals are judged against σ̂. But not every residual has the same spread: points far from x̄ pull the line towards themselves, so their residuals tend to be **smaller**. The amount of pull is the **leverage**, hᵢᵢ; for a line,

hᵢᵢ = 1/n + (xᵢ − x̄)² / S_xx.

The **standardized residual** divides each residual by its own standard error:

rᵢ = eᵢ / (σ̂ √(1 − hᵢᵢ)).

For the spring at 50 N: h = 1/6 + (50 − 35)²/1750 = 0.1667 + 0.1286 = 0.2952, so r = −0.3333 / (0.2708 × √0.7048) = −0.3333 / 0.2273 = **−1.47**.

Standardized residuals are on a scale like z-scores. For normal errors, about 95 % of them lie between −2 and 2, so:

- r beyond ±2: worth a look (expect about one in twenty by chance);
- r beyond ±3: an **outlier**. Check the record for an error before anything else.

The program's normal Q–Q plot (item 13.2) plots the standardized residuals, so a point beyond ±3 on it is an outlier. Item 13.5 adds the question that matters more: does the point change the model?

## Reading the pattern

| Pattern of the residuals | Diagnosis | First thing to try (item 13.6) |
|---|---|---|
| Random band, r mostly within ±2 | Model adequate | Nothing; report σ̂ |
| Curve or arch against ŷ or x | Wrong functional form | Transform x, or add a curved term |
| Funnel against ŷ | Unequal variance | Transform y (often ln y) |
| Runs in collection order | Dependent errors | Find the time-related cause; add it as a predictor |
| One r beyond ±3 | Outlier | Check the data; then judge its influence (13.5) |
| Q–Q plot bent at the ends | Non-normal errors | Often fixed with the form; transform y |

Plot the residuals against **ŷ**, against **each predictor** (including ones you left out of the model: a pattern means it belongs in), and in **collection order**.

## In Python and R

```python
import numpy as np
import statsmodels.api as sm
load = np.array([10, 20, 30, 40, 50, 60])
ext = np.array([2.1, 4.3, 5.9, 8.4, 9.8, 12.3])
fit = sm.OLS(ext, sm.add_constant(load)).fit()
e = fit.resid                                    # y − ŷ
print(e, e.sum(), (e**2).sum(), np.sqrt(fit.mse_resid))
infl = fit.get_influence()
print(infl.hat_matrix_diag, infl.resid_studentized_internal)   # leverages, standardized residuals
```

```r
load <- c(10, 20, 30, 40, 50, 60)
ext <- c(2.1, 4.3, 5.9, 8.4, 9.8, 12.3)
fit <- lm(ext ~ load)
e <- residuals(fit)
e; sum(e); sum(e^2); summary(fit)$sigma
hatvalues(fit); rstandard(fit)                    # leverages, standardized residuals
```

[[code: st.m13.residuals.eg]]

## Common mistakes

- **Fitted minus observed.** The residual is observed minus fitted, y − ŷ; the sign tells you whether the model under- or over-predicts.
- **Dividing by n, or n − 1.** σ̂ divides SS error by n − p (n − 2 for a line).
- **Judging residuals by their raw size.** Compare them with σ̂, or use standardized residuals.
- **Deleting every point with r beyond ±2.** About one in twenty normal errors is that large. Remove a point only for a documented reason (a recording error, a failed test), and say so in the report.

## What to do next

On the **Exercise** tab, compute the seven residuals of an amplifier calibration by hand, then SS error, σ̂ and the Durbin–Watson statistic, and press **Check my answers**.
