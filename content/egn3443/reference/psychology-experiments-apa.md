---
title: Experiments in psychology: effect sizes and APA reporting
---

# Experiments in psychology: effect sizes and APA reporting

| Quantity | Formula |
|---|---|
| Pooled t-test | t = (x̄₁ − x̄₂)/(s_p √(1/n₁ + 1/n₂)), df = n₁ + n₂ − 2 (item 10.4) |
| Pooled SD | s_p = √(((n₁ − 1)s₁² + (n₂ − 1)s₂²)/(n₁ + n₂ − 2)) |
| Cohen's d | (x̄₁ − x̄₂)/s_p; 0.2 small, 0.5 medium, 0.8 large (rough) |
| d from t | d = t √(1/n₁ + 1/n₂) |
| ANOVA effect size | η² = SS_between/SS_total (item 14.4) |

**APA line:** t(df) = 1.13, p = .26, d = 0.33, with each group's M and SD. p with no leading zero, to two or three decimals,
or p < .001. Report non-significant results the same way.

## Python and R

| Task | Python | R |
|---|---|---|
| t-test | `stats.ttest_ind(a, b)` | `t.test(a, b, var.equal = TRUE)` |
| Cohen's d | by the formula, or `pingouin.compute_effsize(a, b, eftype="cohen")` | `effectsize::cohens_d(a, b)` |

**Read more:** American Psychological Association, *Publication Manual*, 7th ed., the section on statistics in text; Cumming,
*Understanding the New Statistics* (Routledge).
