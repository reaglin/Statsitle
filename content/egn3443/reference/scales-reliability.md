---
title: Scales, reliability and validity
---

# Scales, reliability and validity

| Quantity | Formula |
|---|---|
| Reverse scoring (1 to K item) | (K + 1) − answer |
| Scale score | sum (or mean) of the items |
| Cronbach's alpha | α = k/(k − 1) × (1 − Σ sᵢ²/s_total²) |
| Test–retest reliability | correlation of the same people's scores on two occasions |
| Standard error of measurement | SEM = SD √(1 − reliability) |

**Validity:** content (covers the idea), criterion (predicts what it should), construct (relates to similar measures, not to
unrelated ones). Reliability is necessary for validity, not enough for it. Report single Likert items with medians and shares.

## Python and R

| Task | Python | R |
|---|---|---|
| Cronbach's alpha | `pingouin.cronbach_alpha(data=items)` | `psych::alpha(items)` |
| Test–retest | `stats.pearsonr(time1, time2)` | `cor.test(time1, time2)` |

**Read more:** DeVellis and Thorpe, *Scale Development: Theory and Applications* (SAGE).
