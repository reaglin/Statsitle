---
title: Naive Bayes classification
---

# Naive Bayes classification

| Quantity | Formula |
|---|---|
| Bayes' rule | P(A\|B) = P(B\|A)P(A)/P(B) (item 3.4) |
| Odds form | posterior odds = prior odds × LR, LR = P(feature\|class)/P(feature\|other) |
| Naive Bayes | posterior odds = prior odds × Π LRⱼ (features independent within each class) |
| Log form | log posterior odds = log prior odds + Σ log LRⱼ |
| Probability from odds | p = odds/(1 + odds) |
| Laplace smoothing | (count + 1)/(total + number of words), so no rate is zero |

The probabilities are too extreme when features are dependent; the ranking is usually good.

## Python and R

| Task | Python | R |
|---|---|---|
| Text classifier | `sklearn.naive_bayes.MultinomialNB().fit(word_counts, labels)` | `e1071::naiveBayes(class ~ ., data = d)` |

**Read more:** Jurafsky and Martin, *Speech and Language Processing* (draft free at web.stanford.edu/~jurafsky/slp3), the
chapter on naive Bayes and text classification.
