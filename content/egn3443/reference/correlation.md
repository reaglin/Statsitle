---
title: Correlation
---

# Correlation

n pairs (x, y). With the means x̄ and ȳ:

- S_xx = Σ(x − x̄)²
- S_yy = Σ(y − ȳ)²
- S_xy = Σ(x − x̄)(y − ȳ)

**Correlation coefficient (Pearson's r):** r = S_xy / √(S_xx · S_yy), between −1 and +1. It has no units, and it is the same if x and y are swapped or their units changed.

**Coefficient of determination:** R² = r² in simple linear regression: the share of the variation in y explained by the straight line.

| r | Reading |
|---|---|
| ±1 | all points on a line |
| about ±0.8 or beyond | strong linear relation |
| about ±0.5 | moderate |
| near 0 | no **linear** relation (there may still be a curve) |

**Cautions:** r measures straight-line relations only; one unusual point can make or break it; correlation is not causation. Plot the data first.

**Test of H₀: ρ = 0** (ρ, rho, is the population correlation): t = r √(n − 2) / √(1 − r²) on n − 2 degrees of freedom. It is the same test as the slope's t-test in simple regression.

## Python and R

| Task | Python | R |
|---|---|---|
| r | `np.corrcoef(x, y)[0, 1]` | `cor(x, y)` |
| r with its test | `stats.pearsonr(x, y)` | `cor.test(x, y)` |
| scatter plot | `plt.scatter(x, y)` | `plot(x, y)` |

## In the Workbench

**Simple linear regression**: **Correlation (r)** and **R²** in the results; the first figure is the scatter plot with the line.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 11-8 (correlation).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.3.26, Scatter plot: https://www.itl.nist.gov/div898/handbook/eda/section3/scatterp.htm
