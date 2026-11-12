---
id: st.m10.interpreting
title: Interpreting results and decisions
minutes: 45
objectives: [st.m10.o5]
builds-on: st.m10.proportions
example: st.m10.interpreting.eg
exercise: st.m10.interpreting.ex
reference: [two-sample-decisions, type-i-ii-errors-power, two-sample-t-tests]
---

# Interpreting results and making decisions

**Review and new.** Item 9.6 (*Interpreting and communicating results*) separated statistical from practical significance and showed how to report a test. This page goes on to the **decision**: what the result means for the engineer who has to choose, what each kind of error would cost, how to build a practical threshold into the test, and how to write the conclusion in full.

## Lead with the answer, then the evidence

A reader wants to know first what to do. Start with the conclusion in plain words (is there a difference, which way, how big, and what it means), then give the evidence: the test, the statistic and the exact p-value, the confidence interval, and how the assumptions were checked.

Give p as a number. p = 0.048 and p = 0.002 are both "below 0.05", but the second is far stronger evidence.

## Read the interval, not only the decision

The (1 − α) confidence interval for the difference shows **how big** the difference may be and **how precisely** it is known. Four cases, with a threshold of practical importance in mind:

| The interval | What it says | Decision |
|---|---|---|
| excludes 0, and every value in it matters | a real difference that matters | act |
| excludes 0, but every value in it is too small to matter | a real difference that does not matter (often large samples) | no change needed on these grounds |
| contains 0, and is narrow: every value in it is too small to matter | no difference that matters | no change needed |
| contains 0, but also values that would matter | the data are too few to tell | collect more data before deciding |

The last row is the one a bare "not significant" hides.

## The cost of each error

- A **Type I error** (rejecting a true H₀) means acting on a difference that is not there: buying the new equipment, changing the process, retraining staff, for nothing.
- A **Type II error** (not rejecting a false H₀) means missing a real difference: keeping a worse process, or missing a safety problem.

α is the Type I error rate you accept, so choose it from these costs **before** the data come in. When a false alarm is cheap and a miss is dangerous (a safety screen, an early warning), a larger α such as 0.10 can be justified. When acting is expensive and hard to undo, a smaller α such as 0.01. A small sample keeps the Type II error rate high: then "not rejected" is weak evidence of anything (Module 8, item 8.6).

## Build the threshold into the test

If a change pays only when it improves things by more than some Δ₀, test that directly (item 10.4): H₀: μ₁ − μ₂ = Δ₀ against H₁: μ₁ − μ₂ > Δ₀. "Better" and "better by enough" are different questions with different answers, as the robotic cell of item 10.4 showed (faster, p = 0.0005; more than 3 s faster, p = 0.162).

## The Example: a pilot of a new adhesive

[[results: st.m10.interpreting.eg]]

The return rate fell from 21/300 = 7.0 % with the old adhesive to 12/300 = 4.0 % with the new one. z = 1.61 and p = 0.107: at α = 0.05, H₀ is not rejected.

[[figure: st.m10.interpreting.eg 1]]

The observed z (orange line) lies between the two shaded tails, z(0.025) = ±1.960.

A bare "not significant" would end the project. The interval says otherwise. The 95 % interval for p₁ − p₂ is **−0.0064 to 0.0664**: the new adhesive might raise returns by up to 0.6 percentage points, or cut them by as much as 6.6 points, nearly all of the old 7 %. That is the last row of the table above: the pilot was too small to tell.

**How many sensors would tell?** The margin of the interval is 1.96 × √(p₁(1 − p₁)/n + p₂(1 − p₂)/n) with n sensors per adhesive. To make it ±0.02 with rates near the pilot's 7 % and 4 %:

n = 1.96² × (0.07 × 0.93 + 0.04 × 0.96)/0.02² = 3.8416 × 0.1035/0.0004 = 994 sensors per adhesive.

**The written conclusion:**

> In a six-month pilot, 21 of 300 sensors with the old adhesive (7.0 %) and 12 of 300 with the new adhesive (4.0 %) were returned with a leaking housing. A two-sided two-proportion z-test with the pooled proportion found no significant difference at α = 0.05 (z = 1.61, p = 0.107); the conditions held (at least 16 expected returns in each group). The 95 % confidence interval for the difference in return rates, old − new, is −0.6 to 6.6 percentage points. The pilot therefore cannot tell a new adhesive that changes nothing from one that removes nearly all leak returns. We recommend a larger trial, about 1000 sensors per adhesive, which would estimate the difference within about ±2 points, before deciding.

## In Python and R

```python
import numpy as np
from scipy import stats
x1, n1, x2, n2 = 21, 300, 12, 300
p1, p2, pooled = x1 / n1, x2 / n2, (x1 + x2) / (n1 + n2)
z = (p1 - p2) / np.sqrt(pooled * (1 - pooled) * (1 / n1 + 1 / n2))
print(z, 2 * stats.norm.sf(abs(z)))                                  # z = 1.6116, p = 0.107
se = np.sqrt(p1 * (1 - p1) / n1 + p2 * (1 - p2) / n2)
print(p1 - p2 + np.array([-1, 1]) * stats.norm.ppf(0.975) * se)      # -0.0064 to 0.0664
print(stats.norm.ppf(0.975)**2 * (0.07 * 0.93 + 0.04 * 0.96) / 0.02**2)   # n = 994 per group
```

```r
prop.test(c(21, 12), c(300, 300), correct = FALSE)   # X-squared = 2.5974 (= z^2), p-value = 0.107
                                                     # 95 percent confidence interval: -0.0064 0.0664
```

[[code: st.m10.interpreting.eg]]

## Common mistakes

- **"Not significant, so the new adhesive does not work."** The interval holds large improvements; the study was too small.
- **"Significant, so switch."** Check the size against what the change costs, or test against Δ₀.
- **Choosing α after seeing p.** α comes from the costs of the two errors, set beforehand.
- **Only "p < 0.05" in the report.** Give the exact p, the interval and the size of the difference, in the units the reader decides in.

## What to do next

On the **Exercise** tab, a new coolant pays only if it cools the tool by more than 3 °C. Test against that threshold with Welch's test, and press **Check my answers**.
