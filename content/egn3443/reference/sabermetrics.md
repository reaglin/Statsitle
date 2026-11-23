---
title: Sabermetrics
---

# Sabermetrics

| Quantity | Formula |
|---|---|
| Batting average | AVG = H/AB |
| On-base percentage | OBP = (H + BB + HBP)/(AB + BB + HBP + SF) |
| Slugging percentage | SLG = total bases/AB |
| Judging a statistic | regress runs (or wins) on it across teams and compare R² (Module 11) |
| Regression to the mean | predicted next = mean + slope × (this − mean), slope < 1 when performance is part luck |

**Real data:** Sean Lahman's baseball database (seanlahman.com; the R package `Lahman`), Baseball-Reference.com, Retrosheet
(play-by-play).

## Python and R

| Task | Python | R |
|---|---|---|
| Compare statistics | `stats.linregress(obp, runs).rvalue**2` | `summary(lm(runs ~ obp))$r.squared` |
| Lahman data | `pybaseball` | `library(Lahman); Teams` |

**Read more:** Lewis, *Moneyball* (Norton, 2003); Albert and Bennett, *Curve Ball: Baseball, Statistics, and the Role of
Chance in the Game* (Springer).
