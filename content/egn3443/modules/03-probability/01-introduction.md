---
id: st.m3.introduction
title: Introduction to probability
minutes: 30
objectives: [st.m3.o3, st.m3.o4]
example: st.m3.introduction.eg
quiz: quizzes/m3-introduction.json
reference: [law-of-large-numbers, probability-rules]
---

# Introduction to probability

## What a probability is

A **probability** is a number that says how likely an event is. It is always between 0 and 1:

| P | Means |
|---|---|
| 0 | the event cannot happen |
| 0.05 | it happens about 1 time in 20 |
| 0.5 | as likely to happen as not |
| 0.95 | it happens about 19 times in 20 |
| 1 | the event is certain |

A probability can also be written as a percentage (0.05 = 5 %) or as "1 in 20". Engineers use all three; the calculations use the decimal.

## The words

- An **experiment** is anything whose result is decided by chance: flipping a coin, testing a weld, counting the cars at a light.
- An **outcome** is one possible result: heads; the weld passes; 7 cars.
- An **event** is a set of outcomes we care about, written with a capital letter: A = "the weld fails", B = "more than 5 cars".
- P(A) is the probability of event A.

Item 3.2 makes these precise with the sample space, the list of every outcome.

## Three ways to get a probability

1. **Theoretical (classical).** When every outcome is equally likely, count: P(A) = favourable outcomes ÷ possible outcomes. A fair die shows a 4 with probability 1/6 = 0.1667, because one face of six is a 4. No die has to be rolled.
2. **Experimental (empirical, relative frequency).** Repeat the experiment and count: P(A) ≈ times A happened ÷ trials. If 12 of 400 castings from a mold are porous, the estimate is 12/400 = 0.03. Most engineering probabilities come this way, from test records and failure logs.
3. **Subjective.** An expert's judgment when there is neither a model nor data: "I would put the chance of a delay at 30 %." It is used in planning, but it should be replaced by data when data arrive.

## A worked case by hand

A plant's records show that 18 of the last 600 pumps it installed failed in their first year.

1. **The experimental probability of a first-year failure:** 18 ÷ 600 = **0.03**, or 3 %.
2. **The probability that a pump does not fail:** the pump either fails or it does not, so P(no failure) = 1 − 0.03 = **0.97**. This is the **complement rule**, item 3.4.
3. **In plain words:** about 3 pumps in every 100 installed fail in their first year. For an order of 200 pumps, plan on about 200 × 0.03 = **6** first-year replacements.

## Probability as a long-run proportion

A probability does not say what will happen next time. A fair coin can land heads three times in a row. It says what happens **in the long run**: over many flips, heads comes up close to half the time. The Example flips a coin 1000 times on the computer and draws the proportion of heads after each flip:

[[figure: st.m3.introduction.eg 1]]

After a few flips the proportion (blue) jumps around. As the flips pile up it settles on 0.5, the theoretical probability (red). The horizontal axis is on a log scale, so 10, 100 and 1000 flips are equally spaced and the early jumps can be seen. This settling is the **Law of Large Numbers**, item 3.3.

[[results: st.m3.introduction.eg]]

In 1000 flips the program got 499 heads: an experimental probability of 0.499, 0.001 from the theory.

## In Python and R

```python
import numpy as np
rng = np.random.default_rng(1)
flips = rng.integers(0, 2, size=1000)   # 1 = heads
print(flips.mean())                     # the proportion of heads, close to 0.5
```

```r
set.seed(1)
flips <- sample(0:1, 1000, replace = TRUE)   # 1 = heads
mean(flips)                                  # the proportion of heads, close to 0.5
```

Python and R draw different random numbers from the program's, so their counts differ from 499. They will still be close to 500. The Example's full script:

[[code: st.m3.introduction.eg]]

## Common mistakes

- A probability above 1 or below 0. If you get one, a rule was used wrongly: usually two overlapping events were added (item 3.4).
- Thinking a coin is "due" for heads after a run of tails. Each flip is independent; the coin does not remember. This is the **gambler's fallacy**.
- Reporting an experimental probability from a handful of trials as if it were exact. Three failures in 10 tests is an estimate of 0.3, with a lot of uncertainty.

## What to do next

Look at the **Example** tab, and press **Open in the Workbench** to change the number of trials or the seed. Then answer the questions on the **Exercise** tab.
