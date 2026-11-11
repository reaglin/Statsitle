---
title: Theoretical and experimental probability, the Law of Large Numbers
---

# Theoretical and experimental probability

| | Theoretical (classical) | Experimental (empirical) |
|---|---|---|
| Formula | favourable outcomes ÷ possible outcomes | times the event happened ÷ trials |
| Needs | equally likely outcomes, a model | trials |
| Result | exact | an estimate, different each run |

**The Law of Large Numbers:** as the number of trials n grows, the experimental probability gets closer to the theoretical probability p.

**How close:** about 95 % of the time within 2 × √(p(1 − p)/n). The error shrinks like 1/√n: four times the trials halve it.

| p | n = 100 | n = 1 000 | n = 10 000 |
|---|---|---|---|
| 0.5 | ± 0.10 | ± 0.032 | ± 0.010 |
| 1/6 | ± 0.075 | ± 0.024 | ± 0.0075 |

**What it does not say:** nothing about the next trial. After a run of tails a fair coin is still 50–50 (the gambler's fallacy).

**The Monte Carlo method:** when a probability is hard to calculate, simulate the experiment many times and count. Darts thrown at random at a 2 × 2 square land inside the unit circle with probability π/4, so 4 × the proportion estimates π.

## Python and R

| Task | Python (`rng = np.random.default_rng(seed)`) | R (`set.seed(seed)`) |
|---|---|---|
| n coin flips (1 = heads) | `rng.integers(0, 2, size=n)` | `sample(0:1, n, replace = TRUE)` |
| n rolls of two dice, summed | `rng.integers(1, 7, size=(n, 2)).sum(axis=1)` | `rowSums(matrix(sample(1:6, 2 * n, replace = TRUE), ncol = 2))` |
| n darts | `x, y = rng.uniform(-1, 1, (2, n))` | `x <- runif(n, -1, 1); y <- runif(n, -1, 1)` |
| Experimental probability | `np.mean(event)` | `mean(event)` |
| Running proportion | `np.cumsum(event) / np.arange(1, n + 1)` | `cumsum(event) / seq_len(n)` |

Python, R and Statistics+ each draw their own random numbers, so the same seed gives different counts in each. The Workbench's **Simulation** gives the same counts every time for the same seed.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Section 2-2 (interpretations of probability).
- EGN3443 Module 3, Topic 3: https://reaglin.github.io/EGN3443/week-4module-3-probability-concepts/
