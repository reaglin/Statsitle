---
title: Rank tests (Mann–Whitney and Wilcoxon)
---

# Rank tests (Mann–Whitney and Wilcoxon)

Non-parametric alternatives to the t-tests: they use the ranks of the data, so they do not assume normality and an extreme value counts only as the largest rank. Use them for small samples that are clearly skewed or have outliers, or for ordinal data (ratings). They need the raw data.

## Mann–Whitney U (independent samples)

H₀: the two populations have the same distribution. H₁: one tends to give larger values.

| Step | |
|---|---|
| Rank | all n₁ + n₂ values together, 1 = smallest; ties get the average rank |
| Rank sum | R₁ = sum of the ranks of sample 1 |
| U | U₁ = R₁ − n₁(n₁ + 1)/2; U₂ = n₁n₂ − U₁ |
| Under H₀ | mean n₁n₂/2; SD √(n₁n₂(n₁ + n₂ + 1)/12) without ties |
| Normal approximation | z = (D − 0.5) / SD, with D the distance of U₁ from n₁n₂/2 and 0.5 the continuity correction |

## Wilcoxon signed-rank (paired samples)

H₀: the differences are symmetric about 0.

| Step | |
|---|---|
| Differences | d = x₁ − x₂; drop zeros; n = how many are left |
| Rank | the sizes of the d's (without their signs), 1 = smallest |
| Sums | W⁺ = ranks of the positive d's, W⁻ = of the negative ones; W⁺ + W⁻ = n(n + 1)/2 |
| Under H₀ | mean n(n + 1)/4; SD √(n(n + 1)(2n + 1)/24) without ties |
| Normal approximation | z = (W⁺ − n(n + 1)/4) / SD |

## Python and R

| Test | Python (scipy.stats) | R |
|---|---|---|
| Mann–Whitney, normal approximation | `stats.mannwhitneyu(x, y, method='asymptotic')` (statistic = U₁) | `wilcox.test(x, y, exact = FALSE)` (W = U₁) |
| Mann–Whitney, exact (small, no ties) | `stats.mannwhitneyu(x, y)` | `wilcox.test(x, y)` |
| Wilcoxon signed-rank | `stats.wilcoxon(x, y, correction=False, method='approx')` (two-sided: the smaller of W⁺ and W⁻) | `wilcox.test(x, y, paired = TRUE, exact = FALSE, correct = FALSE)` (V = W⁺) |

Statistics+ reports the normal approximation: with the continuity correction for Mann–Whitney, without it for Wilcoxon (as above).

## In the Workbench

**Two-sample test**, Data from two columns, Test *Mann–Whitney U (non-parametric, independent)* or *Wilcoxon signed-rank (non-parametric, paired)*.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 9.9 (signed-rank test) and Section 10.3 (Wilcoxon rank-sum test).
- Mann, H. B. and Whitney, D. R. (1947), On a test of whether one of two random variables is stochastically larger than the other, *Annals of Mathematical Statistics* 18, 50–60.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.3.1: https://www.itl.nist.gov/div898/handbook/prc/section3/prc31.htm
