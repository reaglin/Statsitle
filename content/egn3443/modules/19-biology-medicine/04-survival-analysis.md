---
id: st.m19.survival
title: Survival analysis
minutes: 55
objectives: [st.m19.o4]
example: st.m19.survival.eg
exercise: st.m19.survival.ex
reference: [survival-analysis]
---

# Survival analysis

How long until a cancer returns, a transplanted kidney fails, a wound heals? **Survival analysis** studies the **time to an event**. Engineers know the same mathematics as **reliability** (item 5.6, the Weibull life of a bearing). Medicine adds one complication that changes everything: **censoring**.

## Censoring

A study ends after five years. Some patients have not relapsed yet; some moved away. We know only that their time to relapse is **longer than** the time they were followed. Such a time is **censored**, written with a "+": 17+ months.

Leaving censored patients out, or treating their time as the event time, makes survival look worse than it is. The methods of survival analysis use what a censored time does say: the patient survived at least that long.

## The survival function and the hazard

- **S(t) = P(T > t)**: the share still free of the event at time t. It starts at 1 and falls.
- The **hazard** h(t): the rate of the event at time t among those still at risk. A constant hazard gives the **exponential** model (item 5.4); a rising or falling one the **Weibull** (item 5.6).

## The Kaplan–Meier estimate

Ten patients after a treatment, months to relapse (+ = censored) (illustrative, made up for this book):

3, 5+, 6, 8, 8, 11+, 12, 15, 17+, 20

At each time an event happens, the share surviving is multiplied by (1 − events / number still at risk):

| Month | At risk | Relapses | Factor | S(t) |
|---|---|---|---|---|
| 3 | 10 | 1 | 9/10 | 0.900 |
| 6 | 8 | 1 | 7/8 | 0.788 |
| 8 | 7 | 2 | 5/7 | 0.563 |
| 12 | 4 | 1 | 3/4 | 0.422 |
| 15 | 3 | 1 | 2/3 | 0.281 |
| 20 | 1 | 1 | 0/1 | 0 |

The censored patients (5+, 11+, 17+) leave the "at risk" count without a relapse. Plotted as steps, this is the **Kaplan–Meier curve**, the figure at the centre of most medical studies of survival. The **median survival** is the first time S(t) falls to 0.5 or below: **12 months**.

Two groups (treatment and control) are compared with the **log-rank test**, and a predictor's effect on the hazard is estimated with **Cox regression**; both are in Part II.

## A parametric model: the Example

When the hazard is constant, the time is exponential. Relapse at a constant **0.05 a month** (illustrative):

[[results: st.m19.survival.eg]]

[[figure: st.m19.survival.eg 1]]

- S(12) = e^(−0.05 × 12) = **0.549**: 55 % are still free of relapse at a year.
- The median is ln 2 / 0.05 = **13.9 months**, and the mean 1/0.05 = 20 months.
- The hazard is **0.05 a month** at every time: a patient who has gone a year without relapse is at the same risk as on the first day.

[[see-example: 1]]

A **Weibull** model lets the hazard change: falling (β < 1, risk highest soon after treatment), constant (β = 1) or rising (β > 1, wear-out, like the bearings of item 5.6).

## In Python and R

```python
# Kaplan–Meier: pip install lifelines
from lifelines import KaplanMeierFitter
times = [3, 5, 6, 8, 8, 11, 12, 15, 17, 20]
events = [1, 0, 1, 1, 1, 0, 1, 1, 0, 1]          # 0 = censored
km = KaplanMeierFitter().fit(times, events); print(km.survival_function_, km.median_survival_time_)
```

```r
library(survival)                                 # comes with R
times <- c(3, 5, 6, 8, 8, 11, 12, 15, 17, 20)
events <- c(1, 0, 1, 1, 1, 0, 1, 1, 0, 1)
fit <- survfit(Surv(times, events) ~ 1); summary(fit); plot(fit)
```

[[code: st.m19.survival.eg]]

## Common mistakes

- **Dropping censored patients**, or counting them as events.
- **The mean of the observed times** as the average survival: with censoring it is too small.
- **The survival at one time** taken for the whole curve: two treatments can cross.
- **Fitting a model that ignores censoring** to censored data. The Workbench's distribution fit uses every value as an event time, so use it only when nothing is censored (as in the exercise).

## What to do next

On the **Exercise** tab, 24 wounds that all healed (no censoring): fit a Weibull to the days to healing, and find the chance a wound is still open at 30 days.

[[example: Survival curves: a rising or falling hazard]]

A Weibull model of the time to relapse with scale **η = 20 months**. It starts at shape **β = 1**: the exponential, with a constant hazard of **0.05 a month** and S(12) = **0.549**.

1. **Move the shape to 0.5.** The hazard now falls with time: risk is highest soon after treatment. More patients relapse early, but those who get past the first months do well.
2. **Move it to 3.** A rising hazard: few relapses early, many later, as with wear-out. The survival curve stays high and then drops steeply around the scale.
3. **Back to 1, and move the time of interest (x) to 40.** S(40) = e^(−2) ≈ 0.135 for the exponential.

[[try: st.m19.hazard.try Shape=0.3..4 Scale=5..40 X=0..60 | MoreThan Median HazardRate figure=1]]
