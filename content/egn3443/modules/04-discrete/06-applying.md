---
id: st.m4.applying
title: Applying discrete distributions
minutes: 45
objectives: [st.m4.o3, st.m4.o4, st.m4.o5]
builds-on: st.m4.poisson
example: st.m4.applying.eg
quiz: quizzes/m4-applying.json
reference: [which-distribution, binomial-distribution, poisson-distribution]
---

# Applying discrete distributions

A real problem does not say "use the binomial". You read the situation, decide what X counts, check the conditions, and pick the model. Then you calculate, and then you say what the number means for the decision at hand. This item adds four more models to the binomial and Poisson, so you can recognize all six. The course's *Distribution Detective* game, in Module 4 on the course website (reaglin.github.io/EGN3443), uses the same six.

## Six models and how to tell them apart

| Model | What X counts | Clue in the words | Parameters | Mean |
|---|---|---|---|---|
| **Bernoulli** | 1 if one trial succeeds, 0 if not | "one", "a single" test, part or attempt | p | p |
| **Binomial** | successes in n independent trials | a fixed number n, the same p for each | n, p | np |
| **Poisson** | events in an interval of time or space | "per hour", "per km", "on average λ", no upper limit | λ | λ |
| **Geometric** | trials up to and including the **first** success | "until the first", "how many tries" | p | 1/p |
| **Pascal** (negative binomial) | trials up to and including the **r-th** success | "until the 3rd", "need r good ones" | r, p | r/p |
| **Hypergeometric** | successes in n draws **without replacement** from a lot of N with K successes | a small lot, "drawn without replacement", "a lot of 50 holds 6 bad" | N, K, n | nK/N |

Three questions sort most problems:

1. **Is there a fixed number of trials?** Yes: binomial (or hypergeometric, or Bernoulli when n = 1). No, events in an interval: Poisson. No, you keep going until something happens: geometric or Pascal.
2. **Does p stay the same from trial to trial?** Drawing from a small lot without putting items back changes p each time: hypergeometric.
3. **Waiting for one success, or for r?** One: geometric. r: Pascal.

## The geometric and Pascal distributions

Geometric: P(X = x) = (1 − p)^(x − 1) · p, for x = 1, 2, 3, … The first x − 1 trials fail, then one succeeds.

A test rig passes a part with probability p = 0.2. The number of parts tested until the first pass has P(X = 3) = 0.8² × 0.2 = **0.128**. The chance of a pass within three tries is P(X ≤ 3) = 1 − 0.8³ = **0.488**, and on average it takes 1/p = **5** tries. The geometric is **memoryless**: after four failures, the chance that the next try succeeds is still 0.2.

Pascal: P(X = x) = C(x − 1, r − 1) · p^r · (1 − p)^(x − r), for x = r, r + 1, … The last trial is the r-th success, and the other r − 1 successes fall anywhere in the first x − 1 trials. With r = 2 and p = 0.5, P(X = 4) = C(3, 1) × 0.5² × 0.5² = 3 × 0.0625 = **0.1875**, and the mean is r/p = **4** trials.

## The hypergeometric distribution

P(X = x) = C(K, x) · C(N − K, n − x) / C(N, n)

Choose x of the K defectives and n − x of the N − K good items, out of all the ways to choose n from N. A box of N = 10 fuses holds K = 3 blown ones. Draw n = 2 without replacement: P(no blown fuse) = C(3, 0) × C(7, 2) / C(10, 2) = 21 / 45 = **0.4667**.

When the sample is small next to the lot (n under about a tenth of N), removing items hardly changes p, and the binomial with p = K/N is close. For N = 100, K = 5, n = 10, the hypergeometric gives P(X = 0) = 0.5838 and the binomial 0.95¹⁰ = 0.5987.

## The Example: an acceptance-sampling plan

A receiving inspector takes n = 8 parts, without replacement, from each lot of N = 40, and accepts the lot if none of the eight is defective. How well does this plan catch a lot with K = 4 defective parts (10 %)?

[[figure: st.m4.applying.eg 1]]

[[results: st.m4.applying.eg]]

P(X = 0) = 0.3935: a lot with 10 % defective parts is **accepted about 39 % of the time**. The plan misses bad lots too often. The binomial with p = 0.1 would give 0.9⁸ = 0.4305; here the sample is a fifth of the lot, so the hypergeometric is the right model, and the two differ.

**Interpreting the result** is the last step, and the one the reader cares about. A good statement names the probability, says what it means in the situation, and draws the conclusion: "With this plan, about 4 of every 10 lots that are 10 % defective would be accepted. To reject such lots at least 90 % of the time, the inspector must sample more parts."

## Another example: retrying a radio link

A sensor sends its reading over a radio link. Each attempt gets through with probability 0.8, independently, and the sensor retries until one does. X, the attempt that first gets through, is geometric with p = 0.8.

[[figure: st.m4.applying.geometric 1]]

[[results: st.m4.applying.geometric]]

The reading gets through within three attempts with probability P(X ≤ 3) = 1 − 0.2³ = 0.992, and takes 1/0.8 = 1.25 attempts on average.

## In Python and R

```python
from scipy import stats
print(stats.hypergeom(40, 4, 8).pmf(0))   # (N, K, n): P(X = 0) = 0.3935
print(stats.geom(0.8).cdf(3))             # trials to first success: P(X ≤ 3) = 0.992
print(stats.nbinom(2, 0.5).pmf(4 - 2))    # Pascal: scipy counts failures, so x − r: 0.1875
```

```r
dhyper(0, 4, 40 - 4, 8)   # (x, K, N − K, n): 0.3935
pgeom(3 - 1, 0.8)         # R counts failures before the first success: 0.992
dnbinom(4 - 2, 2, 0.5)    # failures before the r-th success: 0.1875
```

**Watch the counting convention.** scipy's `geom` counts trials, like the course. R's `dgeom` and `pgeom`, and both languages' negative binomial, count **failures**, so subtract 1 (or r) from the number of trials.

[[code: st.m4.applying.eg]]

## Common mistakes

- Choosing the binomial for "test until the first failure". There is no fixed n: it is geometric.
- Choosing the binomial for a small lot sampled without replacement. p changes with each draw: hypergeometric.
- Choosing the Poisson when there is a fixed number of trials with a fair-sized p.
- Stopping at the number. "P = 0.39" helps nobody until you say it means "39 % of bad lots get through".

## What to do next

The **Exercise** tab has ten questions. Most are short cases: decide which model fits each one. The last ones ask for a calculation and an interpretation.
