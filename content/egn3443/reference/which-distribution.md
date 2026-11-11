---
title: Which discrete distribution?
---

# Which discrete distribution?

| Model | X counts | Clue | Parameters | PMF | Mean | Variance |
|---|---|---|---|---|---|---|
| Bernoulli | 1 if one trial succeeds, else 0 | a single trial | p | pˣ(1 − p)¹⁻ˣ, x = 0, 1 | p | p(1 − p) |
| Binomial | successes in n trials | fixed n, same p, independent | n, p | C(n, x)pˣ(1 − p)ⁿ⁻ˣ | np | np(1 − p) |
| Poisson | events in an interval | a rate "per hour", "per km"; no upper limit | λ | e^(−λ)λˣ / x! | λ | λ |
| Geometric | trials to the first success | "until the first" | p | (1 − p)ˣ⁻¹p, x = 1, 2, … | 1/p | (1 − p)/p² |
| Pascal (negative binomial) | trials to the r-th success | "until the r-th" | r, p | C(x − 1, r − 1)pʳ(1 − p)ˣ⁻ʳ, x = r, r + 1, … | r/p | r(1 − p)/p² |
| Hypergeometric | successes in n draws without replacement | a small lot; no replacement | N, K, n | C(K, x)C(N − K, n − x) / C(N, n) | nK/N | n(K/N)(1 − K/N)(N − n)/(N − 1) |

## Three questions

1. **Fixed number of trials?** Yes: binomial, hypergeometric or Bernoulli (n = 1). Events in an interval: Poisson. Keep going until something happens: geometric or Pascal.
2. **Same p on every trial?** Not when drawing without replacement from a small lot: hypergeometric. (Large lot, n under about N/10: the binomial is close.)
3. **Waiting for one success or for r?** One: geometric. r: Pascal.

## Python and R

| Model | Python (scipy.stats) | R |
|---|---|---|
| Binomial | `binom(n, p)` | `dbinom(x, n, p)`, `pbinom` |
| Poisson | `poisson(lam)` | `dpois(x, lam)`, `ppois` |
| Geometric (trials) | `geom(p)` | `dgeom(x - 1, p)`: R counts failures |
| Pascal (trials) | `nbinom(r, p).pmf(x - r)`: scipy counts failures | `dnbinom(x - r, r, p)` |
| Hypergeometric | `hypergeom(N, K, n)` | `dhyper(x, K, N - K, n)` |

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 3-5 to 3-9.
- EGN3443 Module 4, Topic 6, and the *Distribution Detective* case game: https://reaglin.github.io/EGN3443/week-5module-4-discrete-probability-distributions/
