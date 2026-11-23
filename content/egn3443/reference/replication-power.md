---
title: The replication crisis, p-hacking and power
---

# The replication crisis, p-hacking and power

| Quantity | Formula |
|---|---|
| Power, one-sided z-test | P(Z > z_α − (μ₁ − μ₀)√n/σ) (item 8.6) |
| n for power 1 − β (one-sided) | ((z_α + z_β)σ/(μ₁ − μ₀))² |
| n per group, two groups, effect d | ≈ 2(z_{α/2} + z_β)²/d² (d = 0.5: 63; d = 0.33: 145 for 80 % power) |
| Chance of a false positive among m tries | 1 − (1 − α)^m when the tries are independent |

**Causes:** low power, p-hacking (flexible analysis), publication bias, HARKing. **Cures:** preregistration, registered
reports, power analysis with realistic effects, effect sizes and intervals, replication, open data and code.

## Python and R

| Task | Python | R |
|---|---|---|
| Power of a t-test | `TTestIndPower().solve_power(effect_size=d, nobs1=n, alpha=0.05)` | `power.t.test(n = n, delta = d, sd = 1)` |
| n for 80 % power | `TTestIndPower().solve_power(effect_size=d, power=0.8, alpha=0.05)` | `power.t.test(delta = d, sd = 1, power = 0.8)` |

**Read more:** Open Science Collaboration (2015), "Estimating the reproducibility of psychological science", *Science* 349,
aac4716; Simmons, Nelson and Simonsohn (2011), "False-positive psychology", *Psychological Science* 22, 1359–1366.
