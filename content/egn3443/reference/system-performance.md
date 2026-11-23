---
title: Performance and reliability of computer systems
---

# Performance and reliability of computer systems

| Quantity | Formula |
|---|---|
| Benchmark interval | x̄ ± t(α/2, n − 1) s/√n (item 7.3) |
| Arrivals in time t | Poisson with mean λt (item 4.5); times between arrivals exponential (item 5.4) |
| Overload | P(X > capacity) |
| Utilisation (one server) | ρ = arrival rate / service rate; waiting grows without limit as ρ → 1 |
| Availability | MTBF/(MTBF + MTTR) |
| In series | A₁ × A₂ × … |
| In parallel | 1 − (1 − A₁)(1 − A₂)… |
| Downtime per year | (1 − A) × 8,760 hours |

Report percentiles (95th, 99th) of response times, not only the mean; leave out warm-up runs.

## Python and R

| Task | Python | R |
|---|---|---|
| Benchmark interval | `stats.t.interval(0.95, n - 1, loc=mean, scale=s/np.sqrt(n))` | `t.test(times)$conf.int` |
| Overload probability | `stats.poisson.sf(capacity, rate)` | `ppois(capacity, rate, lower.tail = FALSE)` |

**Read more:** Jain, *The Art of Computer Systems Performance Analysis* (Wiley).
