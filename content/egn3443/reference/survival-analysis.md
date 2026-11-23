---
title: Survival analysis
---

# Survival analysis

| Quantity | Formula |
|---|---|
| Survival function | S(t) = P(T > t) |
| Hazard | h(t) = f(t)/S(t), the event rate among those still at risk |
| Exponential (constant hazard λ) | S(t) = e^(−λt); median ln 2/λ; mean 1/λ (item 5.4) |
| Weibull | S(t) = e^(−(t/η)^β); hazard falls (β < 1), constant (β = 1) or rises (β > 1) (item 5.6) |
| Kaplan–Meier | Ŝ(t) = Π over event times tᵢ ≤ t of (1 − dᵢ/nᵢ), dᵢ events among nᵢ at risk |
| Median survival | the first time Ŝ(t) ≤ 0.5 |

**Censoring:** a time known only to be longer than the follow-up (written t+). It lowers the number at risk without an event.
Comparing groups: the log-rank test; a predictor's effect on the hazard: Cox regression (Part II).

## Python and R

| Task | Python | R |
|---|---|---|
| Kaplan–Meier | `lifelines.KaplanMeierFitter().fit(times, events)` | `survival::survfit(Surv(times, events) ~ 1)` |
| Log-rank test | `lifelines.statistics.logrank_test(...)` | `survival::survdiff(Surv(times, events) ~ group)` |
| Weibull fit | `lifelines.WeibullFitter().fit(times, events)` | `survival::survreg(Surv(times, events) ~ 1, dist = "weibull")` |

**Read more:** Kleinbaum and Klein, *Survival Analysis: A Self-Learning Text* (Springer).
