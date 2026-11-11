---
id: st.m9.two-sample
title: Two-sample hypothesis tests
minutes: 50
objectives: [st.m9.o1, st.m9.o2]
example: st.m9.two-sample.eg
exercise: st.m9.two-sample.ex
reference: [two-sample-t-tests, inference-preview]
---

# Two-sample hypothesis tests

## The logic, from Module 8

Every test in this module follows the steps Module 8 teaches with the one-sample test:

1. **Hypotheses.** The **null hypothesis H₀** is the "no difference" claim. The **alternative H₁** is what you want evidence for. It is **two-sided** (≠) when a difference either way matters, and **one-sided** (< or >) only when the direction was fixed before you saw the data.
2. **Significance level α**, chosen first, usually 0.05: the chance you accept of rejecting a true H₀ (a Type I error).
3. **Test statistic:** how far the data are from H₀, in standard errors.
4. **p-value:** the probability, if H₀ were true, of a statistic at least as extreme as the one observed.
5. **Decision:** reject H₀ when p < α; otherwise do not reject it. Not rejecting is not proof that H₀ is true.

If these steps are new to you, read Module 8 first.

## Two independent samples

Two samples are **independent** when the units in one have nothing to do with the units in the other: coupons of alloy A and different coupons of alloy B, or parts from line 1 and other parts from line 2. The question is about the difference of the population means:

H₀: μ₁ − μ₂ = 0 against H₁: μ₁ − μ₂ ≠ 0 (or < 0, or > 0)

The natural estimate of μ₁ − μ₂ is x̄₁ − x̄₂. Every two-sample statistic divides it by its **standard error** (SE), the standard deviation of x̄₁ − x̄₂ from sample to sample. The tests differ in how they get the SE.

| Test | When | Standard error | Reference distribution |
|---|---|---|---|
| **Two-sample z** | both population σ₁ and σ₂ known (from long process history) | √(σ₁²/n₁ + σ₂²/n₂) | standard normal Z |
| **Pooled t** | σ unknown, but the two variances can be taken as equal | s_p √(1/n₁ + 1/n₂) | t with n₁ + n₂ − 2 degrees of freedom |
| **Welch t** | σ unknown, variances may differ (the safe default) | √(s₁²/n₁ + s₂²/n₂) | t with ν degrees of freedom (below) |

The statistic is always (x̄₁ − x̄₂ − Δ₀) / SE, where Δ₀ is the difference H₀ claims, almost always 0.

**Degrees of freedom (df)** count how many independent pieces of information the standard deviations rest on. They set the shape of the t curve: fewer df, heavier tails, larger critical values.

- The **pooled** test estimates one common σ from both samples: s_p² = [(n₁ − 1)s₁² + (n₂ − 1)s₂²] / (n₁ + n₂ − 2), a weighted average of the two variances, with df = n₁ + n₂ − 2.
- The **Welch** test keeps the variances apart. Its df come from the Welch–Satterthwaite formula, with a = s₁²/n₁ and b = s₂²/n₂:

ν = (a + b)² / [a²/(n₁ − 1) + b²/(n₂ − 1)]

ν is usually not a whole number, and it is always between the smaller of n₁ − 1 and n₂ − 1 and n₁ + n₂ − 2.

## A worked case by hand

A plant cures adhesive joints in two ovens and measures the bond strength (kN). Oven A: n₁ = 8 joints, x̄₁ = 24.6, s₁ = 1.2. Oven B: n₂ = 12 joints, x̄₂ = 23.5, s₂ = 2.1. Is there a difference in mean strength at α = 0.05? The difference is x̄₁ − x̄₂ = 1.1 kN.

**If σ were known** (say σ₁ = 1.0 and σ₂ = 2.0 kN from years of records), the z-test: SE = √(1.0²/8 + 2.0²/12) = √0.4583 = 0.6770, z = 1.1 / 0.6770 = **1.625**, and p = 2 × P(Z > 1.625) = **0.104**.

With only the sample standard deviations, use a t-test.

**Pooled t:**

1. s_p² = (7 × 1.44 + 11 × 4.41) / 18 = (10.08 + 48.51) / 18 = 3.255, so s_p = 1.804.
2. SE = 1.804 × √(1/8 + 1/12) = 1.804 × 0.4564 = 0.8235.
3. t = 1.1 / 0.8235 = **1.336**, with df = 8 + 12 − 2 = **18**.
4. p = 2 × P(T₁₈ > 1.336) = **0.198**. The critical value is t(0.025, 18) = 2.101, and 1.336 is well inside it.

**Welch t:**

1. a = 1.44 / 8 = 0.1800 and b = 4.41 / 12 = 0.3675, so SE = √(0.1800 + 0.3675) = √0.5475 = 0.7399.
2. t = 1.1 / 0.7399 = **1.487**.
3. ν = 0.5475² / (0.1800²/7 + 0.3675²/11) = 0.29976 / 0.016906 = **17.73**.
4. p = 2 × P(T₁₇.₇₃ > 1.487) = **0.155**.

Neither test rejects H₀ at α = 0.05: the ovens may well give the same mean strength. Notice that the two t-tests give different statistics. When n₁ ≠ n₂ and the variances differ, pooling gives the larger sample's variance more weight. Here the larger sample (oven B) also has the larger variance, so pooling made the SE bigger and t smaller. When the **smaller** sample has the larger variance, as in the Example, pooling makes the SE too small and p too small: the pooled test then rejects too often. With n₁ = n₂ the two t statistics are equal, and only the df differ.

## The Example: two alloys

A lab tested 10 coupons of alloy A and 8 of alloy B for tensile strength (MPa).

[[data: st.m9.two-sample.eg]]

[[figure: st.m9.two-sample.eg 1]]

The box plots show alloy B lower and more spread out (s_B = 14.20 against s_A = 7.13 MPa). With different sizes and spreads, Welch's test is the right choice.

[[results: st.m9.two-sample.eg]]

t = 2.37 with ν = 9.80 degrees of freedom, so p = 0.0397 < 0.05: reject H₀. The 95 % confidence interval for μ_A − μ_B runs from 0.75 to 25.35 MPa: alloy A is stronger, by somewhere between about 1 and 25 MPa. The pooled test on the same data would give t = 2.55 with 16 df and p = 0.0216, a stronger result than the data justify, because here the smaller sample (alloy B) has the larger variance.

[[figure: st.m9.two-sample.eg 2]]

The curve is the t distribution with 9.80 df. The shaded tails are the rejection region, beyond ±2.234, and the line is the observed t = 2.37.

## In Python and R

```python
import numpy as np
from scipy import stats
a = np.array([512, 498, 521, 505, 515, 509, 502, 518, 507, 511])
b = np.array([495, 520, 478, 501, 488, 512, 483, 497])
print(stats.ttest_ind(a, b, equal_var=False))   # Welch: t = 2.3712, p = 0.0397
print(stats.ttest_ind(a, b, equal_var=True))    # pooled: t = 2.5454, p = 0.0216
# From summary statistics:
print(stats.ttest_ind_from_stats(24.6, 1.2, 8, 23.5, 2.1, 12, equal_var=False))  # the ovens, Welch
```

```r
a <- c(512, 498, 521, 505, 515, 509, 502, 518, 507, 511)
b <- c(495, 520, 478, 501, 488, 512, 483, 497)
t.test(a, b)                    # Welch is R's default: t = 2.3712, df = 9.80, p = 0.0397
t.test(a, b, var.equal = TRUE)  # pooled: t = 2.5454, df = 16, p = 0.0216
```

Both libraries give the confidence interval with the test. Welch is the default in R and the safe choice in both.

[[code: st.m9.two-sample.eg]]

## Common mistakes

- **Pooling when the variances differ.** Unless you have reason to believe σ₁ = σ₂, use Welch. It costs very little when the variances are equal, and protects you when they are not (item 9.4).
- **The wrong degrees of freedom.** n₁ + n₂ − 2 belongs to the pooled test. Welch's ν comes from its own formula.
- **A one-tailed p for a two-sided question.** For H₁: μ₁ ≠ μ₂, p = 2 × P(T > |t|). Halving it to look "more significant" is wrong.
- **Mixing up the order.** t = (x̄₁ − x̄₂)/SE is negative when sample 1 has the smaller mean. Keep the order of H₁ and the statistic the same.

## What to do next

On the **Exercise** tab, compare the pit depth under two coatings with Welch's test. Find t, ν and the p-value, and press **Check my answers**.
