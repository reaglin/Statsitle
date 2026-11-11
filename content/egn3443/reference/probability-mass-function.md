---
title: Probability mass functions and the CDF
---

# Probability mass functions and the CDF

| Term | Definition |
|---|---|
| Random variable X | A number decided by chance; discrete when its values can be listed (counts) |
| Probability mass function (PMF) | f(x) = P(X = x) for each value x |
| Cumulative distribution function (CDF) | F(x) = P(X ≤ x) = Σ f(t) over t ≤ x |
| From the CDF back to the PMF | f(x) = F(x) − F(x − 1) |

**A valid PMF** needs both:

1. f(x) ≥ 0 for every x;
2. Σ f(x) = 1 over every value X can take.

## Events in words

| Words | Calculate |
|---|---|
| exactly k | f(k) |
| at most k, no more than k | F(k) |
| fewer than k, less than k | F(k − 1) |
| at least k | 1 − F(k − 1) |
| more than k | 1 − F(k) |
| from k to k₂ inclusive | F(k₂) − F(k − 1) |

## Python and R

| Task | Python | R |
|---|---|---|
| A typed distribution | `X = stats.rv_discrete(values=(x, p))` | vectors `x` and `p` |
| Check it is valid | `p.min() >= 0 and np.isclose(p.sum(), 1)` | `all(p >= 0); isTRUE(all.equal(sum(p), 1))` |
| P(X = k) | `X.pmf(k)` | `sum(p[x == k])` |
| P(X ≤ k) | `X.cdf(k)` | `sum(p[x <= k])` |
| P(X > k) | `X.sf(k)` | `sum(p[x > k])` |
| The CDF as a list | `np.cumsum(p)` | `cumsum(p)` |
| A bar chart of the PMF | `plt.bar(x, p)` | `barplot(p, names.arg = x)` |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 3-1 to 3-3 (discrete random variables, probability mass functions, cumulative distribution functions).
- EGN3443 Module 4, Topics 1 and 2: https://reaglin.github.io/EGN3443/week-5module-4-discrete-probability-distributions/
