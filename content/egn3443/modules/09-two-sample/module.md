---
id: st.m9
number: 9
title: Hypothesis Testing II
status: draft
minutes: 300
prerequisites: [st.m7]
outcomes:
  - st.m9.o1: Select the appropriate hypothesis test (two-sample z-test, two-sample t-test, or paired-sample t-test) for a given engineering scenario based on data type, sample size, and sample independence
  - st.m9.o2: Compute the test statistic for two-sample and paired-sample hypothesis tests using the correct formula and degrees of freedom
  - st.m9.o3: Evaluate whether the assumptions underlying two-sample and paired-sample tests are satisfied for a given dataset
  - st.m9.o4: Interpret p-values and confidence intervals from two-sample and paired-sample tests to distinguish between statistical significance and practical significance
  - st.m9.o5: Identify an appropriate non-parametric alternative when the assumptions of a parametric two-sample test cannot be satisfied
topics:
  - Two-Sample Hypothesis Tests
  - Paired Sample Comparisons
  - Selecting the Appropriate Statistical Test
  - Assumptions and Conditions for Validity
  - Introduction to Non-Parametric Methods
  - Interpreting and Communicating Results
---

Module 8 tested one sample against a fixed value: is the mean bore 25.000 mm? Engineers more often compare **two** things. Is the new alloy stronger than the old one? Did the retrofit lower the pumps' power draw? Do two gauges measure with the same precision? This module answers questions like these with **two-sample tests**.

The logic is the one from Module 8: state H₀ and H₁, choose α, compute a test statistic, find its p-value, and decide. What is new is the choice of test. It depends on how the data were collected (two independent groups, or pairs of measurements on the same units), on what you know about the spread, and on whether the data look normal. A wrong choice gives a wrong p-value, sometimes badly wrong.

This module has six topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formula, works a small case by hand, and shows the Python and R that do the same test.
- **Example** shows a worked case: the data, the box plots, the test statistic on its reference distribution, every result, and the code. Press **Open in the Workbench** to change the data or the test and watch the results change.
- **Exercise** gives you a new data set to test, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

In the **Workbench** (the Workbench button, or Ctrl+B), choose **Two-sample test** in the list on the left. Set **Test** to the test you want, **Data from** to summary statistics or to two columns of a data table, and the **Alternative hypothesis** and **Significance level (α)**. With columns, it also reports the Shapiro–Wilk and Levene checks of the assumptions. Ctrl+L brings you back to the Course.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % to pass, and you can try again.
