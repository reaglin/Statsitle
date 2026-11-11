---
id: st.m3.theoretical-experimental
title: Theoretical vs experimental probability
minutes: 45
objectives: [st.m3.o3, st.m3.o4]
builds-on: st.m3.sample-spaces
example: st.m3.theoretical-experimental.eg
exercise: st.m3.theoretical-experimental.ex
reference: [law-of-large-numbers, sample-spaces-and-counting]
---

# Theoretical vs experimental probability

## Two ways to the same number

| | Theoretical | Experimental |
|---|---|---|
| **Where it comes from** | Reasoning about the sample space, before anything is done | Doing the experiment and counting |
| **Formula** | favourable outcomes ÷ possible outcomes (equally likely) | times the event happened ÷ trials |
| **Needs** | A model you trust: a fair die, a well-shuffled deck | Trials, the more the better |
| **Gives** | An exact value: 1/6 | An estimate that changes from run to run: 0.168, then 0.159, … |
| **Engineering use** | Designs, codes, quick checks | Test records, failure logs, field data |

When there is a good model, the theoretical value is exact. When there is not (how often does this new pump fail?), the experimental value is all there is. When there are both, comparing them **checks the model**: a die that shows 6 on 30 % of 1000 rolls is not fair.

## The Law of Large Numbers

As the number of trials grows, the experimental probability gets closer and closer to the theoretical probability.

With a few trials it can be far off. The computer flipped a fair coin 20 times and got 13 heads, an experimental probability of 0.65:

[[figure: st.m3.theoretical-experimental.few 1]]

Item 3.1's 1000 flips gave 0.499. How far off to expect is known: about 95 % of the time the experimental probability is within

2 × √(p(1 − p) / n)

of the theoretical p, where n is the number of trials. For a coin, p = 0.5:

| Trials n | Typically within |
|---|---|
| 20 | ± 0.22 |
| 1 000 | ± 0.032 |
| 100 000 | ± 0.0032 |

The error shrinks like 1/√n: a hundred times the trials gives one tenth of the error. So 13 heads in 20 flips (0.65, 0.15 above 0.5) is no evidence the coin is unfair, but 650 heads in 1000 would be.

The law says nothing about the **next** trial. It works by swamping early runs with many more trials, not by correcting them: after five tails in a row, the next flip is still heads with probability 0.5.

## A worked case by hand

A spinner has four equal sections, one of them red. It is spun 200 times and lands on red 51 times.

1. **Theoretical:** one section of four, P(red) = 1/4 = **0.25**.
2. **Experimental:** 51 / 200 = **0.255**.
3. **How far off to expect:** 2 × √(0.25 × 0.75 / 200) = 2 × 0.0306 = **0.061**. The difference, 0.005, is far inside that: the results agree with a fair spinner.
4. Had red come up 80 times (0.40), the difference 0.15 would be more than twice 0.061. Something is wrong with the spinner, or with the assumption that it is fair.

## The Example: darts and π

Throw darts at random at a square 2 units on a side, with a circle of radius 1 drawn inside it. The circle's area is π × 1² = π and the square's is 2 × 2 = 4, so the theoretical probability that a dart lands inside the circle is π/4 = **0.7854**. Turn it around, and four times the experimental proportion **estimates π**.

[[figure: st.m3.theoretical-experimental.eg 2]]

The computer threw 5000 darts (the figure shows the first 2000). The running proportion settles on π/4:

[[figure: st.m3.theoretical-experimental.eg 1]]

[[results: st.m3.theoretical-experimental.eg]]

3929 of the 5000 darts landed inside: 0.7858, and 4 × 0.7858 = 3.1432 for π. This is the **Monte Carlo method**: when a probability is hard to calculate, simulate the experiment many times and count. Engineers use it for reliability, traffic, structural loads, and anything whose theory is too hard to work out by hand. The course's dart game, *The Oche*, plays the same idea.

## In Python and R

```python
import numpy as np
rng = np.random.default_rng(1)
x = rng.uniform(-1, 1, 5000)
y = rng.uniform(-1, 1, 5000)
inside = x**2 + y**2 <= 1
print(inside.mean(), 4 * inside.mean())   # about 0.785 and 3.14
```

```r
set.seed(1)
x <- runif(5000, -1, 1); y <- runif(5000, -1, 1)
inside <- x^2 + y^2 <= 1
mean(inside); 4 * mean(inside)            # about 0.785 and 3.14
```

Your numbers will differ from the program's 3929, because each program draws its own random numbers. The theoretical value never changes.

[[code: st.m3.theoretical-experimental.eg]]

## Common mistakes

- Treating a small experiment as exact: 3 failures in 10 tests is not "P = 0.3" without a lot of uncertainty.
- Expecting the experimental probability to equal the theory exactly. It almost never does; the question is whether it is within the expected error.
- The gambler's fallacy: the Law of Large Numbers does not make heads "due" after a run of tails.

## What to do next

On the **Exercise** tab, two dice are rolled and the event is a sum of **at least 10**. Work out the theoretical probability by counting, then read the simulation's result in the Workbench.
