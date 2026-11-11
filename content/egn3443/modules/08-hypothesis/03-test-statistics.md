---
id: st.m8.test-statistics
title: Test statistics and sampling distributions
minutes: 50
objectives: [st.m8.o3]
builds-on: st.m8.formulating
example: st.m8.test-statistics.eg
exercise: st.m8.test-statistics.ex
reference: [test-statistics, sampling-distribution-of-the-mean]
---

# Test statistics and sampling distributions

## What a test statistic is

A **test statistic** turns the sample into one number that says how far it lies from H₀, measured in **standard errors**:

test statistic = (estimate − value in H₀) / standard error of the estimate

Its **sampling distribution when H₀ is true** is known, so the test can say how unusual the observed value is. That distribution is the **reference distribution** the p-value is read from.

## z or t for a mean

| | σ known | σ unknown (the usual case) |
|---|---|---|
| Statistic | z = (x̄ − μ₀) / (σ/√n) | t = (x̄ − μ₀) / (s/√n) |
| Reference distribution under H₀ | standard normal | t with n − 1 degrees of freedom |
| Assumes | a normal population, or n large enough for the CLT (Module 6) | a normal population, or n large enough |

σ is "known" only when it comes from long records of the same process, not from this sample. When the only spread you have is the sample's s, use **t**.

**Why t is different.** s is itself an estimate that varies from sample to sample. Dividing by it adds uncertainty, so t has **heavier tails** than z: a value of 2.2 is less surprising for t than for z. How much heavier depends on how well s is known, which is measured by the **degrees of freedom**.

**Degrees of freedom (df):** the number of values free to vary once the statistic's estimates are fixed. s is computed from the n deviations x − x̄, which must add to zero, so only n − 1 of them are free: **df = n − 1** for a one-sample t-test. As df grows, t approaches z.

| df | 4 | 11 | 30 | ∞ (z) |
|---|---|---|---|---|
| two-sided critical value at α = 0.05 | 2.776 | 2.201 | 2.042 | 1.960 |

## A proportion

For a proportion the estimate is p̂ = x/n (x items with the attribute out of n), and under H₀ its standard error is √(p₀(1 − p₀)/n), using the value in H₀:

z = (p̂ − p₀) / √(p₀(1 − p₀)/n)

It is close to standard normal when np₀ and n(1 − p₀) are both at least 10. Item 8.4 uses it.

## A worked case by hand

A buffer solution should have pH 7.00. Five readings: 7.08, 7.12, 6.98, 7.10, 7.07. σ is unknown, so this is a t-test of H₀: μ = 7.00 against H₁: μ ≠ 7.00.

1. **Mean:** x̄ = 35.35/5 = **7.07**.
2. **Standard deviation:** the deviations are 0.01, 0.05, −0.09, 0.03, 0.00; their squares add to 0.0116, so s² = 0.0116/4 = 0.0029 and s = **0.0539**.
3. **Standard error:** s/√n = 0.0539/√5 = **0.0241**.
4. **t** = (7.07 − 7.00)/0.0241 = **2.91**, with df = 5 − 1 = **4**.
5. **p-value:** 2 × P(T₄ ≥ 2.91) = **0.0438**. Below 0.05: reject H₀; the buffer reads above 7.00.

Had the analyst used z for the same 2.91, the two-sided p-value would be 0.0037, more than ten times smaller. With only 4 degrees of freedom the difference between z and t is large.

## The Example: coating thickness

A paint line should lay down 50 µm. Twelve panels: x̄ = 52.1 µm, s = 3.0 µm.

[[results: st.m8.test-statistics.eg]]

The standard error is 3.0/√12 = 0.866 µm, so t = 2.1/0.866 = **2.42** with 11 degrees of freedom. The curve is the t distribution with 11 df; the rejection region is beyond ±2.201.

[[figure: st.m8.test-statistics.eg 1]]

t = 2.42 is in the region and p = 0.0337 < 0.05: reject H₀, the coating is thicker than 50 µm. The normal curve would have given p = 0.0153: a z-test here understates the p-value by half.

## In Python and R

From summary statistics:

```python
import numpy as np
from scipy import stats
xbar, s, n, mu0 = 52.1, 3.0, 12, 50
t = (xbar - mu0) / (s / np.sqrt(n))        # 2.42
df = n - 1                                 # 11
p = 2 * stats.t.sf(abs(t), df)             # 0.0337
print(t, df, p)
```

```r
xbar <- 52.1; s <- 3.0; n <- 12; mu0 <- 50
t <- (xbar - mu0) / (s / sqrt(n))
df <- n - 1
2 * pt(abs(t), df, lower.tail = FALSE)     # 0.0337
```

From the data (the pH readings):

```python
x = np.array([7.08, 7.12, 6.98, 7.10, 7.07])
print(stats.ttest_1samp(x, 7.00))          # t = 2.91, p = 0.0438
```

```r
x <- c(7.08, 7.12, 6.98, 7.10, 7.07)
t.test(x, mu = 7.00)                       # t = 2.9066, df = 4, p-value = 0.04383
```

[[code: st.m8.test-statistics.eg]]

## Try it in the Workbench

Press **Open in the Workbench** on the Example tab. Change **Test** to *Mean, σ known (z-test)* and set **Population σ** to 3: the curve becomes the standard normal, the critical values move in to ±1.96, and p falls to 0.0153. Set it back to the t-test: with s from the sample, t is the right choice.

## Common mistakes

- Using z when σ comes from the sample. Then the reference distribution is t with n − 1 df.
- Dividing by s (or σ) instead of the standard error s/√n.
- Using n degrees of freedom instead of n − 1.
- Reading a one-tailed area as a two-sided p-value.

## What to do next

On the **Exercise** tab, twelve aluminum specimens are tested against a claimed mean strength of 310 MPa. The values are in a table. Find t, its degrees of freedom and the two-sided p-value.
