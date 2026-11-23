---
id: st.m24.winprob
title: Ratings and win probability
minutes: 50
objectives: [st.m24.o3]
example: st.m24.winprob.eg
exercise: st.m24.winprob.ex
reference: [ratings-win-probability]
---

# Ratings and win probability

"The home team has a 74 % chance of winning." Television graphics now show such numbers live. Behind them are two statistical ideas: a **rating** for each team or player that turns into a probability of winning, and a **model of the game state** that updates that probability as the score and the clock change.

## Elo ratings

The **Elo system**, devised by the physicist Arpad Elo for chess, gives each player a number. Player A's **expected score** against B (a win counts 1, a draw ½) is

E_A = 1 / (1 + 10^((R_B − R_A)/400)).

A 100-point advantage gives E = 1/(1 + 10^(−0.25)) = **0.64**; 200 points, 0.76; equal ratings, 0.50. After the game, each rating moves toward the result:

R_A(new) = R_A + K (S_A − E_A),

with S_A the actual score (1, ½ or 0) and K a step size (20 or so). A 1,600 player who beats a 1,500 player gains 20 × (1 − 0.64) = **7.2 points**; had she lost, she would lose 20 × 0.64 = 12.8. An upset moves the ratings more than an expected result. Elo-style ratings are used in chess, football (soccer) world rankings and many sports websites.

## Win probability from the game state

During a game, the final margin is the current margin plus what happens in the time left. A simple model treats the rest of the game as random noise:

final margin ~ Normal(mean = current lead (+ any expected edge), SD = σ √(time left)),

and the **win probability** is P(final margin > 0) (item 5.3). The spread shrinks as the clock runs down, so the same lead is worth more later in the game.

## The Example: a 6-point lead with 8 minutes left

A basketball team leads by 6 points with 8 minutes left (illustrative: σ = 2.2 points per √minute, so SD = 2.2 × √8 = 6.22 points).

[[figure: st.m24.winprob.eg 1]]

[[results: st.m24.winprob.eg]]

- z = (0 − 6)/6.22 = −0.96; P(margin > 0) = **0.83**: the leading team wins about 5 times in 6.
- With 2 minutes left the SD is 2.2 × √2 = 3.1, and the same 6-point lead is worth about 97 %.

Real win-probability models (used by broadcasters and teams) are **logistic regressions** fitted to thousands of past games, with the score, time, possession and the teams' ratings as predictors (Part II). The normal model shows the idea.

[[see-example: 1]]

## Winning a series

A team that wins each game with probability p wins a **best-of-seven** series if it wins 4 games before the other does. That is the same as winning at least 4 of 7 games if all seven were played (item 4.4): P(X ≥ 4) with X binomial(7, p). For p = 0.6, the series is won with probability **0.71**: a longer series favours the better team more than a single game does.

## In Python and R

```python
from scipy import stats
e_a = 1 / (1 + 10 ** ((1500 - 1600) / 400))           # Elo expected score
win = stats.norm.sf(0, loc=6, scale=2.2 * 8**0.5)      # win probability
series = stats.binom.sf(3, 7, 0.6)                     # P(at least 4 of 7)
```

```r
1 / (1 + 10^((1500 - 1600) / 400))
pnorm(0, mean = 6, sd = 2.2 * sqrt(8), lower.tail = FALSE)
pbinom(3, 7, 0.6, lower.tail = FALSE)
```

[[code: st.m24.winprob.eg]]

## Common mistakes

- **A win probability read as a prediction of the result.** 83 % means the trailing team wins about one time in six.
- **The same σ for every sport**, or ignoring that the spread shrinks with time.
- **Series probability as the game probability**: a 60 % team wins a seven-game series 71 % of the time.

## What to do next

On the **Exercise** tab, an American-football team trailing by 3 with 12 minutes left: its win probability.

[[example: Winning a best-of-seven series]]

A team that wins each game with probability **0.6**: it wins at least 4 of 7, and so the series, with probability **0.71**.

1. **Move p to 0.5**: an even series, 0.50. **0.55**: about 0.61, a small edge in each game grows in a series.
2. **Back to 0.6, and set the trials to 1** (a single game, k = 1): 0.60. Then **3 trials, k = 2** (best of three): about 0.65. Longer series favour the better team.

[[try: st.m24.series.try Trials SuccessProbability=0.3..0.9 K | AtLeast Exactly Mean figure=1]]
