---
id: st.m24.odds
title: Odds as probability
minutes: 40
objectives: [st.m24.o5]
example: st.m24.odds.eg
exercise: st.m24.odds.ex
reference: [odds-probability]
---

# Odds as probability

Sports pages print odds: "2.50", "6/4", "+150". Each is a probability in disguise. This page treats odds **only as probability**: how to read them, why the probabilities they imply do not add up to 1, and what the **expected value** of a stake is. It gives no betting advice. Its conclusion is a statistical fact: for the person placing a bet, the expected value is negative.

## From odds to implied probability

- **Decimal odds** d (common in Europe and Australia): a $1 stake returns $d in total if the outcome happens. Implied probability **1/d**.
- **Fractional odds** a/b (UK): a profit of $a for each $b staked. Decimal d = 1 + a/b, so 6/4 is d = 2.50.
- **American odds**: +150 means a $150 profit on a $100 stake (d = 2.50); −200 means a $200 stake to make $100 (d = 1.50).

Decimal odds of 2.50 imply a probability of 1/2.50 = **0.40**.

## Why the implied probabilities add up to more than 1

A match with two outcomes is offered at decimal odds 1.80 and 2.10:

1/1.80 + 1/2.10 = 0.556 + 0.476 = **1.032**.

True probabilities of all outcomes add to exactly 1. The extra 0.032 (3.2 %) is the bookmaker's **margin** (the "overround" or "vig"): it is built into every price, so that, whatever happens, the bookmaker expects to keep part of the money staked. Dividing each implied probability by 1.032 gives the market's estimate of the true probabilities: 0.538 and 0.462.

## Expected value

A $10 stake at decimal odds 2.50 gains $15 if the outcome happens and loses $10 if not. If the outcome's true probability is 0.38 (illustrative):

[[figure: st.m24.odds.eg 1]]

[[results: st.m24.odds.eg]]

- E[gain] = 15 × 0.38 + (−10) × 0.62 = 5.70 − 6.20 = **−$0.50** per $10 staked: a loss of 5 % on average.
- The standard deviation is **$12.13**: any one stake is a large gain or a total loss; only the average is predictable. Over many stakes the average loss is close to 5 %, which is the margin at work.
- The expected value would be zero only if the true probability were 1/2.50 = 0.40, and the margin makes the offered odds lower than that for every outcome.

[[see-example: 1]]

The same calculation applies to lotteries, raffles and casino games: for the player, the expected value is negative by design. Responsible-gambling organisations publish these figures for the same reason this page does.

## In Python and R

```python
import numpy as np
odds = np.array([1.80, 2.10]); implied = 1 / odds
margin = implied.sum() - 1; fair = implied / implied.sum()
ev = 15 * 0.38 + (-10) * 0.62
```

```r
odds <- c(1.80, 2.10); implied <- 1 / odds
sum(implied) - 1; implied / sum(implied)
15 * 0.38 + (-10) * 0.62
```

[[code: st.m24.odds.eg]]

## Common mistakes

- **Odds read as the chance of losing**: decimal odds 2.50 imply a 40 % chance the outcome happens.
- **Implied probabilities taken as true probabilities**: they include the margin.
- **One lucky win taken as a winning method**: the standard deviation of a stake is far larger than its expected value, so luck dominates short runs.

## What to do next

On the **Exercise** tab, a charity raffle: the expected net gain of a ticket.

[[example: Expected value and the bookmaker's margin]]

The Learn page's stake: gain **$15** with probability 0.38, lose **$10** with probability 0.62, an expected gain of **−$0.50**. Here is the same stake if the true probability were exactly the **0.40** the odds imply, with no margin:

[[figure: st.m24.stake.try 1]]

[[results: st.m24.stake.try]]

The expected value is exactly **0**: a "fair" price. The margin is the gap between this and the −$0.50 above. Work it out for yourself at 0.35 and 0.65: 15 × 0.35 − 10 × 0.65 = **−$1.25** per $10. To change the numbers, press **Open in the Workbench** on the worked example and edit the table of values.
