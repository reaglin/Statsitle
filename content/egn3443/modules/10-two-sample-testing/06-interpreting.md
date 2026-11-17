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

The (1 − α) confidence interval for the difference shows **how big** the difference may be and **how precisely** it is known. Five cases, with a threshold of practical importance in mind:

| The interval | What it says | Decision |
|---|---|---|
| excludes 0, and every value in it matters | a real difference that matters | act |
| excludes 0, but every value in it is too small to matter | a real difference that does not matter (often large samples) | no change needed on these grounds |
| excludes 0, but holds values on both sides of the threshold | a real difference, possibly too small to matter | test against Δ₀ (item 10.4), or collect more data |
| contains 0, and is narrow: every value in it is too small to matter | no difference that matters | no change needed |
| contains 0, but also values that would matter | the data are too few to tell | collect more data before deciding |

The last row is the one a bare "not significant" hides; the third is the one a bare "significant" hides (the robotic cell of item 10.4).

[[see-example: 1]]

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

> The pilot cannot yet tell whether the new adhesive helps: it may change nothing, or remove nearly all leak returns. We recommend a larger trial, about 1000 sensors per adhesive, before deciding. In a six-month pilot, 21 of 300 sensors with the old adhesive (7.0 %) and 12 of 300 with the new adhesive (4.0 %) were returned with a leaking housing. A two-sided two-proportion z-test with the pooled proportion found no significant difference at α = 0.05 (z = 1.61, p = 0.107); the conditions held (at least 16 expected returns in each group). The 95 % confidence interval for the difference in return rates, old − new, is −0.6 to 6.6 percentage points; the larger trial would estimate the difference within about ±2 points.

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

[[example: Two impellers: the interval against a threshold]]

Ten pumps with a new impeller average **78.6 %** efficiency (s = 1.4), ten with the old one **77.0 %** (s = 1.6). Retooling pays only if the new impeller gains more than **1.0 percentage point**. It starts with the 95 % Welch interval for μ_new − μ_old: **0.19** to **3.01** points. It excludes 0, but holds values on both sides of 1.0: the third row of the table, a real gain that may be too small to pay.

1. **Move both sizes to 60.** The interval narrows to **1.06** to **2.14**: every value in it is over 1.0. The first row: act.
2. **Keep 60 pumps each, and move the new impeller's mean to 77.4.** The interval is **−0.14** to **0.94**: it holds 0, and it is narrow enough that every value in it is under 1.0. The fourth row: no gain worth retooling for.
3. **Put the mean back to 78.6, and move both sizes to 6.** The interval is **−0.34** to **3.54**: it holds 0 and also gains of over 3 points. The last row: too few pumps to tell, so collect more before deciding.
4. **Back to 10 pumps each, and change the confidence to 0.99.** The same estimate, 1.6, with a wider interval, **−0.34** to **3.54**, almost exactly the interval of six pumps at 95 %: higher confidence costs width, as fewer pumps do.

[[try: st.m10.impellers.try SampleMean=75..82 SampleSize=2..80 Sample2Size=2..80 Confidence=0.80..0.99 | Estimate MarginOfError Lower Upper figure=1]]
