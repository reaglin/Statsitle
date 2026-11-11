---
id: st.m5.exponential
title: The exponential distribution
minutes: 45
objectives: [st.m5.o2, st.m5.o3]
builds-on: st.m5.normal
example: st.m5.exponential.eg
exercise: st.m5.exponential.ex
reference: [exponential-distribution, probability-density-function]
---

# The exponential distribution

## When it applies

The **exponential** distribution is the model for a **waiting time**: the time until the next random event, or the life of a part that fails **at random**, not from wear. It goes with the Poisson of Module 4:

- the **number** of events in an interval is Poisson with mean λt;
- the **time between** events is exponential with rate λ.

Customers arriving, packets reaching a router, calls to a help desk, failures of electronic parts in their useful life: if the count per hour is Poisson, the gap between them is exponential.

X ~ Exp(λ) has one parameter, the **rate λ**: events per unit of time (or length). X is never negative, and the density is highest at 0 and falls away: short waits are common and long ones rare. The distribution is skewed to the right.

## The density and the CDF

f(x) = λ e^(−λx) for x ≥ 0

The CDF has a closed form, so no table is needed:

P(X ≤ x) = 1 − e^(−λx)  P(X > x) = e^(−λx)

For a lifetime, P(X > x) is the **reliability** R(x): the chance the part is still working at age x.

## Mean, variance and the rate

μ = 1/λ  σ² = 1/λ²  σ = 1/λ

The **mean is one over the rate**. "A mean time between failures of 400 hours" gives λ = 1/400 = 0.0025 failures per hour, and "3 arrivals an hour" gives a mean gap of 1/3 hour, 20 minutes. Mixing up λ and 1/λ is the most common exponential mistake. Check the units: λ is per hour, the mean is in hours.

The median, ln 2/λ = 0.693/λ, is below the mean: the long right tail pulls the mean up. About 63 % of waits are shorter than the mean, because P(X ≤ 1/λ) = 1 − e⁻¹ = 0.632.

## The memoryless property

The exponential has **no memory**. If a part has already survived s hours, the chance it survives t more is the same as for a new part:

P(X > s + t | X > s) = P(X > t) = e^(−λt)

A part whose life is exponential does not wear out: an old one is as good as a new one. That is true of many electronic parts in their useful life, and **not** of bearings, belts or tires, which wear out (the Weibull of item 5.6). The geometric distribution of item 4.6 is memoryless in the same way.

## A worked case by hand

An electronic control module fails at random at a rate of λ = 0.0002 per hour (a mean life of 1/0.0002 = 5000 hours).

1. **Fails within 2000 hours:** P(X ≤ 2000) = 1 − e^(−0.0002 × 2000) = 1 − e^(−0.4) = **0.3297**.
2. **Still working at the mean life:** R(5000) = e^(−0.0002 × 5000) = e^(−1) = **0.3679**. Only 37 % reach the mean.
3. **Memoryless:** a module has run 3000 hours. The chance it runs another 2000 is P(X > 2000) = e^(−0.4) = **0.6703**, the same as for a new one. The 3000 hours do not enter.
4. **The median life:** ln 2/0.0002 = **3466 hours**: half the modules fail before it.
5. **The Poisson view:** the number of failures in 2000 hours is Poisson with mean λt = 0.4, so P(no failure) = e^(−0.4) = 0.6703, the same as R(2000).

## The Example: waiting for the next car at an EV charger

Cars arrive at a highway EV charger at random, on average 3 an hour. The rate is λ = 3/60 = 0.05 per minute, and the mean time between arrivals is 20 minutes. The shaded area is the probability that the next car arrives within 15 minutes:

[[figure: st.m5.exponential.eg 1]]

[[figure: st.m5.exponential.eg 2]]

[[results: st.m5.exponential.eg]]

P(X ≤ 15) = 1 − e^(−0.05 × 15) = **0.5276**, and the chance of waiting more than 15 minutes is e^(−0.75) = 0.4724. The median gap is 13.86 minutes, below the 20-minute mean. The **hazard rate** is the rate at which the event happens at a given moment, among the cases where it has not happened yet (for parts: failures per hour among the parts still running; item 5.6 uses it for parts that wear out). For the exponential it is λ = 0.05 per minute at every moment: the chance of an arrival in the next minute never depends on how long the charger has been idle.

## In Python and R

```python
from scipy import stats
lam = 0.05
X = stats.expon(scale=1 / lam)   # scipy takes the scale, the MEAN 1/λ
print(X.cdf(15))                 # P(X ≤ 15) = 0.5276
print(X.sf(15))                  # P(X > 15) = 0.4724
print(X.median(), X.mean())      # 13.86 and 20
```

```r
lam <- 0.05
pexp(15, rate = lam)             # R takes the RATE λ: 0.5276
1 - pexp(15, rate = lam)         # 0.4724
qexp(0.5, rate = lam)            # the median, 13.86
```

**Watch the parameter.** R's `pexp` takes the rate λ. scipy's `stats.expon` takes `scale = 1/λ`, the mean. `stats.expon(0.05)` would be wrong: it sets the location, not the rate.

[[code: st.m5.exponential.eg]]

## Common mistakes

- Using the mean as the rate. A mean of 400 hours is λ = 1/400 per hour, not 400.
- Not matching the units: a rate per hour with x in minutes.
- Ignoring the memoryless property: counting the hours already run when the question asks about the hours still to come.
- The wrong tail: "fails within x" is 1 − e^(−λx); "survives past x" is e^(−λx).
- Using the exponential for parts that wear out. Their failure rate rises with age: use the Weibull.

## What to do next

On the **Exercise** tab, a booster pump fails at random with a known mean time between failures. Find the rate first, then two probabilities and a percentile, and press **Check my answers**.
