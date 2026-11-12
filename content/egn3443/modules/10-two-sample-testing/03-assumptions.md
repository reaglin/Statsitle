---
id: st.m10.assumptions
title: Assumptions and conditions
minutes: 45
objectives: [st.m10.o2]
builds-on: st.m10.designs
example: st.m10.assumptions.eg
exercise: st.m10.assumptions.ex
reference: [checking-assumptions, two-proportion-z-test, rank-tests]
---

# Assumptions and conditions

**Review and new.** Item 9.4 (*Assumptions and conditions*) taught the checks for the t-tests: Shapiro–Wilk for normality, Levene's test and the F-test for equal variances. This page puts them in one list with the conditions for **proportions**, which are new, and with what to do when a check fails. Check the assumptions **before** you run the test, and say in the report how you checked.

## What each test needs

| Assumption | Needed by | How to check |
|---|---|---|
| **Random samples** (or random assignment) | every test | from how the data were collected |
| **Independent observations within each sample**; each sample under about 10 % of its population when drawn without replacement | every test | from the design; no test on the numbers can check it |
| **The two samples independent of each other** | pooled t, Welch t, two-proportion z | from the design (item 10.2); if they are linked, use a paired test |
| **Roughly normal populations**, or large samples | pooled t, Welch t | box plot or histogram of each sample; Shapiro–Wilk |
| **Roughly normal differences** | paired t | box plot of the differences; Shapiro–Wilk on the differences |
| **Equal variances** | pooled t only | the variance ratio; Levene's test |
| **Enough successes and failures** | two-proportion z | n₁p̂, n₁(1 − p̂), n₂p̂, n₂(1 − p̂) all at least 10 |

Random sampling lets you generalize to the populations. **Random assignment**, as in an experiment where a coin decides which unit gets which treatment, lets you go further: a difference can be put down to the treatment, not to how the units were chosen.

The **10 % condition**: when you sample without replacement from a small population, the observations are not quite independent. Taking 80 castings from a lot of 600 is 13 %, over the guideline; from a lot of 5000 it is 1.6 %, well under it.

## Normality

- **Look first.** In a small sample, strong skew or one far value is a warning.
- **Shapiro–Wilk:** H₀ is that the sample comes from a normal population. W near 1 looks normal; a **small p** (below 0.05) is evidence against normality. A large p is not proof of normality.
- **Sample size.** With each n above about 30, the t-tests work well even for skewed data, because x̄ is nearly normal (the central limit theorem, Module 6). Very skewed data may need more.
- **When it fails** with small samples: the rank tests of item 9.5 (Mann–Whitney for independent samples, Wilcoxon signed-rank for pairs), and look into the odd value: a recording error is corrected, a real value is kept.

## Equal variances (for the pooled t only)

- **The variance ratio**, larger s² over smaller s². The course's rule of thumb: above about 2 to 3, treat the variances as unequal. (Module 9's rule, a ratio of standard deviations under about 2, allows a variance ratio up to 4; the course's rule is the more cautious one.)
- **Levene's test:** H₀ is equal variances. It uses each value's distance from its sample's **median**, so a single far value moves it little: it is robust. A small p says the variances differ.
- **When it fails, or when in doubt:** use Welch. Welch costs very little when the variances are equal, which is why it is the safe default.

## Proportions: the success–failure condition

The two-proportion z-test of item 10.5 uses the normal curve for p̂₁ − p̂₂. That works when each sample has enough of both outcomes. With the **pooled proportion** p̂ = (x₁ + x₂)/(n₁ + n₂), which item 10.5 explains, all four of n₁p̂, n₁(1 − p̂), n₂p̂ and n₂(1 − p̂) should be **at least 10**. Some textbooks use 5.

**By hand.** Line A has 6 rejects in 80 parts, line B 3 in 90. p̂ = (6 + 3)/(80 + 90) = 9/170 = 0.0529. Then n₁p̂ = 80 × 0.0529 = 4.2 and n₂p̂ = 90 × 0.0529 = 4.8: both under 10 (and under 5). Too few rejects for the z-test. Use **Fisher's exact test** instead, which works from the counts themselves: Python `stats.fisher_exact([[6, 74], [3, 87]])`, R `fisher.test(matrix(c(6, 74, 3, 87), nrow = 2, byrow = TRUE))`.

The two groups must also be different items. If the **same** items are classified twice (each weld inspected by two inspectors, pass or fail), the proportions are paired, and the test is McNemar's test, not the two-proportion z.

## The Example: two resin suppliers

[[data: st.m10.assumptions.eg]]

[[figure: st.m10.assumptions.eg 1]]

The two box plots have about the same height: the spreads look alike, and neither sample has a far value.

[[results: st.m10.assumptions.eg]]

Read the checks at the bottom of the results:

- **Shapiro–Wilk:** p = 0.842 for supplier A and 0.880 for supplier B. No evidence against normality.
- **Variance ratio:** s_A = 0.0538 and s_B = 0.0579, so s_B²/s_A² = 1.16, far under 2.
- **Levene:** p = 0.725. No evidence that the variances differ.

The parts were molded in random order on one press, and parts from different supplier lots have nothing to do with each other: the samples are independent. Every assumption of the pooled t holds. It gives t = −3.32 with 22 degrees of freedom and p = 0.0031: supplier B's resin shrinks more, by 0.029 to 0.123 percentage points (the 95 % interval for μ_A − μ_B is −0.123 to −0.029). With n₁ = n₂, Welch's t is the same number, −3.32, with ν = 21.88; when the checks pass, the two tests agree.

[[figure: st.m10.assumptions.eg 2]]

## In Python and R

```python
import numpy as np
from scipy import stats
a = np.array([1.42, 1.38, 1.51, 1.45, 1.40, 1.47, 1.36, 1.49, 1.44, 1.41, 1.53, 1.39])
b = np.array([1.50, 1.46, 1.58, 1.44, 1.55, 1.52, 1.47, 1.61, 1.49, 1.54, 1.43, 1.57])
print(stats.shapiro(a), stats.shapiro(b))     # p = 0.842 and 0.880
print(b.var(ddof=1) / a.var(ddof=1))          # variance ratio 1.16
print(stats.levene(a, b))                     # center='median': p = 0.725
print(stats.ttest_ind(a, b, equal_var=True))  # pooled: t = -3.3243, p = 0.0031
```

```r
a <- c(1.42, 1.38, 1.51, 1.45, 1.40, 1.47, 1.36, 1.49, 1.44, 1.41, 1.53, 1.39)
b <- c(1.50, 1.46, 1.58, 1.44, 1.55, 1.52, 1.47, 1.61, 1.49, 1.54, 1.43, 1.57)
shapiro.test(a); shapiro.test(b)
var(b) / var(a)
t.test(a, b, var.equal = TRUE)                # t = -3.3243, df = 22, p-value = 0.00308
```

The Code tab writes Levene's test by hand in R, so no extra package is needed.

[[code: st.m10.assumptions.eg]]

## Common mistakes

- **Testing first and checking afterwards**, then choosing whichever test gave the smaller p. Decide from the design and the checks.
- **Reading Shapiro–Wilk backwards.** A small p says *not* normal.
- **Checking the two columns of a paired design.** For the paired t, check the differences.
- **The success–failure check with the wrong counts.** It needs the expected successes and failures in **each** sample, four numbers, not just x₁ and x₂.
- **Deleting a far value to make a check pass.** Find out why it is far. A typing error is corrected; a real but unusual value stays, and the rank test handles it.

## What to do next

On the **Exercise** tab, two tunings of a servo drive are compared. Read the Shapiro–Wilk and Levene checks in the Workbench, and press **Check my answers**.

Then decide whether the checks support a t-test. Look at the box plots and at W for tuning 2 next to W for tuning 1. Notice also what Levene's test says about two samples whose standard deviations differ by a factor of five, and remember that it works from the medians: one far value moves it little. With samples of 10 and 11 and one run far from the rest, the mean and the standard deviation of tuning 2 rest heavily on that one run. Before any test, find out what happened in it; if it is a real result, compare the tunings with the Mann–Whitney test of item 9.5, which ranks the values.
