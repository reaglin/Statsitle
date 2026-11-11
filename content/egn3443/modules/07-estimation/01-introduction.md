---
id: st.m7.introduction
title: Introduction to point estimation
minutes: 40
objectives: [st.m7.o1]
example: st.m7.introduction.eg
quiz: quizzes/m7-introduction.json
reference: [point-estimators, variance-standard-deviation]
---

# Introduction to point estimation

## Parameters and statistics

A **parameter** is a number that describes a whole population: the mean coating thickness μ of every panel a line will paint, the standard deviation σ of that thickness, the proportion p of panels that are too thin. Parameters are fixed, but nobody knows them, because nobody measures every panel.

A **statistic** is a number calculated from a sample: x̄, s, p̂. When a statistic is used to guess a parameter, it is called a **point estimate**.

| Parameter (population) | Point estimate (sample) | How it is calculated |
|---|---|---|
| mean μ | sample mean x̄ | Σx / n |
| variance σ² | sample variance s² | Σ(x − x̄)² / (n − 1) |
| standard deviation σ | sample standard deviation s | √s² |
| proportion p | sample proportion p̂ ("p-hat") | x / n, the count with the attribute ÷ n |

## Estimator and estimate

Two words that sound alike:

- The **estimator** is the rule: "add the values and divide by n". Before the sample is taken, its result is a random variable, written with a capital letter, X̄. Another sample would give another value.
- The **estimate** is the number the rule gives for the sample in hand: x̄ = 50.70 µm.

Because an estimator is a random variable, it has a distribution (its **sampling distribution**, item 7.2), with a mean and a spread. Those two decide whether it is a good estimator.

## Unbiased: right on average

An estimator is **unbiased** when its mean, over all possible samples, equals the parameter:

E[estimator] = parameter

- The sample mean is unbiased: E[X̄] = μ.
- The sample proportion is unbiased: E[P̂] = p.
- The sample variance s², dividing by **n − 1**, is unbiased: E[S²] = σ².
- The variance that divides by **n** is **biased**: on average it gives (n − 1)/n × σ², a little too small. This is why Module 2 divides by n − 1 for a sample. The deviations are measured from x̄, which sits closer to the data than μ does, so dividing by n underestimates the spread.

Unbiased does not mean that one estimate is right. A single x̄ is almost never exactly μ. It means the estimator does not lean high or low: over many samples its errors average out to zero.

(The standard deviation s is slightly biased low, because a square root does not carry an average through. The bias is small, and s is used anyway.)

## Efficient: close every time

Two unbiased estimators can still differ a lot. One may land close to the parameter almost every time, the other far away on either side. The one with the **smaller variance** is the more **efficient** estimator.

For data from a normal population, the sample mean and the sample median are both unbiased for μ. But the median's variance is about π/2 ≈ 1.57 times the mean's. The mean is the more efficient estimator: the median needs about 57 % more data to be as precise. (With outliers or a heavy-tailed population the median can win, which is why Module 2 recommends it for skewed data.)

The spread of an estimator is measured by its **standard error**, its standard deviation. For the sample mean it is σ/√n, estimated by s/√n.

An estimator can be unbiased and still poor (a large variance), or slightly biased and still useful (a small variance). Think of darts: unbiased means centered on the bull's-eye on average; efficient means tightly grouped.

## A worked case by hand

A spray line paints steel panels. Ten panels were measured (coating thickness, µm): 52.1, 49.8, 50.6, 51.3, 48.9, 50.2, 52.7, 49.5, 50.9, 51.0. The specification asks for at least 50 µm.

1. **Mean:** Σx = 507.0, so x̄ = 507.0 / 10 = **50.70 µm**. This estimates μ.
2. **Variance:** Σ(x − x̄)² = 12.20. Dividing by n − 1 = 9 gives s² = **1.3556 µm²**, the unbiased estimate of σ². Dividing by n = 10 gives 1.2200, which is 9/10 of s² and too small on average.
3. **Standard deviation:** s = √1.3556 = **1.164 µm**.
4. **Standard error of the mean:** s/√n = 1.164 / √10 = **0.368 µm**. A typical sample of ten panels gives an x̄ about 0.37 µm from μ.
5. **Proportion too thin:** three panels (49.8, 48.9, 49.5) are under 50 µm, so p̂ = 3/10 = **0.30**.

## The Example

[[data: st.m7.introduction.eg]]

[[figure: st.m7.introduction.eg 1]]

[[results: st.m7.introduction.eg]]

The Workbench shows both variances: the **Variance (s²)**, 1.3556, and the **Population variance (σ²)**, 1.2200, which divides by n. Use s² for a sample. Use the population form only when the data are the whole population.

## Seeing "unbiased" happen

The Simulation analysis can draw many samples from a population whose μ is known. Here, a thousand samples of n = 10 from a normal population with μ = 50 and σ = 1.2:

[[figure: st.m7.introduction.sim 1]]

[[results: st.m7.introduction.sim]]

The thousand sample means average 50.0051, very close to μ = 50: the sample mean is unbiased. Their standard deviation, 0.3708, is close to the standard error σ/√n = 1.2 / √10 = 0.3795. A larger n would make the histogram narrower: x̄ becomes more precise as n grows.

## In Python and R

```python
import numpy as np
x = np.array([52.1, 49.8, 50.6, 51.3, 48.9, 50.2, 52.7, 49.5, 50.9, 51.0])
print(x.mean())                          # x̄ = 50.70, estimates μ
print(x.var(ddof=1), x.var(ddof=0))      # s² = 1.3556 (unbiased), 1.2200 (divides by n)
print(x.std(ddof=1) / np.sqrt(len(x)))   # standard error = 0.3682
print(np.mean(x < 50))                   # p̂ = 0.3
```

```r
x <- c(52.1, 49.8, 50.6, 51.3, 48.9, 50.2, 52.7, 49.5, 50.9, 51.0)
mean(x)                     # x̄ = 50.70
var(x)                      # s² = 1.3556; R's var and sd always divide by n − 1
sd(x) / sqrt(length(x))     # standard error = 0.3682
mean(x < 50)                # p̂ = 0.3
```

numpy's `var` and `std` divide by n unless you give `ddof=1`. R's `var` and `sd` divide by n − 1.

[[code: st.m7.introduction.eg]]

## Common mistakes

- Dividing by n for a sample variance. It is biased low; divide by n − 1.
- Calling one estimate "unbiased" or "biased". Bias is a property of the estimator (the rule), over all samples.
- Confusing the standard deviation s (the spread of the data) with the standard error s/√n (the spread of x̄).
- Choosing an estimator only because it is unbiased. Compare the variances too.

## What to do next

Look at the **Example** tab. Then answer the questions on the **Exercise** tab: a few on bias and efficiency, and a few point estimates to calculate.
