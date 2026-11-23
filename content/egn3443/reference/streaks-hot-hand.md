---
title: Streaks and the hot hand
---

# Streaks and the hot hand

| Quantity | Formula |
|---|---|
| Makes in n independent shots | binomial(n, p) (item 4.4) |
| After-make vs after-miss | 2 × 2 table, chi-square test of independence (item 8.5) |
| Expected runs (n₁ makes, n₂ misses) | 1 + 2n₁n₂/n |
| Variance of the runs | 2n₁n₂(2n₁n₂ − n)/(n²(n − 1)) |
| Runs test | z = (runs − expected)/SD; too few runs suggests streakiness |

**The streak-selection bias:** in finite sequences, the share of makes after a make is biased below the make rate under
independence; correct for it before concluding there is no hot hand.

## Python and R

| Task | Python | R |
|---|---|---|
| 2 × 2 test | `stats.chi2_contingency(table, correction=False)` | `chisq.test(tab, correct = FALSE)` |
| Runs test | `statsmodels.sandbox.stats.runs.runstest_1samp(x)` | `tseries::runs.test(factor(x))` |

**Read more:** Gilovich, Vallone and Tversky (1985), "The hot hand in basketball", *Cognitive Psychology* 17, 295–314;
Miller and Sanjurjo (2018), "Surprised by the hot hand fallacy? A truth in the law of small numbers", *Econometrica* 86,
2019–2047.
