---
title: Density and cumulative distribution function
---

# Density and cumulative distribution function

X is a **continuous** random variable: it can take any value in an interval (a time, a length, a strength).

| Idea | What it is |
|---|---|
| Density f(x) (PDF) | a curve with f(x) ≥ 0 and total area 1. A **height**, in probability per unit of x; it can be above 1 |
| P(a ≤ X ≤ b) | the **area** under f(x) from a to b (the integral) |
| P(X = x) | **0** for every x, so P(X < x) = P(X ≤ x) |
| CDF F(x) | P(X ≤ x), the area to the left of x; rises from 0 to 1 |
| P(X > x) | 1 − F(x) |
| P(a ≤ X ≤ b) | F(b) − F(a) |
| Quantile (percentile) for q | the x with F(x) = q |
| Mean E[X] | the integral of x·f(x): the balance point |
| Variance V[X] | the integral of (x − μ)²·f(x) |

## Discrete and continuous

| | Discrete | Continuous |
|---|---|---|
| Values | a list: 0, 1, 2, … | an interval |
| Distribution | PMF, P(X = x), bars | density f(x), a curve |
| Probability | sum of bars | area under the curve |
| "≤" or "<" | matters | does not matter |

## Python and R

| Quantity | Python (`X = stats.norm(mu, sigma)`, …) | R |
|---|---|---|
| density f(x) | `X.pdf(x)` | `dnorm(x, mu, sigma)`, `dunif`, `dexp`, `dweibull` |
| P(X ≤ x) | `X.cdf(x)` | `pnorm(x, mu, sigma)`, … |
| P(X > x) | `X.sf(x)` | `1 - pnorm(x, mu, sigma)` |
| quantile for q | `X.ppf(q)` | `qnorm(q, mu, sigma)`, … |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 4-1 to 4-4 (continuous random variables, density, CDF, mean and variance).
- EGN3443 Module 5, Topic 1, on the course website: https://reaglin.github.io/EGN3443/week-6module-5-continuous-probability-distributions/
