---
title: The Pythagorean expectation
---

# The Pythagorean expectation

| Quantity | Formula |
|---|---|
| Expected win share | RS^k/(RS^k + RA^k) |
| Log form | ln(W/L) = k ln(RS/RA): a regression through the origin, slope k |
| Exponent | k = 2 (Bill James); about 1.83 for baseball; near 14 for basketball |
| Luck | actual wins − expected wins (the residual) |
| Wins from ln(W/L) near .500 | ΔW ≈ Δln(W/L) × G/4 for G games |

**Real data:** season standings with runs or points for and against, on Baseball-Reference.com and Basketball-Reference.com.

## Python and R

| Task | Python | R |
|---|---|---|
| Fit k | `stats.linregress(np.log(rs/ra), np.log(w/l)).slope` | `coef(lm(log(w/l) ~ log(rs/ra)))[2]` |

**Read more:** James, *The Bill James Baseball Abstract* (1980s editions), where the formula appears.
