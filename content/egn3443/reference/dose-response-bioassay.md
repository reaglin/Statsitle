---
title: Dose–response and bioassay
---

# Dose–response and bioassay

| Quantity | Formula |
|---|---|
| Logit | logit(p) = ln(p/(1 − p)) |
| Empirical logit (k of n respond) | ln((k + 0.5)/(n − k + 0.5)) |
| Dose–response line | logit(p) = b₀ + b₁ log₁₀(dose) (regression, Module 11; properly logistic regression, Part II) |
| LD50 / EC50 | 10^(−b₀/b₁) |
| Dose for p (e.g. LD90) | 10^((logit(p) − b₀)/b₁) |
| Relative potency (parallel lines) | 10^((a₂ − a₁)/b) |

**Data:** Bliss, C. I. (1935), "The calculation of the dosage-mortality curve", *Annals of Applied Biology* 22, 134–167;
tabled in Dobson and Barnett, *An Introduction to Generalized Linear Models* (Chapman & Hall/CRC).

## Python and R

| Task | Python | R |
|---|---|---|
| Least squares on empirical logits | `sm.OLS(logit, sm.add_constant(logdose)).fit()` | `lm(logit ~ logdose)` |
| Logistic regression | `sm.GLM(np.column_stack([k, n - k]), X, family=sm.families.Binomial()).fit()` | `glm(cbind(k, n - k) ~ logdose, family = binomial)` |
| LD50 with interval | from the fit's covariance (delta method) | `MASS::dose.p(fit, p = 0.5)` |
