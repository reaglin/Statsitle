---
title: Classification measures
---

# Classification measures

| Quantity | Formula |
|---|---|
| Accuracy | (TP + TN)/(TP + TN + FP + FN) |
| Recall (sensitivity) | TP/(TP + FN) |
| Precision (positive predictive value) | TP/(TP + FP) |
| Specificity | TN/(TN + FP) |
| F1 score | 2 × precision × recall/(precision + recall) |
| Logistic regression | ln(p/(1 − p)) = b₀ + Σ bⱼxⱼ; flag when p > threshold (Part II) |

**Class imbalance:** when positives are rare, accuracy is high for a useless model; use precision, recall and the
precision–recall curve. The ROC curve and its AUC are item 19.3's.

## Python and R

| Task | Python | R |
|---|---|---|
| Confusion matrix | `sklearn.metrics.confusion_matrix(y, pred)` | `table(actual = y, predicted = pred)` |
| Precision, recall, F1 | `precision_score`, `recall_score`, `f1_score` | from the table, or `yardstick::precision(...)` |
| Logistic regression | `sklearn.linear_model.LogisticRegression().fit(X, y)` | `glm(y ~ ., data = d, family = binomial)` |

**Read more:** James et al., *An Introduction to Statistical Learning*, chapter 4.
