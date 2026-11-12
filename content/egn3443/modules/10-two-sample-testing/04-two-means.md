---
id: st.m10.two-means
title: Comparing two means
minutes: 50
objectives: [st.m10.o3, st.m10.o5]
builds-on: st.m10.assumptions
example: st.m10.two-means.eg
exercise: st.m10.two-means.ex
reference: [two-sample-t-tests, paired-t-test]
---

# Comparing two means

**Review and new.** Items 9.1 and 9.2 worked Welch's test and the paired t-test by hand. This page works the **pooled t** and a paired case with new numbers, then adds two things Module 9 did not use: a **one-sided** test, and a test against a difference **other than zero**. Choose the design first (item 10.2), check the assumptions (item 10.3), then compute.

## The three t-tests

| Design | Test | Standard error | df |
|---|---|---|---|
| Independent, equal variances reasonable | pooled t | sₚ √(1/n₁ + 1/n₂), with sₚ² = [(n₁ − 1)s₁² + (n₂ − 1)s₂²]/(n₁ + n₂ − 2) | n₁ + n₂ − 2 |
| Independent, variances may differ | Welch t | √(s₁²/n₁ + s₂²/n₂) | ν, the Welch–Satterthwaite formula (item 9.1) |
| Paired, n pairs | paired t | s_d/√n | n − 1 |

In every case t = (observed difference − Δ₀)/SE, and the (1 − α) confidence interval is (observed difference) ± t(α/2, df) × SE, where t(α/2, df) is the t value with α/2 of the area above it (Python `stats.t.ppf(1 - alpha/2, df)`, R `qt(1 - alpha/2, df)`).

## The pooled t by hand

The course page *Comparing Two Means* pools two samples: n₁ = 10 with s₁² = 4, and n₂ = 15 with s₂² = 6. It stops at the standard error; give the samples means of x̄₁ = 27.3 and x̄₂ = 25.6 and finish the test of H₀: μ₁ − μ₂ = 0 against H₁: μ₁ − μ₂ ≠ 0 at α = 0.05.

1. **Pooled variance:** sₚ² = (9 × 4 + 14 × 6)/23 = 120/23 = 5.217, so sₚ = 2.284. It is a weighted average of 4 and 6, closer to 6 because the second sample is larger.
2. **Standard error:** SE = 2.284 × √(1/10 + 1/15) = 2.284 × 0.4082 = 0.9325.
3. **Statistic:** t = (27.3 − 25.6)/0.9325 = 1.7/0.9325 = **1.823**, with df = 10 + 15 − 2 = **23**.
4. **p-value:** 2 × P(T > 1.823) with T on 23 df = **0.081**. Not below 0.05: do not reject H₀.
5. **Interval:** t(0.025, 23) = 2.069, so 1.7 ± 2.069 × 0.9325 = 1.7 ± 1.929, from **−0.23 to 3.63**. It contains 0, as the test said.

## The paired t by hand

Six machinists each made the same part with the old CNC program and with a new one, in random order (time, min).

| Machinist | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| Old program | 14.2 | 12.8 | 15.6 | 13.1 | 16.0 | 13.9 |
| New program | 13.0 | 12.5 | 14.1 | 12.6 | 14.8 | 13.2 |
| d = old − new | 1.2 | 0.3 | 1.5 | 0.5 | 1.2 | 0.7 |

1. d̄ = 5.4/6 = **0.9** min.
2. The squared deviations from 0.9 add to 0.09 + 0.36 + 0.36 + 0.16 + 0.09 + 0.04 = 1.10, so s_d = √(1.10/5) = **0.469**.
3. SE = 0.469/√6 = 0.1915, and t = 0.9/0.1915 = **4.70** with 6 − 1 = **5** df.
4. p = 2 × P(T > 4.70) with T on 5 df = **0.0053**: reject H₀. With t(0.025, 5) = 2.571 the 95 % interval for μ_d is 0.9 ± 0.49, from **0.41 to 1.39** min saved per part.

The machinists differ from each other by up to 3.2 min on the old program, far more than the program changes one machinist's time; treated as two independent samples, the same twelve numbers give Welch t = 1.39 and p = 0.197.

## One-sided tests

When only one direction matters, and that was decided **before** the data were seen, H₁ points one way and the whole α goes in one tail. For H₁: μ₁ − μ₂ > 0, p = P(T > t), and H₀ is rejected when t is above t(α, df), the value with α of the area above it. A one-sided test finds a difference in the stated direction with smaller samples, and can never find one in the other direction.

## Testing against a difference that matters, Δ₀

H₀ need not say "no difference". If a change pays only when it gains at least Δ₀, test

H₀: μ₁ − μ₂ = Δ₀ against H₁: μ₁ − μ₂ > Δ₀, with t = (x̄₁ − x̄₂ − Δ₀)/SE.

Rejecting H₀ then says the gain is **more than Δ₀**, not only more than zero. In the Workbench this is **Hypothesised difference (Δ₀)**.

## The Example: a manual and a robotic cell

[[data: st.m10.two-means.eg]]

[[figure: st.m10.two-means.eg 1]]

The robot's times hardly vary (s = 0.91 s) next to the manual cell's (s = 3.07 s): the variance ratio is 11.3, and Levene's p = 0.005. The variances are not equal, so the test is Welch's. Both samples look normal (Shapiro–Wilk p = 0.75 and 0.76).

[[results: st.m10.two-means.eg]]

The manual cell averages 50.53 s and the robot 46.57 s, a difference of 3.96 s. Welch's t = 4.25 with ν = 13.29 degrees of freedom. H₁ is one-sided (>), so p = P(T > 4.25) = **0.0005**: reject H₀. The robot is faster.

[[figure: st.m10.two-means.eg 2]]

The shaded area is the rejection region, now all in the upper tail: t above t(0.05, 13.29) = 1.768. The orange line is the observed t.

**The interval.** The Workbench gives the two-sided 95 % interval for μ_manual − μ_robot, 1.95 to 5.96 s, whatever H₁ is. Its lower end is above 0, so a one-sided test at 0.05 rejects as well. (A one-sided test at α can reject while the two-sided (1 − α) interval still holds 0; the interval matches it exactly only at 1 − 2α, here 90 %.)

**Faster by how much?** Suppose the robot pays for itself only if it saves more than 3 s per assembly. Set **Hypothesised difference (Δ₀)** to 3: t = (3.955 − 3)/0.932 = 1.03 and p = 0.162. The data show the robot is faster, but **not** that it is more than 3 s faster: the interval's lower end, 1.95 s, is below 3. That is a different decision from "it is faster", and item 10.6 builds on it.

## In Python and R

```python
import numpy as np
from scipy import stats
manual = np.array([48.2, 52.6, 45.9, 55.1, 50.3, 47.8, 53.9, 49.6, 51.2, 46.5, 54.4, 50.8])
robot = np.array([46.1, 47.0, 45.4, 47.9, 46.6, 45.8, 47.3, 46.9, 45.2, 47.5])
print(stats.ttest_ind(manual, robot, equal_var=False, alternative='greater'))  # t = 4.2459, p = 0.00046
print(stats.ttest_ind(manual - 3, robot, equal_var=False, alternative='greater'))  # Δ₀ = 3: p = 0.162
print(stats.ttest_ind_from_stats(27.3, 2, 10, 25.6, np.sqrt(6), 15, equal_var=True))  # the pooled case: t = 1.823
```

```r
manual <- c(48.2, 52.6, 45.9, 55.1, 50.3, 47.8, 53.9, 49.6, 51.2, 46.5, 54.4, 50.8)
robot <- c(46.1, 47.0, 45.4, 47.9, 46.6, 45.8, 47.3, 46.9, 45.2, 47.5)
t.test(manual, robot, alternative = "greater")           # Welch: t = 4.2459, df = 13.285, p-value = 0.0004555
t.test(manual, robot, alternative = "greater", mu = 3)   # Δ₀ = 3: t = 1.0252, p-value = 0.1618
```

scipy's `ttest_ind` has no Δ₀ argument; subtracting Δ₀ from the first sample does the same. R's `t.test` takes it as `mu`.

[[code: st.m10.two-means.eg]]

## Common mistakes

- **Pooling by habit.** Pool only when the checks support equal variances; otherwise Welch.
- **n₁ + n₂ degrees of freedom.** The pooled test has n₁ + n₂ − 2, the paired test n − 1 with n the number of pairs.
- **A one-sided test chosen after seeing the data.** It doubles the real Type I error rate.
- **Reading "faster" as "faster by enough".** Rejecting H₀: μ₁ − μ₂ = 0 says only that there is a difference; test against Δ₀, or read the interval, for "by at least Δ₀".

## What to do next

On the **Exercise** tab, laminates cured at two temperatures are compared with the pooled t-test. Find t, its degrees of freedom, the p-value and the confidence interval, and press **Check my answers**.
