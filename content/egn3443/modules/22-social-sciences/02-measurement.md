---
id: st.m22.measurement
title: "Measurement: Likert scales, reliability and validity"
minutes: 50
objectives: [st.m22.o2]
example: st.m22.measurement.eg
quiz: quizzes/m22-measurement.json
reference: [scales-reliability]
---

# Measurement: Likert scales, reliability and validity

Wellbeing, anxiety, job satisfaction, trust in government: none can be weighed. Social scientists measure them with **scales**: several questions on the same theme, each answered on a **Likert** response format, added into one score. Two questions decide whether the score means anything: is it **reliable** (consistent), and is it **valid** (does it measure what it claims)?

## Likert items and scales

A Likert item is a statement with ordered answers:

> "I feel optimistic about the future." 1 strongly disagree · 2 disagree · 3 neutral · 4 agree · 5 strongly agree

One item is a coarse, noisy measure. A **scale** adds several items: five items scored 1 to 5 give a total from 5 to 25. Items worded the other way ("I often feel hopeless") are **reverse-scored** (6 − answer) before adding, so a high total always means the same thing.

A single item's answers are **ordinal**: the gap between "disagree" and "neutral" need not equal the gap between "neutral" and "agree". Report a single item with its median and the share in each answer. A total of several items behaves more like a continuous score, and means and t-tests are commonly used on it.

## The Example: a five-item wellbeing scale

Forty students' total scores on a five-item scale (illustrative data, made up for this book):

[[figure: st.m22.measurement.eg 1]]

[[results: st.m22.measurement.eg]]

The totals average **16.5** (standard deviation **3.64**), with no pile-up at the top (25) or bottom (5): the scale has room to show change. A **ceiling effect**, many students at 25, would hide any improvement among them.

[[see-example: 1]]

## Reliability: Cronbach's alpha

If the five items measure the same thing, people who score high on one tend to score high on the others, and the total's variance is larger than the sum of the items' variances. **Cronbach's alpha** measures this:

α = k/(k − 1) × (1 − Σ sᵢ² / s_total²)

with k items, sᵢ² each item's variance and s_total² the variance of the totals.

For the wellbeing scale, suppose the five item variances are 1.1, 0.9, 1.2, 1.0 and 0.8 (sum **5.0**), and the totals' variance is 3.637² = **13.23**:

α = 5/4 × (1 − 5.0/13.23) = 1.25 × 0.622 = **0.78**.

| Alpha | Usually read as |
|---|---|
| below 0.6 | poor: the items do not hang together |
| 0.7 to 0.8 | acceptable for research |
| above 0.9 | very high; for clinical decisions about one person, or the items may repeat each other |

**Test–retest reliability** is the correlation between the same people's scores on two occasions: a stable trait should give a high one.

## Validity

A reliable scale can still measure the wrong thing: a bathroom scale that always reads 5 kg light is reliable and invalid.

- **Content validity**: do the items cover the whole idea (all of wellbeing, not only mood)?
- **Criterion validity**: does the score correlate with something it should predict (wellbeing with days off sick)?
- **Construct validity**: does it correlate with related measures and not with unrelated ones?

**Factor analysis** (item 22.5, and Part II) checks whether the items measure one idea or several.

## In Python and R

```python
import numpy as np
items = np.array(responses)                          # rows people, columns items
k = items.shape[1]
alpha = k/(k-1) * (1 - items.var(axis=0, ddof=1).sum() / items.sum(axis=1).var(ddof=1))
# or: pip install pingouin; pingouin.cronbach_alpha(data=df)
```

```r
k <- ncol(items)
k/(k-1) * (1 - sum(apply(items, 2, var)) / var(rowSums(items)))
# or: psych::alpha(items)
```

[[code: st.m22.measurement.eg]]

## Common mistakes

- **Forgetting to reverse-score** the negatively worded items.
- **Means of single Likert items** compared as if the steps were equal.
- **A high alpha read as validity**, or as proof the scale measures one thing.
- **One question for a complex idea.**

## What to do next

On the **Exercise** tab, practice questions: reverse scoring, Cronbach's alpha, and kinds of validity.

[[example: Scale scores: ceiling and floor]]

The forty students' totals on the 5-to-25 scale: mean **16.5**, standard deviation **3.64**, nobody at the ceiling.

1. **A ceiling.** Change the six highest scores to 25 (the most anyone can score). The histogram piles up at the top bar, and an intervention could not raise those six any further: the scale can no longer show their improvement.
2. **A floor.** Put them back and change the six lowest to 5: the same problem at the bottom.
3. **A narrow scale.** Imagine only three items (totals 3 to 15): every score would shrink, and fewer different totals are possible. More items give a finer measure.

[[try: st.m22.scale.try Sample | Count Mean Median StandardDeviation Skewness Minimum Maximum figure=1]]
