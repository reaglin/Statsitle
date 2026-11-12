---
id: st.m11
number: 11
title: Regression Analysis
status: draft
minutes: 300
prerequisites: [st.m10]
outcomes:
  - st.m11.o1: Construct a simple linear regression equation using the least squares method from a given dataset
  - st.m11.o2: Interpret regression coefficients, R-squared, Adjusted R-squared, p-values, and the F-statistic from regression output to draw conclusions about model fit and predictor significance
  - st.m11.o3: Differentiate between simple and multiple regression models based on the number of predictor variables and the research question being addressed
  - st.m11.o4: Evaluate a regression model's validity by performing residual analysis and checking key assumptions such as linearity, independence, homoscedasticity, and normality
  - st.m11.o5: Apply regression analysis to a real-world dataset and communicate findings as actionable, data-driven insights appropriate for a non-technical audience
topics:
  - Foundations of Regression Analysis
  - Simple Linear Regression
  - Multiple Regression Models
  - Evaluating Regression Model Performance
  - Interpreting Regression Results
  - Applying Regression to Real-World Datasets
---

Modules 8 to 10 tested claims about means and proportions and compared groups. Engineers just as often ask **how much one quantity changes with another**. How much weaker is concrete with more water in the mix? How much more fuel does a truck burn per tonne of payload? How much timber is in a tree of a given diameter? **Regression** answers questions like these. It fits an equation that predicts one variable (the **response**, y) from another (the **predictor**, x). It also says how good the predictions are and whether the relation could just be chance.

This module builds the **simple linear regression** ŷ = b₀ + b₁x by least squares. It then reads the regression output: the coefficients, R², the standard error, the F-test and the p-values. It checks the model's assumptions with the residuals, and it applies the whole method to real data. One item previews **multiple regression**, with several predictors at once. Module 12 teaches that fully, and Module 13 goes deeper into checking a model.

This module has six topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formula, works a small case by hand, and shows the Python and R that do the same.
- **Example** shows a worked case: the data, the scatter plot with the fitted line, the residual plots, every result, and the code. Press **Open in the Workbench** to change the data and watch the line and the results change.
- **Exercise** gives you a new data set, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

In the **Workbench** (the Workbench button, or Ctrl+B), choose **Simple linear regression** in the list on the left. Paste the data as a data table (**+ Data table**, then Ctrl+V), and set **x column** and **y column** to its columns. Set **Predict at x₀, optional** to predict at a value of x. Item 11.3 uses **Multiple regression** in the same list. Ctrl+L brings you back to the Course.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % to pass, and you can try again.
