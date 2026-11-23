---
title: Risk: expected value, standard deviation, Value at Risk
---

# Risk: expected value, standard deviation, Value at Risk

| Quantity | Formula | Meaning |
|---|---|---|
| Expected value | E[X] = Σ x·p(x) (item 4.3) | the long-run average outcome |
| Risk | SD(X) = √(Σ (x − E[X])²·p(x)) | spread of the outcome, in its own units |
| Chance of a loss | P(X ≤ 0) | add the probabilities of the losses |
| Reward per unit of risk | E[X]/SD(X) (Sharpe ratio: (E[R] − R_f)/SD(R)) | compares projects of different risk |
| Value at Risk, level 1 − a | VaR = −(the a quantile of the change in value) | the loss exceeded with probability a |
| Normal VaR | VaR = −(μ + z_a σ), z₀.₀₅ = −1.645, z₀.₀₁ = −2.326 | assumes normal changes (light tails) |
| Expected shortfall | ES = −E[X \| X ≤ the a quantile] | the average loss on the bad a of days |

**Cautions:** probabilities in a scenario table must add to 1 and are judgements; daily returns have heavier tails than the
normal, so normal VaR understates big losses; VaR is not the worst case.

## Python and R

| Task | Python | R |
|---|---|---|
| Expected value and SD | `ev = (x*p).sum(); sd = np.sqrt(((x-ev)**2*p).sum())` | `ev <- sum(x*p); sd <- sqrt(sum((x-ev)^2*p))` |
| Normal VaR | `-stats.norm.ppf(0.05, mu, sigma)` | `-qnorm(0.05, mu, sigma)` |
| Historical VaR from returns | `-np.quantile(changes, 0.05)` | `-quantile(changes, 0.05)` |

**Read more:** J. C. Hull, *Risk Management and Financial Institutions* (Wiley), the chapters on Value at Risk and
expected shortfall.
