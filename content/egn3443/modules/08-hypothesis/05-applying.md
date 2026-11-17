---
id: st.m8.applying
title: Applying common hypothesis tests
minutes: 60
objectives: [st.m8.o2, st.m8.o3]
builds-on: st.m8.p-values
example: st.m8.applying.eg
exercise: st.m8.applying.ex
reference: [choosing-a-test, chi-square-tests]
---

# Applying common hypothesis tests

## Choosing the test

Every test follows the same steps: hypotheses, α, statistic, p-value, decision. What changes is the **statistic**, and that is chosen from the data:

1. **What kind of data?** Measurements (a length, a time, a strength) or counts in categories (pass/fail, defect type, weekday)?
2. **How many groups or samples?** One, two, or more than two?
3. **For two samples: independent or paired?** Paired means each value in one sample belongs with one value in the other: the same part measured before and after, the same specimen tested by two methods.

| Data | Groups | Test | Taught in |
|---|---|---|---|
| Measurements | one sample against a target | **one-sample t-test** (z if σ is known) | items 8.3, 8.4 |
| Proportion (a share with an attribute) | one sample against a rate | **one-sample z-test on a proportion** | item 8.4 |
| Measurements | two independent samples | **two-sample t-test** | Module 9 |
| Measurements | the same units measured twice | **paired t-test** | Module 9 |
| Measurements | three or more independent groups | **one-way ANOVA** | Module 14 |
| Counts in categories of one variable | one sample against stated proportions | **chi-square goodness of fit** | this item |
| Counts classified by two variables | a contingency table | **chi-square test of independence** | this item |

In the Workbench, two-sample and paired tests are **Two-sample test** and three or more groups is **One-way ANOVA**; Modules 9 and 14 teach them. The rest of this item is the chi-square tests.

## Chi-square goodness of fit

Some data are **counts**: how many items fell in each of k categories. The question is whether the counts follow proportions stated in advance (equal shares, a supplier's claimed mix).

- H₀: the category proportions are p₁, p₂, …, pₖ. H₁: at least one differs.
- **Expected count** in each category if H₀ is true: **Eᵢ = n × pᵢ**, where n is the total count.
- **Statistic:** χ² = Σ (Oᵢ − Eᵢ)² / Eᵢ, adding over the categories, where Oᵢ is the observed count.
- **Reference distribution:** chi-square (χ²) with **df = k − 1** degrees of freedom (one less again for each parameter estimated from the same data). The counts must add to n, so only k − 1 of them are free.
- **p-value:** the area to the **right** of χ². Any departure from H₀, in any category, makes χ² larger, so the test is always upper-tailed.
- **Check:** every expected count at least 5. If not, combine neighbouring categories.

**A worked case by hand.** A die is rolled 60 times: the faces 1 to 6 come up 8, 12, 9, 11, 6 and 14 times. Is it fair? H₀: each face has p = 1/6, so each Eᵢ = 60/6 = 10.

χ² = (8 − 10)²/10 + (12 − 10)²/10 + (9 − 10)²/10 + (11 − 10)²/10 + (6 − 10)²/10 + (14 − 10)²/10 = (4 + 4 + 1 + 1 + 16 + 16)/10 = **4.2**

df = 6 − 1 = 5. The critical value χ²(0.05, 5) is 11.07, and P(χ² ≥ 4.2) with 5 df = **0.521**. Do not reject: the counts are well within what a fair die gives.

[[see-example: 1]]

## The Example: unplanned shutdowns by weekday

A plant had 110 unplanned shutdowns in a year. If the day does not matter, each weekday expects 110/5 = 22.

[[results: st.m8.applying.eg]]

χ² = 6.82 with df = 4, below the critical value 9.49; p = 0.146. Do not reject H₀: the counts do not show that some weekdays have more shutdowns. Monday's 30 is the largest contribution, 2.91, but five categories will always differ a little.

[[figure: st.m8.applying.eg 1]]

The second figure sets the observed counts beside the expected:

[[figure: st.m8.applying.eg 2]]

## Chi-square test of independence

Here each item is classified **two ways**, and the counts form a **contingency table**: r rows (categories of one variable) by c columns (categories of the other). The question is whether the two variables are related.

- H₀: the row and column variables are independent (every row has the same column proportions). H₁: they are not.
- **Expected count** in row i, column j: **Eᵢⱼ = (row i total) × (column j total) / n**.
- **Statistic:** χ² = Σ (Oᵢⱼ − Eᵢⱼ)² / Eᵢⱼ over all r × c cells.
- **df = (r − 1)(c − 1).** Both the row totals and the column totals are fixed.
- **p-value:** the area to the right of χ², as before.

**A worked case.** 180 defective castings were classified by shift and by type of defect:

| | Porosity | Crack | Misrun | Row total |
|---|---|---|---|---|
| Day | 45 | 30 | 15 | 90 |
| Night | 35 | 20 | 35 | 90 |
| **Column total** | 80 | 50 | 50 | **180** |

The row totals are Day 90 and Night 90; the column totals are Porosity 80, Crack 50, Misrun 50. So E for Day and Porosity is 90 × 80/180 = **40**, and in the same way every Day and every Night cell expects 40, 25 and 25. The contributions are:

| | Porosity | Crack | Misrun |
|---|---|---|---|
| Day | (45 − 40)²/40 = 0.625 | (30 − 25)²/25 = 1.0 | (15 − 25)²/25 = 4.0 |
| Night | (35 − 40)²/40 = 0.625 | (20 − 25)²/25 = 1.0 | (35 − 25)²/25 = 4.0 |

χ² = **11.25**, df = (2 − 1)(3 − 1) = **2**, p = **0.0036**. Reject H₀: the type of defect depends on the shift. The large contributions show where: the night shift makes many more misruns (35 against 25 expected).

[[see-example: 2]]

[[results: st.m8.applying.table]]

[[figure: st.m8.applying.table 2]]

## In Python and R

```python
import numpy as np
from scipy import stats
observed = np.array([30, 18, 16, 19, 27])
print(stats.chisquare(observed))                  # equal proportions: chi2 = 6.82, p = 0.146

table = np.array([[45, 30, 15], [35, 20, 35]])
chi2, p, df, expected = stats.chi2_contingency(table, correction=False)
print(chi2, df, p)                                # 11.25 2 0.0036
print(expected)
```

```r
observed <- c(30, 18, 16, 19, 27)
chisq.test(observed)                              # X-squared = 6.8182, df = 4, p-value = 0.1458

tab <- matrix(c(45, 35, 30, 20, 15, 35), nrow = 2)
chisq.test(tab, correct = FALSE)                  # X-squared = 11.25, df = 2, p-value = 0.003607
```

For stated proportions, give them: `stats.chisquare(observed, f_exp=n * p)` in Python, `chisq.test(observed, p = p)` in R. `correct = FALSE` matters only for a 2 × 2 table, where R otherwise applies Yates' correction.

[[code: st.m8.applying.eg]]

## Try it in the Workbench

The Example tab does this with the die's counts. For the shutdowns, press **Open in the Workbench** on the Example tab and change Monday's count (the first in **Observed counts**) from 30 to 34; the total becomes 114. χ² rises to 9.95 with p = 0.041: now the days differ significantly at α = 0.05. Then try **Test** *Independence or homogeneity* to type a table of your own.

## Common mistakes

- Using percentages instead of counts. χ² needs the counts; percentages make it meaningless.
- Expected counts from equal shares when H₀ states other proportions, or Eᵢ = n/k in a contingency table. Use n × pᵢ, and (row total × column total)/n.
- Dividing by O instead of E.
- df = k (or r × c − 1). Goodness of fit has k − 1; independence (r − 1)(c − 1).
- Using a chi-square test on measurements. It is for counts.

## What to do next

On the **Exercise** tab, a supplier claims a mix of 50 % grade A, 35 % grade B and 15 % grade C capacitors, and inspection grades 200 of them. Test the claim: find χ², its degrees of freedom and the p-value.

[[example: Is the die fair?]]

This is the Learn page's worked case: a die rolled 60 times, with faces 1 to 6 coming up 8, 12, 9, 11, 6 and 14 times. It starts there: χ² = **4.2** with **5** degrees of freedom, below the critical value **11.07**, and p = **0.521**: do not reject. In the second figure, the observed bars stay close to the expected 10.

1. **Change the 14 (face 6) to 22.** The total becomes 68, χ² = **14.06** and p = **0.0152**: reject. One face far above its share is enough.
2. **Change it to 20 instead.** χ² = **10.91**, just under 11.07, and p = **0.0532**: do not reject, by a hair.
3. **Back to the start, then make every count three times as large** (24, 36, 27, 33, 18, 42: 180 rolls). The shares are the same, but χ² triples to **12.6** and p = **0.0274**: reject. The same pattern from more rolls is stronger evidence.

[[try: st.m8.die.try Observed | Total Statistic Critical P Decision figure=2]]

[[example: Defect type by shift]]

This is the Learn page's second worked case: 180 defective castings by shift and type of defect. It starts there: χ² = **11.25** with **2** degrees of freedom, p = **0.0036**: reject, the type of defect depends on the shift. Cramér's V is **0.25**.

1. **Change the night shift's misruns from 35 to 25.** The total is 170, χ² falls to **5.18**, under the critical value **5.99**, and p = **0.0750**: do not reject. The night's extra misruns were most of the evidence.
2. **Change the day shift's misruns from 15 to 25 too.** Both shifts now have 25 misruns: χ² = **1.04** and p = **0.594**. The rows look alike, and V falls to **0.08**.
3. **Back to the start** puts the table back.

[[try: st.m8.castings.try @Defects/Misrun @Defects/Porosity @Defects/Crack | Statistic Critical P Decision CramersV figure=2]]
