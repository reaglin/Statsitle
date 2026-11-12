---
id: st.m14.introduction
title: Introduction to ANOVA
minutes: 40
objectives: [st.m14.o1]
example: st.m14.introduction.eg
exercise: st.m14.introduction.ex
reference: [familywise-error, one-way-anova]
---

# Introduction to ANOVA

## What ANOVA is

**Analysis of variance (ANOVA)** tests whether several groups share one mean. The groups are the **levels** of a **factor**: three curing temperatures, four resin suppliers, five nozzle designs. The measurement made on each unit is the **response**. With k groups, the hypotheses are

H₀: μ₁ = μ₂ = … = μₖ (all the group means are equal)

H₁: at least one μᵢ differs from the others

H₁ does **not** say that every mean differs from every other. Rejecting H₀ says only that the means are not all equal. Finding which ones differ is the job of the post-hoc tests in item 14.6.

## Why not just use many t-tests?

With three groups you could run three two-sample t-tests (Modules 9 and 10): 1 against 2, 1 against 3, 2 against 3. With k groups there are k(k − 1)/2 pairs, so k = 4 needs 6 tests and k = 6 needs 15.

Each test at α = 0.05 has a 5 % chance of a false alarm (a Type I error, Module 8) when its H₀ is true. Run many tests and the chance that **at least one** of them gives a false alarm, the **familywise error rate**, grows. If the m tests were independent:

P(at least one false alarm) = 1 − P(no false alarm in any test) = 1 − (1 − α)ᵐ

| Groups k | Pairs m = k(k − 1)/2 | Familywise error rate, 1 − 0.95ᵐ |
|---|---|---|
| 2 | 1 | 0.050 |
| 3 | 3 | 0.143 |
| 4 | 6 | 0.265 |
| 5 | 10 | 0.401 |
| 6 | 15 | 0.537 |

With five groups that are all the same, a researcher who runs every t-test has about a 40 % chance of announcing a difference that is not there. The pairwise tests are not quite independent (the 1-against-2 and 1-against-3 tests share group 1), so the true rate is somewhat lower: a simulation of all 10 pairwise t-tests on five equal groups of 10 gives about 0.28, and of the 3 tests on three groups about 0.12. That is still several times the 0.05 promised.

ANOVA avoids the problem by asking one question, "are all the means equal?", with **one** test at level α. A **correction** such as Bonferroni's is the other way out: run each of the m tests at α/m. With 10 tests at 0.005 each, 1 − 0.995¹⁰ = 0.049, back under 0.05, at the cost of each test being much less able to detect a real difference.

## When ANOVA is the right choice

- The response is a **measurement** (strength, time, viscosity), not a count or a yes/no.
- The groups are levels of one or more **factors** (categories), with **independent** units in each group.
- You want to know whether the factor changes the **mean** response.

ANOVA works with two groups too: then F = t², and it gives the same p-value as the pooled t-test. It earns its keep with three or more.

## One-way and multi-factor designs

A **one-way** ANOVA has one factor: bond strength at three curing temperatures (items 14.3 and 14.4). A **two-way** (multi-factor) ANOVA has two or more factors studied together: yield at two catalysts × three temperatures (item 14.5). The second factor brings a new question: does the effect of one factor depend on the level of the other? That is the **interaction**.

## What ANOVA assumes (item 14.2)

The same three conditions as the pooled t-test of Modules 9 and 10 (item 10.3): the observations are **independent**; within each group the response is roughly **normal**; and the groups have **equal variances**. Item 14.2 shows how to check them, and what to do when they fail.

## The Example: ten t-tests where nothing differs

The **Simulation** analysis can play the researcher with five groups many times. Each trial runs 10 t-tests, every one on data where H₀ is true, and records how many reject at α = 0.05.

[[results: st.m14.introduction.eg]]

In 410 of the 1000 trials at least one of the 10 tests rejected: an experimental probability of 0.410, close to the 0.401 of the formula.

[[figure: st.m14.introduction.eg 1]]

The running proportion settles on the theoretical line as the trials accumulate.

[[figure: st.m14.introduction.eg 2]]

The number of tests that reject in one trial follows a binomial distribution with n = 10 and p = 0.05 (Module 4): no rejection 59.9 % of the time, one 31.5 %, two 7.5 %. The dark bars are the trials with at least one rejection, the event.

The Simulation runs each test as a one-sample t-test on its own sample of n values. Any test of a true H₀ at level α rejects with probability α, so the kind of t-test does not change the answer. The tests are **independent**, so the result matches the formula 1 − (1 − α)ᵐ. As said above, the 10 pairwise comparisons of five real groups share data, and their familywise rate is lower (about 0.28), but far above 0.05 all the same.

## In Python and R

```python
alpha, m = 0.05, 10
print(1 - (1 - alpha) ** m)          # 0.4013: the familywise error rate
print(1 - (1 - alpha / m) ** m)      # 0.0489: with Bonferroni's alpha/m
```

```r
alpha <- 0.05; m <- 10
1 - (1 - alpha)^m                    # 0.4013
1 - (1 - alpha / m)^m                # 0.0489
```

The Code tab writes the simulation itself:

[[code: st.m14.introduction.eg]]

## In the Workbench

Choose **Simulation**, set **Experiment** to *Many t-tests: the familywise error*, and set **Tests per trial**, **Sample size (n)**, **Significance level (α)**, **Number of trials** and **Seed**. Change the tests per trial from 1 to 20 and watch the theoretical probability climb.

## Common mistakes

- **Adding the α's.** Ten tests at 0.05 do not give a familywise rate of 10 × 0.05 = 0.50. That sum is only an upper bound; the probability is 1 − 0.95¹⁰ = 0.401.
- **Counting the groups instead of the pairs.** Four groups give 6 pairwise tests, not 4.
- **Reading H₁ as "all the means differ".** It says at least one differs.
- **Running all the t-tests anyway after ANOVA rejects.** The follow-up needs its own protection against many comparisons (item 14.6).

## What to do next

On the **Exercise** tab, an engineer compares four impeller designs pair by pair. Work out the familywise error rate and check it with the Simulation.
