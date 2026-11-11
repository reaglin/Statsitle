---
id: st.m9.paired
title: Paired sample comparisons
minutes: 45
objectives: [st.m9.o1, st.m9.o2]
builds-on: st.m9.two-sample
example: st.m9.paired.eg
exercise: st.m9.paired.ex
reference: [paired-t-test, two-sample-t-tests]
---

# Paired sample comparisons

## When the data come in pairs

Sometimes the two measurements are taken on the **same** units: each pump's power before and after a retrofit, each specimen measured by two gauges, the left and right tire of the same car. The two numbers in a pair share everything about that unit (its size, its age, its driver), so they are **not independent**. This is a **paired** design.

The paired t-test works on the **differences**. For each of the n pairs, d = x₁ − x₂ (for example before − after). The question about two means becomes a question about one mean, μ_d, and the test is the one-sample t-test of Module 8 applied to the d's:

H₀: μ_d = 0 against H₁: μ_d ≠ 0 (or < 0, or > 0)

t = (d̄ − Δ₀) / (s_d / √n), with n − 1 degrees of freedom

where d̄ is the mean of the differences, s_d their standard deviation (with n − 1), n the number of **pairs**, and Δ₀ the difference H₀ claims (0 here). The confidence interval for μ_d is d̄ ± t(α/2, n − 1) × s_d/√n.

## Why pairing helps

Units differ from each other, often much more than the treatment changes them. Pumps of different sizes draw 15 kW or 25 kW; the retrofit saves well under 1 kW on average. A test on two independent samples sees all that pump-to-pump variation in s₁ and s₂, and the small effect is lost in it. Subtracting within each pair removes the unit-to-unit variation: only the change is left.

## A worked case by hand

Five steel specimens were measured for hardness (HRC) before and after a heat treatment.

| Specimen | 1 | 2 | 3 | 4 | 5 |
|---|---|---|---|---|---|
| Before | 42 | 45 | 39 | 47 | 44 |
| After | 45 | 46 | 43 | 49 | 45 |
| d = before − after | −3 | −1 | −4 | −2 | −1 |

1. **Mean difference:** d̄ = (−3 − 1 − 4 − 2 − 1) / 5 = −11 / 5 = **−2.2** HRC.
2. **Standard deviation of the differences:** the deviations from −2.2 are −0.8, 1.2, −1.8, 0.2, 1.2; their squares add to 0.64 + 1.44 + 3.24 + 0.04 + 1.44 = 6.80; s_d = √(6.80 / 4) = √1.70 = **1.304**.
3. **Standard error:** s_d / √n = 1.304 / √5 = 0.5831.
4. **Statistic:** t = −2.2 / 0.5831 = **−3.773**, with n − 1 = **4** degrees of freedom.
5. **p-value:** p = 2 × P(T > 3.773) with T on 4 df = **0.0196** < 0.05. Reject H₀: the treatment changed the hardness. The 95 % interval for μ_d is −2.2 ± 2.776 × 0.5831, from −3.82 to −0.58 HRC (the treatment raised the hardness).

Had the same ten numbers been treated as two independent samples, Welch's test would give t = −1.31 and p = 0.230: no evidence at all. The specimens differ from each other by up to 8 HRC, and that variation hides a change of about 2.

## The Example: two tire compounds

Eight cars each ran one tire of compound A and one of compound B. The tread wear (mm) of the two tires on a car is a pair.

[[data: st.m9.paired.eg]]

[[figure: st.m9.paired.eg 1]]

The box plot is of the eight differences A − B. All of them are below zero: compound A wore less on every car.

[[results: st.m9.paired.eg]]

d̄ = −0.25 mm, t = −5.92 with 7 df, and p = 0.0006: reject H₀. Compound A wears 0.15 to 0.35 mm less (the 95 % interval for μ_d is −0.350 to −0.150). The wear varies from 3.6 to 5.5 mm between cars, but within a car the difference is steady. Treated as two independent samples, the same data give Welch t = −0.79 and p = 0.440, and the real difference would be missed.

[[figure: st.m9.paired.eg 2]]

## In Python and R

```python
import numpy as np
from scipy import stats
a = np.array([4.1, 3.6, 5.2, 4.8, 3.9, 4.4, 5.0, 3.7])
b = np.array([4.4, 3.8, 5.5, 4.9, 4.3, 4.6, 5.4, 3.8])
print(stats.ttest_rel(a, b))      # t = -5.9161, p = 0.0006
d = a - b
print(stats.ttest_1samp(d, 0))    # the same test, on the differences
```

```r
a <- c(4.1, 3.6, 5.2, 4.8, 3.9, 4.4, 5.0, 3.7)
b <- c(4.4, 3.8, 5.5, 4.9, 4.3, 4.6, 5.4, 3.8)
t.test(a, b, paired = TRUE)       # t = -5.9161, df = 7, p-value = 0.0005898
t.test(a - b, mu = 0)             # the same test, on the differences
```

The order of the columns matters: `ttest_rel(a, b)` uses d = a − b. Rows must line up: row 3 of each column is the same car.

[[code: st.m9.paired.eg]]

## In the Workbench

Choose **Two-sample test**, set **Test** to *Paired samples (paired t)* and **Data from** to the two columns; the "before" column is sample 1. With summary statistics, type d̄, s_d and the number of pairs in the sample 1 boxes.

## Common mistakes

- **Treating paired data as independent.** If each value in one column belongs with one value in the other (the same unit, the same day, the same operator), use the paired test. The independent tests throw the pairing away and usually miss the effect.
- **Pairing data that are not paired.** Two separate groups of different units have no natural pairs; sorting them and subtracting is meaningless.
- **The wrong n.** n is the number of pairs, and the degrees of freedom are n − 1, not 2n − 2.
- **The order of subtraction.** Decide d = before − after (or after − before) first, and write H₁ to match.

## What to do next

On the **Exercise** tab, ten pumps were measured before and after a retrofit. Find the paired t, its degrees of freedom and the p-value, and press **Check my answers**.
