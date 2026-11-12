---
id: st.m10.designs
title: Independent vs paired samples
minutes: 40
objectives: [st.m10.o1]
builds-on: st.m10.foundations
example: st.m10.designs.eg
quiz: quizzes/m10-designs.json
reference: [independent-and-paired-designs, paired-t-test]
---

# Independent vs paired samples

**Review and new.** Item 9.2 (*Paired sample comparisons*) taught the paired t-test on the same units measured twice. This page is about recognizing the design in the first place. Module 9's cases were mostly the same unit measured twice, with natural pairs in passing (item 9.2's two tires on one car); this page names all three kinds of pairing, adds matched pairs, and shows what goes wrong when the design is misread. Decide the design **before** choosing a formula: the data alone cannot tell you.

## The one question

**Does each observation in one group have one particular partner in the other group?**

- **No: independent samples.** Choosing a unit for one group says nothing about which units are in the other. The groups can have different sizes. Example: 40 resistors from one reel and 50 from another.
- **Yes: paired samples.** Each value in group 1 belongs with one value in group 2, and the pair shares something that affects the outcome. Both groups have the same number of values, one per pair, and the rows must line up.

## Three kinds of pairing

| Kind | What makes the pair | Engineering example |
|---|---|---|
| **Repeated measures** | the same unit measured twice: before and after, or under two conditions | the deflection of 10 girders under a test load, before and after strengthening (item 9.2's pumps and specimens) |
| **Matched pairs** | two different units, matched on what else affects the outcome; a coin decides which one gets which treatment | houses matched on floor area, age and orientation; one of each pair is retrofitted (the Example) |
| **Natural pairs** | two units that come as a pair: two halves, two sides, two neighbors | two identical windows side by side in each of 8 offices, one with each glazing film (item 9.2's two tires on one car) |

In every kind, the test works on the **differences** within the pairs, d = (group 1 value) − (group 2 value), and asks whether their mean μ_d is 0. With n pairs, that is a one-sample t-test on the n differences, with n − 1 degrees of freedom.

A pair with a missing value cannot be used: drop the whole pair. Twenty "before" readings and eighteen "after" readings give 18 pairs, not 20.

## Why pairing helps

Units differ from each other, often much more than the treatment changes them. In the Example, the ten houses not retrofitted used between 88 and 140 kWh/m²: a spread of 52. The ten differences within the pairs run only from −13 to −2. Subtracting within a pair removes what the two houses share (their size, age and orientation), and leaves the effect of the insulation and a little noise.

## The Example: attic insulation in matched pairs

[[data: st.m10.designs.eg]]

[[figure: st.m10.designs.eg 1]]

The box plot is of the ten differences, retrofitted − not retrofitted. Every one is below zero: in every pair the retrofitted house used less energy.

[[results: st.m10.designs.eg]]

The mean difference is d̄ = −7.1 kWh/m² with s_d = 3.41, so t = −7.1 / (3.41/√10) = −6.58 with 9 degrees of freedom, and p = 0.0001: reject H₀. The 95 % confidence interval for μ_d is −9.54 to −4.66 kWh/m²: the insulation saves between about 4.7 and 9.5 kWh per m² over a winter. Shapiro–Wilk on the differences gives p = 0.38, no evidence against the normality the paired t assumes.

[[figure: st.m10.designs.eg 2]]

## Misreading the design

**Paired data treated as independent.** Put the same twenty numbers into Welch's test as two independent samples and you get t = −1.05 with ν = 17.40 and p = 0.308: no evidence at all. The house-to-house variation stays in the standard error (s = 13.6 and 16.5 kWh/m² for the two columns, against s_d = 3.41 for the differences) and buries a saving of 7 kWh/m². The independent test is not wrong in its arithmetic; it answers a less precise question than the design allows, so it loses power and misses real effects.

**Independent data treated as paired.** Two separate groups of units have no natural partners. Sorting both columns and subtracting smallest from smallest invents pairs: the differences mean nothing, and the paired test's p-value is wrong. Different sizes (40 and 50) are a sure sign that the samples are independent.

**Pairing that does not pair.** Pairing helps only when the two members of a pair are alike in what drives the outcome. Matching houses on their color would not reduce the variation of heating energy, and would cost degrees of freedom (n − 1 instead of n₁ + n₂ − 2) for nothing.

## In Python and R

```python
import numpy as np
from scipy import stats
retro = np.array([104, 90, 118, 99, 115, 86, 127, 96, 108, 120])
other = np.array([112, 95, 130, 104, 121, 88, 140, 101, 117, 126])
print(stats.ttest_rel(retro, other))                   # paired: t = -6.5765, p = 0.0001
print(stats.ttest_ind(retro, other, equal_var=False))  # the wrong test: t = -1.0501, p = 0.3081
```

```r
retro <- c(104, 90, 118, 99, 115, 86, 127, 96, 108, 120)
other <- c(112, 95, 130, 104, 121, 88, 140, 101, 117, 126)
t.test(retro, other, paired = TRUE)   # t = -6.5765, df = 9, p-value = 0.000102
t.test(retro, other)                  # the wrong test: Welch t = -1.0501, df = 17.397
```

Rows must line up: row 3 of each column is the same pair of houses.

[[code: st.m10.designs.eg]]

## In the Workbench

Press **Open in the Workbench** on the Example tab, or press Ctrl+B and choose **Two-sample test** in the list on the left. Set **Test** to *Paired samples (paired t)*; with **Data from** set to *Columns of a data table*, **Column (sample 1)** is the first member of each pair. To see what misreading the design does, change **Test** to *Two means, unequal variances (Welch t)* and watch p jump from 0.0001 to 0.31. Ctrl+L brings you back to the Course. Ctrl+L brings you back to the Course.

## What to do next

On the **Exercise** tab, eight short studies ask you to name the design. They are practice questions: after a wrong try the explanation shows. You need 80 % to complete the item.
