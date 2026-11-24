---
id: st.m22.replication
title: The replication crisis
minutes: 50
objectives: [st.m22.o6]
example: st.m22.replication.eg
exercise: st.m22.replication.ex
reference: [replication-power]
---

# The replication crisis

In 2015 a team of 270 psychologists repeated 100 published experiments as closely as they could. Fewer than half of the original findings came out significant again, and the effects that did were on average about half as large (Open Science Collaboration, *Science*, 2015). Medicine, economics and other fields have found the same problem. The causes are statistical, and so are the cures.

## How false findings get published

- **Small studies.** A study with low **power** (item 8.6) usually misses a real effect. When it does reach p < 0.05, it is often because chance made the effect look bigger than it is: published small studies **overestimate** effects.
- **p-hacking.** Researchers make many reasonable choices: which outcome, which participants to exclude, which covariates, when to stop collecting data. Trying several and reporting the one with p < 0.05 makes a false positive far more likely than 5 %.
- **Publication bias.** Journals publish "significant" results; the non-significant studies stay in the drawer (the **file-drawer problem**). The published record is a biased sample.
- **HARKing**: Hypothesising After the Results are Known, presenting a pattern found in the data as if it had been predicted.

## The Example: the power of a small study

A study of 20 people tests whether a training raises a score with standard deviation 15, against H₀: μ = 100 (one-sided, α = 0.05). Suppose the true effect is 5 points, a third of a standard deviation.

[[results: st.m22.replication.eg]]

- **Power = 0.44**: the study finds a real 5-point effect less than half the time.
- **80 % power needs 56 people.**
- Item 22.3's recall experiment, with 24 per group, had about 21 % power for its effect: the same story.

[[see-example: 1]]

## The cures

- **Preregistration**: publish the hypotheses, the outcome, the sample size and the analysis **before** collecting data. Choices can no longer follow the data. **Registered reports** go further: the journal accepts the study on its design, before the results.
- **Power analysis**: plan the sample size for a realistic effect, often smaller than the original study reported.
- **Effect sizes and intervals** rather than p-values alone (item 22.3).
- **Replication**: repeat important findings, and publish the repeats whatever they show.
- **Open data and code**, so others can check the analysis.

## In Python and R

```python
from scipy import stats
from statsmodels.stats.power import TTestIndPower
# the Example's one-sided z test (H0: mu = 50, sigma 10, true mean 53), power at n = 20:
stats.norm.sf(stats.norm.ppf(0.95) - (53 - 50) / (10 / 20**0.5))
TTestIndPower().solve_power(effect_size=0.33, nobs1=24, alpha=0.05)     # power of item 22.3's study
TTestIndPower().solve_power(effect_size=0.33, power=0.8, alpha=0.05)    # n per group for 80 % power
```

```r
pnorm(qnorm(0.95) - (53 - 50) / (10 / sqrt(20)), lower.tail = FALSE)   # the Example's power
power.t.test(n = 24, delta = 0.33, sd = 1)          # power of item 22.3's study
power.t.test(delta = 0.33, sd = 1, power = 0.8)     # n per group
```

[[code: st.m22.replication.eg]]

## Common mistakes

- **"It was significant, so it is true."** In a field of small, flexible studies, many significant findings are false.
- **The original study's effect size** used to plan a replication: it is probably too large.
- **"It did not replicate, so the original was fraud."** Usually it was chance, small samples and flexible analysis.

## What to do next

On the **Exercise** tab, plan a replication: the power with 30 participants, and how many 90 % power needs.

[[example: p-hacking: try enough analyses and something is significant]]

Simulated studies where nothing is going on. Each one tries **5** analyses (outcomes or subgroups) and reports any with p < 0.05. In **247 of 1,000** studies at least one was "significant" (theory for 5 independent tests: 1 − 0.95⁵ = 0.226), not 5 %.

1. **Move the number of analyses to 20.** About two studies in three now "find" something.
2. **Move it to 1** (a preregistered single analysis): back to about 5 %.

[[try: st.m22.phacking.try Tests=1..40 Alpha=0.005..0.1 | Occurrences ExperimentalProbability TheoreticalProbability figure=1]]

[[example: Power: the sample size and the effect]]

The small study: 20 people, a true effect of 5 points (σ = 15): power **0.44**; 80 % power needs **56**.

1. **Move the sample size to 56.** The power reaches about 0.80.
2. **Back to 20, and move the true mean to 110** (an effect of two thirds of a standard deviation). The power rises steeply: big effects are easy to find, small ones need big studies.

[[try: st.m22.power.try SampleSize=5..200 AlternativeValue=100..115 | Power SampleSizeForPower figure=1]]
