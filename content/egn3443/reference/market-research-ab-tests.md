---
title: Market research: surveys and A/B tests
---

# Market research: surveys and A/B tests

| Quantity | Formula |
|---|---|
| Survey share | p̂ = x/n |
| Margin of error (95 %) | 1.96 √(p̂(1 − p̂)/n) (item 7.4); largest at p̂ = 0.5: about 1/√n |
| Sample size for margin E | n = 1.96² p(1 − p)/E² (item 7.6); 0.5 × 0.5 when p is unknown |
| Conversion rate | buyers / visitors in a group |
| A/B test | two-proportion z-test of H₀: p_A = p_B (item 10.5) |
| Lift | p_B − p_A (absolute) or (p_B − p_A)/p_A (relative), with its interval |

**Design rules:** randomise the unit (customers, not page views); fix the sample size in advance and test once; one primary
metric; report the interval for the lift and judge whether it is worth having. The margin of error covers only sampling
error, not who chose to answer or how the question was worded.

## Python and R

| Task | Python | R |
|---|---|---|
| Survey interval | `statsmodels.stats.proportion.proportion_confint(222, 600, method="normal")` | `prop.test(222, 600, correct = FALSE)$conf.int` |
| A/B test | `statsmodels.stats.proportion.proportions_ztest([x1, x2], [n1, n2])` | `prop.test(c(x1, x2), c(n1, n2), correct = FALSE)` |
| Sample size per group | `statsmodels.stats.power.NormalIndPower().solve_power(effect_size, alpha=0.05, power=0.8)` | `power.prop.test(p1 = 0.12, p2 = 0.14, power = 0.8)` |

**Read more:** Kohavi, Tang and Xu, *Trustworthy Online Controlled Experiments* (Cambridge University Press, 2020).
