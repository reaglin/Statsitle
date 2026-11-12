---
title: Post-hoc tests (Tukey's HSD and others)
---

# Post-hoc tests (Tukey's HSD and others)

After a significant ANOVA F, a post-hoc test finds which means differ while holding the familywise error rate at α.

| Procedure | Compares | Use when |
|---|---|---|
| Tukey's HSD | all k(k − 1)/2 pairs | the usual choice for all pairs |
| Bonferroni | m chosen comparisons, each at α/m | a few comparisons planned in advance |
| Scheffé | all pairs and all contrasts | exploring contrasts; conservative for pairs |
| Dunnett | each group against one control | new treatments against a standard |
| Plain t-tests | each pair at α | never for many pairs: the familywise error grows |

## Tukey's HSD

HSD = q(α; k, N − k) × √(MS within / n)

- q(α; k, N − k): the upper α point of the studentized range distribution for k means and N − k df.
- Unequal groups (Tukey–Kramer): use √(MS within/2 × (1/nᵢ + 1/nⱼ)) for each pair.
- Means i and j differ when |ȳᵢ − ȳⱼ| > HSD.
- Simultaneous interval: (ȳᵢ − ȳⱼ) ± HSD. All the intervals together have confidence 1 − α. A pair differs when its interval excludes 0.
- Compact letter display: groups sharing a letter are not shown to differ.

## Reporting

"F(3, 20) = 26.28, p < 0.001, η² = 0.80. Tukey's HSD (α = 0.05): …", with the group means and the pairs that differ, and what it means for the decision. (APA style writes p < .001 without the leading zero.)

## Python and R

| | Python | R |
|---|---|---|
| Tukey | `stats.tukey_hsd(a, b, c)`; `statsmodels.stats.multicomp.pairwise_tukeyhsd(y, g)` | `TukeyHSD(aov(y ~ g))` |
| q | `stats.studentized_range.ppf(1 - alpha, k, N - k)` | `qtukey(1 - alpha, k, N - k)` |
| Bonferroni | `multipletests(p, method="bonferroni")` | `pairwise.t.test(y, g, p.adjust.method = "bonferroni")` |
| Dunnett | `stats.dunnett(a, b, control=c0)` (scipy 1.11+) | `DescTools::DunnettTest(y, g)` |

## In the Workbench

**One-way ANOVA** gives **Tukey q critical**, **Tukey HSD pairs** (difference, interval, adjusted p, * when the pair differs), **Smallest Tukey p-value** and **Pairs that differ**.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 13.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.4.7 (multiple comparisons): https://www.itl.nist.gov/div898/handbook/prc/section4/prc47.htm
- Tukey, J. W. (1953), *The Problem of Multiple Comparisons*, unpublished manuscript, Princeton University.
