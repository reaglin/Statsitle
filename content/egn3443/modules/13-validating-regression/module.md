---
id: st.m13
number: 13
title: Validating Regression
status: draft
minutes: 315
prerequisites: [st.m12]
outcomes:
  - st.m13.o1: Identify violations of core regression assumptions — including linearity, homoscedasticity, independence, and normality of residuals — using residual diagnostic plots
  - st.m13.o2: Compute residuals for a given regression model and interpret their patterns to detect model misfit or assumption violations
  - st.m13.o3: Distinguish among the holdout method, k-fold cross-validation, and leave-one-out cross-validation in terms of their procedures and appropriate use cases
  - st.m13.o4: Diagnose common regression model weaknesses — including multicollinearity, outliers, and influential observations — using systematic diagnostic workflows and plots
  - st.m13.o5: Recommend specific model refinement strategies — such as variable removal, variable transformation, or functional form adjustment — based on validation findings
topics:
  - Foundations of Regression Validation
  - Assessing Regression Model Assumptions
  - Residual Analysis
  - Cross-Validation Techniques
  - Identifying and Diagnosing Model Weaknesses
  - Model Refinement and Decision-Making
---

Modules 11 and 12 fitted regression models: a line through (x, y) pairs, then a plane through several predictors. A fitted model can come with a high R² and small p-values whether or not it deserves them. This module asks the questions that come **after** the fit. Is the model the right shape? Are its assumptions met, so that its p-values and intervals mean what they say? Does it predict new data as well as it fits the old? Which observations is it leaning on? And when something is wrong, what should change?

The tools are the **residuals** (what the model misses at each observation), their plots, a few diagnostic numbers (Durbin–Watson, Shapiro–Wilk, leverage, Cook's distance, the variance inflation factor), and **cross-validation**, which tests the model on rows it was not fitted to.

This module has six topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, works a small case, and shows the Python and R that do the same.
- **Example** shows a worked case: the data, the fit, the diagnostic plots, every result, and the code. Press **Open in the Workbench** to change the data or the settings and watch the diagnostics change.
- **Exercise** gives you a new data set, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the rules of thumb, the Python and R functions, and where to read more.

In the **Workbench** (the Workbench button, or Ctrl+B), two analyses in the list on the left do this module's work. **Simple linear regression** fits one predictor; its results end with the diagnostics (Durbin–Watson, Shapiro–Wilk of the residuals, the largest Cook's distance and leverage), its plots include the residuals against the fitted values and a normal Q–Q plot, and **Transform x** and **Transform y** try a log, square root or reciprocal. **Multiple regression** fits several predictors from one data table; besides the same diagnostics it gives each predictor's variance inflation factor, k-fold cross-validation (**Cross-validation folds (k)**) and a hold-out test (**Hold-out share** and **Hold-out seed**). Ctrl+L brings you back to the Course.

The module check, the last item, asks ten questions over all five outcomes, in the spirit of the course's *Fit for Purpose* activity: look at the plots and the numbers, eliminate the weak models with a reason, and choose the one to use. You need 80 % to pass, and you can try again.
