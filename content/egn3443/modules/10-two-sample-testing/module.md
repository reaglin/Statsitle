---
id: st.m10
number: 10
title: Two-Sample Hypothesis Testing
status: draft
minutes: 300
prerequisites: [st.m9]
outcomes:
  - st.m10.o1: Distinguish between independent and paired sample designs based on the relationship between observations across two groups
  - st.m10.o2: Verify the statistical assumptions required for a selected two-sample hypothesis test prior to its application
  - st.m10.o3: Calculate the appropriate test statistic for comparing two population means using either the independent samples t-test or the paired samples t-test
  - st.m10.o4: Compute the z-test statistic for the difference between two population proportions using a pooled proportion estimate
  - st.m10.o5: Evaluate two-sample hypothesis test results by interpreting p-values and confidence intervals within the real-world context of the problem
topics:
  - Foundations of Two-Sample Hypothesis Testing
  - Independent vs. Paired Samples
  - Assumptions and Conditions for Two-Sample Tests
  - Comparing Two Means
  - Comparing Two Proportions
  - Interpreting Results and Making Data-Driven Decisions
---

Module 9 introduced two-sample tests: Welch's and the pooled t-test for two independent samples, the paired t-test, the checks of their assumptions, and the rank tests for when those checks fail. **This module comes back to the same questions with new cases and goes further.** If Module 9 is fresh, parts of each Learn page will read as a review; each item says what is a review, links back to the Module 9 item, and then adds something new:

- the three kinds of **paired design** (the same unit measured twice, matched pairs, natural pairs), and what goes wrong when a design is misread;
- the conditions of the tests, including the **success–failure condition** for proportions;
- **one-sided tests** and tests against a difference other than zero (Δ₀), such as "is the new cell at least 3 seconds faster?";
- the **two-proportion z-test** with a pooled proportion, and the confidence interval for p₁ − p₂, which Module 9 named and left for here;
- turning a result into a **decision**: the cost of each kind of error, a practical threshold, and a written conclusion.

This module has six topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, works a small case by hand, and shows the Python and R that do the same test.
- **Example** shows a worked case: the data, the figures, every result, and the code. Press **Open in the Workbench** to change the data or the test and watch the results change.
- **Exercise** gives you a new case to test, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

In the **Workbench** (the Workbench button, or Ctrl+B), choose **Two-sample test** in the list on the left. Set **Test** to the test you want (*Two proportions (pooled z)* is the new one), **Data from** to *Summary statistics entered here* or *Columns of a data table*, and the **Alternative hypothesis** and **Significance level (α)**. Ctrl+L brings you back to the Course.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % to pass, and you can try again.
