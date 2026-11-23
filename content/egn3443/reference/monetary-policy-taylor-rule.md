---
title: Monetary policy: price indices and the Taylor rule
---

# Monetary policy: price indices and the Taylor rule

| Quantity | Formula |
|---|---|
| Price index | cost of a fixed basket now / cost in the base period × 100 |
| Inflation | (Iₜ − Iₜ₋₁₂)/Iₜ₋₁₂ × 100 % (monthly index, over a year) |
| Real interest rate | r ≈ i − π (nominal rate minus inflation) |
| Taylor rule (Taylor, 1993) | i = r* + π + 0.5(π − π*) + 0.5 y; with r* = π* = 2 %: **i = 1 + 1.5π + 0.5y** |
| Taylor principle | the coefficient on inflation > 1, so the real rate rises with inflation |
| Fitted Taylor rule | Rate = b₀ + b₁·Inflation + b₂·Output gap (multiple regression, Module 12) |

y is the **output gap**, (GDP − potential GDP)/potential GDP × 100 %: an estimate, revised as new data come in. A fitted rule
describes a bank's past decisions; it is not a law, and a bank may change how it reacts.

**Real data:** the Federal Reserve Bank of St. Louis' FRED database (fred.stlouisfed.org): the federal funds rate, the CPI,
real GDP and the Congressional Budget Office's potential GDP.

## Python and R

| Task | Python | R |
|---|---|---|
| Inflation from an index | `100 * (cpi / cpi.shift(12) - 1)` | `100 * (cpi / dplyr::lag(cpi, 12) - 1)` or by hand |
| Fit the rule | `smf.ols("rate ~ inflation + gap", d).fit()` | `lm(rate ~ inflation + gap, d)` |

**Read more:** J. B. Taylor (1993), "Discretion versus policy rules in practice", *Carnegie-Rochester Conference Series on
Public Policy* 39, 195–214.
