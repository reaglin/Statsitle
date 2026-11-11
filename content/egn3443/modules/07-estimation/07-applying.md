---
id: st.m7.applying
title: Applying estimation to real data
minutes: 45
objectives: [st.m7.o3, st.m7.o4, st.m7.o5]
builds-on: st.m7.precision
example: st.m7.applying.eg
exercise: st.m7.applying.ex
reference: [confidence-interval-mean, interpreting-confidence-intervals, margin-of-error-sample-size]
---

# Applying estimation to real data

A real problem arrives as a column of numbers and a question: "Does the battery meet its rated capacity?" This item puts the pieces of the module together in the order an engineer uses them.

## The steps

1. **Name the parameter.** What population, and which number about it? The mean capacity μ of all batteries of this model, or the proportion p that fall below a limit?
2. **Look at the data.** A histogram or box plot (Module 2): any outliers, typing errors, strong skew? Was the sample random, or did it come from one shift, one machine?
3. **Choose the interval.** A mean with σ unknown: t with n − 1 degrees of freedom. σ truly known: z. A proportion: z with p̂. Check the conditions (normal-looking data or n ≥ 30; n·p̂ and n·(1 − p̂) at least 10).
4. **Calculate** the estimate, the standard error, the critical value, the margin of error and the interval.
5. **Interpret in context**, in words a reader can act on, and compare the interval with the claim, target or specification.
6. **Judge the precision.** Is the interval narrow enough for the decision? If not, find the n that would be (item 7.6).

## The Example: battery capacity

This is the course's battery example (Bonus 7-1 in Module 7 on the course website, reaglin.github.io/EGN3443). TechPower Industries makes lithium-ion batteries for electric vehicles and rates its new model at **75 kWh**. Quality control tested 30 randomly selected batteries under standard conditions.

**Step 1.** The parameter is μ, the mean capacity of all batteries of the model. The estimator is x̄.

**Step 2.** Look at the data first:

[[data: st.m7.applying.eg]]

[[figure: st.m7.applying.data 1]]

The histogram is roughly symmetric, with no outliers; capacities run from 73.5 to 76.3 kWh. With n = 30 the CLT applies anyway.

**Step 3.** σ is unknown, so the interval is a t interval with 29 degrees of freedom.

**Step 4.**

[[figure: st.m7.applying.eg 1]]

[[results: st.m7.applying.eg]]

x̄ = 74.863 kWh and s = 0.802 kWh, so the standard error is 0.802 / √30 = 0.1465 kWh. With t_{0.025, 29} = 2.045 the margin of error is 0.300 kWh, and the 95 % interval runs from **74.56 to 75.16 kWh**.

**Step 5.** "We are 95 % confident that the mean capacity of this battery model is between 74.56 and 75.16 kWh." The rated 75 kWh is inside the interval, so the data are consistent with the rating: they do not show that the mean falls short. The sample mean is 0.14 kWh below 75, but a difference that small is within what sampling alone produces.

The specification allows each battery 75 ± 2 kWh. That is a statement about **single batteries**, and the interval is about the **mean**: check it against the data themselves. All 30 batteries are between 73 and 77 kWh.

**Step 6.** The interval is 0.6 kWh wide, about 0.8 % of the rating: precise enough for this decision. How the width moves with the confidence level, from the same 30 batteries:

| Confidence | t (29 df) | Margin of error | Interval (kWh) |
|---|---|---|---|
| 90 % | 1.699 | 0.249 | 74.61 to 75.11 |
| 95 % | 2.045 | 0.300 | 74.56 to 75.16 |
| 99 % | 2.756 | 0.404 | 74.46 to 75.27 |

75 kWh is inside all three.

## A proportion from the same thinking

The same steps work for a proportion. Suppose a warranty paid out for any battery under 74 kWh. The parameter would be p, the share of all batteries below 74 kWh. Five of the thirty are below it (73.5, 73.6, 73.7, 73.8 and 73.9), so p̂ = 5/30 = 0.167. But n·p̂ = 5 is under 10, so the hand formula of item 7.4 is not reliable here: use Wilson's interval, or test more batteries.

## In Python and R

```python
import numpy as np
from scipy import stats
cap = np.array([74.2, 75.8, 73.9, 76.1, 74.5, 75.3, 74.8, 73.5, 75.9, 74.6,
                75.2, 74.1, 76.3, 73.8, 75.5, 74.9, 75.1, 74.3, 75.7, 73.6,
                74.7, 75.4, 74.0, 75.6, 74.4, 75.0, 73.7, 76.0, 74.8, 75.2])
n, xbar, se = len(cap), cap.mean(), cap.std(ddof=1) / np.sqrt(len(cap))
for conf in (0.90, 0.95, 0.99):
    lo, hi = stats.t.interval(conf, n - 1, loc=xbar, scale=se)
    print(conf, round(lo, 2), round(hi, 2))      # 95 %: 74.56 75.16
print(np.mean(np.abs(cap - 75) <= 2))            # share within 75 ± 2 kWh: 1.0
```

```r
cap <- c(74.2, 75.8, 73.9, 76.1, 74.5, 75.3, 74.8, 73.5, 75.9, 74.6,
         75.2, 74.1, 76.3, 73.8, 75.5, 74.9, 75.1, 74.3, 75.7, 73.6,
         74.7, 75.4, 74.0, 75.6, 74.4, 75.0, 73.7, 76.0, 74.8, 75.2)
hist(cap)
t.test(cap, conf.level = 0.95)$conf.int          # 74.56 75.16
mean(abs(cap - 75) <= 2)                         # 1
```

[[code: st.m7.applying.eg]]

## Common mistakes

- Calculating before looking at the data. One typing error (757 for 75.7) can move x̄ and s a long way.
- Comparing a specification for single items with an interval for the mean.
- Saying the interval "proves" the rating. A value inside the interval is plausible, not proved.
- Reporting only the numbers. The reader needs the quantity, the units, the confidence and what it means for the decision.

## What to do next

On the **Exercise** tab, fifteen 3D-printed specimens were tested, and the material's data sheet claims a mean strength of 50 MPa. Build the 99 % interval from the data column, then decide what it says about the claim.
