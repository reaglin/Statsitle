---
id: st.m23.classification
title: Classification and its measures
minutes: 50
objectives: [st.m23.o2]
example: st.m23.classification.eg
quiz: quizzes/m23-classification.json
reference: [classification-measures]
---

# Classification and its measures

Is this transaction fraud? Is this email spam? Will this customer cancel? A **classifier** puts each case into a class, and the question is how good it is. "It is 93 % accurate" can mean a useful model or a useless one. Data science judges a classifier with the **confusion matrix** and the measures built from it, the same ideas as a medical test's sensitivity and predictive value (item 19.3), under other names.

## The confusion matrix

| | Flagged (predicted positive) | Not flagged |
|---|---|---|
| **Actual positive** | true positive (TP) | false negative (FN) |
| **Actual negative** | false positive (FP) | true negative (TN) |

| Measure | Formula | Medical name | The question it answers |
|---|---|---|---|
| Accuracy | (TP + TN)/all | | how many are right |
| **Recall** | TP/(TP + FN) | sensitivity | of the real positives, how many are caught |
| **Precision** | TP/(TP + FP) | positive predictive value | of the flagged, how many are real |
| Specificity | TN/(TN + FP) | specificity | of the negatives, how many are cleared |
| **F1 score** | 2 × precision × recall/(precision + recall) | | one number balancing the two |

## The Example: a fraud classifier

A classifier checked 1,000 card transactions; 50 were actually fraud (illustrative data, made up for this book).

| | Flagged | Not flagged |
|---|---|---|
| Actual fraud | 40 | 10 |
| Actual genuine | 60 | 890 |

- **Accuracy** = (40 + 890)/1000 = **0.93**.
- **Recall** = 40/50 = **0.80**: it catches four frauds in five.
- **Precision** = 40/100 = **0.40**: of the 100 flagged, only 40 are fraud.
- **F1** = 2 × 0.40 × 0.80/1.20 = **0.53**.

The accuracy is misleading. A "classifier" that flags nothing at all is right on the 950 genuine transactions: **95 % accurate**, and useless. When one class is rare (**class imbalance**), look at precision and recall.

[[results: st.m23.classification.eg]]

The chi-square test of the table (item 8.5) says the flags are strongly associated with fraud (χ² = 287, Cramér's V = 0.54): the classifier carries real information. It does not say whether it is good enough.

[[see-example: 1]]

## Thresholds and the trade-off

Most classifiers give each case a **score** (a probability of fraud), and a **threshold** turns it into a flag. A lower threshold flags more: recall rises, precision falls. Plotting recall against 1 − specificity for every threshold gives the **ROC curve** of item 19.3; the **precision–recall curve** is preferred when positives are rare. The threshold is chosen by the costs: a missed fraud against a customer annoyed by a blocked card.

## Logistic regression as a classifier

The workhorse classifier is **logistic regression**: it models the log-odds of the positive class as a linear function of the predictors,

ln(p/(1 − p)) = b₀ + b₁x₁ + … + b_k x_k,

so p = 1/(1 + e^(−(b₀ + b₁x₁ + …))). It is fitted by maximum likelihood (Part II). Item 19.6 drew the same S-curve for one predictor, the dose. A case is flagged when its p is above the threshold.

## In Python and R

```python
from sklearn.metrics import confusion_matrix, precision_score, recall_score, f1_score
from sklearn.linear_model import LogisticRegression
clf = LogisticRegression().fit(X_train, y_train)
pred = clf.predict_proba(X_test)[:, 1] > 0.5
print(confusion_matrix(y_test, pred), precision_score(y_test, pred), recall_score(y_test, pred))
```

```r
fit <- glm(fraud ~ amount + hour, data = train, family = binomial)
p <- predict(fit, test, type = "response")
table(actual = test$fraud, flagged = p > 0.5)
```

[[code: st.m23.classification.eg]]

## Common mistakes

- **Accuracy alone** on imbalanced classes.
- **Precision and recall confused.**
- **A threshold of 0.5 by habit**, without asking what errors cost.
- **Measuring on the training data** (item 23.1).

## What to do next

On the **Exercise** tab, practice questions: precision, recall and F1 from a confusion matrix, and what a threshold change does.

[[example: A confusion matrix: change the threshold]]

The fraud classifier: 40 of 50 frauds flagged, 60 genuine transactions flagged. χ² = **287**, Cramér's V = **0.54**.

1. **A lower threshold.** Move 8 frauds from Not flagged to Flagged (48 and 2) and 140 genuine ones (200 and 750). Recall is now 48/50 = 0.96, but precision falls to 48/248 = 0.19.
2. **A higher threshold.** Back to the start, then make it 25 and 25 frauds, 10 and 940 genuine. Precision rises to 25/35 = 0.71, recall falls to 0.50.

Work out precision and recall by hand from the table each time; the chi-square test shows the association stays strong.

[[try: st.m23.confusion.try @Fraud/Flagged "@Fraud/Not flagged" | Statistic CramersV figure=1]]
