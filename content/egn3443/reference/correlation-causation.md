---
title: Correlation, causation and confounding
---

# Correlation, causation and confounding

| Quantity | Formula |
|---|---|
| Correlation | r = S_xy/√(S_xx S_yy) (item 11.1) |
| Share explained | R² = r² |
| Test of r = 0 | t = r √(n − 2)/√(1 − r²), df = n − 2 |

**Explanations of an association:** X causes Y; Y causes X; a confounder causes both; chance; selection.
**Evidence for cause without an experiment:** strength, dose–response, timing, consistency across studies, adjustment for measured
confounders (multiple regression, Module 12), a plausible mechanism (the Bradford Hill considerations).
**Simpson's paradox:** an association in every group reverses in the total because the groups' mix differs.

## Python and R

| Task | Python | R |
|---|---|---|
| Correlation and test | `stats.pearsonr(x, y)` | `cor.test(x, y)` |
| Adjusting for a confounder | `smf.ols("y ~ x + z", d).fit()` | `lm(y ~ x + z, d)` |

**Read more:** Pearl and Mackenzie, *The Book of Why* (Basic Books).
