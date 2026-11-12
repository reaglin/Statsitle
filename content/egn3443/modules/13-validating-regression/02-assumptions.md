---
id: st.m13.assumptions
title: Assessing model assumptions
minutes: 45
objectives: [st.m13.o1]
builds-on: st.m13.foundations
example: st.m13.assumptions.eg
exercise: st.m13.assumptions.ex
reference: [validation-assumptions, validation-residual-plots]
---

# Assessing model assumptions

Item 13.1 named the four assumptions, **LINE**: linearity, independence, normality and equal variance of the errors. This item takes them one at a time: how to check each, and what goes wrong when it fails.

## Linearity

**Check:** the residuals against the fitted values (and, in multiple regression, against each predictor). A curve or arch means the mean of y is not a straight-line function of the predictors.

**When it fails:** the model is biased. It predicts too high in some ranges and too low in others, and no amount of data fixes that. Every other check is secondary: fix the form first (item 13.6).

## Equal variance (homoscedasticity)

**Check:** the same residual plot. The vertical spread should be about the same from left to right. A funnel, wide at one end and narrow at the other, means **heteroscedasticity** (unequal variances).

**When it fails:** the coefficients are still unbiased, but their standard errors, and so the t-tests, p-values and confidence intervals, are wrong. Prediction intervals are too wide where the scatter is small and too narrow where it is large, which is usually where predictions matter. A log of y often evens out a spread that grows with y (item 13.6).

## Independence

The errors of different observations must be unrelated. It fails most often for data **collected in time order**: an instrument that drifts, a process that warms up, the weather of consecutive days. A large error today then makes a large error tomorrow likely.

**Check:** plot the residuals in the order the data were collected. Long runs of the same sign mean the errors are related (**positively autocorrelated**). The **Durbin–Watson statistic** puts a number on it:

d = Σ(eᵢ − eᵢ₋₁)² / Σeᵢ²

where the top sum runs over successive residuals (i = 2 to n) and the bottom over all of them. d runs from 0 to 4 and is about 2(1 − r₁), where r₁ is the correlation between each residual and the one before it.

| d | Means |
|---|---|
| near 2 | successive residuals unrelated (r₁ near 0) |
| well below 2 | positive autocorrelation: neighbors alike, long runs |
| well above 2 | negative autocorrelation: residuals alternate in sign |

How far below 2 is "well below" depends on n and the number of predictors; published tables give two bounds. For n = 20 and one predictor at α = 0.05 they are d_L = 1.20 and d_U = 1.41: below 1.20 the positive autocorrelation is significant, above 1.41 it is not, and between them the test cannot decide. A common rough rule: below about 1.5 deserves a look.

Two warnings. Durbin–Watson only means something when the **rows are in time (or collection) order**: shuffle the rows and d changes, though the fit does not. And when the rows are sorted by x, a **curved** relation also makes runs and a small d: the braking data of item 13.1 give d = 0.82, from curvature, not from time. Check linearity first.

**When it fails:** the standard errors are too small, so the p-values are too small and the intervals too narrow: the data hold less information than n independent observations would.

## Normality of the errors

**Check:** a **normal Q–Q plot** of the residuals. The residuals, sorted, are plotted against the values a normal sample of the same size would be expected to have. The program plots **standardized residuals** (each residual divided by its standard error, so they are on a scale of about −2 to 2; item 13.3). If the errors are normal, the points lie close to the straight reference line. A bend at the ends means skewness or heavy tails.

The **Shapiro–Wilk test** of Module 9 puts a number on it, applied to the **residuals**, not to y. H₀: the errors are normal. A small p (below 0.05) is evidence against normality. Testing y itself is a common mistake: y is spread out by its trend with x, which is not an error at all.

**When it fails:** of the four, this matters least for the coefficients when n is moderate or large: the central limit theorem (Module 6) makes the estimates nearly normal anyway. It matters most for **prediction intervals**, which are built on the normal shape of single errors, and for small samples.

## What each failure does

| Assumption fails | Coefficients | Standard errors, p-values, intervals | Predictions |
|---|---|---|---|
| Linearity | biased | meaningless | wrong in parts of the range |
| Independence | unbiased | too optimistic (too small) | intervals too narrow |
| Normality | unbiased | fine for large n | prediction intervals off |
| Equal variance | unbiased | wrong | intervals wrong where the spread differs |

## The Example: a load cell that creeps

A load cell weighed the same 100 kg reference mass once a day for 20 days.

[[data: st.m13.assumptions.eg]]

[[figure: st.m13.assumptions.eg 1]]

[[results: st.m13.assumptions.eg]]

The line says the reading drifts by 0.122 kg per day (p < 0.0001). Now the assumptions:

[[figure: st.m13.assumptions.eg 2]]

Here x is the day, so the fitted values increase with the day, and this plot is also the residuals **in time order**. The residuals come in runs: positive on days 1 to 3, negative on all ten days from 4 to 13, then positive on six of the last seven days. **Durbin–Watson = 0.77**, far below the bound 1.20 for n = 20: the errors of successive days are related (their lag-one correlation is 0.59). The reading wanders as well as drifting, so the 20 days hold less information than 20 independent readings, and the standard error of the drift rate, 0.0148 kg per day, is too small.

[[figure: st.m13.assumptions.eg 3]]

The Q–Q plot is close to its line, and Shapiro–Wilk gives W = 0.975 with p = 0.859: no evidence against normality. So one assumption fails and another holds; the checks are separate questions.

What to do? The engineer would look for the cause (temperature in the lab is a common one, and adding it as a predictor may remove the runs), or use a time-series method that models the autocorrelation, which is beyond this course.

## In Python and R

```python
import numpy as np
import statsmodels.api as sm
from statsmodels.stats.stattools import durbin_watson
from scipy import stats
day = np.arange(1, 21)
reading = np.array([100.47, 100.8, 100.4, 99.9, 100.05, 100.44, 100.52, 100.21, 100.25, 100.88,
                    100.58, 101.08, 101.35, 101.59, 101.89, 101.77, 101.81, 102.43, 102.32, 102.23])
fit = sm.OLS(reading, sm.add_constant(day)).fit()
print(durbin_watson(fit.resid))          # 0.771
print(stats.shapiro(fit.resid))          # W and p of the residuals
sm.qqplot(fit.get_influence().resid_studentized_internal, line="45")
```

```r
day <- 1:20
reading <- c(100.47, 100.8, 100.4, 99.9, 100.05, 100.44, 100.52, 100.21, 100.25, 100.88,
             100.58, 101.08, 101.35, 101.59, 101.89, 101.77, 101.81, 102.43, 102.32, 102.23)
fit <- lm(reading ~ day)
e <- residuals(fit)
sum(diff(e)^2) / sum(e^2)                # Durbin–Watson (car::durbinWatsonTest gives a p-value)
shapiro.test(e)
plot(fit, which = 2)                     # normal Q–Q plot of the standardized residuals
```

[[code: st.m13.assumptions.eg]]

## In the Workbench

**Simple linear regression** and **Multiple regression** both report **Durbin–Watson**, **Shapiro–Wilk W of residuals** and **Shapiro–Wilk p of residuals** at the bottom of the results, and draw the residuals against the fitted values and the normal Q–Q plot on the Plots tab. Durbin–Watson uses the rows in the order of the data table, so keep the table in collection order.

## Common mistakes

- **Testing y for normality** instead of the residuals.
- **Reading Durbin–Watson on rows in an arbitrary order.** It is about order; it means nothing if the order means nothing.
- **Blaming independence for curvature.** A small d on rows sorted by x can be a curve the line misses. Look at the residual plot first.
- **Treating a large Shapiro–Wilk p as proof** of normality. With 15 or 20 residuals the test has little power; look at the Q–Q plot too.

## What to do next

On the **Exercise** tab, delivery times are fitted against route length. Find the Durbin–Watson statistic and Shapiro–Wilk's W and p of the residuals, and press **Check my answers**.
