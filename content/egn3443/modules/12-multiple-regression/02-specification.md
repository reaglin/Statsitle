---
id: st.m12.specification
title: Model specification and structure
minutes: 40
objectives: [st.m12.o1, st.m12.o5]
builds-on: st.m12.simple-to-multiple
example: st.m12.specification.eg
quiz: quizzes/m12-specification.json
reference: [multiple-regression]
---

# Model specification and structure

**Specifying** a model means deciding, before fitting, what the response is, which predictors go in and in what form. The least-squares arithmetic is the same for any choice; whether the answer means anything depends on the choice.

## The equation, term by term

Y = β₀ + β₁X₁ + β₂X₂ + … + βₖXₖ + ε

Every term is in the units of Y. So each coefficient βⱼ has the units of **Y per unit of Xⱼ**: kWh per °C, MPa per day, L/100 km per tonne. β₀ is in the units of Y: the mean response when every predictor is 0 (item 12.3 says when that means anything).

Three counts describe a fitted model:

| Symbol | What it counts |
|---|---|
| **n** | observations (rows of the data table) |
| **k** | predictors |
| **p = k + 1** | coefficients estimated, the intercept included |

The **error degrees of freedom** are **n − k − 1** = n − p: the observations left over after p coefficients have been estimated. They are the divisor in σ̂, the degrees of freedom of every coefficient's t-test, and the denominator degrees of freedom of the F-test (item 12.4). With k = 1 they are Module 11's n − 2.

You need more observations than coefficients, n > k + 1, just to fit the model, and many more to estimate it well. A common rule of thumb asks for at least 10 to 15 observations per predictor.

## The Example: electricity use of an office building

A facilities engineer recorded 15 weekdays: the electricity used (kWh), the mean outdoor temperature (°C), the number of people in the building (Occupancy) and the hours of sunshine. The model has k = 3 predictors, p = 4 coefficients, and n − k − 1 = 15 − 3 − 1 = **11** error degrees of freedom.

[[data: st.m12.specification.eg]]

[[results: st.m12.specification.eg]]

Energy = 155.8 + 12.09·Outdoor temp + 1.907·Occupancy − 2.13·Sunshine

- **12.09 kWh per °C**: on a day 1 °C warmer, with the same occupancy and sunshine, the building uses about 12 kWh more (air conditioning).
- **1.907 kWh per person**: each extra person adds about 1.9 kWh at the same weather.
- **−2.13 kWh per hour of sunshine**, but with p = 0.51 and a 95 % interval from −9.03 to 4.77: these data cannot tell whether sunshine matters. Whether to keep it is a model-building decision (item 12.5).

For a day at 28 °C with 90 people and 6 hours of sunshine the model predicts 155.79 + 12.089 × 28 + 1.9072 × 90 − 2.128 × 6 = **653.2 kWh**. The residual standard error σ̂ = 26.7 kWh is the typical size of a day's miss.

## Selecting predictor variables

Choose predictors from knowledge of the process first, and from the data second.

- **A reason to expect an effect.** Temperature drives cooling load; occupancy drives lighting, computers and elevators. A column that happens to be in the spreadsheet is not a reason.
- **Known when you predict.** A predictor you will not have at prediction time (the month's total bill, to predict a day's use) is no use for prediction.
- **Not a consequence of the response.** Using the chiller's run hours to explain electricity use puts part of the answer among the predictors.
- **Not two measures of the same thing.** Temperature in °C and in °F are exact linear functions of each other: least squares cannot separate them, and the fit fails (the Workbench reports the predictors as collinear). Predictors that are nearly the same, such as outdoor temperature and cooling degree-hours, can be fitted, but their coefficients become unstable; that is **multicollinearity**, measured by the variance inflation factor (VIF) in item 12.3 and studied in Module 13.
- **Categories as 0/1 columns.** A category such as "weekday or weekend" enters as an **indicator** column: 1 for weekend, 0 for weekday. A category with c levels needs c − 1 indicators.

## "Linear" means linear in the coefficients

A multiple **linear** regression is linear in the β's, not necessarily in the x's. These are all linear models, fitted by the same least squares:

- y = β₀ + β₁x + β₂x² + ε, a **polynomial** model (the predictors are x and x²);
- y = β₀ + β₁ ln x₁ + β₂x₂ + ε, a transformed predictor;
- y = β₀ + β₁x₁ + β₂x₂ + β₃x₁x₂ + ε, an **interaction** (the effect of x₁ depends on x₂).

y = β₀·exp(β₁x) + ε is not linear in β₁, so it is not a linear model as written (taking logs of y is one way round, as in Module 11).

## The role of the error term

ε collects everything the predictors leave out: factors never measured (the thermostat setting, a door left open), measurement error, and plain day-to-day variation. Its standard deviation σ says how far individual responses scatter around the regression plane, and it is estimated by the **residual standard error**

σ̂ = √(SS_E / (n − k − 1))

## Assumptions underlying the model

The t-tests, F-test and intervals of a multiple regression rest on four assumptions about ε, the same as in Module 11 (remember them as **LINE**):

1. **Linearity**: the mean of Y is β₀ + β₁X₁ + … + βₖXₖ: the right predictors, in the right form.
2. **Independence**: the errors of different observations are independent (no drift from one day to the next).
3. **Normality**: ε is normally distributed.
4. **Equal variance**: ε has the same standard deviation σ at every combination of the predictors.

And one about the predictors: none is an exact linear combination of the others. The residuals are how you check the four; Module 13 teaches the residual plots and tests. A badly specified model, one that leaves out an important predictor or uses the wrong form, usually shows up there as a pattern in the residuals.

## Common mistakes

- **Counting the coefficients wrong.** k predictors give k + 1 coefficients, so the error degrees of freedom are n − k − 1, not n − k.
- **Putting in every column available.** More predictors always fit the sample at least as well; item 12.4 shows why that is not the same as a better model.
- **Forgetting the units.** A coefficient's size depends on its predictor's units (item 12.3).

## What to do next

On the **Exercise** tab, answer six practice questions on specifying a model. A wrong answer shows the explanation, so you can learn from it.
