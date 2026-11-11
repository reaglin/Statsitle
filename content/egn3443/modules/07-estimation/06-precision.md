---
id: st.m7.precision
title: Precision and margin of error
minutes: 45
objectives: [st.m7.o4, st.m7.o3]
builds-on: st.m7.interpreting
example: st.m7.precision.eg
exercise: st.m7.precision.ex
reference: [margin-of-error-sample-size, confidence-interval-mean, confidence-interval-proportion]
---

# Precision and margin of error

## The margin of error

The **margin of error** E is the half-width of a confidence interval: the most the estimate is likely to be off, at the stated confidence. For a mean with σ known,

E = z_{α/2} · σ / √n

A small E means a **precise** estimate. Three things set it, and only one of them is in the engineer's hands before the data come in.

## What changes the width

Fill weights on a packaging line have σ = 2.2 g. With n = 25 boxes, σ/√n = 0.44 g:

| Change | Margin of error E | Why |
|---|---|---|
| 95 %, n = 25 (the start) | 1.960 × 0.44 = **0.862 g** | |
| **Confidence** down to 90 % | 1.645 × 0.44 = **0.724 g** | smaller critical value |
| **Confidence** up to 99 % | 2.576 × 0.44 = **1.133 g** | larger critical value |
| **Sample size** up to n = 100 | 1.960 × 2.2 / 10 = **0.431 g** | √n doubles, E halves |
| **Variability** doubled, σ = 4.4 g | 1.960 × 0.88 = **1.725 g** | E is proportional to σ |

1. **Confidence level:** higher confidence, wider interval. Being surer costs precision.
2. **Sample size:** larger n, narrower interval, but with √n. Four times the data halves E; ten times the data cuts it only to about a third of what it was (1/√10 = 0.32).
3. **Variability:** a more variable population gives a wider interval. Better measurement, a steadier process, or grouping like with like (Module 2) all reduce σ.

The same holds for a t interval (E = t·s/√n, where a small n also raises t) and for a proportion (E = z·√(p̂(1 − p̂)/n), largest when p̂ = 0.5).

## Choosing n before sampling

Turn the margin of error around to find the sample size that achieves a chosen E:

**For a mean:** n = (z_{α/2} · σ / E)²

**For a proportion:** n = (z_{α/2} / E)² · p(1 − p)

Always **round up**: rounding down would leave E a little larger than required. You need a value for σ or p before the data exist:

- σ from past data on the process, from a small pilot sample's s, or, roughly, from (largest likely value − smallest) / 4.
- p from earlier studies, or **p = 0.5** when nothing is known. p(1 − p) is largest at 0.5, so that n is safe whatever the true p is.

## A worked case by hand

A road agency wants the mean compressive strength of a new mix within **±4 MPa**, at **90 %** confidence. Earlier mixes had σ ≈ 12 MPa.

1. z_{0.05} = 1.645 (90 % puts 5 % in each tail).
2. n = (1.645 × 12 / 4)² = (4.935)² = 24.35.
3. Round **up**: **n = 25** cylinders.

To halve the margin to ±2 MPa: n = (1.645 × 12 / 2)² = 97.40, so 98 cylinders. Halving E needs about four times the data.

## The Example: fill weight within ±0.5 g

A plant wants to estimate the mean fill weight of a cereal line within ±0.5 g at 95 % confidence. Past data give σ = 2.2 g.

[[results: st.m7.precision.eg]]

n = (1.960 × 2.2 / 0.5)² = (8.624)² = 74.37, rounded up to **75 boxes**.

## Another example: a proportion within ±3 points

A city wants the share of residents who would use a new bike lane within ±0.03 at 95 % confidence, with no earlier estimate, so p = 0.5:

[[results: st.m7.precision.proportion]]

n = (1.960 / 0.03)² × 0.5 × 0.5 = 1067.07, so **1068 people**. This is why opinion polls of about a thousand people quote "±3 percentage points". Had an earlier study shown p near 0.1, n = (1.960 / 0.03)² × 0.1 × 0.9 = 384.15, so 385 would do.

## In Python and R

```python
import math
from scipy import stats
z = stats.norm.ppf(0.975)                              # 1.960
print(math.ceil((z * 2.2 / 0.5) ** 2))                 # mean: 75
print(math.ceil((z / 0.03) ** 2 * 0.5 * 0.5))          # proportion: 1068
```

```r
z <- qnorm(0.975)
ceiling((z * 2.2 / 0.5)^2)                             # 75
ceiling((z / 0.03)^2 * 0.5 * 0.5)                      # 1068
```

`math.ceil` and `ceiling` round up. `round` would give 74 for the first one, which is not enough.

[[code: st.m7.precision.eg]]

The Workbench's **Sample size** uses z, as the course's formula does. When σ will be estimated by s and the answer is small (under about 30), the t interval from those n observations will come out a little wider than E; add a few observations to be safe.

## Common mistakes

- Rounding n down, or to the nearest whole number. Always round up.
- Forgetting to square: n = z·σ/E is far too small.
- Using the 95 % z (1.960) for a 99 % or 90 % requirement.
- Mixing units: E and σ must be in the same units (both g, both mm), and E for a proportion is a fraction (0.03, not 3).
- Expecting twice the data to halve the margin. It takes four times.

## What to do next

On the **Exercise** tab, a pavement engineer needs the mean deflection of a road section within a given margin at 99 % confidence. Find the critical value and the number of test points.
