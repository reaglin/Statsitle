---
id: st.m4.poisson
title: The Poisson distribution
minutes: 45
objectives: [st.m4.o3, st.m4.o4, st.m4.o5]
builds-on: st.m4.binomial
example: st.m4.poisson.eg
exercise: st.m4.poisson.ex
reference: [poisson-distribution, which-distribution]
---

# The Poisson distribution

## When it applies

The Poisson distribution counts **events in a fixed interval** of time, length, area or volume: calls per hour, flaws per kilometer of cable, potholes per mile, bacteria per milliliter. It applies when:

1. Events happen **one at a time**, at random.
2. Events are **independent**: one event does not make the next more or less likely.
3. The **average rate is constant** over the interval.

There is no fixed number of trials and no upper limit on X: X can be 0, 1, 2, … without end. That is the clearest way to tell a Poisson count from a binomial one.

## The formula

P(X = x) = e^(−λ) · λˣ / x!, for x = 0, 1, 2, …

λ (lambda) is the **mean number of events in the interval of the question**. The Poisson has one parameter, and its mean and variance are both λ:

μ = λ, σ² = λ, σ = √λ

## Scale λ to the interval first

A rate is given per some interval, and the question may use another. Scale the rate before anything else:

λ = (rate per unit) × (length of the interval asked about)

If a weld has 2 flaws per 10 m on average, then 5 m of weld has λ = 1, and 25 m has λ = 5. Using the wrong λ is the most common mistake with the Poisson.

## A worked case by hand

A weld has on average λ = 2 flaws per 10 m. Let X be the number of flaws in a 10 m seam. (e⁻² = 0.135335.)

1. **No flaws:** P(X = 0) = e⁻² × 2⁰ / 0! = **0.1353**.
2. **One flaw:** P(X = 1) = e⁻² × 2¹ / 1! = **0.2707**.
3. **Two flaws:** P(X = 2) = e⁻² × 2² / 2! = 0.135335 × 2 = **0.2707**.
4. **At most two:** P(X ≤ 2) = 0.135335 + 0.270671 + 0.270671 = **0.6767**.
5. **Three or more:** P(X ≥ 3) = 1 − 0.676676 = **0.3233**. There is no other way: X has no upper limit to add up to.
6. **A 5 m seam:** λ = 1, so P(no flaws) = e⁻¹ = **0.3679**.

## Poisson as an approximation to the binomial

When n is large and p is small, a binomial count behaves like a Poisson count with λ = np. For 200 parts each defective with probability 0.01, λ = 2: the binomial gives P(X = 0) = 0.99²⁰⁰ = 0.1340, and the Poisson gives e⁻² = 0.1353. A common rule of thumb is n ≥ 20 with p ≤ 0.05.

## The Example: flaws in a fiber-optic cable

A fiber-optic cable has on average 0.6 flaws per kilometer. A reel holds 5 km, so the number of flaws on a reel has λ = 0.6 × 5 = 3.

[[figure: st.m4.poisson.eg 1]]

[[results: st.m4.poisson.eg]]

P(X = 2) = e⁻³ × 3² / 2! = 0.2240, and a reel has at most two flaws with probability 0.4232. So more than half the reels (0.5768) have three or more. The mean and the variance are both 3, so σ = √3 = 1.73 flaws.

[[figure: st.m4.poisson.eg 2]]

## In Python and R

```python
from scipy import stats
lam = 0.6 * 5          # scale the rate first: 3 flaws per reel
X = stats.poisson(lam)
print(X.pmf(2))        # P(X = 2) = 0.2240
print(X.cdf(2))        # P(X ≤ 2) = 0.4232
print(X.sf(2))         # P(X > 2) = 0.5768
```

```r
lam <- 0.6 * 5
dpois(2, lam)          # P(X = 2) = 0.2240
ppois(2, lam)          # P(X ≤ 2) = 0.4232
1 - ppois(2, lam)      # P(X > 2) = 0.5768
```

[[code: st.m4.poisson.eg]]

## Binomial or Poisson?

| Ask | Binomial | Poisson |
|---|---|---|
| Is there a fixed number of trials n? | yes | no: events in an interval |
| What is the largest X can be? | n | no limit |
| The parameters | n and p | λ, the mean count in the interval |
| Mean and variance | np and np(1 − p): variance below the mean | λ and λ: variance equals the mean |

## Common mistakes

- Using the rate per hour for a question about an 8-hour shift. Scale λ first.
- Trying to add P(X ≥ k) term by term. It never ends; use 1 − P(X ≤ k − 1).
- Using the Poisson when events are not independent, such as failures that cause more failures, or arrivals that come in groups.
- Using the Poisson for a count out of a fixed n with a large p: use the binomial.

## What to do next

On the **Exercise** tab, a utility's emergency line gets calls at a known rate per hour, and you need probabilities for a whole shift. Scale λ first.
