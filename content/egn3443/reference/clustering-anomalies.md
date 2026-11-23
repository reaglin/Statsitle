---
title: Clustering and anomaly detection
---

# Clustering and anomaly detection

| Quantity | Formula |
|---|---|
| z-score rule | flag \|x − x̄\|/s > 3 (masked by the anomalies themselves) |
| Box-plot fences | Q₁ − 1.5 IQR and Q₃ + 1.5 IQR (robust) |
| Control limits | x̄ ± 3s over time (item 1.6) |
| Robust z | (x − median)/(1.4826 × MAD), MAD the median absolute deviation |
| k-means | assign to the nearest centre; move centres to the means; repeat |
| Within-cluster sum of squares | Σ over clusters Σ ‖x − centre‖²; its "elbow" helps choose k |

Standardise variables before clustering; k-means finds k clusters even in data that have none.

## Python and R

| Task | Python | R |
|---|---|---|
| Fences | `np.percentile(x, [25, 75])` | `boxplot.stats(x)$out` |
| k-means | `sklearn.cluster.KMeans(n_clusters=k, n_init=10).fit(X)` | `kmeans(scale(X), centers = k)` |
| Isolation forest | `sklearn.ensemble.IsolationForest().fit(X)` | `isotree::isolation.forest(X)` |

**Read more:** James et al., *An Introduction to Statistical Learning*, chapter 12 (unsupervised learning).
