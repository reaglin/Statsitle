---
id: st.m4
number: 4
title: Discrete Probability Distributions
status: draft
minutes: 270
prerequisites: [st.m3]
outcomes:
  - st.m4.o1: Construct a valid discrete probability distribution by assigning probabilities to all possible outcomes that satisfy both the non-negativity and summation-to-one properties.
  - st.m4.o2: Calculate the expected value and variance of a discrete random variable using its probability mass function.
  - st.m4.o3: Differentiate between the binomial and Poisson distributions by evaluating whether a given scenario satisfies the defining conditions of each distribution.
  - st.m4.o4: Compute exact probabilities for real-world scenarios using the binomial and Poisson probability formulas with correctly identified parameters.
  - st.m4.o5: Interpret computed probabilities, expected values, and variances within the context of an applied engineering or scientific problem to support a data-driven conclusion.
topics:
  - Introduction to Discrete Probability Distributions
  - Probability Mass Functions and Distribution Properties
  - Expected Value and Variance of Discrete Distributions
  - The Binomial Distribution
  - The Poisson Distribution
  - Applying Discrete Distributions to Real-World Problems
---

Module 2 described data that had already been measured. This module goes the other way: it describes what **could** happen before anything is measured. How many of the 20 boards in a batch will be defective? How many calls will the help desk get in the next hour? A **discrete probability distribution** answers questions like these. It lists every count that can happen and how likely each one is.

Engineers use these distributions to set inspection plans, size staff and spare-parts stocks, and decide whether a result is unusual or just bad luck. The same ideas carry into Module 5 (continuous distributions) and every test in Modules 7 to 14.

This module has six topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formula, works a small case by hand, and shows the Python and R that do the same calculation.
- **Example** shows a worked case: the distribution, its probabilities, the bar chart of the probability mass function, the cumulative curve, and the code. Press **Open in the Workbench** to change the numbers and watch the probabilities change.
- **Exercise** gives you a new case to calculate, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

In the **Workbench**, choose **Discrete distribution**. Pick the distribution (binomial, Poisson, geometric, negative binomial, hypergeometric, or a typed table of x and P(x)), enter its parameters and the value k the question is about, and every probability, the mean and the variance appear at once. Use it to check your hand calculations, not to skip them: the module check asks you to calculate small cases yourself.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % to pass, and you can try again.
