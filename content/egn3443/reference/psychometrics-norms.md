---
title: Psychometrics: standard scores, norms and percentiles
---

# Psychometrics: standard scores, norms and percentiles

| Quantity | Formula |
|---|---|
| z-score | z = (x − mean)/SD of the norm group |
| IQ-type score | 100 + 15z |
| T-score | 50 + 10z |
| Percentile (normal norms) | P(Z ≤ z) × 100 (item 5.3) |
| Score at percentile p | mean + z_p × SD |
| Standard error of measurement | SD √(1 − reliability); report a score as a band, ± 2 SEM |

**Factor analysis** finds a few underlying factors that explain the correlations among many items or subtests; loadings are the
items' correlations with the factors (Part II).

## Python and R

| Task | Python | R |
|---|---|---|
| Percentile | `stats.norm.cdf(x, 100, 15)` | `pnorm(x, 100, 15)` |
| Score at a percentile | `stats.norm.ppf(p, 100, 15)` | `qnorm(p, 100, 15)` |
| Factor analysis | `factor_analyzer.FactorAnalyzer(n_factors=2).fit(items)` | `factanal(items, factors = 2)` |

**Read more:** Furr, *Psychometrics: An Introduction* (SAGE).
