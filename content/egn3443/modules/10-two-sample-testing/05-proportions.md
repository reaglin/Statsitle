---
id: st.m10.proportions
title: Comparing two proportions
minutes: 50
objectives: [st.m10.o4, st.m10.o2]
builds-on: st.m10.two-means
example: st.m10.proportions.eg
exercise: st.m10.proportions.ex
reference: [two-proportion-z-test, confidence-interval-proportion]
---

# Comparing two proportions

**New in this module.** Item 9.3 named the two-proportion z-test and left it for here. Many engineering outcomes are pass or fail: a weld passes X-ray or not, a board has a defect or not, a pump fails within warranty or not. Comparing two processes then means comparing two **proportions**. The test is built like the others: (observed difference − 0)/SE, read on the standard normal curve. It extends the one-sample proportion test of items 8.3 and 8.4 and the interval for one proportion of item 7.4 (*Confidence intervals for proportions*).

## Notation

Group 1 has x₁ items with the attribute (a "success": whatever outcome is counted, a defect is a success here) out of n₁, so p̂₁ = x₁/n₁. Group 2 likewise: p̂₂ = x₂/n₂. The populations' proportions are p₁ and p₂.

H₀: p₁ = p₂ (that is, p₁ − p₂ = 0), against H₁: p₁ ≠ p₂, p₁ > p₂ or p₁ < p₂.

## The pooled proportion

If H₀ is true, the two groups share one proportion, and the best estimate of it uses all the data together:

p̂ = (x₁ + x₂)/(n₁ + n₂)

This is the **pooled proportion**: the total successes over the total items. It is a weighted average of p̂₁ and p̂₂, closer to the larger sample's proportion.

## The test statistic

z = (p̂₁ − p̂₂) / √( p̂(1 − p̂)(1/n₁ + 1/n₂) )

The standard error uses the pooled p̂ because the test assumes H₀ while it measures how surprising the data are, as the one-sample test used p₀ (items 8.3 and 8.4). The p-value comes from the standard normal Z: two-sided 2 × P(Z > |z|); for H₁: p₁ > p₂, P(Z > z); for H₁: p₁ < p₂, P(Z < z).

**Condition** (item 10.3): n₁p̂, n₁(1 − p̂), n₂p̂ and n₂(1 − p̂) all at least 10, the groups independent, and different items in the two groups.

## The confidence interval for p₁ − p₂

The interval does **not** assume H₀, so it uses each sample's own proportion:

(p̂₁ − p̂₂) ± z(α/2) × √( p̂₁(1 − p̂₁)/n₁ + p̂₂(1 − p̂₂)/n₂ )

with z(α/2) = 1.960 for 95 %. For this interval, each sample should have at least 10 successes and 10 failures. Because the test and the interval use different standard errors, in a borderline case the test and "0 inside the interval" can disagree slightly.

## A worked case by hand

A foundry tried two mold coatings. With coating 1, 27 of 300 castings had surface defects; with coating 2, 14 of 280. Do the defect rates differ, at α = 0.05?

1. **Proportions:** p̂₁ = 27/300 = 0.0900 and p̂₂ = 14/280 = 0.0500. The difference is 0.0400, four percentage points.
2. **Pooled proportion:** p̂ = (27 + 14)/(300 + 280) = 41/580 = **0.0707**.
3. **Condition:** n₁p̂ = 300 × 0.0707 = 21.2 and n₂p̂ = 280 × 0.0707 = 19.8; the failures are far larger. All at least 10.
4. **Standard error:** √(0.0707 × 0.9293 × (1/300 + 1/280)) = √(0.06570 × 0.006905) = √0.0004536 = 0.02130.
5. **Statistic:** z = 0.0400/0.02130 = **1.878**.
6. **p-value:** 2 × P(Z > 1.878) = **0.060**. Not below 0.05: do not reject H₀.
7. **Interval:** the unpooled SE is √(0.09 × 0.91/300 + 0.05 × 0.95/280) = √0.0004426 = 0.02104, so 0.0400 ± 1.96 × 0.02104 = 0.0400 ± 0.0412, from **−0.0012 to 0.0812**.

Coating 2 may cut the defect rate by as much as 8 points, or not at all: the data cannot yet tell. More castings would.

## The Example: a new solder stencil

[[results: st.m10.proportions.eg]]

p̂₁ = 42/600 = 0.0700 with the old stencil and p̂₂ = 19/550 = 0.0345 with the new one. The pooled proportion is 61/1150 = 0.0530, and every expected count is at least 29, so the condition holds. z = 2.68, and since H₁ is one-sided (p₁ > p₂), p = P(Z > 2.68) = **0.0037**: reject H₀. The new stencil lowers the defect rate. The 95 % interval for p₁ − p₂ is 0.0100 to 0.0609: it cuts defects by between about 1 and 6 percentage points, from a rate of 7 %.

[[figure: st.m10.proportions.eg 1]]

The curve is the standard normal Z. The shaded upper tail is the rejection region, z above 1.645 (one-sided, α = 0.05), and the orange line is the observed z = 2.68.

**The same test as a 2 × 2 table.** Item 8.5 (*Applying common hypothesis tests*) tested independence in a table of counts with chi-square. Put the stencils in the rows and defect / no defect in the columns: χ² = 7.18 with 1 degree of freedom, and that is z² = 2.68². For a 2 × 2 table without Yates' correction, the chi-square test of independence and the two-sided two-proportion z-test are the same test.

## In Python and R

```python
import numpy as np
from scipy import stats
x1, n1, x2, n2 = 42, 600, 19, 550
p1, p2, pooled = x1 / n1, x2 / n2, (x1 + x2) / (n1 + n2)
z = (p1 - p2) / np.sqrt(pooled * (1 - pooled) * (1 / n1 + 1 / n2))
print(z, stats.norm.sf(z))                     # z = 2.6798, one-sided p = 0.0037
se = np.sqrt(p1 * (1 - p1) / n1 + p2 * (1 - p2) / n2)
print(p1 - p2 + np.array([-1, 1]) * stats.norm.ppf(0.975) * se)   # 0.0100 to 0.0609
# statsmodels: proportions_ztest([42, 19], [600, 550], alternative='larger')
```

```r
prop.test(c(42, 19), c(600, 550), alternative = "greater", correct = FALSE)
# X-squared = 7.1812 (= z^2), p-value = 0.003684
```

R's `prop.test` reports χ² = z², not z, and applies a continuity correction unless `correct = FALSE`. With `correct = FALSE` and a two-sided alternative, its interval is the one above; with a one-sided alternative it gives a one-sided bound instead. The Code tab writes the formulas out.

[[code: st.m10.proportions.eg]]

## In the Workbench

Choose **Two-sample test**, set **Test** to *Two proportions (pooled z)* and **Data from** to *Summary statistics entered here*, and type **Successes in sample 1 (x₁)**, **Sample 1 size (n₁)**, **Successes in sample 2 (x₂)** and **Sample 2 size (n₂)**. (With **Data from** set to columns, each column holds 1 for a success and 0 for a failure, one row per item.)

## Common mistakes

- **The unpooled SE in the test.** The test assumes p₁ = p₂, so its SE uses the pooled p̂. The interval uses p̂₁ and p̂₂.
- **Adding percentages instead of counts.** p̂ = (x₁ + x₂)/(n₁ + n₂), not (p̂₁ + p̂₂)/2: with n₁ ≠ n₂ these differ.
- **Swapping the groups halfway.** Fix which group is 1, and write H₁ and the interval as p₁ − p₂.
- **Too few successes.** With fewer than about 10 expected successes in a group, use Fisher's exact test (item 10.3).
- **The same items in both groups.** Two inspectors judging the same welds give paired proportions: McNemar's test.

## What to do next

On the **Exercise** tab, two pump seal designs are compared on their warranty failures. Find z, the p-value and the confidence interval for p₁ − p₂, and press **Check my answers**.
