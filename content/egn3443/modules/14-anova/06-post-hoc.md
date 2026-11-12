---
id: st.m14.post-hoc
title: Post-hoc testing and interpreting results
minutes: 55
objectives: [st.m14.o5, st.m14.o1]
builds-on: st.m14.multi-factor
example: st.m14.post-hoc.eg
exercise: st.m14.post-hoc.ex
reference: [post-hoc-tests, familywise-error]
---

# Post-hoc testing and interpreting results

## Why post-hoc tests are needed

A significant F says the means are not all equal. It does not say which ones differ. A **post-hoc** ("after this") test compares the groups pair by pair, **while holding the familywise error rate at α** for the whole set of comparisons. Running plain t-tests on every pair brings back the inflation of item 14.1.

## The common procedures

| Procedure | Compares | How it controls the family | Use it when |
|---|---|---|---|
| **Tukey's HSD** | every pair | the studentized range distribution, made for all k(k − 1)/2 pairs at once | you want all the pairwise comparisons: the usual choice |
| **Bonferroni** | any m planned comparisons | each test at α/m | only a few comparisons, chosen before seeing the data |
| **Scheffé** | every pair and every contrast (such as "the mean of A and B against C") | the F distribution | you explore contrasts after the fact; the most conservative for pairs |
| **Dunnett** | each group against one control | a distribution made for k − 1 comparisons with a control | new designs against the current one |

The Workbench calculates Tukey's HSD. Statistics texts give the others (Montgomery and Runger, Chapter 13; the NIST/SEMATECH e-Handbook, Section 7.4.7).

## Tukey's HSD

**HSD** stands for **honestly significant difference**: the smallest difference of two means that is significant once all the pairs are taken into account.

HSD = q(α; k, N − k) × √(MS within / n)

- **q(α; k, N − k)** is the upper α point of the **studentized range** distribution: the distribution of (largest mean − smallest mean)/(standard error) for k means when H₀ is true. It depends on the number of groups k and the within-groups df N − k. Python `stats.studentized_range.ppf(1 - alpha, k, N - k)`, R `qtukey(1 - alpha, k, N - k)`. The Workbench reports it as **Tukey q critical**.
- **n** is the size of each group. With unequal groups, the Tukey–Kramer form uses √(MS within/2 × (1/nᵢ + 1/nⱼ)) in place of √(MS within/n) for each pair; the Workbench and R do this automatically.

Two means differ when |ȳᵢ − ȳⱼ| > HSD. Equivalently, each pair gets a **simultaneous confidence interval** (ȳᵢ − ȳⱼ) ± HSD; all of them together cover the true differences with 95 % confidence. A pair differs when its interval does not contain 0. The **adjusted p-value** of a pair is below α exactly then.

## A worked case by hand

The fertilizer data of item 14.3: means 11.5 (A), 8.5 (B), 14.5 (C), n = 4 each, MS within = 1.667 on 9 df, k = 3.

1. q(0.05; 3, 9) = 3.948.
2. HSD = 3.948 × √(1.667/4) = 3.948 × 0.6455 = **2.549**.
3. The differences: A − B = 3.0, C − A = 3.0, C − B = 6.0. All three exceed 2.549, so **every pair differs**: C gives the highest yield, then A, then B.

## The Example: wear life of four coatings

[[data: st.m14.post-hoc.eg]]

[[results: st.m14.post-hoc.eg]]

F = 26.28 on 3 and 20 df, p < 0.0001: some coating differs. Now Tukey. q(0.05; 4, 20) = 3.958, MS within = 4.458, n = 6, so HSD = 3.958 × √(4.458/6) = **3.41** minutes.

| Pair | Difference | Tukey 95 % interval | Adjusted p | Differ? |
|---|---|---|---|---|
| 2 − 1 | 6.33 | 2.92 to 9.75 | 0.0002 | yes |
| 3 − 1 | 3.17 | −0.25 to 6.58 | 0.075 | no |
| 4 − 1 | 10.33 | 6.92 to 13.75 | < 0.0001 | yes |
| 3 − 2 | −3.17 | −6.58 to 0.25 | 0.075 | no |
| 4 − 2 | 4.00 | 0.59 to 7.41 | 0.018 | yes |
| 4 − 3 | 7.17 | 3.75 to 10.58 | < 0.0001 | yes |

The Workbench lists each pair the other way round (1 − 2, …), with the signs reversed.

[[figure: st.m14.post-hoc.eg 2]]

**Reading it.** Coating 4 lasts longest and differs from every other (by 4.0 to 10.3 minutes on average). Coating 1 differs from 2 and from 4. Coating 3 cannot be told apart from 1 or from 2. A compact way to write this gives coatings that share a letter no detectable difference:

| Coating | 1 | 3 | 2 | 4 |
|---|---|---|---|---|
| Mean life (min) | 55.2 | 58.3 | 61.5 | 65.5 |
| Group | a | a b | b | c |

"Not different" is not "the same": 3 − 1 = 3.17 minutes could be a real difference these 24 tools could not show.

Plain t-tests on each pair, using the pooled MS within on 20 df and no correction (R `pairwise.t.test(life, coating, p.adjust.method = "none")`), would call **all six** pairs different: the 3 − 1 and 3 − 2 pairs give p = 0.017 each. With six comparisons, that is the familywise error of item 14.1 at work.

## Drawing conclusions and reporting them

A results paragraph names the test, the F with its two df, the p-value, the effect size, the post-hoc procedure, and what it means for the engineering question:

"A one-way ANOVA found that the coating affected tool life, F(3, 20) = 26.28, p < 0.001, η² = 0.80. Tukey's HSD (α = 0.05) showed coating 4 (mean 65.5 min) outlasting all the others, and coating 1 (55.2 min) shorter-lived than coatings 2 and 4. Coating 4 is recommended."

The APA style used in many journals writes this F(3, 20) = 26.28, p < .001, η² = .80, without the zero before the decimal point. Give exact p-values above 0.001, and "< 0.001" below.

## In Python and R

```python
from scipy import stats
c1 = [54, 58, 51, 56, 55, 57]; c2 = [62, 59, 64, 61, 63, 60]
c3 = [57, 61, 55, 60, 58, 59]; c4 = [66, 63, 68, 65, 67, 64]
print(stats.f_oneway(c1, c2, c3, c4))
print(stats.tukey_hsd(c1, c2, c3, c4))               # scipy 1.8 or later: intervals and adjusted p
print(stats.studentized_range.ppf(0.95, 4, 20))     # q = 3.958
```

```r
life <- c(54, 58, 51, 56, 55, 57, 62, 59, 64, 61, 63, 60, 57, 61, 55, 60, 58, 59, 66, 63, 68, 65, 67, 64)
coating <- factor(rep(1:4, each = 6))
fit <- aov(life ~ coating)
TukeyHSD(fit)                                       # diff, lwr, upr, p adj for every pair
qtukey(0.95, 4, 20)
pairwise.t.test(life, coating, p.adjust.method = "bonferroni")
```

[[code: st.m14.post-hoc.eg]]

## In the Workbench

**One-way ANOVA** gives **Tukey q critical**, the **Tukey HSD pairs** (each difference, its interval, its adjusted p, and * when it differs) and **Pairs that differ**. The second figure draws each group mean with its 95 % interval; those intervals are for one mean at a time, so use the Tukey pairs, not overlapping bars, to decide which pairs differ.

## Common mistakes

- **Post-hoc tests without a reason.** If the F test does not reject, there is usually nothing for them to find.
- **Plain t-tests on every pair.** They inflate the familywise error.
- **Reading overlapping single-mean intervals** as "no difference". Compare the pairwise (Tukey) intervals.
- **Calling non-significant pairs equal.** They are not shown to differ; the difference may still matter.
- **Using Bonferroni for all pairs of many groups.** It is valid but loses more power than Tukey.

## What to do next

On the **Exercise** tab, five nozzle designs. Find Tukey's q and how many of the ten pairs differ.
