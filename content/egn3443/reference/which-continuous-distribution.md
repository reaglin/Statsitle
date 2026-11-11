---
title: Which continuous distribution?
---

# Which continuous distribution?

| Model | The quantity | Shape | Parameters | Mean |
|---|---|---|---|---|
| Uniform | equally likely anywhere in a range | flat | a, b | (a + b)/2 |
| Normal | a measurement scattered symmetrically around a target | bell | μ, σ | μ |
| Exponential | waiting time to a random event; life with failures at random | skewed right, peak at 0 | λ | 1/λ |
| Weibull | life of a part that wears out (β > 1) or fails early (β < 1); strength of brittle materials | flexible | β, η | η Γ(1 + 1/β) |
| Lognormal | a quantity whose logarithm is normal: particle sizes, repair times | skewed right | μ and σ of ln X | e^(μ + σ²/2) |

Γ is the gamma function (Python `scipy.special.gamma`, R `gamma`).

## Three questions

1. Can it be negative, and is it symmetric? A symmetric measurement: **normal**. A time or a life, skewed right: **exponential** or **Weibull**.
2. Does the failure rate depend on age? No: **exponential**. Rises with age (wear-out): **Weibull**, β > 1.
3. Is every value in a range equally likely? **Uniform**.

## Checking against data

Fit each candidate to the data, then look at the histogram with the fitted curve and the Q–Q plot, and compare the Q–Q correlation r: the larger r fits better. When two fit about equally well, report the simpler one, and the one the physics supports.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 4 and Section 6-7 (probability plots).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.6.6, Gallery of Distributions: https://www.itl.nist.gov/div898/handbook/eda/section3/eda366.htm
- EGN3443 Module 5, Topic 6, and the *Fit to Predict* game, on the course website: https://reaglin.github.io/EGN3443/week-6module-5-continuous-probability-distributions/
