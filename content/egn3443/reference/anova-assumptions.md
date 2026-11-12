---
title: Checking the assumptions of ANOVA
---

# Checking the assumptions of ANOVA

| Assumption | How to check | If it fails |
|---|---|---|
| Independent observations | from the design: separate units, random assignment, no repeated readings of one unit | redesign, or a method for the dependence (repeated measures, blocking) |
| Normal errors | box plots; normal Q–Q plot and Shapiro–Wilk of the **residuals** eᵢⱼ = yᵢⱼ − ȳᵢ | mild: little harm, especially with equal groups of 10 or more; strong skew: transform (log) or Kruskal–Wallis |
| Equal variances | the course's rule: largest group variance more than about 2 to 3 times the smallest → treat as unequal (item 10.3); Levene's test (median-centered); Bartlett's test | Welch's ANOVA; a transformation; equal group sizes make F more robust |

## The tests

| Test | H₀ | Statistic | Note |
|---|---|---|---|
| Shapiro–Wilk (residuals) | the residuals are normal | W, near 1 for normal data | small p: evidence against normality |
| Levene (Brown–Forsythe) | σ₁² = … = σₖ² | one-way ANOVA F on zᵢⱼ = \|yᵢⱼ − median of group i\|, with k − 1 and N − k df | robust to non-normality: the one to decide by |
| Bartlett | σ₁² = … = σₖ² | χ² on k − 1 df | more powerful for normal data, but rejects for non-normality too |
| Kruskal–Wallis | all k groups have the same distribution | H = 12/(N(N + 1)) Σ Rᵢ²/nᵢ − 3(N + 1), χ² on k − 1 df (ties corrected) | the rank-based alternative to one-way ANOVA |
| Welch's ANOVA | μ₁ = … = μₖ, variances not assumed equal | F with k − 1 and an adjusted denominator df | the remedy for unequal variances |

## Python and R

| Check | Python | R |
|---|---|---|
| Shapiro–Wilk | `stats.shapiro(residuals)` | `shapiro.test(residuals(fit))` |
| Levene (median) | `stats.levene(a, b, c)` | `car::leveneTest(y ~ g)`, or `summary(aov(abs(y - ave(y, g, FUN = median)) ~ g))` |
| Bartlett | `stats.bartlett(a, b, c)` | `bartlett.test(y ~ g)` |
| Kruskal–Wallis | `stats.kruskal(a, b, c)` | `kruskal.test(y ~ g)` |
| Welch's ANOVA | `statsmodels.stats.oneway.anova_oneway([a, b, c], use_var="unequal")` | `oneway.test(y ~ g)` |

## In the Workbench

**One-way ANOVA** reports Bartlett, Levene, Shapiro–Wilk of the residuals and Kruskal–Wallis with the ANOVA table, and draws the box plots and the residuals' Q–Q plot. It does not calculate Welch's ANOVA.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 13.2 (residual analysis).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Sections 1.3.5.7 (Bartlett's test) and 1.3.5.10 (Levene test): https://www.itl.nist.gov/div898/handbook/eda/section3/eda357.htm
- Kruskal, W. H. and Wallis, W. A. (1952), Use of ranks in one-criterion variance analysis, *Journal of the American Statistical Association* 47, 583–621.
