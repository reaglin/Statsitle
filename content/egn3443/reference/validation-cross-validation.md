---
title: Cross-validation
---

# Cross-validation

RMSE = √(Σ(yᵢ − ŷᵢ)²/m) over the m predictions tested, in y's units.

| Measure | How | Fits | Notes |
|---|---|---|---|
| In-sample RMSE | √(SS error/n), all rows, model fitted to all | 1 | optimistic; σ̂ = √(SS error/(n − p)) is slightly larger |
| Holdout | fit to a random training set (70–80 %), RMSE on the test set | 1 | depends on the draw; suits large n; keep a final test set apart |
| k-fold | k folds; each predicted by the fit to the other k − 1; pool all n errors | k | k = 5 or 10 usual; the standard choice for small and moderate n |
| Leave-one-out (LOOCV) | k = n | n (1 for least squares) | no randomness; more variable estimate |

**PRESS shortcut** (least squares): the leave-one-out error of row i is eᵢ/(1 − hᵢᵢ), so LOOCV RMSE = √(Σ(eᵢ/(1 − hᵢᵢ))²/n).

**Reading the result.** Cross-validated RMSE close to the in-sample RMSE: the model generalizes. Much larger: overfitting; simplify. Compare candidate models by their cross-validated RMSE, not by R².

## Python and R

| Task | Python | R |
|---|---|---|
| k-fold predictions | `cross_val_predict(LinearRegression(), X, y, cv=KFold(5))` | a loop over the folds with `lm` on the training rows and `predict` on the test rows |
| LOOCV | `cv=LeaveOneOut()` | `sqrt(mean((residuals(fit) / (1 - hatvalues(fit)))^2))` |
| Holdout split | `train_test_split(X, y, test_size=0.25, random_state=1)` | `test <- sample(n, round(0.25 * n))` |

## In the Workbench

**Multiple regression**: **Cross-validation folds (k)** (contiguous folds in table order; k = n for leave-one-out; 0 skips it) gives **Cross-validated RMSE** beside **In-sample RMSE**. **Hold-out share** and **Hold-out seed** give **Hold-out test rows**, **Rows in the test set** and **Hold-out test RMSE**. The seed is the program's own; Python and R would draw other rows from the same seed, so the Code tab lists the rows.

## Sources

- James, G., Witten, D., Hastie, T. and Tibshirani, R., *An Introduction to Statistical Learning*, 2nd ed., 2021, Section 5.1 (cross-validation).
- Montgomery, D. C., Peck, E. A. and Vining, G. G., *Introduction to Linear Regression Analysis*, 5th ed., 2012, Chapter 11 (validation of regression models, data splitting) and Section 4.3 (the PRESS statistic).
