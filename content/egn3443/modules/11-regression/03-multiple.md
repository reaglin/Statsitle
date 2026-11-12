---
id: st.m11.multiple
title: Multiple regression models (a preview)
minutes: 35
objectives: [st.m11.o3]
builds-on: st.m11.simple
example: st.m11.multiple.eg
quiz: quizzes/m11-multiple.json
reference: [multiple-regression-preview, simple-linear-regression]
---

# Multiple regression models (a preview)

This item is a first look. **Module 12 teaches multiple regression in full**: how the coefficients are found, how to read each one, and how to choose the predictors. Here the aim is to tell a simple model from a multiple one, and to know when a question needs more than one predictor.

## One predictor or several?

A **simple** linear regression has **one** predictor:

ŷ = b₀ + b₁x

A **multiple** linear regression has **two or more** predictors, x₁, x₂, … x_k:

ŷ = b₀ + b₁x₁ + b₂x₂ + … + b_kx_k

It is still fitted by least squares: the b's are the values that make the sum of squared residuals Σ(y − ŷ)² as small as possible. With two predictors the fitted "line" is a flat plane over the (x₁, x₂) floor, and with more it cannot be drawn. The fit is therefore checked with plots of the residuals and of observed against predicted y, not with a scatter plot and a line.

**Which one to use depends on the question**, not on the software:

| The question | The model |
|---|---|
| How does fuel use change with payload? | simple: one predictor |
| How does strength depend on the water–cement ratio **and** the curing time? | multiple: two predictors |
| Predict a house's energy use from its floor area, age and insulation | multiple: three predictors |
| Does the new sensor read the same as the reference sensor? | simple: one predictor (calibration) |

Use a multiple regression when:

- the response has **several known causes** and you want the effect of each one **with the others held constant**;
- the predictions must be **more accurate** than one predictor allows;
- a predictor of interest **moves together with another** one, so a simple regression would mix their effects (the Example shows this).

Keep a simple regression when one predictor answers the question, or when there are too few observations: each extra coefficient needs more data. A rough rule is 10 to 15 observations per predictor (item 12.2).

## The Example: stack loss of a nitric acid plant

These are real data, published by Brownlee in 1965 and often used to teach regression. They come from 21 days of operation of a plant that oxidizes ammonia to nitric acid. The **stack loss** is the ammonia that escapes unabsorbed up the stack (ten times the percentage lost), a cost and a pollutant. The engineers recorded the **air flow** (a coded rate of operation) and the **cooling-water temperature** (°C).

[[data: st.m11.multiple.eg]]

**One predictor first.** Stack loss on air flow alone, a simple regression:

[[figure: st.m11.multiple.simple 1]]

ŷ = −44.13 + 1.020 × air flow, with R² = 0.846 and a residual standard error of 4.10. (The residual standard error σ̂ is the typical size of a residual, in y's units; item 11.4 explains it.)

**Both predictors.** The multiple regression of stack loss on air flow and water temperature:

ŷ = −50.36 + 0.671 × air flow + 1.295 × water temp

[[results: st.m11.multiple.eg]]

[[figure: st.m11.multiple.eg 1]]

This picture plots each day's observed stack loss against the model's prediction. A perfect model would put every point on the gray line, where observed = predicted. Compare the two models:

| | Air flow only | Air flow + water temp |
|---|---|---|
| R² | 0.846 | 0.909 |
| Adjusted R² | 0.838 | 0.899 |
| Residual standard error σ̂ | 4.10 | 3.24 |

The second predictor explains more of the variation and shrinks the typical miss from about 4.1 to 3.2. **Adjusted R²** is R² with a penalty for every predictor added. Plain R² never falls when a predictor is added, even a useless one; adjusted R² rises only if the new predictor earns its place. Here it rises, from 0.838 to 0.899. Item 11.4 gives its formula.

**Why the air-flow coefficient changed.** Alone, air flow's slope is 1.020. With water temperature in the model it is 0.671. On this plant the two went up and down together (their correlation is r = 0.78). On its own, air flow carries part of the water temperature's effect. The multiple regression separates them: 0.671 is the change in stack loss for one more unit of air flow **at the same water temperature**. Reading coefficients this way, "with the others held constant", is the heart of Module 12.

## In the Workbench

Press **Open in the Workbench** on the Example tab: it opens this model with its data. To set it up yourself, press **Copy the data** on the Example tab, press Ctrl+B, click **+ Data table** and paste (Ctrl+V). Choose **Multiple regression** in the list on the left. Set **Data table** to the table, **Response column (y)** to *Stack loss* and **Predictor columns** to *Air flow, Water temp* (column names separated by commas), with **Variable selection** at *All the predictors listed*. The results give the equation and a coefficient table, one row per term; Module 12 explains every column. The rows after it (VIF, cross-validation, hold-out, Durbin–Watson, Cook's distance, leverage) belong to Modules 12 and 13. Ctrl+L brings you back.

## In Python and R

```python
import pandas as pd
import statsmodels.formula.api as smf
plant = pd.DataFrame({
    "air":   [80, 80, 75, 62, 62, 62, 62, 62, 58, 58, 58, 58, 58, 58, 50, 50, 50, 50, 50, 56, 70],
    "water": [27, 27, 25, 24, 22, 23, 24, 24, 23, 18, 18, 17, 18, 19, 18, 18, 19, 19, 20, 20, 20],
    "loss":  [42, 37, 37, 28, 18, 18, 19, 20, 15, 14, 14, 13, 11, 12, 8, 7, 8, 8, 9, 15, 15]})
simple = smf.ols("loss ~ air", plant).fit()
multiple = smf.ols("loss ~ air + water", plant).fit()
print(simple.rsquared_adj, multiple.rsquared_adj)   # 0.8377, 0.8986
print(multiple.params)                               # -50.36, 0.6712, 1.2954
```

```r
# R has these data built in as stackloss
simple   <- lm(stack.loss ~ Air.Flow, data = stackloss)
multiple <- lm(stack.loss ~ Air.Flow + Water.Temp, data = stackloss)
summary(multiple)        # coefficients, R², adjusted R², F
```

In both, adding a predictor is one more name after the `~`, joined with `+`.

[[code: st.m11.multiple.eg]]

## Common mistakes

- **Adding predictors to push R² up.** R² always rises. Judge by adjusted R² and by whether the predictor makes engineering sense. Module 12 shows how to choose.
- **Reading a multiple-regression coefficient as the simple slope.** 0.671 is air flow's effect at a fixed water temperature, not its effect on its own.
- **Too many predictors for too few rows.** With 21 observations, two predictors is about the limit.

## What to do next

On the **Exercise** tab, answer five practice questions on choosing between a simple and a multiple model and on reading the comparison.
