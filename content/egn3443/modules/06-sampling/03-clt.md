---
id: st.m6.clt
title: The Central Limit Theorem
minutes: 50
objectives: [st.m6.o2, st.m6.o4]
builds-on: st.m6.parameters-statistics
example: st.m6.clt.eg
exercise: st.m6.clt.ex
reference: [sampling-distribution-of-the-mean]
---

# The Central Limit Theorem

## The theorem

Take random samples of size n from **any** population with mean μ and standard deviation σ. Then the sample mean x̄ has

- **mean** equal to μ (the mean of x̄ is the population mean),
- **standard deviation** σ/√n, called the **standard error of the mean**,

and, as n grows, the shape of its distribution gets closer and closer to **normal**, whatever the shape of the population. That last part is the **Central Limit Theorem** (CLT).

x̄ ≈ Normal(μ, σ/√n) for large n

The first two facts hold for every n. Only the normal shape needs n to be large enough.

## When it applies

| The population | x̄ is close to normal when |
|---|---|
| Normal | any n, even n = 2 (it is exactly normal) |
| Roughly symmetric, no outliers | n of about 15 or more |
| Skewed (times, lifetimes, incomes) | n of about **30 or more** |
| Very skewed, or with extreme outliers | n can need to be much larger than 30 |

The samples must also be **random** and **independent**: one item's value must not affect another's. When sampling without replacement, keep n under about 10 % of the population.

"n ≥ 30" is a rule of thumb, not a law. The more skewed the population, the larger n has to be.

## Reading a probability from it: z and Φ

Once x̄ is normal, a probability about x̄ comes from a **z-score** (Module 5), here the number of standard errors x̄ lies from μ:

z = (x̄ − μ) / (σ/√n)

and **Φ(z)**, the area under the standard normal curve to the left of z (Module 5 teaches the normal distribution in full; Python `stats.norm.cdf(z)`, R `pnorm(z)`). Then P(x̄ ≤ a) = Φ(z) and P(x̄ ≥ a) = 1 − Φ(z).

The difference from Module 5 is the **denominator**: a single value divides by σ; a sample mean divides by σ/√n.

## A worked case by hand

The lifetimes of a type of LED driver are skewed to the right, with mean μ = 50 000 hours and standard deviation σ = 20 000 hours. A test lab runs n = 64 drivers. What is the probability that their mean lifetime is below 46 000 hours?

1. **Can the CLT be used?** The population is skewed, but n = 64 ≥ 30, and the drivers are independent. Yes.
2. **Mean of x̄:** the mean of x̄ is μ = **50 000 h**.
3. **Standard error:** σ/√n = 20 000/√64 = 20 000/8 = **2500 h**.
4. **z:** (46 000 − 50 000)/2500 = **−1.60**.
5. **Probability:** P(x̄ < 46 000) = Φ(−1.60) = **0.0548**.

Only about 5 % of test runs of 64 drivers would average under 46 000 hours. For one driver, z would be (46 000 − 50 000)/20 000 = −0.20, and a single driver lasts under 46 000 hours far more often than that. (That single-driver probability cannot be read from Φ, though: the lifetimes are skewed, not normal. Only the mean of many is normal.)

## The Example: the CLT from a skewed population

Times between calls to a help desk are **exponential** with mean 4 minutes and standard deviation 4 minutes. That population is strongly skewed: most gaps are short, and a few are long. The computer drew 5000 samples at each of three sample sizes and plotted the means, with the normal curve the CLT predicts.

**n = 2:** the means are still strongly skewed to the right, like the population. The normal curve is a poor fit.

[[figure: st.m6.clt.n2 1]]

**n = 5:** less skewed, but the left side is still squeezed and the right tail is still long.

[[figure: st.m6.clt.n5 1]]

**n = 30:** close to the normal curve, centered on 4 minutes with standard error 4/√30 = 0.7303 minutes.

[[figure: st.m6.clt.eg 1]]

[[results: st.m6.clt.eg]]

| n | Standard error σ/√n | Standard deviation of the 5000 means | Median of the means, about |
|---|---|---|---|
| 2 | 2.8284 | 2.7891 | 3.3 |
| 5 | 1.7889 | 1.7803 | 3.7 |
| 30 | 0.7303 | 0.7336 | 3.95 |

The center and the standard error match the formulas at **every** n, even n = 2. What changes with n is the shape: the median of the means moves up towards the mean of 4 as the skew goes away. At n = 30 the means are not perfectly normal, but close enough for the probabilities a normal curve gives.

## In Python and R

The LED drivers by formula:

```python
import numpy as np
from scipy import stats
mu, sigma, n = 50000, 20000, 64
se = sigma / np.sqrt(n)                    # 2500
print(stats.norm(mu, se).cdf(46000))       # P(x̄ < 46 000) = 0.0548
```

```r
mu <- 50000; sigma <- 20000; n <- 64
se <- sigma / sqrt(n)                      # 2500
pnorm(46000, mu, se)                       # 0.0548
```

The simulation in the Example:

```python
rng = np.random.default_rng(1)
means = rng.exponential(scale=4, size=(5000, 30)).mean(axis=1)   # scale = 1/λ = 4
print(means.mean(), means.std(ddof=1), 4 / np.sqrt(30))
```

```r
set.seed(1)
means <- replicate(5000, mean(rexp(30, rate = 0.25)))
mean(means); sd(means); 4 / sqrt(30)
```

[[code: st.m6.clt.eg]]

## Try it in the Workbench

Press **Open in the Workbench** on the Example tab. Set **Sample size (n)** to 2, then 5, 10 and 30, and watch the histogram change shape. Then set **Population** to *Uniform (a, b)*: a flat population needs a much smaller n before its means look normal.

## Common mistakes

- Thinking the CLT makes the **data** normal. It is about the distribution of x̄. The individual values keep the population's shape.
- Dividing by σ instead of σ/√n when the question is about a sample mean.
- Using the normal for x̄ with a small n from a skewed population. Check the shape and the size first.

## What to do next

On the **Exercise** tab, repair times at a service desk are skewed, and a supervisor averages 36 of them each day. Find the mean and standard error of x̄ and the probability that a day's mean is 10 minutes or less.
