---
id: st.m12
number: 12
title: Multiple Regression
status: draft
minutes: 260
prerequisites: [st.m11]
outcomes:
  - st.m12.o1: Construct a multiple regression equation by selecting appropriate predictor variables and estimating coefficients using ordinary least squares
  - st.m12.o2: Interpret partial regression coefficients in a multiple regression model as the unique effect of each predictor on the outcome while holding all other predictors constant
  - st.m12.o3: Differentiate between R-squared and adjusted R-squared as measures of model fit in the context of multiple predictor variables
  - st.m12.o4: Evaluate overall multiple regression model significance using the F-test to determine whether the model explains a statistically significant proportion of variance in the outcome variable
  - st.m12.o5: Justify the inclusion or removal of predictor variables during model refinement based on adjusted R-squared, coefficient significance, and underlying regression assumptions
topics:
  - From Simple to Multiple Regression
  - Model Specification and Structure
  - Interpreting Regression Coefficients
  - Assessing Model Fit
  - Building and Evaluating Multiple Regression Models
---

Module 11 fitted a straight line, ŷ = b₀ + b₁x, to one predictor. Engineering responses rarely depend on one thing. The strength of concrete depends on how long it cured **and** on how much water went into the mix. The fuel a truck burns depends on its load **and** its speed. **Multiple regression** puts several predictors into one equation,

ŷ = b₀ + b₁x₁ + b₂x₂ + … + bₖxₖ,

fits it by the same least-squares rule, and reads each coefficient as the effect of its predictor **with the others held fixed**. That last phrase is the point of the module: when predictors move together in the data, a one-predictor line mixes their effects, and multiple regression pulls them apart.

This module has five topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formulas, works a case from its numbers, and shows the Python and R that fit the same model.
- **Example** shows a worked case: the data table, the fitted model with its coefficient table, the figures, every result, and the code. Press **Open in the Workbench** to change the predictors or the selection and watch the results change.
- **Exercise** gives you a new data set to fit, or a set of questions. Fit the model in the Workbench, in Python or in R (the matrix arithmetic of least squares is not done by hand), type the answers, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

In the **Workbench** (the Workbench button, or Ctrl+B), paste the data as a data table (**+ Data table**, then Ctrl+V), then choose **Multiple regression** in the list on the left. Set **Data table**, **Response column (y)** and **Predictor columns** (the names, separated by commas), and **Variable selection**. The results begin with the equation, the coefficient table, R², adjusted R² and the F-test; the residual checks, influence measures and cross-validation further down belong to Module 13. Ctrl+L brings you back to the Course.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % to pass, and you can try again.
