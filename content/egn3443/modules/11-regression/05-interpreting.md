---
id: st.m11.interpreting
title: Interpreting regression results
minutes: 50
objectives: [st.m11.o2, st.m11.o5]
builds-on: st.m11.evaluating
example: st.m11.interpreting.eg
exercise: st.m11.interpreting.ex
reference: [reading-regression-output, simple-linear-regression]
---

# Interpreting regression results

A fitted line is an estimate from one sample. Another ten joints, another day of plant data, would give a slightly different b₀ and b₁. This item asks what the data say about the **true** slope β₁, and how sure a prediction is. It then turns the numbers into conclusions an engineer can act on.

## The test on the slope

The question "does y depend on x at all?" is the hypothesis test

H₀: β₁ = 0 (no linear relation) against H₁: β₁ ≠ 0

It follows the steps of Module 8. The slope's **standard error** says how much b₁ would vary from sample to sample:

SE(b₁) = σ̂ / √S_xx

The test statistic is t = b₁ / SE(b₁), on **n − 2 degrees of freedom**, and the p-value is 2 × P(T > |t|), where T has the t distribution with n − 2 degrees of freedom. A small p (below α, usually 0.05) is evidence that the slope is not 0. In simple regression this is the same test as the F-test of item 11.4 (F = t²).

The **confidence interval for the slope** gives the range of slopes the data are consistent with:

b₁ ± t(α/2, n − 2) × SE(b₁)

where t(α/2, n − 2) is the t value with n − 2 degrees of freedom and α/2 above it (item 7.3). If the interval does not contain 0, the test rejects H₀ at that α.

**The four points by hand** (items 11.1 to 11.4): σ̂ = 0.949, S_xx = 5, so SE(b₁) = 0.949 / √5 = 0.424, and t = 0.8 / 0.424 = 1.886 on 2 df, p = 0.200. With t(0.025, 2) = 4.303, the 95 % interval is 0.8 ± 4.303 × 0.424 = 0.8 ± 1.825, from −1.03 to 2.63. It contains 0, so the data cannot rule out "no relation". Four points are too few to show one.

There is a test on the intercept too (H₀: β₀ = 0). It is rarely of interest, because x = 0 is usually far from the data.

## How sure is a prediction?

At a chosen x₀ the line gives ŷ₀ = b₀ + b₁x₀. Two different questions have two different intervals:

| Question | Interval | Its half-width, t(α/2, n − 2) × … |
|---|---|---|
| What is the **mean** y of all units at x₀? | **confidence interval** on the mean response | σ̂ √(1/n + (x₀ − x̄)²/S_xx) |
| What will **one new** unit at x₀ give? | **prediction interval** on a new observation | σ̂ √(**1** + 1/n + (x₀ − x̄)²/S_xx) |

The extra **1** under the square root is the scatter of a single unit around the mean. The prediction interval is therefore always wider, and it does not shrink to zero however much data you collect. Both intervals are narrowest at x₀ = x̄ and widen as x₀ moves away from it. That is one more reason not to extrapolate.

Use the **prediction interval** for a single part, a single trip or a single tree, which is the usual engineering question. Use the **confidence interval** for a long-run average, such as the mean strength of a whole production run.

## The Example: cure temperature and bond strength

Ten lap joints were cured at 60 to 105 °C and tested in shear (MPa).

[[data: st.m11.interpreting.eg]]

[[figure: st.m11.interpreting.eg 1]]

[[results: st.m11.interpreting.eg]]

Reading the results:

- **Slope** b₁ = 0.1285 MPa per °C, SE(b₁) = 0.0117, t = 10.99 on 8 df, p = 0.000004. The strength clearly rises with cure temperature.
- **95 % interval for the slope:** 0.1015 to 0.1554 MPa per °C. In engineering terms, each 10 °C more adds between about 1.0 and 1.6 MPa.
- **R² = 0.938**, σ̂ = 0.53 MPa.
- **At 88 °C**, ŷ₀ = 17.97 MPa. The mean strength of all joints cured at 88 °C is between 17.55 and 18.38 MPa (95 % confidence interval). One new joint cured at 88 °C is expected, with 95 % confidence, to be between 16.67 and 19.26 MPa (the prediction interval), more than three times as wide.

The residual plot and the Q–Q plot are under the Example's results. Check them before you trust the intervals (item 11.4).

## Statistical and practical significance

A small p says the slope is not 0. It does not say the slope is **large** or **useful**. With hundreds of observations, a slope too small to matter can have p < 0.001. Always read the size of the slope, with its interval, in the units of the problem: "between 1.0 and 1.6 MPa per 10 °C" lets an engineer decide whether raising the oven temperature is worth the energy.

And a large p is **not** proof of no relation. Twelve operators' assembly times against their age (item 11.1) give slope −0.017 min per year, p = 0.475. Its 95 % interval is −0.070 to 0.035 min per year, which contains 0. The data show no evidence of a relation, and they also rule out a large one: at most about 0.07 minutes per year faster, or 0.035 minutes per year slower, for each year of age.

## Turning results into conclusions

A good conclusion gives the direction, the size with its uncertainty, the strength of the fit, and the limits:

> Over cure temperatures from 60 to 105 °C, shear strength rose by about 1.3 MPa for each 10 °C (95 % CI 1.0 to 1.6 MPa). Temperature explained 94 % of the variation in strength. At 88 °C a single joint is expected to reach between 16.7 and 19.3 MPa. These results hold only within the tested range, for this adhesive.

Check every conclusion against these pitfalls:

- **Extrapolation:** say the range of x the data cover, and do not predict beyond it.
- **Causation:** the joints' temperature was set by the experimenter, so here a causal reading is fair. With observational data (x only recorded, not set) say "associated with", not "causes".
- **Assumptions:** if a residual check failed, the p-values and intervals cannot be trusted. Say so.

## In Python and R

```python
import numpy as np
import statsmodels.api as sm
temp = np.array([60, 65, 70, 75, 80, 85, 90, 95, 100, 105])
strength = np.array([14.6, 14.8, 15.4, 16.4, 16.8, 17.4, 19.4, 18.1, 19.7, 20.0])
fit = sm.OLS(strength, sm.add_constant(temp)).fit()
print(fit.summary())              # coefficients with se, t, p
print(fit.conf_int(0.05))         # slope: 0.1015 to 0.1554
pred = fit.get_prediction(np.array([[1.0, 88.0]])).summary_frame(alpha=0.05)
print(pred[["mean", "mean_ci_lower", "mean_ci_upper", "obs_ci_lower", "obs_ci_upper"]])
```

```r
temp <- c(60, 65, 70, 75, 80, 85, 90, 95, 100, 105)
strength <- c(14.6, 14.8, 15.4, 16.4, 16.8, 17.4, 19.4, 18.1, 19.7, 20.0)
fit <- lm(strength ~ temp)
summary(fit)                      # Estimate, Std. Error, t value, Pr(>|t|)
confint(fit)                      # 95 % intervals for b0 and b1
new <- data.frame(temp = 88)
predict(fit, new, interval = "confidence")   # mean response: 17.55 to 18.38
predict(fit, new, interval = "prediction")   # one new joint: 16.67 to 19.26
```

In statsmodels, `mean_ci` is the confidence interval and `obs_ci` the prediction interval.

[[code: st.m11.interpreting.eg]]

## Common mistakes

- **Using the confidence interval for one unit.** A single joint, trip or part needs the prediction interval, which is wider.
- **z in place of t.** The slope's interval and the prediction intervals use t with n − 2 degrees of freedom. With n = 12, t(0.025, 10) = 2.228, not 1.96.
- **"p > 0.05, so x has no effect."** It means the data cannot show one. Read the confidence interval to see how large an effect is still possible.
- **"p < 0.05, so x matters a lot."** Significance is not size. Read the slope in its units.
- **Leaving out the range.** Every prediction and conclusion holds only over the x values the data cover.

## What to do next

On the **Exercise** tab, find the 95 % interval for the slope of fuel use against payload, and the prediction interval for one 20-tonne trip, and press **Check my answers**.
