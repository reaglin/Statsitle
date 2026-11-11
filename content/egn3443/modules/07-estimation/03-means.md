---
id: st.m7.means
title: Confidence intervals for means
minutes: 50
objectives: [st.m7.o3, st.m7.o2]
builds-on: st.m7.sampling-clt
example: st.m7.means.eg
exercise: st.m7.means.ex
reference: [confidence-interval-mean, clt-for-inference]
---

# Confidence intervals for means

## The shape of every interval

Every confidence interval in this course has the same three parts:

estimate ± critical value × standard error

- The **estimate** is the point estimate, here x̄.
- The **standard error** is the spread of the estimate: σ/√n, or s/√n when σ is estimated.
- The **critical value** sets the confidence. It comes from the z or the t distribution.
- critical value × standard error is the **margin of error** E, the half-width of the interval.

The **confidence level** is written 1 − α: 95 % confidence means α = 0.05, split as α/2 = 0.025 in each tail.

## σ known: the z interval

When the population standard deviation σ is known (from long experience with a process, or a calibrated instrument), and the population is normal or n is large (item 7.2):

x̄ ± z_{α/2} · σ/√n

| Confidence | 90 % | 95 % | 99 % |
|---|---|---|---|
| z_{α/2} | 1.645 | 1.960 | 2.576 |

## σ unknown: the t interval

In most engineering work σ is not known, and the sample standard deviation s takes its place. s varies from sample to sample too, and that adds uncertainty. The **t distribution** allows for it. It looks like the standard normal but has heavier tails, and it depends on the **degrees of freedom** (df), here n − 1, the number of deviations that are free once x̄ is fixed. As df grows, t approaches z.

x̄ ± t_{α/2, n−1} · s/√n

| df = n − 1 | 5 | 11 | 29 | very large |
|---|---|---|---|---|
| t for 95 % | 2.571 | 2.201 | 2.045 | 1.960 |

**Which one?** Use z only when σ is genuinely known. With s from the sample, use t, whatever n is. For n under about 30 the difference is large; for big samples t is close to z, but t is still the right choice.

The t interval assumes the data come from a roughly normal population. With n ≥ 30 the CLT relaxes that; with a small sample, look at a histogram or box plot first (Module 2) for strong skew or outliers.

## A worked case with σ known

A check weight is weighed n = 25 times on a calibrated scale whose readings have σ = 0.5 g. The mean reading is x̄ = 100.3 g. Find the 95 % confidence interval for the true mean reading μ.

1. **Standard error:** σ/√n = 0.5 / 5 = 0.1 g.
2. **Critical value:** z_{0.025} = 1.960.
3. **Margin of error:** 1.960 × 0.1 = 0.196 g.
4. **Interval:** 100.3 ± 0.196, from **100.104 to 100.496 g**.

[[figure: st.m7.means.z 1]]

## A worked case with σ unknown

Six asphalt cores from a new pavement have a mean air-void content of x̄ = 4.2 % with s = 0.5 %. Find the 95 % confidence interval for the mean air voids μ.

1. **Standard error:** s/√n = 0.5 / √6 = 0.2041 %.
2. **Degrees of freedom:** n − 1 = 5. **Critical value:** t_{0.025, 5} = 2.571.
3. **Margin of error:** 2.571 × 0.2041 = 0.525 %.
4. **Interval:** 4.2 ± 0.525, from **3.675 to 4.725 %**.

With z = 1.960 in place of t the margin would be 0.400 %, an interval about a quarter too narrow that would claim more precision than six cores give.

## The Example: tank drain times

A test rig drained a tank twelve times and recorded the time in seconds. σ is not known, so the interval uses t with 11 degrees of freedom. The data are in a data table, and the Workbench's **Data from** setting is *Columns of a data table*: it calculates x̄ and s from the column for you.

[[data: st.m7.means.eg]]

[[figure: st.m7.means.eg 1]]

[[results: st.m7.means.eg]]

x̄ = 40.975 s and s = 1.890 s, so the standard error is 1.890 / √12 = 0.5455 s. With t_{0.025, 11} = 2.201 the margin of error is 1.201 s, and the 95 % interval runs from 39.77 to 42.18 s. "We are 95 % confident that the mean drain time of this rig is between 39.8 and 42.2 seconds."

## One-sided bounds

Sometimes only one side matters: a specification sets a **minimum** strength, or a **maximum** drain time. A one-sided 95 % bound puts all of α = 0.05 in one tail, so it uses t_{α, n−1} instead of t_{α/2, n−1}:

- upper bound: μ ≤ x̄ + t_{α, n−1} · s/√n
- lower bound: μ ≥ x̄ − t_{α, n−1} · s/√n

For the drain times, t_{0.05, 11} = 1.796, so μ ≤ 40.975 + 1.796 × 0.5455 = **41.95 s** with 95 % confidence. In the Workbench, a one-sided 95 % bound is the matching end of the **90 %** two-sided interval.

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([41.2, 38.7, 43.5, 40.1, 39.8, 42.6, 44.0, 37.9, 41.5, 40.8, 42.2, 39.4])
n, xbar, s = len(x), x.mean(), x.std(ddof=1)
se = s / np.sqrt(n)                       # 0.5455
t_crit = stats.t.ppf(0.975, n - 1)        # 2.2010
print(xbar - t_crit * se, xbar + t_crit * se)    # 39.7743 42.1757
print(stats.t.interval(0.95, n - 1, loc=xbar, scale=se))   # the same, in one call
```

```r
x <- c(41.2, 38.7, 43.5, 40.1, 39.8, 42.6, 44.0, 37.9, 41.5, 40.8, 42.2, 39.4)
n <- length(x); se <- sd(x) / sqrt(n)
t_crit <- qt(0.975, n - 1)                       # 2.2010
mean(x) + c(-1, 1) * t_crit * se                 # 39.7743 42.1757
t.test(x, conf.level = 0.95)$conf.int            # the same
```

For σ known, use `stats.norm.ppf(0.975)` or `qnorm(0.975)` (1.960) and σ in place of s.

[[code: st.m7.means.eg]]

## Common mistakes

- Using z = 1.96 when σ was estimated by s, especially with a small n. The interval comes out too narrow.
- Using n instead of n − 1 for the degrees of freedom.
- Multiplying the critical value by s instead of the standard error s/√n. The interval comes out √n times too wide.
- Using t_{0.05} (one tail) for a two-sided 95 % interval. Two-sided needs α/2 = 0.025 in each tail.

## What to do next

On the **Exercise** tab, a lab tested eight concrete cores. From their mean and standard deviation, calculate the standard error, the critical value, the margin of error and the interval.
