---
id: st.m23.experiments
title: Online experiments
minutes: 45
objectives: [st.m23.o3]
example: st.m23.experiments.eg
exercise: st.m23.experiments.ex
reference: [online-experiments]
---

# Online experiments

Large websites and apps run thousands of **A/B tests** a year: a new layout, a new ranking, a new price, each shown to a random share of users. Item 20.6 introduced the A/B test for a shop's checkout page. At the scale of millions of users, three things change: **tiny differences become significant**, the temptation to **peek** at results grows, and every test tracks **many metrics**.

## The Example: 200,000 users in each arm

A streaming service tested a new recommendation layout. 200,000 users were randomly assigned to each version; the outcome is starting a second title (illustrative data, made up for this book): **5.20 %** on the old layout (A), **5.35 %** on the new (B).

[[results: st.m23.experiments.eg]]

[[figure: st.m23.experiments.eg 1]]

- z = −2.12, p = **0.034**: the difference is "significant".
- The lift is **0.15 percentage points** (95 % interval 0.01 to 0.29), a relative lift of 2.9 %.
- **Practical significance** is now the real question. Is a lift that might be as small as 0.01 points worth the cost of building and maintaining the new layout? At this scale p-values are small for almost anything; the interval for the lift is what informs the decision.

[[see-example: 1]]

## Peeking

A dashboard shows the p-value updating every hour. Stopping the test the first time p < 0.05 makes a false winner far more likely than 5 %: with daily looks over a month, the chance of crossing 0.05 at some point under H₀ is several times α. Remedies:

- **Fix the sample size in advance** from a power calculation (item 22.6), and test once.
- Use **sequential tests** designed for repeated looks, which spend α gradually (Part II).

## Many metrics

Each test reports dozens of metrics: clicks, sessions, revenue, cancellations, by country and device. With 30 metrics and nothing really changed, the chance that at least one is "significant" at 0.05 is 1 − 0.95³⁰ ≈ **0.79**. Good practice:

- name **one primary metric** before the test;
- treat the rest as **guardrails** (things that must not get worse) or as ideas for the next test;
- correct for multiplicity (the false discovery rate of item 19.5) when many are judged.

## Other habits of online experimentation

- **A/A tests**: run two identical versions to check that the system finds no difference about 95 % of the time.
- **Sample ratio mismatch**: if 50/50 assignment gave 200,000 and 196,000 users, something is broken; check before reading the results.
- **Novelty effects**: users click a new design because it is new; run long enough for it to wear off.

## In Python and R

```python
from statsmodels.stats.proportion import proportions_ztest, confint_proportions_2indep
z, p = proportions_ztest([10400, 10700], [200000, 200000])
low, high = confint_proportions_2indep(10700, 200000, 10400, 200000, method="wald")
```

```r
prop.test(c(10400, 10700), c(200000, 200000), correct = FALSE)
1 - 0.95^30                                  # chance of a false winner among 30 metrics
```

[[code: st.m23.experiments.eg]]

## Common mistakes

- **Significant equals important** at huge sample sizes.
- **Stopping at the first p < 0.05.**
- **Reporting the metric that moved**, out of thirty.
- **Ignoring a sample ratio mismatch.**

## What to do next

On the **Exercise** tab, a new sign-up button tested on 150,000 visitors per arm: the test, the interval for the lift, and whether it is worth having.

[[example: Many metrics: something always moves]]

Simulated A/B tests where the change does nothing, each checking **30 metrics** at α = 0.05. In **779 of 1,000** tests at least one metric was "significant" (theory: 1 − 0.95³⁰ = 0.785).

1. **Move the number of metrics to 1**: back to about 5 %, the rate a single primary metric promises.
2. **Back to 30, and move α to 0.05/30 = 0.0017** (a Bonferroni correction): the chance of any false winner falls to about 5 %.

[[try: st.m23.metrics.try Tests=1..60 Alpha=0.001..0.1 | Occurrences ExperimentalProbability TheoreticalProbability figure=1]]
