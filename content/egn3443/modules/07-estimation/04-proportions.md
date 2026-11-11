---
id: st.m7.proportions
title: Confidence intervals for proportions
minutes: 45
objectives: [st.m7.o3]
builds-on: st.m7.means
example: st.m7.proportions.eg
exercise: st.m7.proportions.ex
reference: [confidence-interval-proportion]
---

# Confidence intervals for proportions

## The estimate and its standard error

Many engineering questions are about a fraction: the share of welds with porosity, of chips that fail burn-in, of drivers who use a new ramp. The parameter is the population proportion p. Its point estimate is the sample proportion

p̂ = x / n

where x is the number of items in the sample with the attribute (the "successes", even when they are defects). From item 7.2, p̂ is approximately normal with mean p and standard error √(p(1 − p)/n). p is unknown, so the interval uses p̂ in its place:

SE = √(p̂(1 − p̂) / n)

## The interval

p̂ ± z_{α/2} · √(p̂(1 − p̂) / n)

A proportion interval always uses **z**, never t. There is no separate s to estimate: the spread of 0s and 1s is fixed by p̂ itself.

**When it works.** The normal approximation needs enough successes and failures. Check that

n·p̂ ≥ 10 and n·(1 − p̂) ≥ 10

and that the sample is random, with independent items. With fewer than 10 successes or failures, use Wilson's interval (below) or an exact binomial method.

## A worked case by hand

A lab calibrated 150 new pressure sensors, and 12 failed calibration. Find the 95 % confidence interval for the proportion p of all such sensors that fail.

1. **Estimate:** p̂ = 12 / 150 = **0.080**.
2. **Check:** n·p̂ = 12 and n·(1 − p̂) = 138 are both at least 10.
3. **Standard error:** √(0.08 × 0.92 / 150) = √0.000491 = **0.02215**.
4. **Margin of error:** 1.960 × 0.02215 = **0.0434**.
5. **Interval:** 0.080 ± 0.0434, from **0.0366 to 0.1234**.

"We are 95 % confident that between 3.7 % and 12.3 % of these sensors fail calibration." A range that wide comes from only 150 sensors; item 7.6 shows how many would make it narrower.

## The Example: porosity in welds

An inspector X-rayed 400 welds from a production line, and 26 showed porosity.

[[figure: st.m7.proportions.eg 1]]

[[results: st.m7.proportions.eg]]

p̂ = 26 / 400 = 0.065, and n·p̂ = 26 is well above 10. The standard error is √(0.065 × 0.935 / 400) = 0.0123, the margin of error 1.960 × 0.0123 = 0.0242, and the 95 % interval runs from 0.0408 to 0.0892: between about 4 % and 9 % of the line's welds have porosity.

## Wilson's score interval

The interval above (often called the **Wald** interval) is the course's hand formula. It is too narrow when n is small or p̂ is near 0 or 1, and its lower end can even go below 0. **Wilson's score interval** fixes that by solving the z-inequality for p instead of putting p̂ in the standard error. The Workbench shows it below the Wald limits. For the welds it runs from 0.0447 to 0.0935: shifted a little toward 0.5, about as wide. With large samples the two agree; with small ones, prefer Wilson.

## In Python and R

```python
import numpy as np
from scipy import stats
x, n = 26, 400
p_hat = x / n                                   # 0.065
se = np.sqrt(p_hat * (1 - p_hat) / n)           # 0.0123
z = stats.norm.ppf(0.975)                       # 1.960
print(p_hat - z * se, p_hat + z * se)           # 0.0408 0.0892
```

```r
x <- 26; n <- 400
p_hat <- x / n
se <- sqrt(p_hat * (1 - p_hat) / n)
p_hat + c(-1, 1) * qnorm(0.975) * se            # 0.0408 0.0892
prop.test(x, n, correct = FALSE)$conf.int       # Wilson: 0.0447 0.0935
```

R's `prop.test` gives Wilson's interval, and by default with a continuity correction, which widens it a little more; `correct = FALSE` gives the plain Wilson interval. In Python, `statsmodels`' `proportion_confint(x, n, method="wilson")` gives it.

[[code: st.m7.proportions.eg]]

## Common mistakes

- Forgetting to divide by n inside the square root, or forgetting the square root.
- Using t instead of z. Proportions use z.
- Typing the count x where p̂ is needed.
- Using the interval with very few successes, such as 3 defects in 200. Check n·p̂ ≥ 10 first.
- Reporting a proportion interval as percentages without saying so: write 0.041 to 0.089, or 4.1 % to 8.9 %.

## What to do next

On the **Exercise** tab, 48 of 320 solder joints failed a thermal-cycling test. Find the 90 % confidence interval for the failure proportion.
