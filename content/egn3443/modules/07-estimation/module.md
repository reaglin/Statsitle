---
id: st.m7
number: 7
title: Point Estimation and Confidence Intervals
status: draft
minutes: 330
prerequisites: [st.m6]
outcomes:
  - st.m7.o1: Distinguish between the properties of unbiasedness and efficiency when evaluating point estimators such as the sample mean and sample proportion.
  - st.m7.o2: Apply the Central Limit Theorem to justify the use of normal-based inference methods for the sampling distribution of the sample mean.
  - st.m7.o3: Construct confidence intervals for population means and proportions by selecting the appropriate distribution (z or t), computing the margin of error, and forming the interval.
  - st.m7.o4: Evaluate the effect of confidence level, sample size, and population variability on the width and precision of a confidence interval.
  - st.m7.o5: Interpret a confidence interval in context using correct statistical language that accurately reflects the meaning of the confidence level.
topics:
  - Introduction to Point Estimation
  - Sampling Distributions and the Central Limit Theorem
  - Constructing Confidence Intervals for Means
  - Constructing Confidence Intervals for Proportions
  - Interpreting Confidence Intervals
  - Factors Affecting Precision and Margin of Error
  - Applying Estimation in Real-World Data Analysis
---

An engineer almost never measures every part, every battery or every weld. She measures a **sample** and uses it to say something about the whole **population**. This module is about the two ways to do that.

- A **point estimate** is one number: the sample mean x̄ = 74.86 kWh estimates the mean capacity μ of all the batteries made.
- A **confidence interval** is a range with a stated confidence: "we are 95 % confident that the mean capacity is between 74.56 and 75.16 kWh". It says how precise the estimate is.

Item 1.5 gave a first look at a confidence interval. This module builds it properly: what makes an estimator good, why the normal and t distributions apply (the central limit theorem), how to build intervals for a mean and for a proportion, what the confidence level really means, and how many observations a given precision needs. Module 8 then uses the same pieces for hypothesis tests.

This module has seven topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formula, works a small case by hand, and shows the Python and R that do the same calculation.
- **Example** shows a worked case: the data, every result, the figure, and the code. Press **Open in the Workbench** to change the inputs and watch the interval move.
- **Exercise** gives you a new case to calculate, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

In the **Workbench** (the Workbench button at the top, or Ctrl+B), choose **Confidence interval** in the list of analyses on the left for items 7.3, 7.4 and 7.7, **Sample size** for item 7.6, **Sampling distribution** for item 7.2, and **Simulation** to watch many samples at once. Press **Course** (Ctrl+L) to come back. Use the Workbench to check your hand calculations, not to skip them: the module check asks you to calculate small cases yourself.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % to pass, and you can try again.
