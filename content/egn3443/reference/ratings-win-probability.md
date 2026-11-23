---
title: Ratings and win probability
---

# Ratings and win probability

| Quantity | Formula |
|---|---|
| Elo expected score | E_A = 1/(1 + 10^((R_B − R_A)/400)) |
| Elo update | R_A ← R_A + K (S_A − E_A), S_A = 1, ½ or 0 |
| Win probability (normal model) | P(final margin > 0), margin ~ Normal(lead, σ√(time left)) (item 5.3) |
| Best-of-(2m − 1) series | P(at least m wins in 2m − 1 games), binomial (item 4.4) |

Real win-probability models are logistic regressions on past games (Part II).

## Python and R

| Task | Python | R |
|---|---|---|
| Elo | `1 / (1 + 10 ** ((rb - ra) / 400))` | `1 / (1 + 10^((rb - ra) / 400))` |
| Win probability | `stats.norm.sf(0, lead, sigma * np.sqrt(t))` | `pnorm(0, lead, sigma * sqrt(t), lower.tail = FALSE)` |
| Series | `stats.binom.sf(m - 1, 2*m - 1, p)` | `pbinom(m - 1, 2*m - 1, p, lower.tail = FALSE)` |

**Read more:** Elo, *The Rating of Chessplayers, Past and Present* (Arco, 1978).
