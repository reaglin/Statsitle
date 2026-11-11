---
id: st.m9.interpreting
title: Interpreting and communicating results
minutes: 40
objectives: [st.m9.o4]
builds-on: st.m9.nonparametric
example: st.m9.interpreting.eg
exercise: st.m9.interpreting.ex
reference: [two-sample-t-tests, inference-preview]
---

# Interpreting and communicating results

A test ends with "reject H₀" or "do not reject H₀". An engineer's decision needs more: **how big** is the difference, **how sure** are we of its size, and **does it matter** for the design, the cost or the safety margin? The p-value answers none of these alone.

## What the p-value says, and what it does not

The p-value is the probability, **if H₀ were true**, of a difference at least as large as the one observed. A small p says the data would be surprising if there were no difference.

It is **not**:

- the probability that H₀ is true;
- the size of the difference (a tiny difference gives a tiny p when n is large);
- a measure of importance.

And "do not reject H₀" is not "the means are equal". It means the data cannot tell them apart, often because the samples are small.

## The confidence interval for the difference

The (1 − α) confidence interval for μ₁ − μ₂ gives the range of differences the data are consistent with:

(x̄₁ − x̄₂) ± t(α/2, df) × SE

with the same SE and df as the test (Welch's ν for Welch; d̄ ± t × s_d/√n for pairs). It carries the test inside it: **a two-sided test at level α rejects H₀: μ₁ − μ₂ = 0 exactly when 0 is outside the (1 − α) interval.** And it shows the size: the ends are the smallest and largest differences that are still plausible.

For the two ovens of item 9.1 (Welch: t = 1.487, p = 0.155), the 95 % interval is −0.46 to 2.66 kN. It contains 0, so the test does not reject. But it also contains 2.66 kN: the data cannot rule out a difference of more than 10 % of the strength. More joints are needed before saying "no difference".

## Effect size

**Cohen's d** measures the difference in standard deviations of the data, not in standard errors:

d = (x̄₁ − x̄₂) / s_p, with s_p the pooled standard deviation

For the ovens, d = 1.1 / 1.804 = 0.61. As a rough guide (Cohen, 1988): about 0.2 is small, 0.5 medium, 0.8 large. Unlike t, d does not grow with the sample size. The t statistic is roughly d × √(n/2) for two samples of n each, so with n = 500, a d of 0.13 already gives t ≈ 2.

## Statistical and practical significance

**Statistically significant** means p < α: the difference is probably real. **Practically significant** means it is large enough to matter: compare it, or better the whole confidence interval, with a tolerance, a specification or a cost. The two can disagree either way:

| | Practically important | Not important |
|---|---|---|
| **p < α** | act on it | real but too small to matter (large samples) |
| **p ≥ α** | possibly important, data too few: collect more | no evidence of a difference that matters |

## The Example: resistors from two suppliers

Five hundred resistors from each supplier, rated 100 Ω ± 5 %:

[[results: st.m9.interpreting.eg]]

p = 0.0398 < 0.05: the difference is statistically significant. But the interval says the true difference is between 0.0065 and 0.2735 Ω, at most about 0.27 % of the nominal value, against a tolerance of ±5 Ω. Cohen's d = 0.13 is small. With 500 parts per sample, the test can detect a difference far too small to matter. The buyer should choose on price or delivery, not on this difference.

[[figure: st.m9.interpreting.eg 1]]

## Reporting a two-sample test

A good report says, in this order:

1. The question and the design: "Ten coupons of alloy A and eight of alloy B (independent samples) were tested for tensile strength."
2. The test and why: "Welch's t-test, because the spreads differed (7.1 and 14.2 MPa); Shapiro–Wilk showed no departure from normality."
3. The result with its numbers: "t = 2.37, ν = 9.80, p = 0.040."
4. The size, with its interval: "Alloy A was stronger by 13.05 MPa on average (95 % CI 0.75 to 25.35 MPa; d = 1.21)."
5. The meaning: "The difference is real, but the interval is wide: it may be less than 1 MPa. More coupons would pin it down before the alloy is specified on strength."

Give p as a number (p = 0.040), not only "p < 0.05". Say whether the test was one- or two-sided, and decide that before seeing the data: halving a two-sided p after the fact doubles the real Type I error rate.

## In Python and R

```python
from scipy import stats
res = stats.ttest_ind_from_stats(100.12, 1.05, 500, 99.98, 1.10, 500, equal_var=False)
print(res)                       # t = 2.0586, p = 0.0398
# The interval: (x̄₁ − x̄₂) ± t(0.975, ν) × SE, as on the Code tab
```

```r
# With the data, t.test(x, y) prints the interval with the test:
# 95 percent confidence interval: ...
t.test(a, b)$conf.int            # a, b: the two samples (the alloys of item 9.1 give 0.753 to 25.347)
```

[[code: st.m9.interpreting.eg]]

## Common mistakes

- **"p = 0.04, so there is a 4 % chance H₀ is true."** p is calculated assuming H₀ is true; it is not the probability of H₀.
- **"Not significant, so the mixes are the same."** Look at the interval: if it holds differences that matter, the study was too small to tell.
- **"Highly significant, so it is a big effect."** With large samples, tiny effects are significant. Report the difference, its interval and d.
- **Using z = 1.96 in the interval for small samples.** Use the t quantile with the test's degrees of freedom.

## What to do next

On the **Exercise** tab, two concrete mixes were compared and the test did not reject H₀. Find the confidence interval and Cohen's d, and see what they say that the p-value does not.
