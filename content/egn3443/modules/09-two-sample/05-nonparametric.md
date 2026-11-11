---
id: st.m9.nonparametric
title: Introduction to non-parametric methods
minutes: 45
objectives: [st.m9.o5, st.m9.o3]
builds-on: st.m9.assumptions
example: st.m9.nonparametric.eg
exercise: st.m9.nonparametric.ex
reference: [rank-tests, checking-assumptions]
---

# Introduction to non-parametric methods

## When the t-test's assumption fails

The t-tests assume normal populations. With small samples that are clearly skewed, or that hold an extreme value, that assumption fails, and one far value can pull a mean and inflate a standard deviation enough to hide a real difference. **Non-parametric** tests do not assume a normal distribution (they assume no particular "parameterized" family at all). The two in this item replace the values by their **ranks**: the smallest value gets rank 1, the next rank 2, and so on. A value that is ten times the others is just the largest rank.

| Design | Parametric test | Non-parametric alternative |
|---|---|---|
| Two independent samples | Pooled or Welch t | **Mann–Whitney U** (also called the Wilcoxon rank-sum test) |
| Paired samples | Paired t | **Wilcoxon signed-rank** |

The hypotheses change too. Instead of means, H₀ says the two populations have the same distribution (Mann–Whitney), or that the differences are symmetric about 0 (Wilcoxon). Informally, the tests ask whether one sample **tends to give larger values** than the other.

The price: when the data really are normal, a rank test is slightly less powerful than the t-test. When they are not, it is often much more powerful. Use it when the assumptions of the t-test cannot be met, not to shop for a smaller p-value.

## The Mann–Whitney U test

1. Rank all n₁ + n₂ values together (ties get the average of their ranks).
2. Add the ranks of sample 1: R₁.
3. U₁ = R₁ − n₁(n₁ + 1)/2, and U₂ = n₁n₂ − U₁. U₁ counts, over all n₁n₂ pairs of one value from each sample, how often the sample 1 value is the larger.
4. Under H₀, U has mean n₁n₂/2 and standard deviation √(n₁n₂(n₁ + n₂ + 1)/12) (without ties). For moderate samples, z = (|U₁ − n₁n₂/2| − 0.5) / SD is close to standard normal; the 0.5 is the **continuity correction**, because U moves in whole steps.

**By hand.** The set time (hours) of two adhesives: A = 3.1, 4.6, 2.8, 5.9 and B = 5.2, 7.4, 6.1, 4.9, 8.3.

| Value | 2.8 | 3.1 | 4.6 | 4.9 | 5.2 | 5.9 | 6.1 | 7.4 | 8.3 |
|---|---|---|---|---|---|---|---|---|---|
| Sample | A | A | A | B | B | A | B | B | B |
| Rank | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |

R₁ = 1 + 2 + 3 + 6 = 12, so U₁ = 12 − 4 × 5/2 = 12 − 10 = **2** and U₂ = 20 − 2 = 18. Under H₀ the mean is 4 × 5/2 = 10 and the SD √(20 × 10/12) = 4.082. z = (|2 − 10| − 0.5)/4.082 = **1.837**, and p = 2 × P(Z > 1.837) = **0.066**. Not significant at 0.05: with only four and five values, even this clear separation is weak evidence. (For samples this small the exact p-value, 0.0635, is better; both libraries give it by default. Statistics+ reports the normal approximation.)

## The Wilcoxon signed-rank test

For pairs:

1. Find each difference d and drop any that are exactly 0.
2. Rank the **sizes** |d|, from 1 (the smallest) to n.
3. Add the ranks of the positive differences, W⁺, and of the negative ones, W⁻. They add to n(n + 1)/2.
4. Under H₀, W⁺ has mean n(n + 1)/4 and SD √(n(n + 1)(2n + 1)/24), and z = (W⁺ − n(n + 1)/4)/SD.

## The Example: two control screens

Ten operators did the same task on an old screen and a new one.

[[data: st.m9.nonparametric.eg]]

[[figure: st.m9.nonparametric.eg 1]]

The differences old − new are 1.3, −0.1, 1.9, 0.8, 1.5, 0.7, 1.2, 9.3, 0.9 and 1.1 s. Nine of ten are positive (faster on the new screen), and one is huge: 9.3 s. Ranked by size, −0.1 is rank 1 and 9.3 rank 10, so W⁻ = 1 and W⁺ = 2 + 3 + … + 10 = 54.

[[results: st.m9.nonparametric.eg]]

The statistic shown is the smaller sum, W = 1 (as scipy reports it for a two-sided test; R's `wilcox.test` reports V = W⁺ = 54). z = (54 − 27.5)/9.811 = 2.70 and p = 0.0069: the new screen is faster.

The paired t-test on the same data gives t = 2.20 and p = 0.055, **not** significant. The one 9.3 s difference inflates s_d so much that it hides nine steady improvements. Shapiro–Wilk on the differences gives p = 0.00002: the t-test's assumption plainly fails here, so the Wilcoxon result is the one to report.

## In Python and R

```python
from scipy import stats
# a, b: two independent samples; old, new: the two columns of a paired design (numpy arrays)
stats.mannwhitneyu(a, b, method='asymptotic')     # U for sample a, normal approximation with continuity correction
stats.mannwhitneyu(a, b)                          # exact p for small samples without ties
stats.wilcoxon(old, new, correction=False, method='approx')   # signed-rank, normal approximation
```

```r
wilcox.test(a, b, exact = FALSE)                  # Mann–Whitney: W = U for sample a
wilcox.test(old, new, paired = TRUE, exact = FALSE, correct = FALSE)   # signed-rank: V = W⁺
```

[[code: st.m9.nonparametric.eg]]

## In the Workbench

Choose **Two-sample test**, set **Data from** to the two columns (the rank tests need the data, not summaries), and set **Test** to *Mann–Whitney U (non-parametric, independent)* or *Wilcoxon signed-rank (non-parametric, paired)*.

## Common mistakes

- **Using a rank test on summary statistics.** Ranks need the raw values.
- **Mann–Whitney on paired data**, or Wilcoxon signed-rank on independent samples. The design decides, as for the t-tests.
- **Reading the wrong U.** U₁ and U₂ add to n₁n₂; say which one you report. R₁ itself is not U₁: subtract n₁(n₁ + 1)/2.
- **Switching to a rank test because the t-test "did not work".** Decide from the design and the assumption checks, before you look at p.

## What to do next

On the **Exercise** tab, compare the repair times of two crews with the Mann–Whitney U test. Find U₁ and the p-value, and press **Check my answers**.
