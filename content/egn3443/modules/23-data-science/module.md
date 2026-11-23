---
id: st.m23
number: 23
part: Part III · Additional Applications of Statistics
title: "Data Science and Computing"
status: draft
minutes: 300
prerequisites: [st.m13]
outcomes:
  - st.m23.o1: Judge a predictive model on data it has not seen, with a training and test split and cross-validation, and recognise overfitting
  - st.m23.o2: Read a classifier's confusion matrix and calculate accuracy, precision, recall and the F1 score, and explain logistic regression as a classifier
  - st.m23.o3: Run and read an online A/B test at scale, including the traps of peeking and many metrics, and judge practical significance
  - st.m23.o4: Classify with naive Bayes by combining a prior with the evidence of several features
  - st.m23.o5: Group data with k-means and flag anomalies with z-scores, fences and control limits
  - st.m23.o6: Benchmark a system's performance with a confidence interval, model arrivals with the Poisson distribution, and calculate availability
topics:
  - "Learning from data: training, testing, cross-validation"
  - "Classification and its measures"
  - "Online experiments"
  - "Naive Bayes"
  - "Clustering and anomaly detection"
  - "Performance and reliability of systems"
---

Machine learning, recommendation engines, spam filters, fraud detection, the A/B tests behind every large website: much of what is called data science is statistics, run at scale and under other names. A "model trained on data" is often a regression. Its "accuracy on the test set" is Module 13's hold-out error. A spam filter is Bayes' rule. An "anomaly" is an outlier.

This module shows **how statistics is used in data science and computing**: judging a predictive model, measuring a classifier, running experiments on millions of users, and measuring the performance and reliability of computer systems. It does not teach the methods again. Where a method was taught in Part I, the page says where. Where it belongs to Part II (logistic regression fitted by maximum likelihood, for example), the page explains only as much as the application needs, and Part II will cover it in full.

**The data in this module are illustrative**: made up for this book, with realistic sizes and patterns, and labelled as such. They are not data from real companies or systems.

This module has six topics and a module check. Each topic is an item with four lesson tabs (and My notes, for your own notes):

- **Learn** explains how the field uses the method, works a small case, and shows the Python and R.
- **Example** has an example to try, with a figure that moves as you change the data, and the worked example with every result and the code.
- **Exercise** gives you a new data set from the field, or questions. Work out the answers, type them in, and press **Check my answers**.
- **Reference** has the formulas, the field's vocabulary, the Python and R functions, and where to read more.

You need Part I's probability rules (item 3.4), descriptive statistics (Module 2), the two-proportion test (item 10.5), multiple regression (Module 12) and validation (Module 13). The module check, the last item, asks ten questions over all six outcomes. You need 80 % to pass, and you can try again.
