---
id: st.m22.psychometrics
title: "Psychometrics: scores, norms and percentiles"
minutes: 45
objectives: [st.m22.o5]
example: st.m22.psychometrics.eg
exercise: st.m22.psychometrics.ex
reference: [psychometrics-norms]
---

# Psychometrics: scores, norms and percentiles

A child answers 34 of 50 questions on a reading test. Is that good? A raw score means nothing until it is compared with others. **Psychometrics**, the science of psychological measurement, turns raw scores into **standard scores** and **percentiles** against a **norm group**, using the normal distribution of Module 5.

## Norms

A test is given to a large, representative **norm group** (thousands of children of each age, say). Their raw scores give the mean and standard deviation against which everyone later is compared. A score is then reported as:

- a **z-score**: z = (raw − mean)/SD, how many SDs from the norm group's mean;
- a **standard score** on a fixed scale: IQ-type scores have mean 100 and SD 15 (score = 100 + 15z); T-scores have mean 50 and SD 10; many achievement tests use mean 500 and SD 100;
- a **percentile**: the share of the norm group scoring at or below it.

## The Example: a score of 120

On a scale with mean 100 and standard deviation 15 (item 5.3):

[[figure: st.m22.psychometrics.eg 1]]

[[results: st.m22.psychometrics.eg]]

- z = (120 − 100)/15 = **1.33**.
- P(X ≤ 120) = **0.909**: a score of 120 is at about the **91st percentile**, higher than 91 % of the norm group.
- The 90th percentile is **119.2**: 90 % of the norm group score below it.

[[see-example: 1]]

## Measurement error in a score

No test is perfectly reliable (item 22.2). The **standard error of measurement**, SEM = SD √(1 − reliability), says how far an observed score may be from the person's true score. With SD = 15 and reliability 0.91: SEM = 15 √0.09 = **4.5**. A score of 120 is then best reported as a band, about 120 ± 9 (two SEMs): roughly the 77th to the 97th percentile. Decisions about one person (a diagnosis, a school placement) should not hang on a few points.

## Factor analysis: what the items measure

An intelligence test has dozens of subtests: vocabulary, arithmetic, puzzles, memory. Their scores correlate with each other. **Factor analysis** asks how few underlying abilities, **factors**, could explain those correlations: one general ability with some specific ones, for example. Each subtest gets **loadings**, the correlations with each factor. It is how psychologists check that a scale measures one idea, or several (Part II covers it).

## In Python and R

```python
from scipy import stats
z = (120 - 100) / 15
pct = stats.norm.cdf(120, 100, 15)        # percentile
p90 = stats.norm.ppf(0.90, 100, 15)       # score at the 90th percentile
```

```r
(120 - 100) / 15
pnorm(120, 100, 15); qnorm(0.90, 100, 15)
# factor analysis: factanal(items, factors = 2)
```

[[code: st.m22.psychometrics.eg]]

## Common mistakes

- **A percentile read as a percentage correct.** The 91st percentile can be 60 % of the questions.
- **Norms from the wrong group**: an old norm group, or another country's.
- **Exact cut-offs** for one person, ignoring the standard error of measurement.
- **Percentiles averaged or subtracted**: they are not on an equal-interval scale; z-scores are.

## What to do next

On the **Exercise** tab, a reading test normed to mean 500 and SD 100: a pupil's percentile and z-score, and the score at the 25th percentile.

[[example: Norms: percentiles and scores]]

A normed scale with mean **100** and SD **15**. A score of 120 is at the **91st percentile**; the 90th percentile is **119.2**.

1. **Move the score (x) to 130**: z = 2.0, about the 98th percentile. Two standard deviations above the mean is rare.
2. **Move it to 85**: z = −1, about the 16th percentile.
3. **Set the probability to 0.5**: the median, 100. **0.02**: about 69, the score below which 2 % of the norm group fall.

[[try: st.m22.norms.try X=40..160 Probability | AtMost ZScore Quantile figure=1]]
