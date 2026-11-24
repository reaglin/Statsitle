---
id: st.m24.hothand
title: Streaks and the hot hand
minutes: 50
objectives: [st.m24.o4]
example: st.m24.hothand.eg
exercise: st.m24.hothand.ex
reference: [streaks-hot-hand]
---

# Streaks and the hot hand

Players, coaches and fans believe in the **hot hand**: a shooter who has made a few in a row is more likely to make the next. In 1985 the psychologists Gilovich, Vallone and Tversky tested it on basketball shots and found no evidence: the streaks were what chance alone produces. For thirty years that was the textbook story of a "cognitive illusion". In 2018 Miller and Sanjurjo showed that the original analysis had a subtle bias, and that, corrected, the data do show a modest hot hand. The debate is a lesson in how much chance produces, and in how carefully it must be measured.

## What chance alone produces

A 50 % shooter takes 20 shots, each independent of the others (no hot hand). The number made is binomial (item 4.4): on average 10, but games with 14 or more makes happen about **6 % of the time**. A long run of makes inside one game is even more likely than people expect. Streaks are evidence of a hot hand only if there are **more** of them than chance gives.

## Test 1: after a make and after a miss

Classify each shot by the result of the shot before it, and compare the make rates: a 2 × 2 table and the chi-square test of independence (item 8.5).

The Example: 400 shots by one player (illustrative data, made up for this book).

| | Made | Missed |
|---|---|---|
| After a make | 104 | 96 |
| After a miss | 96 | 104 |

[[results: st.m24.hothand.eg]]

[[figure: st.m24.hothand.eg 1]]

After a make, 52 % went in; after a miss, 48 %. χ² = 0.64, p = **0.42**: a difference this size is easily chance. On these data, no evidence of a hot hand.

[[see-example: 1]]

## Test 2: the runs test

A **run** is a stretch of makes or of misses. Too **few** runs means makes cluster together (streaky); about the expected number means no pattern. For n₁ makes and n₂ misses (n = n₁ + n₂), under independence

expected runs = 1 + 2n₁n₂/n, Var = 2n₁n₂(2n₁n₂ − n)/(n²(n − 1)).

Twenty shots, H = make, M = miss: **HHH MM H MMM HH M HHH MM H MM**: 10 makes, 10 misses, **10 runs**. Expected 1 + 2 × 10 × 10/20 = 11, SD = √4.74 = 2.18, z = (10 − 11)/2.18 = −0.46: no evidence of streakiness. (This is the Wald–Wolfowitz runs test.)

## The 2018 correction

Here is the trap. In a **finite** sequence of shots by a player with no hot hand at all, the share of makes that follow a make is, on average, **below** the player's make rate. (In 4 shots of a 50 % shooter, the expected share of makes after a make, averaged over the 14 of the 16 equally likely sequences in which some shot follows a make, is about 40 %, not 50 %.) The original studies compared the make rate after makes with the rate after misses and found them about equal, and concluded "no hot hand". But since chance alone would put the after-make rate *lower*, finding them equal is evidence that shooters do get a little hotter after makes. Miller and Sanjurjo re-analysed the original data with this correction and found a hot hand of meaningful size (they compared shooting after three makes with shooting after three misses).

The lessons are general: (1) know what chance produces before you claim a pattern, and (2) check that your estimate is unbiased, even when it looks like a simple proportion.

## In Python and R

```python
from scipy import stats
chi2, p, dof, exp = stats.chi2_contingency([[104, 96], [96, 104]], correction=False)
stats.binom.sf(13, 20, 0.5)                      # P(14 or more of 20)
# runs test: statsmodels.sandbox.stats.runs.runstest_1samp
```

```r
chisq.test(matrix(c(104, 96, 96, 104), nrow = 2, byrow = TRUE), correct = FALSE)
pbinom(13, 20, 0.5, lower.tail = FALSE)
# runs test: tseries::runs.test(factor(shots))
```

[[code: st.m24.hothand.eg]]

## Common mistakes

- **A streak taken as proof**: chance produces streaks.
- **Selecting the streaks after the fact** ("he made 6 in a row!") and ignoring the misses around them.
- **The naive after-make rate** in short sequences, which is biased downward.
- **Shot difficulty ignored**: a "hot" player may take harder shots, which hides a hot hand.

## What to do next

On the **Exercise** tab, 600 free throws classified by the previous attempt: test for a hot hand.

[[example: How many makes in 20 shots, by chance alone?]]

A 50 % shooter taking 20 independent shots. With k = 14, P(at least 14 makes) = **0.058**: about one game in 17, with no hot hand at all.

1. **Set k to 15**: about 0.02. **k = 10**: P(exactly 10) is only about 0.18; most games are not exactly average.
2. **Move p to 0.45** (a slightly worse shooter) with k = 14: such games become rarer, but still happen.

[[try: st.m24.streak.try K SuccessProbability=0.3..0.7 Trials | Exactly AtLeast Mean StandardDeviation figure=1]]
