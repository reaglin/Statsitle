---
title: Trend tests for climate and environmental records
---

# Trend tests for climate and environmental records

| Quantity | Formula |
|---|---|
| Linear trend | y = b₀ + b₁·year (item 11.2); per decade = 10·b₁ |
| Mann–Kendall S | Σ over i < j of sign(xⱼ − xᵢ) |
| Var(S), no ties | n(n − 1)(2n + 5)/18 |
| Z | (S − 1)/√Var(S) if S > 0; 0 if S = 0; (S + 1)/√Var(S) if S < 0 |
| Sen's slope | median of (xⱼ − xᵢ)/(j − i) over all pairs |
| Durbin–Watson | near 2: independent residuals; well below 2: positive autocorrelation (item 13.2) |

**Stationarity:** frequency analysis assumes the distribution does not change over time; test annual maxima for trends first.

## Python and R

| Task | Python | R |
|---|---|---|
| Regression trend | `stats.linregress(year, y)` | `lm(y ~ year)` |
| Mann–Kendall | `stats.kendalltau(year, y)` | `cor.test(year, y, method = "kendall")` or `Kendall::MannKendall(y)` |
| Sen's slope | `stats.theilslopes(y, year)` | `trend::sens.slope(y)` |

**Read more:** Helsel, Hirsch, Ryberg, Archfield and Gilroy (2020), *Statistical Methods in Water Resources*, USGS Techniques
and Methods 4-A3 (free), chapter 12 on trend analysis.
