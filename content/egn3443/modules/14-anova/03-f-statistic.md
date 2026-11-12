---
id: st.m14.f-statistic
title: The F-statistic and ANOVA logic
minutes: 50
objectives: [st.m14.o3]
builds-on: st.m14.assumptions
example: st.m14.f-statistic.eg
exercise: st.m14.f-statistic.ex
reference: [one-way-anova]
---

# The F-statistic and ANOVA logic

## Partitioning the variation

Write yᵢⱼ for observation j in group i, nᵢ for the size of group i, k for the number of groups and N = n₁ + … + nₖ for the number of observations. ȳᵢ is the mean of group i and ȳ (the **grand mean**) the mean of all N values.

Every observation's distance from the grand mean splits in two: its group's distance from the grand mean, and its own distance from its group's mean.

yᵢⱼ − ȳ = (ȳᵢ − ȳ) + (yᵢⱼ − ȳᵢ)

Square these and add them over all N observations, and the cross terms cancel. The **total sum of squares** splits exactly:

SS total = SS between + SS within

| Sum of squares | Formula | Measures |
|---|---|---|
| **SS between** (groups, treatments) | Σ nᵢ(ȳᵢ − ȳ)² | how far the group means are from each other |
| **SS within** (error, residual) | Σ Σ (yᵢⱼ − ȳᵢ)² = Σ (nᵢ − 1)sᵢ² | the scatter inside the groups |
| **SS total** | Σ Σ (yᵢⱼ − ȳ)² = (N − 1)s² | all the variation |

In SS between, each group's squared distance counts **once per observation** in the group (the nᵢ).

## From sums of squares to F

A sum of squares grows with the number of terms, so each is divided by its **degrees of freedom** to give a **mean square** (MS), an estimate of a variance:

- df between = k − 1 (k group means, less one for the grand mean), MS between = SS between / (k − 1)
- df within = N − k (N values, less one for each group mean), MS within = SS within / (N − k)
- df total = N − 1 = (k − 1) + (N − k)

MS within is the **pooled variance** of the groups, the k-group form of Module 9's sₚ². It estimates σ² whether H₀ is true or not. MS between estimates σ² too **when H₀ is true**, because then the group means differ only by chance; when the means really differ, it is larger. So

F = MS between / MS within

is near 1 when H₀ is true and large when it is false. Under H₀, F follows the **F distribution** with k − 1 and N − k degrees of freedom (Module 9 met it in the test of two variances). Only large F counts against H₀, so the p-value is the area **above** the observed F under that curve. Reject H₀ when p < α, or, the same thing, when F is above the critical value F(α; k − 1, N − k).

## The ANOVA table

Every program prints the results in the same layout:

| Source | SS | df | MS | F | p |
|---|---|---|---|---|---|
| Between groups | SS between | k − 1 | SS between/(k − 1) | MS between/MS within | P(F > F observed) |
| Within groups | SS within | N − k | SS within/(N − k) | | |
| Total | SS total | N − 1 | | | |

The SS and df columns add up; the MS column does not.

## A worked case by hand

The course's reading on ANOVA (Module 14 on the course website, reaglin.github.io/EGN3443) gives the crop yield under three fertilizers, four plots each, and leaves the calculation to you:

| Fertilizer | Yields | Mean |
|---|---|---|
| A | 10, 12, 11, 13 | 11.5 |
| B | 8, 9, 7, 10 | 8.5 |
| C | 14, 15, 13, 16 | 14.5 |

k = 3, n = 4 in each, N = 12, and the grand mean is ȳ = 138/12 = 11.5.

1. **SS between** = 4 × [(11.5 − 11.5)² + (8.5 − 11.5)² + (14.5 − 11.5)²] = 4 × (0 + 9 + 9) = **72**.
2. **SS within.** In group A the deviations from 11.5 are −1.5, 0.5, −0.5, 1.5, and their squares add to 2.25 + 0.25 + 0.25 + 2.25 = 5. Groups B and C have the same pattern around their means, so SS within = 5 + 5 + 5 = **15**.
3. **SS total** = 72 + 15 = 87 (check: the squared deviations of all 12 yields from 11.5 add to 87).
4. **Mean squares:** MS between = 72/2 = 36; MS within = 15/9 = 1.667.
5. **F** = 36/1.667 = **21.6**, with 2 and 9 degrees of freedom.
6. **Decision:** F(0.05; 2, 9) = 4.256, and 21.6 is far beyond it. The area above 21.6 under the F curve with 2 and 9 df is p = **0.0004** < 0.05: reject H₀. The fertilizers do not all give the same mean yield.

| Source | SS | df | MS | F | p |
|---|---|---|---|---|---|
| Fertilizer | 72 | 2 | 36.00 | 21.6 | 0.0004 |
| Within | 15 | 9 | 1.667 | | |
| Total | 87 | 11 | | | |

## The Example: concrete slump after three mixing times

[[data: st.m14.f-statistic.eg]]

[[figure: st.m14.f-statistic.eg 1]]

The box plots run across the page, 5 minutes at the top. The 10-minute batches have the largest slumps and the 5-minute batches the smallest, but the whiskers overlap. Is the spread of the means more than the scatter inside the groups would give by chance?

[[results: st.m14.f-statistic.eg]]

The means are 80.8, 87.0 and 84.8 mm around a grand mean of 84.2. SS between = 98.8 on 2 df, so MS between = 49.4; SS within = 87.6 on 12 df, so MS within = 7.3. F = 49.4/7.3 = 6.77, beyond the critical value F(0.05; 2, 12) = 3.885, and p = 0.0108: reject H₀. Mixing time changes the mean slump. Note that SS between and SS within add to SS total = 186.4.

[[figure: st.m14.f-statistic.eg 2]]

Each group mean with its 95 % interval, using the pooled standard deviation √(MS within). Item 14.6 shows how to compare the pairs properly.

## In Python and R

```python
from scipy import stats
print(stats.f_oneway([10, 12, 11, 13], [8, 9, 7, 10], [14, 15, 13, 16]))   # F = 21.6, p = 0.000367
print(stats.f.sf(21.6, 2, 9))      # the same p from F and its df
print(stats.f.ppf(0.95, 2, 9))     # the critical value, 4.256
```

```r
y <- c(10, 12, 11, 13, 8, 9, 7, 10, 14, 15, 13, 16)
g <- factor(rep(c("A", "B", "C"), each = 4))
summary(aov(y ~ g))                # the ANOVA table: F = 21.6, Pr(>F) = 0.000367
pf(21.6, 2, 9, lower.tail = FALSE)
qf(0.95, 2, 9)
```

[[code: st.m14.f-statistic.eg]]

## Common mistakes

- **Leaving out nᵢ in SS between.** Each group mean's squared distance is weighted by the group's size.
- **Dividing SS by SS.** F is the ratio of the **mean squares**, each SS divided by its own df.
- **The wrong df.** Between: k − 1. Within: N − k, not N − 1.
- **A two-tailed p.** Only a large F is evidence against H₀; the p-value is the upper tail only.
- **Reading a large F as "all the means differ".** It says at least one does.

## What to do next

On the **Exercise** tab, build the ANOVA table for the diameters of rod drawn through three dies: SS between, SS within and F.
