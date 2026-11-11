---
id: st.m6
number: 6
title: Sampling Distributions
status: draft
minutes: 240
prerequisites: [st.m5]
outcomes:
  - st.m6.o1: Distinguish between population parameters and sample statistics in the context of a given data analysis scenario.
  - st.m6.o2: Calculate the mean and standard error of a sampling distribution of the sample mean using the Central Limit Theorem formulas.
  - st.m6.o3: Predict how changes in sample size affect the spread of a sampling distribution by applying the inverse relationship between sample size and standard error.
  - st.m6.o4: Identify the conditions under which the Central Limit Theorem justifies using a normal distribution approximation for the sampling distribution of the mean.
  - st.m6.o5: Interpret a sampling distribution to distinguish natural sampling variability from meaningful differences in a real-world engineering data scenario.
topics:
  - Introduction to Sampling Distributions
  - Population Parameters vs. Sample Statistics
  - The Central Limit Theorem
  - The Effect of Sample Size on Variability
  - Interpreting and Applying Sampling Distributions
---

An engineer almost never measures every part, every batch or every day. She measures a **sample** and uses its mean to say something about the whole **population**. But a second sample would give a different mean, and a third another. How far can a sample mean be from the truth? This module answers that question, and the answer is the base of every confidence interval and test in Modules 7 to 14.

The key idea is the **sampling distribution**: the distribution of a statistic, such as the sample mean x̄, over all the samples that could have been taken. Its centre is the population mean μ, its spread is the **standard error** σ/√n, and by the **Central Limit Theorem** its shape is close to normal once n is large enough, whatever the shape of the population.

This module uses the normal distribution, which Module 5 (continuous distributions) teaches in full. Here you need only two things from it: the **z-value**, how many standard deviations a value lies from the mean, and **Φ(z)**, the area under the standard normal curve to the left of z. Item 6.3 shows both again, with the Python and R that give them.

This module has five topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the formula, works a small case by hand, and shows the Python and R that do the same calculation.
- **Example** shows a worked case: a simulation of many samples, or a sampling distribution with its probabilities, the figure and the code. Press **Open in the Workbench** to change the numbers and watch the results change.
- **Exercise** gives you a new case to calculate, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the formulas, the Python and R functions, and where to read more.

Two analyses in the **Workbench** (Ctrl+B, then choose one in the list on the left) go with this module. **Sampling distribution** takes the population's μ and σ and the sample size n and gives the mean and standard error of x̄ and the probability of a sample mean above, below or between values. **Simulation**, with Experiment *Sample means (central limit theorem)*, draws thousands of samples from a normal, uniform or exponential population and shows the histogram of their means. Press **Course** (Ctrl+L) to come back.

The module check, the last item, asks ten questions over all five outcomes. You need 80 % to pass, and you can try again.
