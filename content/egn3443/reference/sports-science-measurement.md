---
title: Sports science: reliability and the smallest worthwhile change
---

# Sports science: reliability and the smallest worthwhile change

| Quantity | Formula |
|---|---|
| Test–retest reliability | correlation of two trials of the same athletes (item 11.1) |
| Typical error | SD(trial 2 − trial 1)/√2 |
| Noise in a change between two tests | √2 × typical error |
| Typical error of the mean of m trials | TE/√m |
| Smallest worthwhile change | 0.2 × between-athlete SD (a small effect, item 22.3) |

A change is worth acting on when it is clearly larger than both the noise and the smallest worthwhile change. A high
correlation does not mean a small typical error: check both, and the mean difference.

## Python and R

| Task | Python | R |
|---|---|---|
| Typical error | `np.std(t2 - t1, ddof=1) / np.sqrt(2)` | `sd(t2 - t1) / sqrt(2)` |
| Smallest worthwhile change | `0.2 * np.std(t1, ddof=1)` | `0.2 * sd(t1)` |

**Read more:** Hopkins (2000), "Measures of reliability in sports medicine and science", *Sports Medicine* 30, 1–15.
