---
id: st.m9.assumptions
title: Assumptions and conditions
minutes: 45
objectives: [st.m9.o3]
builds-on: st.m9.selecting
example: st.m9.assumptions.eg
exercise: st.m9.assumptions.ex
reference: [checking-assumptions, choosing-a-two-sample-test]
---

# Assumptions and conditions

Every test's p-value is calculated under assumptions. When they fail, the p-value can be far from the truth, and nothing on the screen warns you. Checking them is part of the test, and the report should say how you checked.

## What each test assumes

| Test | Assumes |
|---|---|
| All of them | **Random samples**, and **independent observations** within each sample (one part's value does not affect the next) |
| Two-sample z, pooled t, Welch t | The two samples are **independent of each other**; each comes from a roughly **normal** population (or n is large) |
| Pooled t | Also **equal variances**, σ₁² = σ₂² |
| Paired t | The pairs are independent of each other; the **differences** are roughly normal |
| F-test for two variances | Both populations **normal**. It is very sensitive to non-normality |
| Mann–Whitney, Wilcoxon | Random, independent observations; no normality needed (item 9.5) |

Independence and random sampling come from **how the data were collected**: no test on the numbers can check them. Read the description of the study. Normality and equal variances can be checked on the data.

## Checking normality

- **Look first.** A box plot or histogram of each sample (the differences, for pairs). Strong skew or a far outlier in a small sample is a warning.
- **Shapiro–Wilk test.** H₀: the sample comes from a normal population. Its statistic W is near 1 for normal-looking data. A **small p** (below 0.05) is evidence **against** normality. A large p does not prove normality; it only says the data give no reason to doubt it.
- **Sample size.** With each n above about 30, the t-tests work well even for non-normal data, because x̄ is nearly normal (the central limit theorem, Module 6). With large samples Shapiro–Wilk flags tiny departures that do not matter. With small samples it has little power, so look at the plots too.

## Checking equal variances

- **Rule of thumb (the course's):** divide the larger variance s² by the smaller. Above about 2 to 3, treat the variances as unequal and use Welch. Square the standard deviations first: s's of 3 and 2 give a variance ratio of 9/4 = 2.25, already at the edge. Pooling does least harm when n₁ = n₂.
- **Levene's test.** H₀: σ₁² = σ₂². It compares the average distance of each value from its sample's median. A **small p** says the variances differ: do not pool, use Welch. Levene's test is robust: it works even when the data are not normal.
- **F-test.** F = s₁²/s₂², with n₁ − 1 and n₂ − 1 degrees of freedom. If the variances are equal, F is near 1. The **F distribution** is the distribution this ratio follows when both samples come from normal populations with equal variances: it takes only positive values, is skewed to the right, and has two degrees of freedom, one for the top (n₁ − 1) and one for the bottom (n₂ − 1). F(0.025; 9, 11) is the F value with 0.025 of the area above it, with 9 and 11 degrees of freedom: Python stats.f.ppf(0.975, 9, 11), R qf(0.975, 9, 11), which both take the area below it. The two-sided p-value is twice the smaller tail. It answers the question "are the spreads different?" well when the data are normal, but it is easily fooled by non-normal data. So use Levene to decide whether to pool, and the F-test when the variance itself is the question (repeatability, consistency), with normal data.

## A worked F-test by hand

Two filling heads are compared for consistency. Head 1: n₁ = 10, s₁ = 3.0 g. Head 2: n₂ = 12, s₂ = 1.5 g.

1. F = s₁²/s₂² = 9.00 / 2.25 = **4.00**, with 9 and 11 degrees of freedom.
2. The upper critical value at α = 0.05, two-sided, is F(0.025; 9, 11) = 3.588. 4.00 is beyond it.
3. p = 2 × P(F > 4.00) with F on 9 and 11 df = **0.034** < 0.05. Reject H₀: head 1 is less consistent.

Put the larger variance on top if you like (then only the upper tail matters), but keep it matched to H₁ and to the order of the samples.

## The Example: two filling machines

[[data: st.m9.assumptions.eg]]

[[figure: st.m9.assumptions.eg 1]]

The old machine's packs spread from 497.6 to 503.1 g, the new machine's only from 499.6 to 500.5 g.

[[results: st.m9.assumptions.eg]]

Read the checks at the bottom of the results:

- **Shapiro–Wilk:** p = 0.961 for the new machine and 0.290 for the old one. No evidence against normality in either.
- **Levene:** W = 21.64 (Levene's statistic: the Workbench calls it W too; it is not the Shapiro–Wilk W), p = 0.0001. Strong evidence that the variances differ (s = 0.28 g against 1.93 g, almost seven times). Do not pool.

So the test is **Welch's**: t = −0.37 with ν = 11.46 df, p = 0.717. The mean fill weights do not differ detectably. The real difference between the machines is their consistency, which the means hide. Notice ν = 11.46, close to n − 1 = 11 of the old machine: with such unequal variances, almost all the uncertainty comes from the more variable sample.

## In Python and R

```python
import numpy as np
from scipy import stats
new = np.array([500.2, 499.8, 500.5, 500.1, 499.6, 500.3, 499.9, 500.4, 500.0, 499.7, 500.2, 500.1])
old = np.array([501.8, 498.1, 500.9, 497.6, 502.4, 499.0, 503.1, 498.7, 501.2, 497.9, 500.6, 502.0])
print(stats.shapiro(new))           # W and p for each sample
print(stats.shapiro(old))
print(stats.levene(new, old))       # center='median' by default
F = old.var(ddof=1) / new.var(ddof=1)
p = 2 * min(stats.f.sf(F, len(old) - 1, len(new) - 1), stats.f.cdf(F, len(old) - 1, len(new) - 1))
```

```r
new <- c(500.2, 499.8, 500.5, 500.1, 499.6, 500.3, 499.9, 500.4, 500.0, 499.7, 500.2, 500.1)
old <- c(501.8, 498.1, 500.9, 497.6, 502.4, 499.0, 503.1, 498.7, 501.2, 497.9, 500.6, 502.0)
shapiro.test(new); shapiro.test(old)
car::leveneTest(c(new, old) ~ factor(rep(c("new", "old"), c(12, 12))))   # needs the car package
var.test(old, new)                  # the F-test, with its confidence interval for the ratio
```

The **Code** tab of the Example writes Levene's test by hand in R, so no extra package is needed.

[[code: st.m9.assumptions.eg]]

## In the Workbench

Choose **Two-sample test** with **Data from** set to the two columns, and any of the mean tests: the Shapiro–Wilk and Levene rows appear with the results. For the F-test, set **Test** to *Two variances (F)*.

## Common mistakes

- **Reading Shapiro–Wilk backwards.** A small p says *not* normal; a large p says "no evidence against normal".
- **Using the F-test to decide whether to pool, on skewed data.** It rejects equal variances too often when the data are not normal. Levene is the safer check.
- **F with standard deviations.** F is the ratio of **variances**: square the standard deviations first.
- **Checking the two columns of a paired design separately.** For the paired t, check the **differences**.

## What to do next

On the **Exercise** tab, two thickness gauges are compared for repeatability with the F-test. Find F and its p-value, and press **Check my answers**.
