---
title: Odds as probability
---

# Odds as probability

| Quantity | Formula |
|---|---|
| Decimal odds d | total return per unit staked; implied probability 1/d |
| Fractional a/b | d = 1 + a/b |
| American +A | d = 1 + A/100; American −A: d = 1 + 100/A |
| Margin (overround) | Σ 1/dᵢ − 1 over all outcomes |
| Market's fair probabilities | (1/dᵢ)/Σ(1/dⱼ) |
| Expected value of a stake s | s(d − 1)·p − s(1 − p) = s(d·p − 1) |

The expected value is zero only when p = 1/d; the margin makes the offered odds shorter than that, so for the person staking
it is negative. This page is about probability only, not advice.

## Python and R

| Task | Python | R |
|---|---|---|
| Implied probabilities | `1 / np.array(odds)` | `1 / odds` |
| Margin | `(1 / np.array(odds)).sum() - 1` | `sum(1 / odds) - 1` |
