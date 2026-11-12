---
title: Building a regression model
---

# Building a regression model

## The steps

1. Specify the response and candidate predictors from knowledge of the process.
2. Fit all the candidates by least squares.
3. Assess the fit: R², adjusted R², σ̂.
4. Test the whole model: the overall F-test.
5. Check the assumptions with the residuals (Module 13).
6. Refine one predictor at a time, with reasons; repeat 2–5.
7. Interpret and report: coefficients with units and "the others held fixed", the fit, and how the model was chosen.

## Selection procedures

| Procedure | Starts with | Each step | Stops when |
|---|---|---|---|
| Forward selection | no predictors | adds the predictor with the smallest p-value | no candidate would enter with p < 0.05 |
| Backward elimination | all candidates | removes the predictor with the largest p-value | every p-value in the model < 0.05 |
| Stepwise | no predictors | forward, but a predictor may also leave | no change |
| Best subsets | every combination | compares adjusted R² (or AIC, BIC) | — |

R's `step()` selects by AIC, not by p-values. Selection is exploratory: the chosen model's p-values and R² are optimistic, so validate it on new data (Module 13).

## Evidence for keeping or removing a predictor

| Evidence | Keep | Remove |
|---|---|---|
| t-test of its coefficient | small p | large p |
| Adjusted R², σ̂ | worse without it | better without it |
| Engineering knowledge | a known mechanism | no plausible mechanism |
| Residuals | a pattern appears without it | no change |
| VIF | | very high, and another predictor carries the same information |

Remove one predictor at a time: the others' p-values change when one leaves. Never choose by R², which always favors more predictors.

## In the Workbench

**Multiple regression**, **Variable selection**: *All the predictors listed*, *Forward selection (add while p < 0.05)* or *Backward elimination (drop while p ≥ 0.05)*. The **Selection** result lists each step.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 12-6 (aspects of multiple regression modeling: variable selection, multicollinearity).
- EGN3443 Module 12 on the course website (reaglin.github.io/EGN3443), "Building and Evaluating Multiple Regression Models".
