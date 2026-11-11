---
id: st.m1.quality
title: Applications in engineering analysis and quality control
minutes: 45
objectives: [st.m1.o5, st.m1.o1, st.m1.o4]
builds-on: st.m1.inference
example: st.m1.quality.eg
exercise: st.m1.quality.ex
reference: [quality-control, quartiles-and-outliers, sources-of-variability]
---

# Applications in engineering analysis and quality control

Quality control is where everything in this module meets: descriptive statistics to summarize a process, distributions to say what is normal for it, and inference to decide whether it has changed. This item shows four of the uses an engineer meets first.

## 1. Spotting the unusual: outliers

The first question about any process data is whether something unusual happened. **Tukey's fences** flag values far from the middle of the data:

- lower fence = Q1 − 1.5 × IQR;
- upper fence = Q3 + 1.5 × IQR.

A value beyond a fence is an **outlier**. In a process, an outlier is often the trace of a **special cause**: a misfeed, a wrong part, a sensor fault. It is investigated, not simply deleted.

The fences use the quartiles, not the mean and standard deviation, for a reason: an outlier pulls the mean and inflates the standard deviation, but hardly moves the quartiles. So the fences still work when the outlier is in the data.

**The Example.** An automatic nut runner tightens wheel nuts to 120 N·m. Fifteen final torques, in newton-meters (N·m):

[[data: st.m1.quality.eg]]

[[figure: st.m1.quality.eg 2]]

[[results: st.m1.quality.eg]]

Reading it:

- Q1 = 119.35 N·m and Q3 = 120.90 N·m, so IQR = 1.55 N·m.
- Lower fence = 119.35 − 1.5 × 1.55 = **117.025 N·m**; upper fence = 120.90 + 1.5 × 1.55 = **123.225 N·m**.
- The torque of **104.5 N·m** is far below the lower fence: an outlier. A nut 15 N·m short of target could work loose, so the engineer pulls that wheel and finds out why (a cross-threaded nut, a slipping socket).
- The outlier drags the mean (119.25 N·m) below the median (120.20 N·m) and inflates the standard deviation to 4.20 N·m. Without it, the other fourteen nuts have a mean of 120.30 N·m and a standard deviation of only 1.05 N·m. The skewness, −3.49, also comes almost entirely from the one nut.

## 2. Statistical process control (SPC)

**Statistical process control** watches a process over time, so that a special cause is caught when it happens rather than at final inspection. Its main tool is the **control chart**: the measurements plotted in time order, with a center line at the process mean and two **control limits**.

A simple form of the limits uses the mean and standard deviation of the process while it is stable:

UCL = x̄ + 3s,  LCL = x̄ − 3s

For a normal process, 99.7 % of values fall within ±3 standard deviations of the mean (item 1.4), so a point outside the limits is very unlikely to be common-cause variation. It signals a special cause to look for.

**Worked case.** From the fourteen nuts without the faulty one, x̄ = 120.30 N·m and s = 1.05 N·m:

- UCL = 120.30 + 3 × 1.05 = **123.45 N·m**;
- LCL = 120.30 − 3 × 1.05 = **117.15 N·m**.

All fourteen are inside (118.6 to 122.1 N·m), and 104.5 N·m is far below the LCL: the chart would have flagged it the moment it happened. Note that the limits come from the stable period. Limits worked out with the outlier included would be much wider, because the outlier inflates s.

**Control limits are not specification limits.** Control limits describe what the process **does**; they come from data. Specification (tolerance) limits describe what the customer **needs**; they come from the design. A process can be in control and still make parts out of specification, if its spread is too large for the tolerance.

Textbooks on quality control estimate the spread for these charts in other ways too (from the moving range, or from subgroups); you will meet those later. The idea is the same.

## 3. Reliability and failure analysis

The distributions of item 1.4 predict failures. If a component fails at random at a rate of λ = 0.5 failures per year, the number of failures in a year is Poisson, and the chance of a year with no failure is e^(−0.5) = 0.61. If 2 % of a supplier's relays are defective, the chance that a panel of 10 relays has none is 0.98¹⁰ = 0.82 (binomial). Module 5 adds the exponential and Weibull distributions for times to failure.

## 4. Acceptance sampling

A customer receiving a lot of 5,000 parts cannot test them all. **Acceptance sampling** tests a random sample and accepts or rejects the whole lot by a rule, such as "test 50; accept the lot if at most 1 is defective". The binomial distribution gives the chance that a lot with a given defect rate is accepted. For a lot that is 2 % defective, P(X ≤ 1) with n = 50 and p = 0.02 is 0.74: such a lot is accepted about three times in four. Whether that is acceptable is an engineering and business decision.

## Variability management and tolerance analysis

The quality engineer's job comes down to **reducing variability**:

1. Remove special causes (the outliers, the shifts) as they are found.
2. Reduce common-cause variation by improving the process: better fixtures, tighter material control, calibrated gauges.
3. Center the process between the tolerance limits.

A process whose ±3s spread fits well inside the tolerance makes almost no defective parts, and leaves room for small shifts before any part is out of specification.

## In Python and R

```python
import numpy as np
x = np.array([121.4, 119.8, 120.6, 118.9, 122.1, 120.2, 104.5, 119.5,
              121.0, 120.8, 118.6, 121.7, 120.0, 119.2, 120.4])
q1, q3 = np.percentile(x, [25, 75])
iqr = q3 - q1
low, high = q1 - 1.5 * iqr, q3 + 1.5 * iqr
print(low, high)                          # 117.025 123.225
print(x[(x < low) | (x > high)])          # [104.5]
```

```r
x <- c(121.4, 119.8, 120.6, 118.9, 122.1, 120.2, 104.5, 119.5,
       121.0, 120.8, 118.6, 121.7, 120.0, 119.2, 120.4)
q <- quantile(x, c(0.25, 0.75)); iqr <- q[[2]] - q[[1]]
low <- q[[1]] - 1.5 * iqr; high <- q[[2]] + 1.5 * iqr
c(low, high)                  # 117.025 123.225
x[x < low | x > high]         # 104.5
```

Python may print the fences with a long tail of digits, such as 117.02499999999998: the same number, stored in binary.

## Common mistakes

- Deleting an outlier from the record without investigating it. It may be the most important data point you have.
- Working out control limits from data that include special causes, which makes the limits too wide to catch anything.
- Confusing control limits with specification limits.
- Judging a lot by a sample that was not taken at random.

## What to do next

On the **Exercise** tab, thirteen tablets' coating thicknesses are waiting. Find Q1, Q3 and the two fences, type them in, and press **Check my answers**. Then decide which tablet, if any, a quality engineer should investigate.
