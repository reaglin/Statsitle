---
title: Survey sampling and polling
---

# Survey sampling and polling

| Quantity | Formula |
|---|---|
| Share | p̂ = x/n |
| Margin of error (95 %) | 1.96 √(p̂(1 − p̂)/n) (item 7.4); at most about 1/√n |
| Sample size for margin E | n = 1.96² p(1 − p)/E² (item 7.6) |
| Post-stratification weight | (group's share of the population) / (group's share of the sample) |
| Weighted share | Σ (population share of group × group's share in favour) |
| Design effect of weighting | ≈ 1 + CV² of the weights: the margin of error grows by its square root |

**Not in the margin of error:** coverage error, nonresponse bias, question wording and order, who counts as a likely voter.

## Python and R

| Task | Python | R |
|---|---|---|
| Interval | `statsmodels.stats.proportion.proportion_confint(x, n)` | `prop.test(x, n, correct = FALSE)$conf.int` |
| Weighted estimate | `np.average(y, weights=w)` | `survey::svymean(~y, svydesign(ids = ~1, weights = ~w, data = d))` |

**Read more:** Groves et al., *Survey Methodology* (Wiley); the AAPOR's guidance on reporting polls (aapor.org).
