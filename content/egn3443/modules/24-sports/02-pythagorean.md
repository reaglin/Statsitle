---
id: st.m24.pythagorean
title: The Pythagorean expectation
minutes: 45
objectives: [st.m24.o2]
example: st.m24.pythagorean.eg
exercise: st.m24.pythagorean.ex
reference: [pythagorean-expectation]
---

# The Pythagorean expectation

A team that scores many more runs than it allows should win more games. Bill James put a number on it, the **Pythagorean expectation**:

expected win share = RS^k / (RS^k + RA^k)

with RS the runs scored, RA the runs allowed and k an exponent. James used k = 2 (hence "Pythagorean"); for baseball, about 1.83 fits better. Other sports have their own k: basketball's is near 14, because a basketball game has so many scoring plays that the better team wins more reliably.

## Fitting the exponent with a regression

The formula looks nothing like a straight line, but take logs. Win share W/(W + L) = RS^k/(RS^k + RA^k) is the same as

W/L = (RS/RA)^k, so **ln(W/L) = k · ln(RS/RA)**.

A regression of ln(W/L) on ln(RS/RA) (Module 11) has the exponent k as its **slope**, and an intercept near 0.

## The Example: a baseball league

One 162-game season of 30 teams (illustrative data, made up for this book):

[[data: st.m24.pythagorean.eg]]

[[figure: st.m24.pythagorean.eg 1]]

[[results: st.m24.pythagorean.eg]]

- The fitted exponent is **k = 1.89** (95 % interval 1.47 to 2.31), consistent with the 1.83 used for real baseball.
- The intercept, −0.03, is close to 0, as the formula says it should be.
- R² = 0.75: the log of the ratio of runs scored to runs allowed explains most of the variation in winning.

[[see-example: 1]]

## Luck is the residual

The formula says how many games a team *should* have won from its runs. The difference between its actual and expected wins is, to a statistician, the residual; to a manager, **luck**: mostly the result of close games, which a team wins about half the time whatever its quality. A team that outperforms its Pythagorean record by 8 wins rarely does it again the next season (item 24.1's regression to the mean). Front offices use the expected record, not the actual one, to judge how good a team really is.

The residual standard error here, 0.158 in ln(W/L), is about **6 wins** over 162 games for a .500 team: the luck in one season.

## In Python and R

```python
import numpy as np
from scipy import stats
fit = stats.linregress(np.log(rs/ra), np.log(w/l))
k = fit.slope
expected_w = (rs**k / (rs**k + ra**k)) * 162
luck = w - expected_w
```

```r
fit <- lm(log(w/l) ~ log(rs/ra)); k <- coef(fit)[2]
expected_w <- rs^k / (rs^k + ra^k) * 162
w - expected_w                          # luck, in wins
```

[[code: st.m24.pythagorean.eg]]

## Common mistakes

- **Regressing the win share** directly on RS/RA: the relation is curved; the log form is straight.
- **Luck read as skill**: a team that wins its close games is usually not "clutch", just lucky this year.
- **One exponent for every sport.**

## What to do next

On the **Exercise** tab, a basketball league: fit the exponent, and see why it is so much larger than baseball's.

[[example: Pythagoras and luck: one team's close games]]

The thirty teams: **k = 1.89**, intercept −0.03, R² = **0.75**.

1. **A luckier season.** Take the first team (ln(W/L) = −0.4992, a bad team that was also below its line) and raise its ln(W/L) by 0.25, to −0.2492: about 10 more wins, as if it had won most of its close games. Its point moves up toward the line, so the fit even improves a little (R² **0.76**), and the exponent moves only to **1.80**. One team's luck hardly changes the league's k.
2. **Put it back**, and lower a team that is above the line by 0.25: its good luck is taken away and its point moves onto the line. Over 30 teams the lucky and unlucky seasons roughly cancel, which is why the fitted k is still sensible.

[[try: st.m24.luck.try "@League/ln(W/L)" | Equation Slope Intercept RSquared ResidualSe Influential figure=1]]
