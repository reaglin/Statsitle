---
title: Epidemiology: rates, risks and odds
---

# Epidemiology: rates, risks and odds

| Quantity | Formula |
|---|---|
| Prevalence | people with the disease now / population |
| Incidence (risk) | new cases over a period / people at risk at its start |
| Relative risk (cohort) | [a/(a + b)] / [c/(c + d)] |
| Odds ratio | (a·d)/(b·c); ≈ RR when the disease is rare; the only ratio from a case-control study |
| Attributable risk | a/(a + b) − c/(c + d) |
| Test of association | chi-square test of independence on the 2 × 2 table (item 8.5) |

Table: rows exposed (a, b) and unexposed (c, d); columns disease and no disease.

**Designs:** cohort (follow exposed and unexposed forward: RR), case-control (start from cases and controls: OR), cross-sectional
(one moment: prevalence). **Confounding:** stratify (Mantel–Haenszel), adjust in a regression (logistic, Part II), or match.

## Python and R

| Task | Python | R |
|---|---|---|
| Chi-square test | `scipy.stats.chi2_contingency(table, correction=False)` | `chisq.test(tab, correct = FALSE)` |
| Odds ratio with interval | `statsmodels.stats.contingency_tables.Table2x2(table).oddsratio_confint()` | `fisher.test(tab)$conf.int` |
| Stratified (Mantel–Haenszel) | `statsmodels.stats.contingency_tables.StratifiedTable(tables)` | `mantelhaen.test(array3d)` |

**Read more:** Rothman, *Epidemiology: An Introduction* (Oxford University Press).
