---
id: st.m14
number: 14
title: ANOVA
status: draft
minutes: 330
prerequisites: [st.m13]
outcomes:
  - st.m14.o1: Justify the use of ANOVA over multiple t-tests when comparing means across three or more groups by explaining how multiple t-tests inflate the Type I error rate
  - st.m14.o2: Verify that a dataset meets the core ANOVA assumptions of normality, homogeneity of variance, and independence of observations using visual methods and formal statistical tests
  - st.m14.o3: Calculate the F-statistic for a one-way ANOVA by partitioning total variance into between-group and within-group components and organizing results in an ANOVA summary table
  - st.m14.o4: Differentiate between main effects and interaction effects in a multi-factor ANOVA design by analyzing F-statistics and their associated p-values for each variance component
  - st.m14.o5: Select an appropriate post-hoc testing procedure and interpret its pairwise comparison output to identify which specific group means differ following a significant ANOVA result
topics:
  - Introduction to ANOVA
  - Assumptions Underlying ANOVA
  - The F-Statistic and ANOVA Logic
  - One-Way ANOVA
  - Multi-Factor ANOVA Designs
  - Post-Hoc Testing and Interpreting Results
---

Modules 9 and 10 compared two groups: two alloys, two ovens, two coatings. Engineers often have more: four suppliers of resin, three curing temperatures, five nozzle designs. **Analysis of variance (ANOVA)** compares the means of any number of groups in one test, so the chance of a false alarm stays at the α you chose.

The name says how it works. ANOVA splits the total variation of the measurements into the part **between** the groups (how far the group means are from each other) and the part **within** the groups (the scatter of the measurements around their own group mean). If the group means differ by more than the scatter inside the groups can explain, the groups really differ. The ratio of the two is the **F statistic**.

This module has six topics and a module check. The first two say why ANOVA is needed and what it assumes; the next two build the ANOVA table and run the whole one-way analysis; the fifth adds a second factor, and with it the **interaction**; the last finds out *which* means differ once ANOVA says some do. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formulas, works a small case by hand, and shows the Python and R that do the same.
- **Example** shows a worked case: the data, the figures (box plots, group means, the interaction plot), every result, and the code. Press **Open in the Workbench** to change the data and watch the results change.
- **Exercise** gives you a new data set. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

In the **Workbench** (the Workbench button, or Ctrl+B), choose **One-way ANOVA** or **Two-way ANOVA** in the list on the left. One-way ANOVA reads a data table either with one column per group or with a factor column and a response column; it gives the ANOVA table, η², Tukey's HSD for every pair, the Bartlett, Levene and Shapiro–Wilk checks, and the Kruskal–Wallis test. Two-way ANOVA reads two factor columns and a response column. Item 14.1 also uses **Simulation**. Ctrl+L brings you back to the Course.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % (8 of 10) to pass, and you can try again.

## The end of the course

This is the last module of EGN3443, and it draws on almost all of the others. The groups are described with the means and standard deviations of Module 2. The F distribution, which Module 9 used for the ratio of two variances, is the sampling distribution (Module 6's idea) of a ratio of two variance estimates. The test follows the steps of Module 8, and the assumption checks are the ones Modules 9 and 10 used for two samples (item 10.3). The ANOVA table is the same partition of variation, total = explained + unexplained, that gave R² in regression (Modules 11–13): a one-way ANOVA is a multiple regression on 0/1 variables that say which group each value is in, and η² is that regression's R². When you have finished, the Workbench stays: every analysis of the course is in its list, ready for the next data set you meet at work.
