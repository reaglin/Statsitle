---
id: st.m13.cross-validation
title: Cross-validation techniques
minutes: 50
objectives: [st.m13.o3]
builds-on: st.m13.residuals
example: st.m13.cross-validation.eg
exercise: st.m13.cross-validation.ex
reference: [validation-cross-validation]
---

# Cross-validation techniques

## Why the fit's own error is optimistic

Every number so far (R², σ̂, the residuals) was computed on the same rows the model was fitted to. Least squares chose the coefficients to make **those** residuals small, so they flatter the model. Add any predictor, even a column of random numbers, and SS error falls and R² rises. A model with enough terms can follow the noise in its data as well as the signal: it is **overfitted**, and it predicts new data worse than its fit suggests.

The question an engineer asks of a model is "how far off will its **next** prediction be?". The honest way to answer is to test it on rows it has **not seen**. **Cross-validation** is the family of methods that do this with the data you have.

All of them report the error as a **root-mean-square error**, in y's units:

RMSE = √( Σ (yᵢ − ŷᵢ)² / m )

over the m predictions being tested. Two versions of it appear in this item:

- **In-sample RMSE** = √(SS error / n): every row, predicted by the model fitted to all the rows, itself included. It is a little smaller than σ̂ = √(SS error/(n − p)), which corrects for the p fitted coefficients, and it is the optimistic number.
- **Out-of-sample RMSE**: each row predicted by a model fitted **without** it. This is the estimate of the error on new data.

## The holdout method (train/test split)

(The idea is the *holdout method*; the Workbench's settings call it **Hold-out**.)

1. Set aside a share of the rows at random as the **test set** (20 to 30 % is usual).
2. Fit the model to the rest, the **training set**.
3. Predict the test rows and compute the RMSE on them.

It mirrors how a model is used (fitted once, then applied to new cases) and needs only one fit, so it suits **large** data sets. Its weaknesses show with small ones: the model is fitted to fewer rows, the test RMSE rests on only a few predictions, and it depends heavily on **which rows were drawn**.

## k-fold cross-validation

1. Cut the rows into k groups (**folds**) of nearly equal size.
2. For each fold in turn: fit the model to the other k − 1 folds and predict the rows of this fold.
3. Every row has now been predicted once by a model that did not see it. Pool all n squared prediction errors: CV RMSE = √( Σ (yᵢ − ŷ₍ᵢ₎)² / n ), where ŷ₍ᵢ₎ is the prediction for row i from the fit without its fold.

k = 5 or 10 is usual. Every row is used for testing once and for training k − 1 times, so k-fold uses the data much better than one split, and its estimate is steadier. It costs k fits.

Statistics+ cuts the folds as **contiguous blocks** of rows in table order (rows 1–4, 5–8, … for n = 20 and k = 5), like scikit-learn's KFold without shuffling, so its result can be reproduced exactly. (When k does not divide n, the two put the one-row-larger folds at opposite ends, so their results can differ slightly; the Code tab writes the program's own folds.) If the table is sorted by a predictor or by time, contiguous folds make each fold a separate stretch of the data, and the model must extrapolate to it. Shuffle the rows first if that is not the question you mean to ask.

## Leave-one-out cross-validation (LOOCV)

LOOCV is k-fold with **k = n**: each row is its own fold. Fit to n − 1 rows, predict the one left out, n times. It uses as much data as possible for each fit, and it has no randomness: the same data always give the same answer. Its estimate is more variable than 5- or 10-fold's, and for most models it is the most expensive: n fits.

For least-squares regression there is a shortcut, so LOOCV costs a single fit. The prediction error for the left-out row is

yᵢ − ŷ₍ᵢ₎ = eᵢ / (1 − hᵢᵢ)

where eᵢ is the ordinary residual and hᵢᵢ the leverage of item 13.3. The sum of these squared is called **PRESS** (the prediction sum of squares), and LOOCV RMSE = √(PRESS/n). A high-leverage row has a small residual but a large leave-one-out error: the fit leaned on it.

## Comparing the three

| | Holdout | k-fold | Leave-one-out |
|---|---|---|---|
| Procedure | One random split, train and test | k folds, each held out once | n folds of one row |
| Fits needed | 1 | k | n (1 for least squares, with PRESS) |
| Depends on the random draw | Yes, strongly | Somewhat, when the rows are shuffled into folds (Statistics+ does not shuffle) | No |
| Each estimate rests on | the test rows only | all n rows | all n rows |
| Use when | n is large; a quick check, or a final test kept apart from model choosing | the usual choice for small and moderate n | n is small; you want an exact, repeatable number |

A common practice with plenty of data is to do both: choose the model by k-fold cross-validation on a training set, then report its error once on a test set that was never used for choosing.

## The Example: energy use of an office building

[[data: st.m13.cross-validation.eg]]

The model predicts daily electricity use from the heating degree-days and the occupancy.

[[results: st.m13.cross-validation.eg]]

[[figure: st.m13.cross-validation.eg 1]]

Read the three error numbers together:

| Measure | RMSE (kWh) |
|---|---|
| In-sample | 12.44 |
| 5-fold cross-validation | 13.68 |
| Leave-one-out (k = 20) | 14.49 |
| Hold-out, 5 test days, seed 1 | 11.80 |

The cross-validated errors are 10 to 17 % above the in-sample error: that is the usual price of predicting rows the model has not seen, and it is modest, so the model **generalizes** well. Expect new days to be predicted to within about 14 kWh (one RMSE) typically.

The hold-out RMSE, 11.80 kWh, is **below** the in-sample error. That is luck: seed 1 happened to draw five days the model predicts well. Change **Hold-out seed** and the test rows and the RMSE change:

| Hold-out seed | Test rows | Hold-out RMSE (kWh) |
|---|---|---|
| 1 | 3, 5, 7, 13, 14 | 11.80 |
| 2 | 1, 6, 7, 17, 19 | 12.58 |
| 3 | 7, 12, 16, 17, 20 | 11.61 |
| 4 | 1, 5, 10, 16, 17 | 10.94 |
| 5 | 1, 3, 8, 13, 16 | 14.50 |
| 6 | 4, 8, 11, 15, 16 | 16.86 |

One split of 20 rows gives anything from 10.9 to 16.9 kWh. That spread is why, with small data sets, k-fold or leave-one-out is preferred: every row is tested, and the answer does not hang on one draw.

## Cross-validation catches a useless predictor

The table also records the wind speed. Add it: set **Predictor columns** to *HDD, Occupancy, Wind*.

| Model | R² | Adjusted R² | In-sample RMSE | 5-fold CV RMSE | LOOCV RMSE |
|---|---|---|---|---|---|
| HDD, Occupancy | 0.9774 | 0.9747 | 12.44 | 13.68 | 14.49 |
| HDD, Occupancy, Wind | 0.9787 | 0.9748 | 12.05 | 14.12 | 14.60 |

Adding Wind raised R² and lowered the in-sample RMSE, as adding any column must. Adjusted R² barely moved. But both cross-validated errors **rose**: the extra coefficient fitted noise in these 20 days and made the predictions of unseen days worse. Wind's own p-value is 0.32. The simpler model is the better one to use.

## In Python and R

```python
import numpy as np
from sklearn.linear_model import LinearRegression
from sklearn.model_selection import KFold, LeaveOneOut, cross_val_predict
# hdd, occupancy, kwh: the Example's columns as numpy arrays (Copy the data, or the Code tab, has them)
X = np.column_stack([hdd, occupancy]); y = kwh
for cv in (KFold(5), LeaveOneOut()):                        # KFold(5) without shuffling: contiguous folds
    pred = cross_val_predict(LinearRegression(), X, y, cv=cv)
    print(np.sqrt(np.mean((y - pred) ** 2)))
```

```r
fit <- lm(kWh ~ HDD + Occupancy, data = energy)               # energy: a data frame of the Example's columns (the Code tab builds it)
sqrt(mean(residuals(fit)^2))                                 # in-sample RMSE
sqrt(mean((residuals(fit) / (1 - hatvalues(fit)))^2))        # LOOCV RMSE, by PRESS
test <- c(3, 5, 7, 13, 14)                                    # the rows seed 1 drew
tr <- lm(kWh ~ HDD + Occupancy, data = energy[-test, ])
sqrt(mean((energy$kWh[test] - predict(tr, energy[test, ]))^2)) # hold-out RMSE
```

The Example's **Code** tab writes the whole calculation, the folds and the hold-out rows included, without scikit-learn. A hold-out split in Python (train_test_split) or R (sample) would draw different rows from the same seed, which is why the program lists the rows it drew.

[[code: st.m13.cross-validation.eg]]

## In the Workbench

**Multiple regression** reports **In-sample RMSE** and **Cross-validated RMSE** (set **Cross-validation folds (k)**; k = n gives leave-one-out, 0 skips it), and **Hold-out test rows**, **Rows in the test set** and **Hold-out test RMSE** (set **Hold-out share**, 0 to skip, and **Hold-out seed**). A model with one predictor can be cross-validated there too: list just that column.

## Common mistakes

- **Judging a model by its in-sample error.** It always looks better than the model will do.
- **Trusting one hold-out split of a small data set.** Change the seed and see how much it moves.
- **Comparing models by R².** R² never falls when a term is added. Compare by cross-validated RMSE (or adjusted R²).
- **Letting the test rows leak into the choice.** If you try many models and keep the one with the best test RMSE, that RMSE is optimistic too.

## What to do next

On the **Exercise** tab, a surface-roughness model is cross-validated three ways. Find the in-sample, leave-one-out and hold-out RMSEs, and press **Check my answers**.
