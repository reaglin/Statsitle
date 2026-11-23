---
id: st.m23.anomaly
title: Clustering and anomaly detection
minutes: 45
objectives: [st.m23.o5]
example: st.m23.anomaly.eg
exercise: st.m23.anomaly.ex
reference: [clustering-anomalies]
---

# Clustering and anomaly detection

Two everyday data-science tasks need no labelled examples at all. **Clustering** finds groups in data (customers who behave alike, sensor readings from different machine states). **Anomaly detection** finds the cases that fit no group: a failing server, a fraudulent login, a broken sensor. Both rest on Module 2's ideas of centre and spread.

## Anomalies as outliers

The simplest anomaly detectors are Part I's outlier rules:

- **z-score**: flag |x − x̄|/s > 3. Simple, but the mean and SD are themselves pulled by the anomalies.
- **The box-plot fences**: flag values beyond Q₁ − 1.5 IQR or Q₃ + 1.5 IQR (item 2.3). The quartiles are hardly moved by a few extreme values, so the fences are **robust**.
- **Control limits**: x̄ ± 3s on a control chart (item 1.6), watched over time.

## The Example: response times of a web service

Fifty response times (ms) in one minute (illustrative data, made up for this book):

[[figure: st.m23.anomaly.eg 2]]

[[results: st.m23.anomaly.eg]]

- The **median** is 122 ms, the **mean** 139.5 ms: a few slow requests pull the mean up.
- The fences are **71.5** and **177.5 ms**. They flag **five** anomalies: 52 (suspiciously fast, perhaps an error page), 203, 385, 410 and 455 ms.
- The control limits, x̄ ± 3s = −84 to 363 ms, flag only the three slowest. The anomalies inflated s to 74.6 ms, so the 3s rule **missed** 203 and 52. This **masking** is why robust rules are preferred for anomaly detection.

[[see-example: 1]]

## Clustering with k-means

k-means splits the data into k groups:

1. Pick k starting centres.
2. Assign each point to the nearest centre.
3. Move each centre to the mean of its points.
4. Repeat 2 and 3 until nothing changes.

Six values, k = 2, starting centres 2 and 12: 2, 3, 4, 10, 11, 12. Each value goes to the nearer centre: {2, 3, 4} and {10, 11, 12}. The new centres are their means, **3** and **11**, and nothing changes after that.

The number of clusters k is chosen by the data scientist (an "elbow" in the within-cluster variance as k grows helps). Variables on different scales must be **standardised** (z-scores) first, or the one with the largest numbers decides the clusters. A point far from every centre is an anomaly too.

## In Python and R

```python
import numpy as np
q1, q3 = np.percentile(x, [25, 75]); iqr = q3 - q1
anomalies = x[(x < q1 - 1.5*iqr) | (x > q3 + 1.5*iqr)]
from sklearn.cluster import KMeans
KMeans(n_clusters=2, n_init=10).fit(x.reshape(-1, 1)).cluster_centers_
```

```r
boxplot.stats(x)$out                      # values beyond the fences
kmeans(x, centers = 2)$centers
```

[[code: st.m23.anomaly.eg]]

## Common mistakes

- **The 3-standard-deviation rule** on data that contain the anomalies (masking).
- **Every anomaly treated as an error**: some are the most important observations.
- **k-means on unscaled variables.**
- **Trusting clusters** k-means will always find, even in data with none.

## What to do next

On the **Exercise** tab, a day of server-rack temperatures: the fences and the number of anomalies.

[[example: Anomalies: fences, means and medians]]

Fifty response times: median **122 ms**, mean **139.5 ms**, five values beyond the fences.

1. **Remove the three slowest** (385, 410, 455). The mean falls sharply, the median barely moves, and the control limits narrow: 203 is now caught by them too.
2. **Add a burst of slow requests**: add 300, 320, 340, 360 and 380. The upper fence rises with the third quartile, and the burst is still flagged. When anomalies are common, they stop being anomalies: then a time series of the rate matters more.

[[try: st.m23.latency.try Sample | Count Mean Median StandardDeviation UpperFence Outliers OutlierCount UpperControlLimit figure=2]]
