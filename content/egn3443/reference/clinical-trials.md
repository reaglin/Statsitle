---
title: Clinical trials: design and measures
---

# Clinical trials: design and measures

| Design element | What it protects against |
|---|---|
| Control group (placebo or standard treatment) | crediting the treatment with recovery that would have happened anyway |
| Randomisation | confounding: the groups differ only by chance in everything else |
| Blinding (single, double) | expectations changing how patients feel and how outcomes are judged |
| Registered primary outcome and analysis | choosing, after the fact, the outcome that looks best |
| Planned sample size | a trial too small to detect a benefit that matters |

| Measure | Formula |
|---|---|
| Risk difference | p₁ − p₂ (two-proportion interval, item 10.5) |
| Relative risk | p₁ / p₂ |
| Number needed to treat | 1 / \|p₁ − p₂\| |
| n per group (two proportions) | (z_{α/2} + z_β)² [p₁(1 − p₁) + p₂(1 − p₂)] / (p₁ − p₂)²; z_β = 0.842 for 80 % power |

## Python and R

| Task | Python | R |
|---|---|---|
| Compare two rates | `statsmodels.stats.proportion.proportions_ztest([x1, x2], [n1, n2])` | `prop.test(c(x1, x2), c(n1, n2), correct = FALSE)` |
| Sample size | `NormalIndPower().solve_power(proportion_effectsize(p1, p2), alpha=0.05, power=0.8)` | `power.prop.test(p1 = p1, p2 = p2, power = 0.8)` |

**Read more:** Friedman, Furberg, DeMets et al., *Fundamentals of Clinical Trials* (Springer); the CONSORT statement on reporting
trials (consort-statement.org).
