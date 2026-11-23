---
id: st.m24.science
title: Sports science and measurement
minutes: 45
objectives: [st.m24.o6]
example: st.m24.science.eg
exercise: st.m24.science.ex
reference: [sports-science-measurement]
---

# Sports science and measurement

An athlete's sprint time improves from 4.30 to 4.26 seconds after a training block. Did she get faster, or is that measurement noise? Sports scientists answer with three numbers from Part I's ideas of variation: the **test–retest reliability**, the **typical error** of the test, and the **smallest worthwhile change**.

## Test–retest reliability

Measure the same athletes twice, close enough in time that nothing real has changed. If the test is reliable, the two trials agree: the correlation between them (item 11.1) is high.

The Example: twenty athletes ran a timed 30 m sprint twice, a week apart (illustrative data, made up for this book).

[[figure: st.m24.science.eg 1]]

[[results: st.m24.science.eg]]

- r = **0.96**: athletes keep their order closely from trial to trial; the test separates fast and slow athletes well.
- The slope is close to 1 (0.93) and the trials agree on average (mean difference −0.006 s): no learning or fatigue effect between trials.

## The typical error

The correlation says how well the test ranks athletes. To judge **one** athlete's change, what matters is the noise in one measurement, the **typical error**:

typical error = SD of the trial-to-trial differences / √2.

Here the SD of the differences is 0.054 s, so the typical error is **0.038 s**. A single sprint time is uncertain by about ±0.04 s (one typical error), and a change between two tests by about √2 × 0.038 ≈ 0.054 s.

## The smallest worthwhile change

What size of change matters? A common rule (from Hopkins's work in sports science) is **0.2 × the between-athlete standard deviation**: a "small" effect in Cohen's terms (item 22.3). The athletes' times have an SD of 0.188 s, so the smallest worthwhile change is **0.038 s**.

The two numbers are about equal: the sprint test can only just detect a change that matters. A 0.04 s improvement could be real or noise. To be confident, the athlete needs several trials averaged (the typical error of a mean of m trials is TE/√m), or a more precise timing system (light gates rather than a hand-held stopwatch).

[[see-example: 1]]

## Tracking data

Modern sport records GPS positions, heart rate and accelerations many times a second. The same questions apply to every number: how reliable is it, how large is its typical error, and is a change larger than both the noise and the smallest change worth acting on?

## In Python and R

```python
import numpy as np
diff = trial2 - trial1
te = diff.std(ddof=1) / np.sqrt(2)                # typical error
swc = 0.2 * trial1.std(ddof=1)                    # smallest worthwhile change
r = np.corrcoef(trial1, trial2)[0, 1]             # test-retest reliability
```

```r
diff <- trial2 - trial1
sd(diff) / sqrt(2); 0.2 * sd(trial1); cor(trial1, trial2)
```

[[code: st.m24.science.eg]]

## Common mistakes

- **A high correlation read as precision**: r says the test ranks athletes; the typical error says how noisy one score is.
- **Any improvement reported as real**, without comparing it with the typical error.
- **One trial** where several averaged would cut the noise.
- **The smallest worthwhile change ignored**: a real but tiny change may not matter.

## What to do next

On the **Exercise** tab, a jump test done twice: the mean and SD of the differences, and from them the typical error.

[[example: Reliability: one bad trial]]

The twenty athletes' two sprint trials: r = **0.96**, slope **0.93**.

1. **One slip.** Change the first athlete's trial 2 by adding 0.30 s (a stumble at the start). The correlation drops: one bad trial makes the test look less reliable. In practice it would be repeated.
2. **Put it back.** Now add 0.05 s to every trial 2 (a slower track on the second day). The correlation does not change, but the intercept does: r measures agreement in order, not agreement in value. That is why the mean difference is checked too.

[[try: st.m24.retest.try "@Sprint/Trial 2 (s)" | Equation Slope Intercept Correlation RSquared figure=1]]
