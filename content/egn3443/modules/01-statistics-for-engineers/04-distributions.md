---
id: st.m1.distributions
title: Probability distributions
minutes: 45
objectives: [st.m1.o2]
builds-on: st.m1.variability
example: st.m1.distributions.eg
quiz: quizzes/m1-distributions.json
reference: [distributions-preview]
---

# Probability distributions

*A preview. Modules 4 and 5 teach these distributions fully, with their formulas worked step by step. This item is about what a distribution is, the three that engineers use most, and how to choose one.*

## What a probability distribution is

The histogram in item 1.3 showed how often each value occurred in the **data**. A **probability distribution** is the same idea for the **process**: a model that says how likely each value is, before you measure. A quantity whose value depends on chance is a **random variable**, written with a capital letter: X = the number of defective chips in a sample, or the volume in a bottle.

- A **discrete** random variable takes separate values, usually counts: 0, 1, 2, … Its distribution gives the probability of each value, P(X = k).
- A **continuous** random variable can take any value in a range: a length, a time, a volume. Its distribution is a curve, the **density**, and a probability is an **area** under it: P(X ≤ x).

With a distribution, an engineer can predict: how many parts in a lot will be defective, how often a server is overloaded, how many bottles are underfilled.

## The binomial distribution: successes in n trials

Use it when you count how many of **n** independent trials have some outcome, and each trial has the **same probability p** of it. "Success" is just the outcome you count; for an inspector it is often a defect.

P(X = k) = C(n, k) pᵏ (1 − p)ⁿ⁻ᵏ, with mean μ = np.

**Worked case.** A process makes 5 % defective chips, and 20 chips are tested. X, the number defective, is binomial with n = 20 and p = 0.05.

- No defective chips: P(X = 0) = 0.95²⁰ = **0.3585**.
- Exactly one: P(X = 1) = 20 × 0.05 × 0.95¹⁹ = **0.3774**.
- At least one: P(X ≥ 1) = 1 − 0.3585 = **0.6415**.
- On average, μ = np = 20 × 0.05 = **1** defective chip per sample.

The Example works this case:

[[figure: st.m1.distributions.eg 1]]

[[results: st.m1.distributions.eg]]

## The Poisson distribution: events in an interval

Use it when you count **events that happen at random** in a fixed interval of time, length, area or volume, at a known **mean rate λ** per interval, with no fixed number of trials: calls per hour, flaws per meter of cable, cars arriving per minute, failures per year.

P(X = k) = e^(−λ) λᵏ / k!, with mean μ = λ.

**Worked case.** A help desk receives on average λ = 3 calls an hour. The chance of no call in an hour is P(X = 0) = e⁻³ = **0.0498**. The chance of exactly 5 calls is e⁻³ × 3⁵ / 5! = 0.0498 × 243 / 120 = **0.1008**.

[[figure: st.m1.distributions.poisson 1]]

[[results: st.m1.distributions.poisson]]

If the desk is staffed for 5 calls an hour, P(X > 5) = 0.0839: in about one hour in twelve, more calls arrive than it can handle.

## The normal distribution: the bell curve

Use it for **continuous measurements** that vary symmetrically around a mean because of many small independent effects: dimensions, fill volumes, strengths, measurement errors. It has two parameters, the mean μ and the standard deviation σ.

The **z-score** says how many standard deviations a value is from the mean: z = (x − μ) / σ. The **empirical rule**: about 68 % of values are within μ ± σ, 95 % within μ ± 2σ, and 99.7 % within μ ± 3σ.

**Worked case.** A filler puts μ = 500 mL in each bottle with σ = 4 mL. The label says 492 mL. z = (492 − 500) / 4 = −2, so 492 mL is two standard deviations below the mean. By the empirical rule about 95 % of bottles are within 492 to 508 mL, and the 5 % outside are split equally between the two tails, so about 2.5 % hold less than 492 mL. The exact value, from the normal table or the program, is **0.0228** (2.28 %).

[[figure: st.m1.distributions.normal 1]]

[[results: st.m1.distributions.normal]]

## Selecting the right distribution

Ask these questions in order:

| Question | If yes |
|---|---|
| Is the quantity measured on a continuous scale, and roughly symmetric around a mean? | **Normal** |
| Is it a count of successes in a fixed number n of independent trials, each with the same p? | **Binomial** |
| Is it a count of random events in an interval, at a mean rate, with no fixed number of trials? | **Poisson** |

| Scenario | Distribution | Parameters |
|---|---|---|
| Defective units among 50 sampled from a line that makes 2 % defects | binomial | n = 50, p = 0.02 |
| Potholes per kilometer of road, averaging 1.5 | Poisson | λ = 1.5 per km |
| Diameters of shafts from a lathe | normal | μ, σ |
| Bolts in a box of 100 that pass a proof test, each passing with probability 0.99 | binomial | n = 100, p = 0.99 |
| Cars arriving at a toll booth in a minute, averaging 4 | Poisson | λ = 4 per minute |

A clue: if you can say "out of how many?", the count is binomial. If there is no "out of", only "per hour" or "per meter", it is Poisson.

## Using distributions to predict

Once a distribution fits, it answers questions about the future: the chance a lot passes inspection, the staffing a help desk needs, the share of bottles below the label. Item 1.6 uses the normal distribution to set the limits of a control chart, and Modules 4 and 5 teach many more distributions (geometric, hypergeometric, exponential, Weibull …).

## In Python and R

```python
from scipy import stats
print(stats.binom.pmf(0, 20, 0.05))     # 0.3585  P(X = 0), binomial n = 20, p = 0.05
print(stats.poisson.pmf(5, 3))          # 0.1008  P(X = 5), Poisson λ = 3
print(stats.norm.cdf(492, 500, 4))      # 0.0228  P(X ≤ 492), normal μ = 500, σ = 4
```

```r
dbinom(0, 20, 0.05)     # 0.3585
dpois(5, 3)             # 0.1008
pnorm(492, 500, 4)      # 0.0228
```

In both languages, `pmf` / `d…` gives P(X = k) and `cdf` / `p…` gives P(X ≤ k).

## Common mistakes

- Using the binomial when the probability changes from trial to trial, or the trials are not independent.
- Using the Poisson when there is a fixed number of trials (that is binomial).
- Using the normal for a count of rare events, or for a strongly skewed quantity such as repair times.
- Asking a continuous distribution for P(X = x). For a continuous variable that probability is 0; ask for P(X ≤ x) or the probability of a range.

## What to do next

Open the **Example** tab and press **Open in the Workbench**. Change the defect rate p from 0.05 to 0.10 and watch P(X = 0) fall. Then answer the "which distribution?" questions on the **Exercise** tab.
