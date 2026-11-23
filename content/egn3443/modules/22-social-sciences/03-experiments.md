---
id: st.m22.experiments
title: Experiments in psychology
minutes: 50
objectives: [st.m22.o3]
example: st.m22.experiments.eg
exercise: st.m22.experiments.ex
reference: [psychology-experiments-apa]
---

# Experiments in psychology

Psychology answers many of its questions with **experiments**: people are randomly assigned to conditions, and an outcome is compared. Does practising recall help memory more than rereading? Does a walk in a park lift mood? The analysis is Module 10's two-sample t-test or Module 14's ANOVA. What the field adds is a standard way to **report** it, and the habit of reporting **how big** an effect is, not only whether it is "significant".

## Effect size: Cohen's d

A t-test says whether a difference could be chance. Its p-value shrinks as the sample grows, even for a trivial difference. The **effect size** says how large the difference is, in standard deviations:

d = (x̄₁ − x̄₂) / s_pooled.

| d | Cohen's rough labels |
|---|---|
| 0.2 | small |
| 0.5 | medium |
| 0.8 | large |

The labels are rough: a "small" effect of a cheap intervention given to millions can matter more than a "large" one in a lab.

## The Example: retrieval practice and recall

Forty-eight volunteers were randomly assigned, 24 to a session of retrieval practice (testing themselves) and 24 to rereading; a week later each took a recall test out of 100 (illustrative data, made up for this book).

[[figure: st.m22.experiments.eg 1]]

[[results: st.m22.experiments.eg]]

- The practice group scored higher on average, but t(46) = **1.13**, p = **0.26**: the difference could be chance at α = 0.05.
- Cohen's d = **0.33**: a small-to-medium effect, if real. The 95 % interval for the difference runs from −2.7 to +9.7 points: the data cannot rule out no effect, or a fairly large one.
- **Not significant is not "no effect".** With 24 per group, a true d of 0.33 would be detected only about one time in five (power about 0.21, item 22.6). This study was too small to answer its question.
- The checks (item 9.4): Shapiro–Wilk p = 0.22 and 0.50, Levene p = 0.96; the pooled t-test's assumptions look fine.

[[see-example: 1]]

## The APA results line

Psychology journals follow the style of the American Psychological Association (APA). A t-test is reported in one sentence with the statistic, its degrees of freedom, the p-value and the effect size:

> Recall did not differ significantly between the practice group (M = 64.8, SD = 10.5) and the rereading group (M = 61.2, SD = 10.9), t(46) = 1.13, p = .26, d = 0.33.

The conventions: the degrees of freedom in brackets, two decimals for t and d, p to two or three decimals without a leading zero (p = .26, or p < .001), and the means and standard deviations of the groups. A reader can check the result and use it in a **meta-analysis**, which combines the effect sizes of many studies.

## In Python and R

```python
from scipy import stats
t, p = stats.ttest_ind(practice, rereading)              # pooled
sp = np.sqrt(((n1-1)*s1**2 + (n2-1)*s2**2) / (n1+n2-2))
d = (practice.mean() - rereading.mean()) / sp
```

```r
t.test(practice, rereading, var.equal = TRUE)
# Cohen's d: effectsize::cohens_d(practice, rereading)
```

[[code: st.m22.experiments.eg]]

## Common mistakes

- **Reporting only p.** Without the effect size the reader cannot tell a trivial from an important difference.
- **"No effect" from a non-significant result** of a small study.
- **Leaving out the degrees of freedom** or the group means in the results line.
- **Random sampling confused with random assignment.** Volunteers are rarely a random sample of anyone; random assignment is what makes the comparison fair.

## What to do next

On the **Exercise** tab, a mood experiment with 30 students in each condition: run the t-test, give Cohen's d, and write the APA line.

[[example: Recall scores: the effect size and the sample]]

The recall experiment: t(46) = **1.13**, p = **0.26**, d = **0.33**.

1. **Add 10 points to every score in the practice column.** The difference grows: t(46) = **4.38**, p = **0.0001**, d = **1.26**, a large effect.
2. **Put the scores back.** Now paste the 24 practice scores twice into their column, and the rereading scores twice into theirs (48 per group, same means and SDs). d hardly moves (**0.33**), but t grows to **1.62** and p falls to **0.11**: the same effect, more evidence, and still not enough. About 145 per group would give 80 % power for d = 0.33.

[[try: st.m22.recall.try @Recall/Practice @Recall/Rereading | Statistic DegreesOfFreedom PValue EffectSize CiLower CiUpper figure=1]]
