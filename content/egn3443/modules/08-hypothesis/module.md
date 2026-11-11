---
id: st.m8
number: 8
title: Hypothesis Testing
status: draft
minutes: 300
prerequisites: [st.m7]
outcomes:
  - st.m8.o1: Construct correctly structured null and alternative hypotheses — including directional and non-directional forms — from a given real-world engineering claim.
  - st.m8.o2: Select the appropriate hypothesis test (one-sample t-test, two-sample t-test, paired t-test, chi-square, or ANOVA) for a given data scenario based on data type, number of groups, and sample characteristics.
  - st.m8.o3: Calculate a test statistic and corresponding p-value for a given sample dataset using the correct sampling distribution.
  - st.m8.o4: Evaluate a hypothesis test conclusion by comparing the p-value to a pre-specified significance level and distinguishing statistical significance from practical significance.
  - st.m8.o5: Differentiate between Type I and Type II errors in a hypothesis testing scenario and identify the consequences of each error type for a given engineering context.
topics:
  - Foundations of Hypothesis Testing
  - Formulating Null and Alternative Hypotheses
  - Test Statistics and Sampling Distributions
  - P-Values and Significance Levels
  - Applying Common Hypothesis Tests
  - Type I and Type II Errors
  - Interpreting and Communicating Results
---

An engineer is often asked a yes-or-no question about a process: is the machine still on target? Did the new die lower the defect rate? Does the supplier's material meet its claim? A sample answers it, but a sample varies by chance. A **hypothesis test** is the rule that decides whether a difference in the sample is too large to be chance alone.

Module 6 built the tool: the **sampling distribution** says how far a sample mean wanders from μ by chance, and the z-score says how many standard errors it lies away. Module 1 (item 1.5) previewed a test on bearing bores. This module makes the method complete: state the hypotheses, compute a **test statistic**, find its **p-value**, compare it with the **significance level** α, and say what the decision means, including the two ways it can be wrong. It covers tests on one mean (z and t), on one proportion, and on counts (**chi-square** tests). Module 9 compares two samples, and Module 14 compares several means (ANOVA).

The course's Module 8 pages (Week 9, Hypothesis Testing, on reaglin.github.io/EGN3443) have the readings, videos and the "burden of proof" activity. Module 7 (point estimation and confidence intervals) comes before it in the course. This module uses a confidence interval (Module 7) only in passing, and says so where it does.

This module has seven topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formula, works a small case by hand, and shows the Python and R that do the same calculation.
- **Example** shows a worked test: the data, the results, the reference curve with the rejection region, and the code. Press **Open in the Workbench** to change the numbers and watch the decision change.
- **Exercise** gives you a new case to test, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

Two analyses in the **Workbench** (Ctrl+B, then choose one in the list on the left) go with this module. **One-sample test** tests a mean (z or t), a proportion or a variance, and gives the statistic, the p-value, the decision and the power. **Chi-square tests** tests counts: goodness of fit to stated proportions, and independence in a table of counts. Press **Course** (Ctrl+L) to come back.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % to pass, and you can try again.
