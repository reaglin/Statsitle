---
id: st.m14.assumptions
title: Assumptions underlying ANOVA
minutes: 50
objectives: [st.m14.o2]
builds-on: st.m14.introduction
example: st.m14.assumptions.eg
exercise: st.m14.assumptions.ex
reference: [anova-assumptions, checking-assumptions, rank-tests]
---

# Assumptions underlying ANOVA

The F test's p-value is calculated under three assumptions. They are the pooled t-test's assumptions from Module 9, extended to k groups.

| Assumption | What it means | How to check |
|---|---|---|
| **Independence** | One observation does not affect another, within a group or between groups | From how the data were collected: random assignment, separate units, no repeated measurements of one unit. No test on the numbers can check it |
| **Normality** | Within each group the response is roughly normal. Equivalently, the **residuals** eᵢⱼ = yᵢⱼ − ȳᵢ (each value minus its group's mean) are roughly normal | Box plots of the groups; a normal Q–Q plot of the residuals; the Shapiro–Wilk test on the residuals |
| **Equal variances** (homogeneity of variance, homoscedasticity) | The groups have the same population variance, σ₁² = σ₂² = … = σₖ² | Box plots; the largest standard deviation against the smallest; Levene's and Bartlett's tests |

Here yᵢⱼ is observation j in group i and ȳᵢ is the mean of group i. Pooling the residuals of all the groups gives one sample of N values to check for normality, which works much better than checking k small groups one by one.

## The checks

- **Look first.** Box plots side by side show a group that is much more spread out, a skewed group, or a far outlier. The normal Q–Q plot of the residuals (Module 5) should be close to a straight line.
- **Shapiro–Wilk on the residuals.** H₀: the residuals come from a normal distribution. A small p (below 0.05) is evidence against normality.
- **Rule of thumb for spread.** If the largest group standard deviation is less than about twice the smallest, the F test is little affected, especially when the groups are the same size.
- **Levene's test.** H₀: all k variances are equal. Take each value's distance from its own group's **median**, zᵢⱼ = |yᵢⱼ − ỹᵢ|, and run a one-way ANOVA on the z's; Levene's W is that ANOVA's F, with k − 1 and N − k degrees of freedom. A small p says the variances differ. Centered on the median (the Brown–Forsythe form, used by Statistics+, scipy and R's car package), it is robust to non-normal data.
- **Bartlett's test.** Also H₀: equal variances, compared with a χ² distribution on k − 1 df. It is more powerful than Levene's when the data are normal, but it cannot tell unequal variances from non-normality: a skewed group or an outlier makes it reject too. Use Levene to decide; read Bartlett alongside.

## What happens when they fail

- **Dependence** is the most serious: if repeated measurements of one unit are treated as separate observations, the p-values are wrong, and no later fix on the numbers repairs that. Design the study for independence.
- **Mild non-normality** matters little, especially with equal group sizes and ten or more per group, because the group means are nearly normal (the central limit theorem, Module 6). Strong skew or outliers in small groups do matter.
- **Unequal variances** matter most when the group sizes also differ: if the small groups have the large variances, the F test rejects too often; if the large groups do, it loses power. With equal sizes the F test is fairly robust.

## Remedies

- **Welch's ANOVA** keeps each group's own variance, just as Welch's t-test does for two groups. R: `oneway.test(y ~ group)`; Python: `anova_oneway([a, b, c], use_var="unequal")` from statsmodels. The Workbench does not calculate it, so use the code below when Levene's test rejects.
- **Transform** the response. When the spread grows with the mean, the logarithm often makes the variances equal and the groups less skewed. Then analyze the transformed values.
- **Kruskal–Wallis test**, the rank-based alternative to one-way ANOVA (the k-group form of Module 9's Mann–Whitney test). Rank all N values together (1 = smallest, ties share the average rank), let Rᵢ be the rank sum of group i, and

H = 12 / (N(N + 1)) × Σ Rᵢ²/nᵢ − 3(N + 1),

compared with χ² on k − 1 df (the Workbench also corrects for ties). H₀: all the groups have the same distribution. It needs no normality, and an outlier counts only as the largest rank. It compares where the groups lie, not their means, and it still assumes the groups have similar shapes and spreads.

## The Example: weld strength from three suppliers

[[data: st.m14.assumptions.eg]]

[[figure: st.m14.assumptions.eg 1]]

The box plot draws supplier A at the top and C at the bottom. Supplier C's welds spread from 389 to 440 MPa; A's and B's stay within 405 to 425 MPa. The standard deviations are 4.31, 3.11 and 18.69 MPa: C's is six times B's.

[[results: st.m14.assumptions.eg]]

Read the checks at the bottom of the results:

- **Levene** W = 19.77, p = 0.00001: strong evidence that the variances differ.
- **Bartlett** gives 22.78, p = 0.00001, the same conclusion.
- **Shapiro–Wilk of the residuals**, p = 0.242: no evidence against normality.

So the equal-variance assumption fails, and the F test above it cannot be trusted. Its F = 1.42 and p = 0.265 say "no difference", because the one pooled MS within (125.8) is dominated by supplier C and swamps the clear difference between A and B (means 411.4 and 420.8 MPa, each with a small spread). Welch's ANOVA, which keeps the variances apart, gives F = 11.83 on 2 and 12.19 df, p = 0.0014: the suppliers do differ. Kruskal–Wallis gives H = 5.55, p = 0.062. It compares ranks, and C's welds are spread from the lowest to the highest values, so the ranks blur the A–B difference.

[[figure: st.m14.assumptions.eg 3]]

The Q–Q plot of the residuals lies close to its line: the problem here is spread, not shape.

## In Python and R

```python
from scipy import stats
a = [412, 405, 418, 409, 415, 411, 407, 414]
b = [421, 416, 425, 419, 423, 418, 420, 424]
c = [398, 431, 405, 440, 389, 426, 412, 435]
print(stats.levene(a, b, c))        # center='median' by default: W = 19.77
print(stats.bartlett(a, b, c))
print(stats.kruskal(a, b, c))       # H = 5.55, p = 0.062
from statsmodels.stats.oneway import anova_oneway
print(anova_oneway([a, b, c], use_var="unequal"))   # Welch's ANOVA: F = 11.83, p = 0.0014
```

```r
y <- c(412, 405, 418, 409, 415, 411, 407, 414, 421, 416, 425, 419, 423, 418, 420, 424,
       398, 431, 405, 440, 389, 426, 412, 435)
g <- factor(rep(c("A", "B", "C"), each = 8))
bartlett.test(y ~ g)
z <- abs(y - ave(y, g, FUN = median)); summary(aov(z ~ g))   # Levene (median): W = 19.77
shapiro.test(residuals(aov(y ~ g)))
kruskal.test(y ~ g)
oneway.test(y ~ g)                  # Welch's ANOVA
```

[[code: st.m14.assumptions.eg]]

## In the Workbench

**One-way ANOVA** reports Bartlett, Levene, Shapiro–Wilk of the residuals and Kruskal–Wallis under the ANOVA table, and draws the box plots and the Q–Q plot. Read them before the F.

## Common mistakes

- **Checking normality on the raw data of all the groups together.** Different group means make the pooled raw data look non-normal (even two-humped). Check the **residuals**.
- **Reading a large p as proof.** Levene p = 0.4 does not prove the variances equal; with small groups the tests have little power, so look at the plots too.
- **Trusting Bartlett on skewed data.** It rejects for non-normality as well as for unequal spread.
- **Forgetting independence.** It comes from the design. Five readings of each of four specimens are not 20 independent observations.

## What to do next

On the **Exercise** tab, check the assumptions for the cycle times of three molding machines, and run the Kruskal–Wallis test.
