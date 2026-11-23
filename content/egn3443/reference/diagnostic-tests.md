---
title: Diagnostic tests: sensitivity, specificity, predictive values
---

# Diagnostic tests: sensitivity, specificity, predictive values

| Quantity | Formula |
|---|---|
| Sensitivity | P(positive \| disease) = TP/(TP + FN) |
| Specificity | P(negative \| no disease) = TN/(TN + FP) |
| False positive rate | 1 − specificity |
| P(positive) | sens × prev + (1 − spec)(1 − prev) |
| Positive predictive value | sens × prev / P(positive) (Bayes' rule, item 3.4) |
| Negative predictive value | spec (1 − prev) / [spec (1 − prev) + (1 − sens) prev] |
| ROC curve | sensitivity against 1 − specificity for every cut-off; AUC 0.5 chance, 1.0 perfect |

The predictive values depend on the prevalence; sensitivity and specificity do not.

## Python and R

| Task | Python | R |
|---|---|---|
| PPV and NPV | by the formulas above | by the formulas above |
| ROC and AUC | `sklearn.metrics.roc_curve(y, score)`, `roc_auc_score(y, score)` | `pROC::roc(y, score)`, `pROC::auc(...)` |

**Read more:** Altman and Bland (1994), "Diagnostic tests 1: sensitivity and specificity" and "2: predictive values",
*BMJ* 308.
