---
title: Model evaluation: training, testing and cross-validation
---

# Model evaluation: training, testing and cross-validation

| Quantity | Formula |
|---|---|
| RMSE | √((1/n) Σ (yᵢ − ŷᵢ)²) |
| In-sample (training) error | the error on the data used to fit the model: optimistic |
| k-fold cross-validation | fit on k − 1 folds, predict the held-out fold, k times; average the errors (item 13.4) |
| Leave-one-out | k = n |
| Hold-out (test) error | the error on data kept out of all fitting and model choice |

**Workflow:** split off the test set; choose the model by cross-validation on the training set; fit; report the test error
once. **Leakage:** any information about the test cases or the outcome in the training predictors.

## Python and R

| Task | Python | R |
|---|---|---|
| Train/test split | `sklearn.model_selection.train_test_split(X, y, test_size=0.25)` | `rsample::initial_split(d, prop = 0.75)` |
| Cross-validation | `cross_val_score(model, X, y, cv=5, scoring="neg_root_mean_squared_error")` | `caret::train(..., trControl = trainControl(method = "cv", number = 5))` |

**Read more:** James, Witten, Hastie and Tibshirani, *An Introduction to Statistical Learning*, 2nd ed. (free at
statlearning.com), chapter 5.
