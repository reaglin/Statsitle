---
id: st.m24.sabermetrics
title: Sabermetrics and moneyball
minutes: 50
objectives: [st.m24.o1]
example: st.m24.sabermetrics.eg
exercise: st.m24.sabermetrics.ex
reference: [sabermetrics]
---

# Sabermetrics and moneyball

For a century, baseball judged hitters by their **batting average**: hits per at-bat. In the 1970s and 1980s Bill James and others asked a statistical question instead: **which numbers actually go with scoring runs?** Their answer, **sabermetrics**, changed how teams pick players. Michael Lewis's book *Moneyball* (2003) told how the Oakland Athletics, with one of the smallest budgets, used it to field winning teams by paying for what the statistics valued and the market did not.

## Two batting statistics

- **Batting average** (AVG) = hits / at-bats. It ignores walks.
- **On-base percentage** (OBP) = (hits + walks + hit-by-pitch) / (at-bats + walks + hit-by-pitch + sacrifice flies). A walk puts a runner on base as surely as a single.

The test of a statistic is a regression (Module 11): across teams, how well does it predict **runs scored**?

## The Example: thirty teams

One season of a 30-team league (illustrative data, made up for this book):

[[figure: st.m24.sabermetrics.eg 1]]

[[results: st.m24.sabermetrics.eg]]

- Runs per game = −7.01 + 35.3 × OBP: each extra 0.010 of OBP is worth about **0.35 runs a game**, some 57 runs over a 162-game season.
- r = 0.93, **R² = 0.87**: OBP alone accounts for most of the difference in run scoring between teams.
- The exercise fits the same league's runs on **batting average**: compare its R². The moneyball insight was that OBP tracks runs better than AVG, while the market paid for AVG, so high-OBP, low-AVG hitters were cheap.

## Regression to the mean

A player hits .340 in the first half of a season, far above his usual .265. What should we expect in the second half? Lower, and not because he got worse. A first-half average is **talent plus luck**. The players at the top of any list are mostly good *and* lucky, and the luck does not carry over. So the second half **regresses toward the mean**.

This is why a rookie of the year often has a weaker second season (the "sophomore slump"), and why a team that wins a record number of close games usually does not repeat it. Item 11.1's regression shows it as a slope below 1: second half on first half.

[[see-example: 1]]

Real data: Sean Lahman's baseball database and Baseball-Reference.com publish every team's and player's season statistics.

## In Python and R

```python
from scipy import stats
fit = stats.linregress(obp, runs_per_game)
print(fit.slope, fit.rvalue**2)
fit_avg = stats.linregress(avg, runs_per_game)        # compare the R²
```

```r
summary(lm(runs ~ obp))$r.squared
summary(lm(runs ~ avg))$r.squared
# Lahman package: library(Lahman); Teams
```

[[code: st.m24.sabermetrics.eg]]

## Common mistakes

- **A statistic judged by habit** rather than by how well it predicts what matters (runs, wins).
- **Correlation from one season** taken as a law: check other seasons.
- **A hot first half** read as a new level of talent: regression to the mean.
- **Comparing players across eras** without adjusting for how much scoring there was.

## What to do next

On the **Exercise** tab, another season of the same league: regress runs on batting average, and compare its R² with OBP's.

[[example: Regression to the mean: first half and second half]]

Forty players' batting averages in the first and second half of a season. The slope of second half on first half is **0.43**, far below 1, and r = **0.42**: a player who hit **.320** in the first half is predicted to hit about **.280** in the second.

1. **Change the best first half** (the 10th player, .325) to .400, with the same second half. The slope falls to **0.31**: that player's second half stayed ordinary, so a first half this hot predicts little. One hot half is mostly luck, and the line says so.
2. **Watch the prediction at .320.** Move it toward .265 (the league's mean): the predicted second half approaches the mean too.

[[try: st.m24.regression-mean.try "@Halves/First half" PredictAt=0.2..0.4 | Equation Slope Correlation FittedAt figure=1]]
