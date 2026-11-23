---
id: st.m23.performance
title: Performance and reliability of systems
minutes: 45
objectives: [st.m23.o6]
example: st.m23.performance.eg
exercise: st.m23.performance.ex
reference: [system-performance]
---

# Performance and reliability of systems

Is the new version of a program faster? How many requests a second must a server handle? How often will the service be down? Computer engineers answer with statistics: run times vary from run to run, requests arrive at random, and components fail.

## Benchmarking: a run time is a sample

Run the same program on the same input 25 times and the times differ: other processes, caches, the CPU's clock. One run proves nothing. Report the **mean with a confidence interval** (item 7.3), or the median and percentiles.

The Example: 25 runs, mean 4.82 s, standard deviation 0.31 s (illustrative, made up for this book).

[[results: st.m23.performance.eg]]

[[figure: st.m23.performance.eg 1]]

- The 95 % interval for the mean run time is **4.69 to 4.95 s** (t with 24 degrees of freedom, 2.064).
- To claim a new version is faster, benchmark both the same way and compare them with a two-sample test (Module 10), or, for the same inputs run on both, a paired test (item 10.2).
- Do the **warm-up runs** first and leave them out: the first runs fill caches and are slower.

[[see-example: 1]]

## Arrivals: the Poisson model

Requests to a web server, calls to a help desk, jobs to a printer arrive one by one, independently, at a steady average rate: their number in a fixed time is **Poisson** (item 4.5), and the times between them exponential (item 5.4).

At an average of 8 requests a second, a server that can handle 12 a second is overloaded in a second with 13 or more: P(X > 12) = **0.064**, about one second in 16. **Queueing theory** builds on this: as the arrival rate approaches the service rate, waiting times grow without limit.

## Availability

A service's **availability** is the share of time it works:

availability = MTBF / (MTBF + MTTR) (mean time between failures, mean time to repair).

| Availability | Downtime per year |
|---|---|
| 99 % ("two nines") | 3.65 days |
| 99.9 % | 8.8 hours |
| 99.99 % | 53 minutes |

Systems combine components (item 3.4's rules for independent events):

- **In series** (all must work): A = A₁ × A₂. Two parts at 99 %: 98.0 %. A chain is weaker than its parts.
- **In parallel** (any one is enough, a backup): A = 1 − (1 − A₁)(1 − A₂). Two servers at 99 %: **99.99 %**. Redundancy is how reliable services are built from unreliable machines, as long as their failures are independent.

## In Python and R

```python
from scipy import stats
t = stats.t.ppf(0.975, df=24); half = t * 0.31 / 25**0.5    # benchmark interval
stats.poisson.sf(12, 8)                                       # P(more than 12 a second)
1 - (1 - 0.99) * (1 - 0.99)                                  # two servers in parallel
```

```r
4.82 + c(-1, 1) * qt(0.975, 24) * 0.31 / sqrt(25)
ppois(12, 8, lower.tail = FALSE)
1 - (1 - 0.99)^2
```

[[code: st.m23.performance.eg]]

## Common mistakes

- **One run** as the benchmark.
- **Including warm-up runs.**
- **The mean response time alone**: users notice the slow tail; report the 95th or 99th percentile too.
- **Parallel components that fail together** (one power supply, one data centre) counted as independent.

## What to do next

On the **Exercise** tab, a database query timed 16 times: the 95 % interval for its mean time.

[[example: Requests arriving at a server (Poisson)]]

Requests at an average of **8 a second**; the server handles **12**. P(more than 12 in a second) = **0.064**.

1. **Traffic grows to 10 a second** (move the mean to 10). Overload seconds become far more common: about one in five.
2. **Back to 8, and upgrade the server to 15 a second** (k = 15). Overload becomes rare: P(X > 15) is under 1 %.

[[try: st.m23.arrivals.try Rate=1..20 K | Exactly AtMost MoreThan Mean figure=1]]
