---
title: Margin of error and sample size
---

# Margin of error and sample size

## The margin of error

| Interval | Margin of error E |
|---|---|
| Mean, σ known | z_{α/2} · σ/√n |
| Mean, σ unknown | t_{α/2, n−1} · s/√n |
| Proportion | z_{α/2} · √(p̂(1 − p̂)/n) |

| Change | Effect on E |
|---|---|
| Confidence level up | wider (larger critical value) |
| Sample size × 4 | E halved |
| σ × 2 | E doubled |
| p̂ nearer 0.5 | wider (p(1 − p) largest at 0.5) |

## The sample size for a margin E

| Estimating | n (always round **up**) | Planning value |
|---|---|---|
| a mean | (z_{α/2} · σ / E)² | σ from past data, a pilot sample's s, or (range)/4 |
| a proportion | (z_{α/2} / E)² · p(1 − p) | p from earlier data, or 0.5 when unknown (the largest n) |

z_{α/2}: 1.645 (90 %), 1.960 (95 %), 2.576 (99 %).

Examples: σ = 2.2, E = 0.5, 95 % → 74.37 → **75**. p = 0.5, E = 0.03, 95 % → 1067.07 → **1068**.

## Python and R

| Quantity | Python | R |
|---|---|---|
| n for a mean | `math.ceil((stats.norm.ppf(0.975) * sigma / E) ** 2)` | `ceiling((qnorm(0.975) * sigma / E)^2)` |
| n for a proportion | `math.ceil((stats.norm.ppf(0.975) / E) ** 2 * p * (1 - p))` | `ceiling((qnorm(0.975) / E)^2 * p * (1 - p))` |

## In the Workbench

**Sample size**: choose a mean or a proportion, the confidence level, E, and σ or p.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 8-1.3 (choice of sample size for a mean) and Section 8-4 (for a proportion).
- EGN3443 Module 7 on the course website (reaglin.github.io/EGN3443), topic 6, Factors Affecting Precision and Margin of Error.
