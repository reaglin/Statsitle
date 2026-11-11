---
id: st.m8.p-values
title: P-values and significance levels
minutes: 45
objectives: [st.m8.o3, st.m8.o4]
builds-on: st.m8.test-statistics
example: st.m8.p-values.eg
exercise: st.m8.p-values.ex
reference: [hypothesis-testing-steps, test-statistics]
---

# P-values and significance levels

## The p-value

The **p-value** is the probability, computed assuming H₀ is true, of a test statistic **at least as extreme** as the one observed, in the direction(s) H₁ points. It is an area under the reference curve:

| H₁ | p-value for a z statistic | Area |
|---|---|---|
| μ > μ₀ (upper) | 1 − Φ(z) | to the right of z |
| μ < μ₀ (lower) | Φ(z) | to the left of z |
| μ ≠ μ₀ (two-sided) | 2 × (1 − Φ(\|z\|)) | both tails beyond ±\|z\| |

For t, the same areas come from the t distribution with its degrees of freedom.

The smaller the p-value, the stronger the evidence against H₀: a small p means the sample would be rare if H₀ were true.

## The significance level α

**α** is the largest p-value at which you will reject H₀. It is chosen **before** the data, from the cost of a false alarm (a Type I error, item 8.6):

| α | When |
|---|---|
| 0.10 | an early screening, where a false alarm costs little and a missed change costs more |
| 0.05 | the usual convention |
| 0.01 | a false alarm is costly: stopping a line, recalling a product |

**The decision rule:** reject H₀ if p < α; otherwise do not reject.

The same decision comes from the **critical value**: reject when the statistic falls in the **rejection region**, the values of the statistic whose tail area is α (z > 1.645 for an upper-tailed z-test at 0.05; |z| > 1.96 two-sided). The p-value says more: not just "reject" but how strongly.

## A worked case by hand: one z, three alternatives

A test gives z = 1.80.

| H₁ | p-value | At α = 0.05 |
|---|---|---|
| μ > μ₀ | 1 − Φ(1.80) = 1 − 0.9641 = **0.0359** | reject |
| μ < μ₀ | Φ(1.80) = **0.9641** | do not reject: the sample points the other way |
| μ ≠ μ₀ | 2 × 0.0359 = **0.0719** | do not reject |

The same number gives three different p-values. The tail is part of the question (item 8.2).

## What a p-value is not

- **Not the probability that H₀ is true.** It is computed assuming H₀ is true.
- **Not the probability the result is due to chance.** Same mistake in other words.
- **Not a measure of how big or how important the effect is.** A tiny effect with a huge n gives a tiny p (item 8.7).
- **p = 0.049 and p = 0.051 are almost the same evidence.** α is a line drawn for a decision; report the p-value itself, not only which side of the line it fell.

## The Example: field failures

Ten percent of a maker's sensors fail in their first year. A customer suspects a new batch is worse: 31 of 250 failed, p̂ = 0.124. H₀: p = 0.10, H₁: p > 0.10. Check first: np₀ = 25 and n(1 − p₀) = 225 are both at least 10, so z is close to normal.

The standard error under H₀ is √(0.10 × 0.90/250) = 0.0190, so z = (0.124 − 0.10)/0.0190 = **1.26**.

[[figure: st.m8.p-values.eg 1]]

The shaded tail is the rejection region, z > 1.645 (its area is α = 0.05). The p-value is a different area: everything to the right of the orange vertical line at the observed z = 1.26, which is **0.1030**. The line falls short of the shaded region.

[[results: st.m8.p-values.eg]]

The p-value is **0.1030**. It is above 0.05, and z is short of the critical value 1.645: do not reject H₀. A 12.4 % failure rate in 250 sensors is not strong evidence that the batch is worse than 10 %; about one batch in ten would look this bad by chance. The customer could collect more data, but on this sample cannot claim the batch is worse.

## In Python and R

```python
import numpy as np
from scipy import stats
x, n, p0 = 31, 250, 0.10
p_hat = x / n
z = (p_hat - p0) / np.sqrt(p0 * (1 - p0) / n)    # 1.26
print(stats.norm.sf(z))                          # upper-tailed p = 0.1030
```

```r
x <- 31; n <- 250; p0 <- 0.10
z <- (x / n - p0) / sqrt(p0 * (1 - p0) / n)
pnorm(z, lower.tail = FALSE)                     # 0.1030
prop.test(x, n, p = p0, alternative = "greater", correct = FALSE)   # the same p
```

`prop.test` reports χ² = z² = 1.6 and the same p-value. Leave `correct = FALSE`, or R adds a continuity correction the course does not use.

[[code: st.m8.p-values.eg]]

## Try it in the Workbench

Press **Open in the Workbench** on the Example tab and change **Successes (x)** from 31 to 36. p̂ = 0.144, z = 2.32 and p = 0.0102: now the batch is significantly worse, at 0.05 and almost at 0.01.

## Common mistakes

- Reading the p-value as the probability that H₀ is true.
- Doubling (or not doubling) the tail area without looking at H₁.
- Using p̂ in the standard error of a proportion test. The test assumes H₀, so it uses p₀.
- Moving α after seeing p.

## What to do next

On the **Exercise** tab, a new die is fitted to a stamping press, and 9 of 400 parts are defective. Find z and the p-value for H₁: p < 0.04. Then compare: is the reduction significant at α = 0.05? At α = 0.01?
