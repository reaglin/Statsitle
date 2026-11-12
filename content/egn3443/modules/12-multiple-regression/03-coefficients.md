---
id: st.m12.coefficients
title: Interpreting regression coefficients
minutes: 45
objectives: [st.m12.o2]
builds-on: st.m12.specification
example: st.m12.coefficients.eg
exercise: st.m12.coefficients.ex
reference: [multiple-regression]
---

# Interpreting regression coefficients

## Partial regression coefficients

In a multiple regression each coefficient bⱼ is a **partial regression coefficient**: the estimated change in the mean response for **one unit more of xⱼ, with every other predictor in the model held fixed**. "Partial" because it is the part of the relation between y and xⱼ that is left once the other predictors have been accounted for. It is the **unique effect** of xⱼ: the effect it has over and above what the others already explain.

The phrase "with the others held fixed" is not decoration. Item 12.1 showed the concrete data: the simple slope on curing time was 0.121 MPa per day, the partial coefficient with W/C ratio held fixed was 0.582. The two answer different questions. The simple slope compares cylinders that differ in curing time **and** in whatever moved with it; the partial coefficient compares cylinders at the same W/C ratio.

## The Example: roughness of turned shafts

Fifteen shafts were turned at five feed rates (mm per revolution) and three cutting speeds (m/min), and their surface roughness Ra (µm) was measured.

[[data: st.m12.coefficients.eg]]

[[results: st.m12.coefficients.eg]]

Roughness = 0.572 + 8.033·Feed − 0.00472·Cutting speed

## Reading the coefficient table

The **Coefficients** result has one line per term, with seven numbers:

| Number | What it is |
|---|---|
| estimate | the coefficient bⱼ |
| se | its **standard error** SE(bⱼ): how much bⱼ would vary from sample to sample |
| t | bⱼ / SE(bⱼ), the test of H₀: βⱼ = 0 with the other predictors in the model |
| p | the two-sided p-value of that t, on n − k − 1 degrees of freedom |
| CI lower, CI upper | the confidence interval bⱼ ± t(α/2, n − k − 1) × SE(bⱼ) |
| VIF | the variance inflation factor (below) |

For Feed: b₁ = 8.033, SE = 0.2587, t = 8.033 / 0.2587 = 31.05 on 15 − 2 − 1 = 12 degrees of freedom, p < 0.0001. With t(0.025, 12) = 2.179 (the t value with 2.5 % above it on 12 df), the 95 % interval is 8.033 ± 2.179 × 0.2587, from 7.470 to 8.597 µm per mm/rev.

## Positive and negative coefficients

The sign says which way the response moves when the predictor rises **and the others stay put**:

- **Feed, +8.033 µm per mm/rev.** At a fixed cutting speed, a coarser feed leaves a rougher surface (the tool's marks are farther apart). One step of the trial's feed, 0.05 mm/rev, adds 8.033 × 0.05 = **0.40 µm**.
- **Cutting speed, −0.00472 µm per m/min.** At a fixed feed, a faster cut is smoother. One step of 75 m/min lowers Ra by 0.00472 × 75 = **0.35 µm**.

## The intercept

b₀ = 0.572 µm is the predicted roughness at feed 0 and cutting speed 0, a lathe that is not cutting. It lies far outside the data and means nothing physical. The intercept anchors the fitted plane; interpret it only when every predictor can be 0 and the data come near that point.

## Units and comparing coefficients

The feed coefficient (8.033) is about 1700 times the size of the cutting-speed coefficient (−0.00472). That does **not** make feed 1700 times as important: feed is measured in tenths of a millimetre and speed in hundreds of metres per minute. A coefficient's size depends on its predictor's units; change feed to µm per revolution and its coefficient becomes 0.008033.

To compare, put the predictors on a common footing:

- **Over the range studied:** feed from 0.10 to 0.30 raises Ra by 8.033 × 0.20 = **1.61 µm**; speed from 100 to 250 lowers it by 0.00472 × 150 = **0.71 µm**. Over this trial's ranges, feed matters about twice as much.
- **Per standard deviation of each predictor** (standardized coefficients): 0.588 µm for feed and −0.299 µm for speed, the same conclusion.

## Unique contribution: the t-test of a coefficient

The t-test on a coefficient's line asks: **does this predictor add information beyond the others in the model?** A small p-value says yes. It does not say the predictor matters on its own, and a large p-value does not prove it has no effect; it may be that the other predictors already carry the same information.

That happens when predictors are correlated. Here they are not: the trial ran every combination of feed and speed, so the two predictors are uncorrelated by design, and the **VIF** of each is 1.000. The **variance inflation factor** of xⱼ is 1/(1 − Rⱼ²), where Rⱼ² is the R² from regressing xⱼ on the other predictors. VIF = 1 means no overlap; above about 5 (some say 10) the predictors overlap so much that SE(bⱼ) is inflated and the coefficient can swing, or even change sign, from one sample to the next. One consequence of VIF = 1 here: the simple slope of Roughness on Feed alone is also 8.033. In a designed experiment like this one, adding a predictor does not change the others' coefficients. In field data, where the predictors move together, it does (item 12.1). Module 13 (item 13.5) treats multicollinearity in full.

## In Python and R

```python
fit = sm.OLS(roughness, sm.add_constant(pd.DataFrame({'feed': feed, 'speed': speed}))).fit()
print(fit.summary())        # the coefficient table: coef, std err, t, P>|t|, [0.025 0.975]
print(fit.params['feed'], fit.bse['feed'], fit.tvalues['feed'], fit.pvalues['feed'])
print(fit.conf_int(alpha=0.05))
```

```r
fit <- lm(roughness ~ feed + speed)
summary(fit)                 # Estimate, Std. Error, t value, Pr(>|t|)
coef(summary(fit))["feed", ] # one line of the table
confint(fit, level = 0.95)
```

In the Workbench, type a predictor's name in **Coefficient to read** to see its line as separate numbers (**Chosen coefficient (b)**, **Standard error of b**, **t statistic of b**, **p-value of b**). The program's code for the Example:

[[code: st.m12.coefficients.eg]]

## Common misinterpretations to avoid

- **"The bigger coefficient is the more important predictor."** Only if the predictors are in the same units. Compare effects over a realistic change in each predictor.
- **Forgetting "with the others held fixed".** b₁ is not what happens to y when x₁ rises and the other predictors rise with it, as they may in practice.
- **Holding fixed what cannot be held fixed.** In y = β₀ + β₁x + β₂x², x² cannot stay fixed while x changes, so β₁ alone is not "the effect of x".
- **Holding fixed outside the data.** If long curing only ever came with wet mixes, the model has never seen long curing with a dry mix; a prediction there is an extrapolation.
- **Causation.** A coefficient from observational data describes an association adjusted for the predictors in the model. Only a designed experiment, like the turning trial, supports reading it as cause and effect.
- **"Not significant" read as "no effect".** It means these data could not detect one, given the other predictors.

## What to do next

On the **Exercise** tab, fit a weld-strength model with current and travel speed, read the line for travel speed, and see how it differs from the simple slope.
