---
id: st.m9.selecting
title: Selecting the appropriate test
minutes: 35
objectives: [st.m9.o1, st.m9.o5]
builds-on: st.m9.paired
quiz: quizzes/m9-selecting.json
reference: [choosing-a-two-sample-test, two-sample-t-tests, paired-t-test]
---

# Selecting the appropriate test

A problem never says "use Welch". You read how the data were collected and what is being compared, and the test follows. Choosing the wrong test does not give an error message: it gives a wrong p-value, and a wrong decision.

## Four questions

Ask them in this order.

1. **What is compared?** Means of a measurement (strength, time, diameter) are the usual case. **Spreads** (repeatability, consistency) are compared with the F-test for two variances (item 9.4). **Proportions** (the share defective, the share that pass) use the two-proportion z-test, which Module 10 teaches.
2. **Independent or paired?** If each value in one sample belongs with one value in the other (the same unit measured twice, before and after, the same specimen on two machines, matched left and right), the data are **paired**. If the two samples are separate groups of units, they are **independent**.
3. **Is σ known?** Only when long process history gives the population standard deviations, and the samples are reasonably large, use the **z-test**. Otherwise (the usual case) use a **t-test**.
4. **Are the assumptions reasonable?** The t-tests assume each sample (or the differences, for pairs) comes from a roughly normal population. With small samples that are clearly skewed or have extreme values, use the **rank test** instead (item 9.5). For independent samples, the pooled t also assumes equal variances; if you cannot assume that, use **Welch**.

## Two checks you will see quoted

Problems (and the Workbench, when the data come from columns) often quote two tests of these assumptions. Item 9.4 teaches them; for now, know what their p-values say:

- **Shapiro–Wilk** tests whether a sample could come from a normal population. A **small p** (below 0.05) is evidence that it is **not normal**; a large p means no evidence against normality.
- **Levene's test** tests whether two populations have equal variances. A **small p** is evidence that the **variances differ** (use Welch, not the pooled t); a large p means no evidence that they differ.

Neither check proves the assumption holds, and with small samples both miss real departures, so look at the data too.

## The decision table

| What you have | Test | Statistic and df |
|---|---|---|
| Independent samples, σ₁ and σ₂ known | Two-sample z | z = (x̄₁ − x̄₂)/√(σ₁²/n₁ + σ₂²/n₂) |
| Independent samples, σ unknown, equal variances reasonable | Pooled t | df = n₁ + n₂ − 2 |
| Independent samples, σ unknown, variances may differ | Welch t (the safe default) | ν by Welch–Satterthwaite |
| Paired measurements, differences roughly normal | Paired t | df = n − 1 (n pairs) |
| Independent samples, far from normal, small n | Mann–Whitney U | rank sums |
| Paired measurements, differences far from normal, small n | Wilcoxon signed-rank | ranks of the differences |
| Two variances (spreads) | F-test | F = s₁²/s₂², df = n₁ − 1 and n₂ − 1 |
| Two proportions | Two-proportion z (Module 10) | pooled p̂ |

## Sample size

Sample size changes the choice in two ways:

- **Large samples** (each n above about 30) make the t-tests robust: by the central limit theorem (Module 6), x̄ is nearly normal even when the data are not. The rank tests matter most for small samples.
- With large samples, t and z give nearly the same p-value, but use z only when σ is truly known. Plugging s in for σ and calling it a z-test is a common shortcut; with small samples it makes p too small.

## Two cases worked through

**A gauge study.** Twelve shafts are each measured once with a laser micrometer and once with a contact micrometer. Compared: the mean reading (a measurement). Paired: yes, each shaft gives two readings. σ known: no. The differences look normal in a box plot. **Paired t**, with 11 df.

**Two suppliers' bolts.** 20 bolts from supplier 1 and 35 from supplier 2 are tested for clamp load. Compared: the mean load. Paired: no, different bolts. σ known: no. The standard deviations are 1.8 and 4.2 kN, very different. **Welch t.**

[[see-example: 1]]

## What to do next

On the **Exercise** tab, eight short cases ask you to choose the test. They are practice questions: after a wrong try the explanation shows, walking through the four questions for that case. You need 80 % to complete the item.

[[example: Two suppliers' bolts: when the choice matters]]

This is the Learn page's second case, with means added: supplier 1's 20 bolts average **41.0 kN** (s = 1.8), supplier 2's 35 bolts **37.0 kN** (s = 4.2). It starts with Welch's test: t = **4.90** on **50.11** df, p below **0.0001**. With samples this large and a difference this clear, the choice hardly matters.

1. **Change the test to the pooled t.** t = **4.04** with **53** df and p = **0.0002**. Pooling mixes a small and a large spread, so the standard error is wrong, but the decision stands.
2. **Change the test back to Welch's t and move the sizes to 5 and 6 bolts.** Now t = **2.11** on **7.02** df and p = **0.073**: not rejected.
3. **Change the test to the two-sample z**, using the sample standard deviations as if they were σ. The same statistic, 2.11, read on the normal curve gives p = **0.035**, and H₀ is rejected. That is the shortcut the Learn page warns about: with small samples it makes p too small.
4. **Change the test to the pooled t.** t = **1.97** with **9** df and p = **0.080**.

[[try: st.m9.bolts.try TestKind SampleSize=2..40 Sample2Size=2..40 | Statistic DegreesOfFreedom PValue Decision figure=1]]
