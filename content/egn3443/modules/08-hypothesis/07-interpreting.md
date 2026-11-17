---
id: st.m8.interpreting
title: Interpreting and communicating results
minutes: 45
objectives: [st.m8.o4, st.m8.o5]
builds-on: st.m8.errors
example: st.m8.interpreting.eg
exercise: st.m8.interpreting.ex
reference: [hypothesis-testing-steps, chi-square-tests]
---

# Interpreting and communicating results

## Saying what the test showed

A test ends in one of two decisions. Say each in words about the **population** and the **engineering question**, with the numbers that support it.

| Decision | Say | Do not say |
|---|---|---|
| p < α: reject H₀ | "The data give evidence that the mean coating thickness differs from 50 µm (t = 2.42, df = 11, p = 0.034)." | "The test proves the thickness is wrong." |
| p ≥ α: do not reject H₀ | "The data do not give evidence that the failure rate is above 10 % (z = 1.26, p = 0.103)." | "The failure rate is 10 %." / "We accept H₀." |

A complete statement has five parts:

1. the **test** and its hypotheses, in words;
2. the **statistic**, its degrees of freedom if it has them, and the **p-value**;
3. the **decision** at the α chosen in advance;
4. the **size of the effect** in the units that matter (x̄ − μ₀, p̂ − p₀, or Cramér's V for a table), with an interval if you have one (Module 7);
5. what it means for the **decision** the engineer has to make, including the error that may have been made.

## Statistical and practical significance

**Statistically significant** means the difference is too large to be chance alone. **Practically significant** means it is large enough to matter: to the tolerance, the cost, the safety margin. They are different questions.

- With a **large n**, the standard error is tiny, and a difference too small to matter becomes statistically significant.
- With a **small n**, a difference that matters can fail to be significant (low power, item 8.6).

So report the effect's size beside the p-value, and judge it against an engineering yardstick: a tolerance, a cost, a specification.

For a table of counts the size of the effect is **Cramér's V**:

V = √( χ² / (n × (min(r, c) − 1)) )

It runs from 0 (no association) to 1 (each row fixes the column). About 0.1 is weak, 0.3 moderate, 0.5 strong.

## A worked case: one report

The Example of item 8.3, written up:

> A one-sample t-test compared the mean coating thickness of 12 panels with the 50 µm target (H₀: μ = 50, H₁: μ ≠ 50, α = 0.05). The mean was 52.1 µm (s = 3.0 µm), so t = 2.42 with 11 degrees of freedom, p = 0.034. We reject H₀: the line is laying down about 2 µm more than its target. The specification is 50 ± 5 µm, so the offset uses 40 % of the tolerance on one side; the line should be adjusted at the next stop. A Type I error is possible (at most 5 %), and its cost here is one unnecessary adjustment.

[[see-example: 1]]

## The Example: rejects by supplier

Two suppliers each sent 2000 brackets: A had 60 rejects (3.0 %), B had 90 (4.5 %).

| | Accepted | Rejected | Total |
|---|---|---|---|
| Supplier A | 1940 | 60 | 2000 |
| Supplier B | 1910 | 90 | 2000 |
| **Total** | 3850 | 150 | **4000** |

[[results: st.m8.interpreting.eg]]

χ² = 6.23 with 1 degree of freedom and p = 0.0125: the reject rate depends on the supplier. But Cramér's V is **0.039**, very weak: the supplier explains almost none of the variation in which brackets are rejected. The practical question is different: is 1.5 more rejects per hundred brackets worth acting on? At 4000 brackets a year it is 60 extra rejects; whether that matters is a cost question, and the χ² test cannot answer it.

The observed and expected counts are almost the same height in every cell; that is what a weak association looks like, however small the p-value:

[[figure: st.m8.interpreting.eg 2]]

A good report: *"Supplier B's reject rate (4.5 %) was higher than supplier A's (3.0 %); the difference is statistically significant (χ² = 6.23, df = 1, p = 0.013), but the association is weak (Cramér's V = 0.04). At our volume it means about 30 extra rejects per 2000 brackets."*

## In Python and R

```python
import numpy as np
from scipy import stats
table = np.array([[1940, 60], [1910, 90]])
chi2, p, df, expected = stats.chi2_contingency(table, correction=False)
v = stats.contingency.association(table, method="cramer")
print(chi2, df, p, v)                             # 6.23 1 0.0125 0.039
```

```r
tab <- matrix(c(1940, 1910, 60, 90), nrow = 2)
test <- chisq.test(tab, correct = FALSE)
test                                              # X-squared = 6.2338, df = 1, p-value = 0.01253
sqrt(unname(test$statistic) / (sum(tab) * (min(dim(tab)) - 1)))   # Cramér's V = 0.039
```

[[code: st.m8.interpreting.eg]]

## Communicating error risk

- After **rejecting**, the risk is a Type I error; its probability, if H₀ was true, is α.
- After **not rejecting**, the risk is a Type II error; give the power against the smallest difference that matters (item 8.6). "Not significant, with a power of 0.30" says the test could hardly have found anything.
- Do not hide behind α: give the p-value, so a reader with a different α can decide for themselves.

## Common mistakes

- "p < 0.05, so the effect is large." Significance is not size.
- "p > 0.05, so there is no difference." Absence of evidence is not evidence of absence.
- Reporting "significant" with no numbers, no units, and no direction.
- Quoting p = 0.000. Write p < 0.001.

## What to do next

On the **Exercise** tab, three plants cast the same housing, and 1500 castings were judged within spec, rework or scrap. Test whether the outcome depends on the plant, and find Cramér's V. Use **Statistical and practical significance** above, and Cramér's V, to judge whether the relation matters.

[[example: Coating thickness: significant is not the same as important]]

This is the worked case of the report above: 12 panels, x̄ = 52.1 µm, s = 3.0 µm, against the 50 µm target. It starts there: t = **2.42** with **11** degrees of freedom, p = **0.0337**, reject H₀; the 95 % interval for μ is **50.19 to 54.01 µm**.

1. **Change x̄ to 50.5 µm**, only 0.5 µm above target. t = **0.58** and p = **0.5753**: not significant, and the interval, **48.59 to 52.41 µm**, includes 50.
2. **Keep x̄ = 50.5 and change n to 400.** t = **3.33** and p = **0.0009**: highly significant. Yet the interval, **50.21 to 50.79 µm**, says the line is less than 1 µm off, a small part of the 50 ± 5 µm tolerance. Statistically significant, practically unimportant.
3. **Change n to 1000.** p is below 0.001 (the Workbench shows **0.0000**: write p < 0.001). The offset is still about 0.5 µm. Report the size of the effect beside the p-value.

[[try: st.m8.thickness.try SampleMean=49..54 SampleSize=4..1000 | Statistic DegreesOfFreedom PValue Decision CiLower CiUpper figure=1]]
