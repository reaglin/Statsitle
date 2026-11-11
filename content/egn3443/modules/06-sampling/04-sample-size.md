---
id: st.m6.sample-size
title: The effect of sample size on variability
minutes: 40
objectives: [st.m6.o3]
builds-on: st.m6.clt
example: st.m6.sample-size.eg
exercise: st.m6.sample-size.ex
reference: [sampling-distribution-of-the-mean]
---

# The effect of sample size on variability

## The inverse square-root rule

The standard error of the mean is

SE = σ/√n

The population's σ is fixed; the engineer chooses n. A larger n gives a smaller SE, but through the **square root**:

- **4 times** the sample size gives **half** the standard error.
- **9 times** the sample size gives **one third** of it.
- In general, to divide the SE by k, multiply n by **k²**.

So the first few measurements buy a lot of precision, and later ones less and less. With σ = 10:

| n | 1 | 4 | 16 | 64 | 100 | 400 |
|---|---|---|---|---|---|---|
| SE = 10/√n | 10 | 5 | 2.5 | 1.25 | 1 | 0.5 |

Going from 1 to 4 measurements cuts the SE by 5; going from 100 to 400 cuts it by 0.5.

## The sample size for a target standard error

Turn SE = σ/√n around:

n = (σ / SE)²

and **round up** to a whole number, because a smaller n would give an SE above the target.

## A worked case by hand

A pressure transmitter's readings have σ = 14 kPa. An engineer averages n = 16 readings.

1. **The SE now:** 14/√16 = 14/4 = **3.5 kPa**.
2. **To halve it:** multiply n by 2² = 4, so n = 4 × 16 = **64** readings. Check: 14/√64 = 14/8 = 1.75 kPa, half of 3.5.
3. **Doubling n is not enough:** n = 32 gives 14/√32 = 2.47 kPa, only 29 % smaller.
4. **For an SE of at most 1 kPa:** n = (14/1)² = **196** readings.
5. **For an SE of at most 0.8 kPa:** n = (14/0.8)² = 306.25, rounded **up** to 307.

## Small samples, large samples

- **Small n:** x̄ jumps around from sample to sample, so one sample mean can be far from μ. Small samples from skewed populations also fail the CLT's shape condition (item 6.3).
- **Large n:** x̄ is precise, but every measurement costs time, money or a destroyed specimen. Beyond some n, the extra precision is not worth it.
- The engineer's trade-off: choose the smallest n that gives the precision the decision needs. Module 7 turns this into the sample size for a confidence interval of a chosen width.

A larger n reduces **sampling error** only. It does nothing for bias: a miscalibrated gauge read 1000 times is precisely wrong.

## The Example: concrete cylinders, n = 4 and n = 16

A ready-mix plant's concrete strengths have μ = 4200 psi and σ = 350 psi. The light curve is the population of single cylinders; the dark curve is the sampling distribution of the mean of **n = 4** cylinders, SE = 350/√4 = 175 psi:

[[figure: st.m6.sample-size.n4 1]]

With **n = 16** the SE is 350/√16 = 87.5 psi, half of 175. The dark curve is half as wide and twice as tall:

[[figure: st.m6.sample-size.eg 1]]

[[results: st.m6.sample-size.eg]]

95 % of the means of 16 cylinders fall between 4028.5 and 4371.5 psi (μ ± 1.96 × 87.5). For means of 4 cylinders that range is twice as wide: 3857.0 to 4543.0 psi.

## In Python and R

```python
import numpy as np, math
sigma = 350
for n in [4, 16, 64]:
    print(n, sigma / np.sqrt(n))         # 175, 87.5, 43.75
print(math.ceil((sigma / 50) ** 2))      # n for SE ≤ 50 psi: 49
```

```r
sigma <- 350
n <- c(4, 16, 64)
sigma / sqrt(n)                           # 175 87.5 43.75
ceiling((sigma / 50)^2)                   # n for SE ≤ 50 psi: 49
```

`ceil` and `ceiling` round up to the next whole number.

[[code: st.m6.sample-size.eg]]

## Try it in the Workbench

Press **Open in the Workbench** on the Example tab and change **Sample size (n)** from 16 to 64. The standard error halves again, to 43.75 psi.

## Common mistakes

- Doubling n to halve the SE. It takes four times n.
- Dividing σ by n instead of √n.
- Rounding the sample size down. n = 306.25 means 307.
- Expecting a large sample to fix a biased measurement.

## What to do next

On the **Exercise** tab, a lab quadruples the number of wire specimens it tests. Find the new standard error and the range the middle 95 % of sample means fall in.
