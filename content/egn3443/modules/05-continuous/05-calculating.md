---
id: st.m5.calculating
title: Calculating and interpreting probabilities
minutes: 40
objectives: [st.m5.o1, st.m5.o2, st.m5.o4]
builds-on: st.m5.exponential
example: st.m5.calculating.eg
quiz: quizzes/m5-calculating.json
reference: [probability-density-function, normal-distribution, exponential-distribution, uniform-distribution]
---

# Calculating and interpreting probabilities

The last three items each taught one family. This one puts them side by side, so that whatever the family, you calculate a probability the same way and say what it means.

## One method for every family

1. **Name X and its units.** "X = the gap between the panels, in mm."
2. **Choose the family and its parameters** from the problem: U(a, b), N(μ, σ²) or Exp(λ).
3. **Sketch the density and shade the area asked for.** Left tail, right tail, or between.
4. **Calculate the area** with the CDF.
5. **Interpret it** in the words of the problem.

| | Uniform U(a, b) | Normal N(μ, σ²) | Exponential Exp(λ) |
|---|---|---|---|
| P(X ≤ x) | (x − a)/(b − a) | Φ((x − μ)/σ) | 1 − e^(−λx) |
| P(X > x) | (b − x)/(b − a) | 1 − Φ((x − μ)/σ) | e^(−λx) |
| Percentile for q | a + q(b − a) | μ + zσ, with Φ(z) = q | −ln(1 − q)/λ |
| Mean | (a + b)/2 | μ | 1/λ |
| Standard deviation | (b − a)/√12 | σ | 1/λ |
| How you get the area | rectangle, by hand | table of Φ or a computer | formula, by hand |

There z is the value with Φ(z) = q: z = 1.645 for q = 0.95. The between probability is always P(X ≤ x₂) − P(X ≤ x₁).

## Tables and technology

A **table** of Φ(z) is enough for the normal and gives z to two decimals. A **computer** gives more digits and every family. The functions follow one pattern:

| You want | Python (`X = stats.norm(mu, sigma)`, …) | R |
|---|---|---|
| the density f(x), a height | `X.pdf(x)` | `dnorm`, `dunif`, `dexp` |
| P(X ≤ x) | `X.cdf(x)` | `pnorm`, `punif`, `pexp` |
| P(X > x) | `X.sf(x)` ("survival function") | `1 - pnorm(…)` or `pnorm(…, lower.tail = FALSE)` |
| the percentile for q | `X.ppf(q)` | `qnorm`, `qunif`, `qexp` |

Read the arguments every time: scipy wants σ (normal), loc and **width** (uniform) and **scale = 1/λ** (exponential); R wants σ, the two limits, and the **rate** λ.

## A density is not a probability

The Example's panel gaps are normal with μ = 2.0 mm and σ = 0.25 mm, and the specification asks for a gap between **1.8 and 2.2 mm**. The density at 1.8 mm is 1.16, **more than 1**:

[[figure: st.m5.calculating.eg 1]]

[[results: st.m5.calculating.eg]]

That is not a probability above 1. The density is probability **per millimeter**, and the values are packed into about a millimeter, so the curve must be tall to enclose an area of 1. The probability is the **shaded area**: P(1.8 ≤ X ≤ 2.2) = Φ(0.8) − Φ(−0.8) = **0.5763**. Only 58 % of the gaps meet the specification.

## Interpreting a probability

A number is not an answer until it is tied to the decision. A good statement:

1. **says what the probability is of**, in the problem's words;
2. **turns it into a rate or a count** a reader can picture;
3. **draws the conclusion** for the decision.

For the panel gaps: "P(1.8 ≤ X ≤ 2.2) = 0.58. About 42 of every 100 car bodies have a gap outside 1.8 to 2.2 mm. The process cannot meet this specification as it runs: σ must be reduced, or the specification widened."

Some phrases to translate carefully:

| Words | Means |
|---|---|
| "at most", "no more than", "within" | P(X ≤ x) |
| "more than", "exceeds", "survives past" | P(X > x) |
| "between", "in tolerance" | P(x₁ ≤ X ≤ x₂) |
| "the value 95 % are below", "95th percentile" | the quantile for q = 0.95 |
| "the value 95 % exceed" | the quantile for q = 0.05 |

## Common errors

- **The density read as a probability.** Only areas are probabilities.
- **The wrong tail.** Sketch first.
- **The wrong parameter.** σ² for σ in the normal; the mean for the rate in the exponential; the upper limit for the width in scipy's uniform.
- **Units that do not match.** A rate per hour with x in minutes.
- **A number with no meaning.** "0.58" helps nobody; "42 % of bodies fail the gap check" does.

## In Python and R

```python
from scipy import stats
X = stats.norm(2.0, 0.25)
print(X.pdf(1.8))                 # 1.1588: a height, per mm
print(X.cdf(2.2) - X.cdf(1.8))    # 0.5763: a probability
```

```r
dnorm(1.8, 2.0, 0.25)                         # 1.1588
pnorm(2.2, 2.0, 0.25) - pnorm(1.8, 2.0, 0.25) # 0.5763
```

[[code: st.m5.calculating.eg]]

## What to do next

The **Exercise** tab has eight questions: probabilities from each of the three families, reading the right function, and saying what a result means.
