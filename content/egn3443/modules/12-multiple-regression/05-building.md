---
id: st.m12.building
title: Building and evaluating models
minutes: 50
objectives: [st.m12.o1, st.m12.o3, st.m12.o4, st.m12.o5]
builds-on: st.m12.fit
example: st.m12.building.eg
exercise: st.m12.building.ex
reference: [model-building, multiple-regression]
---

# Building and evaluating multiple regression models

Items 12.1 to 12.4 each took one step. This item puts them in order, from a list of candidate predictors to a model you can report and defend.

## The steps

1. **Specify.** Choose the response and the candidate predictors from knowledge of the process (item 12.2).
2. **Estimate.** Fit the model with all the candidates by least squares (item 12.1).
3. **Assess the fit.** R², adjusted R² and σ̂ (item 12.4).
4. **Test the model.** The overall F-test: does it explain anything at all? (item 12.4)
5. **Check the assumptions.** The residuals: no pattern, roughly normal, even spread (item 11.4 showed the plots; item 12.2; Module 13 in full).
6. **Refine.** Remove or add predictors, one at a time, with reasons (below), and repeat steps 2 to 5.
7. **Interpret and communicate.** Each coefficient with its units and "the others held fixed", the fit, and how the model was chosen.

## Refining the model: variable selection strategies

When there are several candidate predictors, a procedure can help decide which to keep. The Workbench's **Variable selection** offers two, both with the rule p < 0.05:

- **Forward selection** starts with no predictors. At each step it tries every predictor not yet in the model, alongside those already in, and adds the one with the smallest p-value, as long as that p-value is below 0.05. It stops when no remaining predictor would enter with p < 0.05.
- **Backward elimination** starts with all the candidates. At each step it removes the predictor with the largest p-value, as long as that p-value is 0.05 or more. It stops when every remaining predictor has p < 0.05.

Two others you will meet: **stepwise** selection (forward, but a predictor already in can leave again if its p-value grows), and **best subsets** (fit every combination and compare adjusted R² or a criterion such as AIC). R's `step()` function works backward and forward by **AIC**, the Akaike information criterion, a measure of fit with a charge for each coefficient, not by p-values; it can keep a predictor whose p-value is a little above 0.05, so it does not always agree with the p < 0.05 rule.

## The Example: rut depth in asphalt (backward elimination)

Twenty asphalt test sections were cored after a summer. Four candidate predictors of the rut depth (mm): air voids in the mix (%), binder content (%), the highest pavement temperature (°C) and the truck traffic (thousands of ESALs, equivalent single-axle loads).

[[data: st.m12.building.eg]]

**The full model** with all four candidates (fit it in the Workbench by setting **Variable selection** to *All the predictors listed*):

| Predictor | b | p-value |
|---|---|---|
| Air voids | 0.6015 | 0.0017 |
| Binder | −0.0218 | 0.966 |
| Pavement temp | 0.1314 | 0.0006 |
| Traffic | 0.00544 | 0.0004 |

R² = 0.8286, adjusted R² = 0.7829, F = 18.13 on 4 and 15 df (p < 0.0001). The model is significant, and Binder's p-value (0.966) says it adds nothing beyond the other three.

**Backward elimination** removes Binder (p = 0.9662) and stops, because every remaining p-value is below 0.05. The **Selection** result records each step.

[[results: st.m12.building.eg]]

| Model | k | R² | Adjusted R² | σ̂ (mm) | F |
|---|---|---|---|---|---|
| All four | 4 | 0.8286 | 0.7829 | 1.071 | 18.13 |
| Without Binder | 3 | 0.8286 | 0.7964 | 1.037 | 25.78 |

Dropping Binder cost nothing in R² (it is 0.8286 to four decimals either way), raised adjusted R² and lowered σ̂. Forward selection on the same data adds Pavement temp, then Traffic, then Air voids, and stops before Binder (p = 0.966): the same model.

**Check the assumptions** before trusting it:

[[figure: st.m12.building.eg 2]]

The residuals against the fitted values show no curve and no funnel: the spread is about the same from 4 mm to 12 mm of predicted rutting. One section (row 13) lies 2.5 mm above its prediction, the largest miss; Module 13 shows how to judge whether one point like that changes the model. The largest VIF is 1.17, so the coefficients are not inflated by overlapping predictors.

## Justifying each decision

This module's last outcome asks you to **justify** including or removing a predictor. A procedure is not a justification on its own. Bring together:

| Evidence | Points to keeping the predictor | Points to removing it |
|---|---|---|
| Its coefficient's t-test | small p-value: it adds information beyond the others | large p-value |
| Adjusted R² (and σ̂, which moves with it) | adjusted R² falls and σ̂ rises without it | adjusted R² rises and σ̂ falls without it |
| Engineering knowledge | a known mechanism, or a standard model requires it | no plausible mechanism |
| The assumptions | the residuals show a pattern when it is left out | the residuals are no better with it |
| Overlap (VIF) | | very high VIF, and another predictor carries the same information |

For Binder: p = 0.966, adjusted R² rises from 0.7829 to 0.7964 without it, and the residuals of the three-predictor model show no pattern, so the data give no reason to keep it. An engineer might still keep a predictor with p a little above 0.05 when the physics says it matters and adjusted R² is higher with it; say so in the report.

Two cautions about automatic selection:

- **It is exploratory.** After a procedure has picked the predictors that look best in this sample, their p-values look better than they are (many tests on one data set: the familywise error rate of item 14.1), and R² is optimistic. Test the chosen model on new data: Module 13's cross-validation does that.
- **Forward and backward can disagree**, when predictors overlap or a p-value is near 0.05. Neither is "the right answer"; look at the evidence above.

## Interpreting and communicating the result

The final rut model:

Rut depth = −5.256 + 0.5987·Air voids + 0.1317·Pavement temp + 0.005431·Traffic

A report says what was fitted, how it was chosen, and what each number means in the units of the problem:

> Rut depth in 20 test sections was modeled by multiple linear regression on four candidate predictors. Backward elimination at α = 0.05 removed binder content (p = 0.97). The final model, with air voids, maximum pavement temperature and truck traffic, explains 83 % of the variation in rut depth (R² = 0.829, adjusted R² = 0.796, F = 25.8 on 3 and 16 df, p < 0.001); the residual standard error is 1.04 mm. With the other two predictors held fixed, each 1 % of air voids adds 0.60 mm of rutting (95 % CI 0.30 to 0.89), each 1 °C of pavement temperature 0.13 mm (0.07 to 0.19), and each 100 thousand ESALs 0.54 mm (0.30 to 0.78). The residuals showed no pattern.

## In Python and R

```python
import statsmodels.formula.api as smf
# df: a pandas DataFrame with the columns voids, binder, temp, traffic and rut
full = smf.ols('rut ~ voids + binder + temp + traffic', data=df).fit()
print(full.pvalues)                          # binder 0.966: the candidate to drop
final = smf.ols('rut ~ voids + temp + traffic', data=df).fit()
print(final.rsquared_adj, full.rsquared_adj) # 0.7964 against 0.7829
```

```r
# d: a data frame with the columns voids, binder, temp, traffic and rut
full <- lm(rut ~ voids + binder + temp + traffic, data = d)
summary(full)                       # binder p = 0.966
final <- update(full, . ~ . - binder)
summary(final)$adj.r.squared        # 0.7964
step(full)                          # selection by AIC: here it also drops binder
```

The program's code for the Example (it fits the model selection chose, and lists the steps):

[[code: st.m12.building.eg]]

## Common mistakes

- **Keeping a predictor because R² went up.** R² always goes up. Use adjusted R², the p-value and the residuals.
- **Reporting the selected model's p-values as if the model had been chosen in advance.** Say the predictors were selected, and validate on new data.
- **Dropping several predictors at once.** Remove one at a time: when one leaves, the others' p-values change.
- **Letting the procedure override engineering sense.** A predictor with a known mechanism may stay with p slightly above 0.05; a significant one with no mechanism deserves suspicion.

## What to do next

On the **Exercise** tab, run forward selection on a milling trial's tool-wear data, and report how many predictors it keeps and how well the chosen model fits.
