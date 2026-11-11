---
id: st.m5
number: 5
title: Continuous Probability Distributions
status: draft
minutes: 285
prerequisites: [st.m4]
outcomes:
  - st.m5.o1: Distinguish between discrete and continuous random variables by identifying the role of the probability density function and the cumulative distribution function in describing continuous probability distributions.
  - st.m5.o2: Calculate probabilities for the uniform, normal, and exponential distributions using their respective formulas, z-score conversion, and cumulative distribution functions.
  - st.m5.o3: Select the appropriate continuous probability distribution to model a given real-world scenario by evaluating the characteristics of the uniform, normal, and exponential distributions against the properties of the data.
  - st.m5.o4: Interpret calculated probabilities from continuous distributions within the context of a real-world engineering or professional problem to support data-driven decision-making.
topics:
  - Introduction to Continuous Probability Distributions
  - The Uniform Distribution
  - The Normal Distribution
  - The Exponential Distribution
  - Calculating and Interpreting Probabilities
  - Applying Continuous Distributions to Real-World Scenarios
---

Module 4 counted things: defects, calls, trucks. Most engineering quantities are **measured** instead: a diameter, a strength, a time to failure, a fill volume. A measured quantity can take any value in a range, so it is a **continuous random variable**, and its distribution is a curve, not a set of bars. Probabilities are **areas** under that curve.

This module teaches the three continuous distributions an engineer meets first:

- the **uniform**, for a value equally likely anywhere in a range;
- the **normal**, the bell curve of measurements, with the z-score and Φ that Modules 6 to 14 use all the time;
- the **exponential**, for the waiting time between random events and the life of parts that fail at random.

The last topic adds the **Weibull**, the model for the life of parts that wear out, which the course reading uses, and shows how to **fit** a distribution to measured data and judge whether it suits them.

This module has six topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formula, works a small case by hand, and shows the Python and R that do the same calculation.
- **Example** shows a worked case: the density with the probability shaded, the cumulative curve, every result, and the code. Press **Open in the Workbench** to change the numbers and watch the area change.
- **Exercise** gives you a new case to calculate, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

In the **Workbench** (the Workbench button, or Ctrl+B), choose **Continuous distribution** in the list on the left. Pick the **Distribution** (uniform, normal, exponential, Weibull, and others), type its parameters, the **Value of interest (x)**, an **Upper value (x₂)** for a range, and a **Probability for the quantile (q)** for a percentile, and every probability, the mean and the spread appear at once. **Parameters from** can also be set to *Fitted to a data column*, to fit the distribution to measurements (item 5.6). Ctrl+L brings you back to the Course. Use the Workbench to check your hand calculations, not to skip them: the module check asks you to calculate small cases yourself.

The module check, the last item, asks ten questions over all four outcomes. You need 80 % to pass, and you can try again.
