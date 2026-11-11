---
id: st.m5.uniform
title: The uniform distribution
minutes: 35
objectives: [st.m5.o2, st.m5.o3]
builds-on: st.m5.introduction
example: st.m5.uniform.eg
exercise: st.m5.uniform.ex
reference: [uniform-distribution, probability-density-function]
---

# The uniform distribution

## When it applies

A **uniform** random variable is equally likely to fall anywhere between a smallest value a and a largest value b. Nothing makes one part of the range more likely than another. Engineers use it when:

- a moment is **random within a cycle**: the wait for a signal that changes every 60 s, the position of a rotating part when a sensor reads it;
- an **error is spread evenly**: the error from rounding a reading to the nearest 1 mm is anywhere from −0.5 to +0.5 mm;
- only the **limits are known**: "the delivery comes between 8:00 and 8:30", with no reason to favour any time;
- a **simulation** needs random numbers: every random-number generator starts from the uniform on 0 to 1.

X ~ U(a, b), with two parameters, the limits a and b.

## The density and the CDF

The density is a **rectangle**. Its area must be 1 and its width is b − a, so its height is 1/(b − a):

f(x) = 1/(b − a) for a ≤ x ≤ b, and 0 outside.

A probability is the area of a smaller rectangle: **the width of the interval times the height**.

P(c ≤ X ≤ d) = (d − c)/(b − a), for a ≤ c ≤ d ≤ b

The CDF is a straight line from 0 at a to 1 at b: F(x) = (x − a)/(b − a).

## Mean and variance

μ = (a + b)/2, the middle of the range

σ² = (b − a)²/12, σ = (b − a)/√12

The 12 comes from the integral; you do not need to derive it. Square the width first, then divide by 12.

## A worked case by hand

A distance sensor rounds its reading to the nearest millimeter, so the rounding error E is uniform between a = −0.5 and b = +0.5 mm. The height of the density is 1/(0.5 − (−0.5)) = 1 per mm.

1. **An error within ±0.2 mm:** P(−0.2 ≤ E ≤ 0.2) = (0.2 − (−0.2))/1 = **0.4**.
2. **An error above 0.3 mm:** P(E > 0.3) = (0.5 − 0.3)/1 = **0.2**.
3. **Mean:** μ = (−0.5 + 0.5)/2 = **0 mm**. Rounding does not bias the readings.
4. **Spread:** σ² = 1²/12 = 0.0833 mm², σ = 1/√12 = **0.289 mm**.

That 0.289 is why a reading rounded to the nearest unit is said to carry a "standard uncertainty" of 0.29 units.

## The Example: waiting at a red light

A traffic signal's red phase lasts 60 s. A car that arrives during the red, at a random moment, waits X seconds: uniform between 0 and 60. The shaded area is the probability of waiting between 15 and 45 s:

[[figure: st.m5.uniform.eg 1]]

The CDF is a straight line, because every second of the range adds the same probability:

[[figure: st.m5.uniform.eg 2]]

[[results: st.m5.uniform.eg]]

P(15 ≤ X ≤ 45) = 30/60 = **0.5**, and P(X ≤ 15) = 15/60 = 0.25. The mean wait is 30 s, with a standard deviation of 60/√12 = 17.32 s. The quantile for q = 0.9 is 54 s: nine cars in ten wait 54 s or less.

## In Python and R

```python
from scipy import stats
a, b = 0, 60
X = stats.uniform(loc=a, scale=b - a)   # scipy: loc = a, scale = the width b − a
print(X.cdf(45) - X.cdf(15))            # 0.5
print(X.mean(), X.std())                # 30.0 and 17.32
```

```r
a <- 0; b <- 60
punif(45, a, b) - punif(15, a, b)       # R takes the limits: 0.5
(a + b) / 2; (b - a) / sqrt(12)         # 30 and 17.32
```

**Watch the parameters.** R's `punif(x, min, max)` takes the two limits. scipy's `stats.uniform(loc, scale)` takes the lower limit and the **width**: `stats.uniform(40, 52)` is the uniform from 40 to 92, not 40 to 52.

[[code: st.m5.uniform.eg]]

## Common mistakes

- Reading the height 1/(b − a) as the probability. Multiply it by the width of the interval.
- Forgetting to square the width in the variance: it is (b − a)²/12, not (b − a)/12.
- Giving the variance when the standard deviation is asked.
- Using the uniform when values bunch toward the middle or one end. Then a normal or another shape fits better (item 5.6).
- Giving scipy the upper limit as its `scale`.

## What to do next

On the **Exercise** tab, a batch reactor's cycle time is uniform. Find two probabilities, the mean and the standard deviation, and press **Check my answers**.
