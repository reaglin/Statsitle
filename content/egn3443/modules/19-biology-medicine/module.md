---
id: st.m19
number: 19
part: Part III · Additional Applications of Statistics
title: "Biology and Medicine"
status: draft
minutes: 300
prerequisites: [st.m10]
outcomes:
  - st.m19.o1: Explain how a clinical trial uses randomisation, blinding and a planned sample size, and compare two treatments' outcome rates
  - st.m19.o2: Calculate incidence, prevalence, relative risk and the odds ratio from a study's 2 × 2 table, and explain confounding
  - st.m19.o3: Find a diagnostic test's predictive values from its sensitivity, specificity and the prevalence with Bayes' rule, and read an ROC curve
  - st.m19.o4: Estimate a survival curve with the Kaplan–Meier method, and describe time to an event with the hazard and a parametric model
  - st.m19.o5: Test Hardy–Weinberg equilibrium with a chi-square test, and control the false discovery rate when many genes are tested at once
  - st.m19.o6: Fit a dose–response line on the logit scale and estimate the LD50 or EC50
topics:
  - "Clinical trials"
  - "Epidemiology"
  - "Diagnostic tests"
  - "Survival analysis"
  - "Genetics and genomics"
  - "Dose–response and bioassay"
---

Medicine and biology were among the first fields to build their decisions on statistics, and they are the strictest about it: a new drug is approved, a public-health warning issued, or a gene linked to a disease only when the numbers hold up. **Biostatistics** uses the methods of Part I with its own designs and words: the randomised trial, the cohort and case-control study, sensitivity and specificity, the survival curve, the odds ratio.

This module shows **how statistics is used in biology and medicine**. It does not teach the methods again. Where a method was taught in Part I, the page says where ("see item 10.5"). Where it belongs to Part II (logistic regression and maximum likelihood, for example), the page explains only as much as the application needs, and Part II will cover it in full.

**The data in this module are illustrative** (made up for this book, and labelled as such), except the beetle data of item 19.6, which are a classic published data set (Bliss, 1935). None of the illustrative numbers describe a real drug, disease or population.

This module has six topics and a module check. Each topic is an item with four lesson tabs (and My notes, for your own notes):

- **Learn** explains how the field uses the method, works a small case, and shows the Python and R.
- **Example** has an example to try, with a figure that moves as you change the data, and the worked example with every result and the code.
- **Exercise** gives you a new data set from the field. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**.
- **Reference** has the formulas, the field's vocabulary, the Python and R functions, and where to read more.

You need Part I's probability rules (item 3.4), continuous distributions (Module 5), the two-proportion test (item 10.5), the chi-square tests (item 8.5) and simple regression (Module 11). The module check, the last item, asks ten questions over all six outcomes. You need 80 % to pass, and you can try again.
