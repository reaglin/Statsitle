---
id: st.m23.learning
title: "Learning from data: training, testing and cross-validation"
minutes: 50
objectives: [st.m23.o1]
example: st.m23.learning.eg
exercise: st.m23.learning.ex
reference: [model-evaluation]
---

# Learning from data: training, testing and cross-validation

"Machine learning" fits a model to data so that it **predicts well on data it has not seen**. That last part is the whole point, and the main trap. A model can always be made to fit the data it was trained on better, by adding predictors or flexibility, while it gets **worse** at predicting new cases. This is **overfitting**, and data science guards against it with the tools of Module 13: a **test set** and **cross-validation**.

## Training error and test error

- The **training** (in-sample) error: how well the model fits the data used to fit it. It always falls as the model grows.
- The **test** (out-of-sample) error: how well it predicts data held back from fitting. It falls, then **rises** once the model starts fitting the noise.

The **root mean squared error**, RMSE = √(mean of (y − ŷ)²), is the usual measure for a number to be predicted.

## The Example: predicting delivery times

Thirty deliveries: distance (km), number of items, the last digit of the order number (which cannot matter), and the delivery time in minutes (illustrative data, made up for this book). A multiple regression (Module 12) on all three:

[[results: st.m23.learning.eg]]

[[figure: st.m23.learning.eg 1]]

- Distance and items matter (p < 0.001); the order digit does not (p = 0.32), as it should not. Yet it was fitted, and it lowers the training error a little.
- **In-sample RMSE: 3.09 minutes.** This is how well the model fits the 30 deliveries it has already seen.
- **5-fold cross-validated RMSE: 3.78 minutes.** The data are split into five parts; the model is fitted on four and predicts the fifth, five times (item 13.4).
- **Hold-out RMSE: 4.88 minutes**, on a quarter of the data (8 deliveries) kept out of fitting entirely.

The in-sample error is the **optimistic** one. A data scientist who reports 3.09 minutes will be surprised in production. Cross-validation and the hold-out set estimate the error on new deliveries; with only 8 hold-out cases, the hold-out figure is itself uncertain, which is why cross-validation, which uses every case as a test case once, is preferred for small data.

[[see-example: 1]]

## The workflow

1. **Split** off a test set first, and do not look at it.
2. On the training set, **choose** the model (predictors, flexibility) by cross-validation.
3. **Fit** the chosen model on the whole training set.
4. **Report** its error on the test set, once. Using the test set to choose the model makes its error optimistic too.

**Data leakage** is the subtle failure: information from the test cases slips into training (a variable recorded after the outcome, or scaling computed on all the data), and the test error looks better than reality.

## In Python and R

```python
from sklearn.linear_model import LinearRegression
from sklearn.model_selection import train_test_split, cross_val_score
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.25, random_state=1)
cv = cross_val_score(LinearRegression(), X_train, y_train, cv=5, scoring="neg_root_mean_squared_error")
model = LinearRegression().fit(X_train, y_train)
```

```r
library(caret)    # or rsample + parsnip
ctrl <- trainControl(method = "cv", number = 5)
train(minutes ~ distance + items, data = d, method = "lm", trControl = ctrl)
```

[[code: st.m23.learning.eg]]

## Common mistakes

- **Reporting the training error** as the model's accuracy.
- **Choosing the model on the test set**, then reporting the test error.
- **Leakage**: a predictor that would not be known at prediction time.
- **More predictors as always better.**

## What to do next

On the **Exercise** tab, forty deliveries from another depot: the in-sample, 10-fold cross-validated and hold-out RMSE.

[[example: Overfitting: add a predictor that cannot matter]]

The thirty deliveries with distance, items and the order digit: in-sample RMSE **3.09**, cross-validated **3.78**, hold-out **4.88** minutes.

1. **Set Selection to Backward.** Backward elimination drops the order digit (its p-value is 0.32). The in-sample error rises slightly, to **3.16**: the useless predictor had been fitting noise. The hold-out error falls to **4.35**, while the 5-fold cross-validated error moves a little the other way (**3.86**): with 30 deliveries every estimate of the error is noisy. The simpler model predicts about as well, and is the one to keep.
2. **Set the folds to 30** (leave-one-out): every delivery is predicted from the other 29, and the cross-validated error is **3.54**.
3. **Back to all three predictors and 5 folds, and set the hold-out seed to 2.** A different quarter of the data is held out, and the hold-out RMSE moves to **4.55**: with 8 cases, it is a noisy estimate.

[[try: st.m23.overfit.try Selection Folds HoldOutSeed | PredictorsUsed InSampleRmse CvRmse HoldOutRmse AdjustedRSquared figure=1]]
