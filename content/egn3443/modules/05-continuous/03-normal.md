---
id: st.m5.normal
title: The normal distribution
minutes: 60
objectives: [st.m5.o2, st.m5.o3]
builds-on: st.m5.uniform
example: st.m5.normal.eg
exercise: st.m5.normal.ex
reference: [normal-distribution, probability-density-function]
---

# The normal distribution

## The bell curve

The **normal** (or Gaussian) distribution is the symmetric bell curve. It describes measurements that vary around a target because of many small, independent effects added together: dimensions of machined parts, fill volumes, strengths, measurement errors. Module 6 shows a second reason it matters: the mean of a sample is close to normal even when single values are not.

X ~ N(μ, σ²) has two parameters:

- the **mean μ**, the center: the curve is symmetric about it, so μ is also the median and the most likely region;
- the **standard deviation σ**, the spread: the distance from μ to where the curve bends (its inflection point).

**Watch the notation.** N(μ, σ²) gives the **variance** second. N(420, 225) means σ² = 225 and σ = 15. Some books and programs write N(μ, σ) instead, so check which one a problem uses.

The density is f(x) = (1/(σ√(2π))) · e^(−(x − μ)²/(2σ²)). It has no simple area formula, so probabilities come from a table or a computer.

## The empirical rule

For every normal distribution:

| Range | Share of values |
|---|---|
| μ ± 1σ | 68.3 % |
| μ ± 2σ | 95.4 % |
| μ ± 3σ | 99.7 % |

It is the quick check every engineer carries: a value more than 3σ from the mean happens about 3 times in 1000.

## The standard normal and z-scores

The **standard normal** Z has μ = 0 and σ = 1. Any normal X becomes standard by the **z-score**:

z = (x − μ)/σ

z counts how many standard deviations x lies from the mean: z = −1.5 is one and a half σ below it. The area to the left of z under the standard normal is written **Φ(z)** ("phi of z"), and it is the same as the area to the left of x under X's curve:

P(X ≤ x) = Φ((x − μ)/σ)

A **standard normal table** lists Φ(z): find the row for z to one decimal and the column for the second decimal. A few values to know:

| z | −2.326 | −1.645 | −1 | 0 | 1 | 1.645 | 1.960 | 2.326 |
|---|---|---|---|---|---|---|---|---|
| Φ(z) | 0.01 | 0.05 | 0.1587 | 0.5 | 0.8413 | 0.95 | 0.975 | 0.99 |

The curve is symmetric, so Φ(−z) = 1 − Φ(z).

## The three kinds of question

| Question | Calculate |
|---|---|
| P(X ≤ x), "below x" | z = (x − μ)/σ, then Φ(z) |
| P(X > x), "above x" | 1 − Φ(z) |
| P(x₁ ≤ X ≤ x₂), "between" | Φ(z₂) − Φ(z₁) |
| the value with a given share below it (a **percentile**) | find z with Φ(z) = q, then x = μ + z·σ |

The last one runs the z-score backwards. The 95th percentile is μ + 1.645σ; the 5th is μ − 1.645σ.

## A worked case by hand

Bags of cement are filled to a mean of μ = 50 kg with σ = 0.8 kg, normally distributed.

1. **A bag under 49 kg:** z = (49 − 50)/0.8 = −1.25. P(X < 49) = Φ(−1.25) = **0.1056**. About one bag in ten is under 49 kg.
2. **Between 49 and 51 kg:** z₁ = −1.25, z₂ = (51 − 50)/0.8 = 1.25. P = Φ(1.25) − Φ(−1.25) = 0.8944 − 0.1056 = **0.7887**.
3. **Over 51.5 kg:** z = 1.5/0.8 = 1.875. P(X > 51.5) = 1 − Φ(1.875) = **0.0304**.
4. **The weight 95 % of bags are below:** Φ(z) = 0.95 gives z = 1.645, so x = 50 + 1.645 × 0.8 = **51.32 kg**.

Draw a quick sketch of the bell with the area shaded before you calculate. It catches the most common error, the wrong tail.

## The Example: shaft diameters against a tolerance

A lathe turns shafts with diameters normal with μ = 25.00 mm and σ = 0.02 mm. The drawing allows 25.00 ± 0.03 mm. The shaded area is the share of shafts in tolerance:

[[figure: st.m5.normal.eg 1]]

[[figure: st.m5.normal.eg 2]]

[[results: st.m5.normal.eg]]

The lower limit 24.97 mm is z = −1.5, so P(X ≤ 24.97) = Φ(−1.5) = 0.0668, and by symmetry 0.0668 are also above 25.03 mm. The share in tolerance is 1 − 2 × 0.0668 = **0.8664**: about 13 % of shafts would be scrapped or reworked. The quantile for q = 0.99 is 25.0465 mm: 99 % of shafts are below it.

Notice the density at 24.97 mm, 6.48 per mm. It is far above 1 because σ is tiny. A density is not a probability.

## In Python and R

```python
from scipy import stats
mu, sigma = 25.00, 0.02
X = stats.norm(mu, sigma)            # scipy takes σ, not σ²
z = (24.97 - mu) / sigma             # −1.5
print(stats.norm.cdf(z))             # Φ(−1.5) = 0.0668
print(X.cdf(25.03) - X.cdf(24.97))   # 0.8664, in tolerance
print(X.ppf(0.99))                   # 25.0465, the 99th percentile
```

```r
mu <- 25.00; sigma <- 0.02
pnorm(-1.5)                                      # Φ(−1.5) = 0.0668
pnorm(25.03, mu, sigma) - pnorm(24.97, mu, sigma) # 0.8664
qnorm(0.99, mu, sigma)                           # 25.0465
```

Both languages take the **standard deviation** σ. Given a variance, take its square root first.

[[code: st.m5.normal.eg]]

## Common mistakes

- Using the variance σ² where σ belongs. N(420, 225) has σ = 15, not 225.
- The wrong tail: giving Φ(z) when the question asks "above x". Sketch the curve and shade the area first.
- For "between", subtracting the z-scores instead of the areas. It is Φ(z₂) − Φ(z₁).
- For a percentile, using q for the wrong side: "the strength 99 % of joints exceed" is the 1st percentile, not the 99th.
- Using the normal for a quantity that cannot be negative and is strongly skewed, such as a repair time.

## What to do next

On the **Exercise** tab, the strength of welded joints is given as N(μ, σ²). Find the z-score, two probabilities and a percentile, and press **Check my answers**.
