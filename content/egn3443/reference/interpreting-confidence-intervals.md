---
title: Interpreting confidence intervals
---

# Interpreting confidence intervals

## The template

"We are **[confidence] %** confident that the **[parameter, in context]** is between **[lower]** and **[upper] [units]**."

If asked what the confidence means: "If the sampling were repeated many times, about [confidence] % of the intervals built this way would contain the true [parameter]."

## Right and wrong

| Statement about a 95 % interval (L, U) for μ | |
|---|---|
| We are 95 % confident that μ is between L and U. | ✔ |
| About 95 % of intervals made this way contain μ. | ✔ |
| There is a 95 % probability that μ is between L and U. | ✘ μ is fixed; the 95 % belongs to the method |
| 95 % of the individual values are between L and U. | ✘ the interval is for the mean |
| 95 % of sample means fall between L and U. | ✘ it is centered on this x̄, not on μ |
| μ equals the center of the interval. | ✘ the center is the estimate |

## Using it for a decision

- A claimed or target value **inside** the interval is plausible: the data do not contradict it.
- A value **outside** is not plausible at that confidence: the data argue against it (evidence, not proof).
- A two-sided test at level α rejects μ₀ exactly when μ₀ is outside the (1 − α) interval (Module 8).
- Compare a specification for **single items** with the data, not with the interval for the mean.

## Width

Higher confidence → wider. Larger n → narrower (with √n). More variability → wider.

## In the Workbench

**Simulation** ▸ *Confidence intervals: how many contain μ* builds many intervals from a population whose μ it knows; figure 2 draws the first 100, with the misses in red.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 8-1.2 (interpreting a confidence interval).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.5.2, Confidence Limits for the Mean: https://www.itl.nist.gov/div898/handbook/eda/section3/eda352.htm
