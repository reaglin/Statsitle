---
id: st.m11.evaluating
title: Evaluating model performance
minutes: 55
objectives: [st.m11.o2, st.m11.o4]
builds-on: st.m11.simple
example: st.m11.evaluating.eg
exercise: st.m11.evaluating.ex
reference: [regression-fit-and-residuals, simple-linear-regression]
---

# Evaluating model performance

Least squares always gives a line, even through points with no relation at all. Two questions decide whether the line is any use:

1. **How well does it fit?** R², the residual standard error and the F-test measure that.
2. **Is a straight line with random scatter the right model?** The residuals answer that.

## Splitting the variation: the sums of squares

The variation of y around its mean splits into two parts: what the line explains, and what is left over.

| Sum of squares | Formula | Measures |
|---|---|---|
| **SS total** | Σ(y − ȳ)² | all the variation in y |
| **SS regression** | Σ(ŷ − ȳ)² | the part the line explains |
| **SS error** | Σ(y − ŷ)² | the part left over: the squared residuals |

and always **SS total = SS regression + SS error**. For the four points of items 11.1 and 11.2: SS total = S_yy = 5, SS error = 1.80 (the table in item 11.2), so SS regression = 3.20.

## R² and adjusted R²

**R² = SS regression / SS total = 1 − SS error / SS total**, the share of the variation in y the line explains. For the four points, R² = 3.20 / 5 = 0.64, the same as r² = 0.80² in item 11.1. R² is between 0 and 1. In simple regression R² = r².

How high is "good"? It depends on the field. A calibration curve should have R² above 0.99. A model of field data such as traffic, soils or human behavior may be useful at 0.5. R² alone never proves the model is right: a curved relation can have a high R² (below).

**Adjusted R²** charges for each predictor:

adjusted R² = 1 − (1 − R²)(n − 1) / (n − k − 1)

where k is the number of predictors (1 in simple regression). For the four points: 1 − 0.36 × 3/2 = **0.46**. With few observations the penalty is large; with many it is small. It matters when models with different numbers of predictors are compared (item 11.3, Module 12).

## The typical miss: σ̂ and RMSE

R² has no units. An engineer also needs to know **how far off a prediction typically is, in y's units**.

- The **residual standard error** σ̂ = √(SS error / (n − 2)) estimates σ, the standard deviation of the points around the true line. It divides by n − 2, the **error degrees of freedom**: two are used up by estimating b₀ and b₁. For the four points, σ̂ = √(1.80/2) = **0.949**. Statistics+, R's `summary(lm)` and statsmodels report this one.
- The **root-mean-square error** RMSE = √(SS error / n) divides by n. It is the usual measure when models are compared on prediction (Module 13), and the course's reading uses it. For the four points, RMSE = √(1.80/4) = 0.671.

With many observations the two are nearly the same. Either way, about 95 % of the points lie within ±2σ̂ of the line, if the residuals are roughly normal.

## The F-test: is there a relation at all?

The **ANOVA table** for regression sets out the sums of squares with their degrees of freedom (df). A **mean square** (MS) is a sum of squares divided by its df:

| Source | df | SS | MS | F |
|---|---|---|---|---|
| Regression | 1 | SS regression | MS_R = SS regression / 1 | F = MS_R / MS_E |
| Error | n − 2 | SS error | MS_E = SS error / (n − 2) = σ̂² | |
| Total | n − 1 | SS total | | |

F tests H₀: β₁ = 0 (y does not depend on x linearly) against H₁: β₁ ≠ 0. If H₀ were true, F would follow the F distribution with 1 and n − 2 degrees of freedom (item 9.4 met the F distribution). A large F, and a p-value below α, is evidence of a relation.

For the four points: MS_R = 3.20, MS_E = 0.90, F = 3.20 / 0.90 = **3.56** on 1 and 2 df, p = 0.200. With only four points, even r = 0.80 is not significant at α = 0.05.

In simple regression the F-test and the t-test on the slope (item 11.5) are the same test: **F = t²**, and their p-values are equal. F matters more in multiple regression, where it tests all the predictors at once (Module 12).

## The Example: pavement condition

Twelve road sections, aged 1 to 12 years since resurfacing, with their Pavement Condition Index (PCI, 100 = new).

[[data: st.m11.evaluating.eg]]

[[figure: st.m11.evaluating.eg 1]]

[[results: st.m11.evaluating.eg]]

Reading the fit: ŷ = 95.47 − 3.059 × age, so the PCI falls about 3.1 points a year. R² = 0.957: age explains 96 % of the variation in PCI. σ̂ = 2.46 PCI points is the typical miss, and about 95 % of sections are within ±5 PCI of the line (±2σ̂). F = 221.6 on 1 and 10 df with p < 0.0001, so the relation is real. (The RMSE would be √(60.41/12) = 2.24.)

## Checking the assumptions with the residuals

The tests and intervals of regression rest on four assumptions about the errors. A memory aid is **LINE**:

| Assumption | Means | Check |
|---|---|---|
| **L**inearity | the mean of y is a straight line in x | residuals against fitted values: no curve |
| **I**ndependence | one error tells nothing about the next | residuals in time order: no runs or waves |
| **N**ormality | errors are roughly normal | normal Q–Q plot of residuals; Shapiro–Wilk p |
| **E**qual variance | the same spread at every x (**homoscedasticity**) | residuals against fitted values: no funnel |

The main tool is the plot of **residuals against fitted values**. If the model is right, it is a shapeless horizontal band around 0. The pavement data pass:

[[figure: st.m11.evaluating.eg 2]]

The **normal Q–Q plot** puts the sorted standardized residuals against the values a normal distribution would give. Normal residuals lie close to the gray line:

[[figure: st.m11.evaluating.eg 3]]

The results also give the **Shapiro–Wilk p of residuals** (item 9.4 introduced the test): 0.988 here, no evidence against normality. Independence is about the order the data were collected in. Plot the residuals in that order, and look for runs of the same sign. Module 13 tests it with the Durbin–Watson statistic.

### What a failed check looks like

**A curve: linearity fails.** Tool wear speeds up as the cutting edge degrades. The straight line still gets R² = 0.952:

[[figure: st.m11.evaluating.curved 1]]

The residuals show what R² hides. They are positive at both ends and negative in the middle, a U shape:

[[figure: st.m11.evaluating.curved 2]]

The line underpredicts at the start and the end, and overpredicts in the middle. A curve, or a transformation of x or y, fits better. Module 13 shows how.

**A funnel: equal variance fails.** Larger office buildings cost more to run, and their costs vary more:

[[figure: st.m11.evaluating.funnel 2]]

The residuals fan out as the fitted value grows. The line itself is fine, but σ̂ is an average spread that is too large for small buildings and too small for large ones, so the intervals of item 11.5 are wrong at both ends. A log of y often evens the spread (Module 13).

When a check fails, say so, and do not trust the p-values and intervals until the model is fixed. Module 13 goes further: leverage and Cook's distance for single influential points, the Durbin–Watson test, transformations, and cross-validation.

## In Python and R

```python
import numpy as np
import statsmodels.api as sm
age = np.arange(1, 13)
pci = np.array([93, 85, 88, 88, 79, 75, 73, 73, 69, 65, 60, 59])
fit = sm.OLS(pci, sm.add_constant(age)).fit()
print(fit.rsquared, fit.rsquared_adj)        # 0.9568, 0.9525
print(np.sqrt(fit.mse_resid))                # sigma-hat 2.4579
print(np.sqrt(fit.ssr / len(pci)))           # RMSE 2.2437
print(fit.fvalue, fit.f_pvalue)              # 221.56, 3.8e-08
# residual plot: plt.scatter(fit.fittedvalues, fit.resid); plt.axhline(0)
```

```r
age <- 1:12
pci <- c(93, 85, 88, 88, 79, 75, 73, 73, 69, 65, 60, 59)
fit <- lm(pci ~ age)
summary(fit)            # R², adjusted R², residual standard error, F
anova(fit)              # the ANOVA table
plot(fit, which = 1:2)  # residuals vs fitted, normal Q-Q
```

[[code: st.m11.evaluating.eg]]

## Common mistakes

- **Judging a model by R² alone.** The tool-wear line has R² = 0.95 and the wrong shape. Always look at the residual plot.
- **Dividing by n for σ̂.** σ̂ divides SS error by n − 2. √(SS error / n) is the RMSE, a different (smaller) number.
- **Giving σ̂² for σ̂.** MS_E = σ̂² is a variance, in squared units. Take its square root.
- **Reading F as t.** In simple regression F = t², so F is the square of the slope's t.
- **Calling the residual plot "fine" because the points are near 0.** Look at the pattern, not the size: a curve or a funnel matters even when the residuals are small.

## What to do next

On the **Exercise** tab, judge how well a line fits fourteen compressors' energy use against their age: find R², σ̂ and F, and press **Check my answers**.
