---
id: st.m7.interpreting
title: Interpreting confidence intervals
minutes: 40
objectives: [st.m7.o5, st.m7.o4]
builds-on: st.m7.proportions
example: st.m7.interpreting.eg
quiz: quizzes/m7-interpreting.json
reference: [interpreting-confidence-intervals]
---

# Interpreting confidence intervals

## What "95 % confident" means

Before a sample is taken, the interval X̄ ± t·S/√n is random: its center and its width both depend on the sample. The **method** has a 95 % chance of producing an interval that contains μ. That is what the confidence level describes.

After the sample is taken, the interval is two fixed numbers, say 39.77 to 42.18 s, and μ is a fixed number too. Either μ is inside or it is not. There is nothing random left, so it is not right to say "there is a 95 % probability that μ is between 39.77 and 42.18". We say instead:

**"We are 95 % confident that the mean drain time is between 39.77 and 42.18 seconds."**

and, if asked what that means: **"If we repeated the sampling many times, about 95 % of the intervals built this way would contain the true mean."**

## Seeing it: a thousand intervals

The Simulation analysis knows μ, which real life never tells us. Here it drew 1000 samples of n = 10 from a normal population with μ = 50 and σ = 2, and built a 95 % t interval from each one. The figure shows the first 100. Each horizontal line is one interval, sample 1 at the bottom; the vertical line is μ. Intervals that contain μ are blue, and those that miss it are red and thicker.

[[figure: st.m7.interpreting.eg 2]]

[[results: st.m7.interpreting.eg]]

949 of the 1000 intervals contain μ, an experimental proportion of 0.949 against the theoretical 0.95. In the first 100, eight miss. Notice:

- The intervals have **different centers** (each x̄ is different) and **different widths** (each s is different).
- The ones that miss are not "wrong calculations". Their samples happened to fall far from μ. Nothing in a single interval tells you whether it is one of the misses.
- The running proportion settles near 0.95 as the intervals pile up:

[[figure: st.m7.interpreting.eg 1]]

## A lower confidence level

The same experiment with 90 % intervals (the same seed, so the same samples):

[[figure: st.m7.interpreting.ninety 2]]

[[results: st.m7.interpreting.ninety]]

906 of 1000 contain μ, and 15 of the first 100 miss. The 90 % intervals are narrower (on average 2.16 against 2.66 for the 95 % ones), so they miss more often. Confidence is bought with width: item 7.6 takes this up.

## Statements that are right, and statements that are not

For a 95 % interval for the mean drain time of 39.77 to 42.18 s:

| Statement | Right? |
|---|---|
| We are 95 % confident that the mean drain time μ is between 39.77 and 42.18 s. | ✔ |
| About 95 % of intervals made this way, from repeated samples, contain μ. | ✔ |
| There is a 95 % probability that μ is between 39.77 and 42.18 s. | ✘ μ is fixed; the probability belongs to the method, not to this interval |
| 95 % of the drain times are between 39.77 and 42.18 s. | ✘ the interval is for the **mean**; single drain times spread much more (s = 1.89 s) |
| 95 % of sample means will fall between 39.77 and 42.18 s. | ✘ the interval is centered on this x̄, not on μ |
| The true mean is 40.975 s, give or take. | ✘ x̄ is an estimate; the interval says how far off it may be |

## Using an interval for a decision

An interval answers "which values of μ are plausible?". So:

- A **target or a claim inside** the interval is consistent with the data: the data do not contradict it.
- A value **outside** the interval is not plausible at that confidence. If a supplier claims a mean of 44 s and the interval is 39.77 to 42.18 s, the data argue against the claim.
- Compare the interval with what matters in practice. If every value in it is acceptable, the decision is clear whatever μ is. If it straddles a limit, more data are needed.

Module 8 makes this formal: a two-sided test at α = 0.05 rejects a value of μ exactly when it is outside the 95 % interval.

## In Python and R

The simulation is a few lines in either language. The numbers differ from the Workbench's, because each draws its own random samples, but the share that contain μ comes out near 0.95.

```python
import numpy as np
from scipy import stats
rng = np.random.default_rng(1)
mu, sigma, n, trials = 50, 2, 10, 1000
t_crit = stats.t.ppf(0.975, n - 1)
hits = 0
for _ in range(trials):
    x = rng.normal(mu, sigma, n)
    e = t_crit * x.std(ddof=1) / np.sqrt(n)
    hits += (x.mean() - e <= mu <= x.mean() + e)
print(hits / trials)        # close to 0.95
```

```r
set.seed(1)
mu <- 50; sigma <- 2; n <- 10
covers <- replicate(1000, {
  ci <- t.test(rnorm(n, mu, sigma))$conf.int
  ci[1] <= mu && mu <= ci[2]
})
mean(covers)                # close to 0.95
```

[[code: st.m7.interpreting.eg]]

## Common mistakes

- Saying there is a 95 % probability that μ is in the interval you calculated.
- Reading the interval as a range for individual parts or measurements.
- Thinking a 99 % interval is "better" than a 95 % one. It is wider: more confidence, less precision.
- Forgetting the context. "(39.77, 42.18)" means nothing to a reader; "the mean drain time is between 39.8 and 42.2 seconds, with 95 % confidence" does.

## What to do next

The **Exercise** tab has questions on wording: which statements about an interval are right, what a confidence level promises, and how to report an interval in context.
