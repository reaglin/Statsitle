---
id: st.m10.foundations
title: Foundations of two-sample testing
minutes: 35
objectives: [st.m10.o1, st.m10.o5]
builds-on: st.m9.interpreting
quiz: quizzes/m10-foundations.json
reference: [hypothesis-testing-steps, independent-and-paired-designs, choosing-a-two-sample-test]
---

# Foundations of two-sample testing

**A review, with one new part.** Item 9.1 (*Two-sample hypothesis tests*) set out the logic of a two-sample test. This page collects it in one place, adds **proportions**, which are new in this module, and asks you to set up tests before any arithmetic.

## Why two samples

Module 8 compared one sample with a fixed number: a specification, a legal limit, a rated value. Often there is no such number. The question is **relational**: is the new process better than the old one? Do two suppliers differ? Then you sample both and compare them with each other.

## What is compared

| Outcome | Parameter | H₀ (no difference) | Test |
|---|---|---|---|
| A measurement, two independent groups | the means μ₁ and μ₂ | μ₁ − μ₂ = 0 | two-sample t (Welch or pooled), or z when σ₁ and σ₂ are known |
| A measurement, paired | the mean difference μ_d | μ_d = 0 | paired t |
| Pass or fail, yes or no | the proportions p₁ and p₂ | p₁ − p₂ = 0 | **two-proportion z** (item 10.5) |
| The spread of a measurement | the variances σ₁² and σ₂² | σ₁² = σ₂² | F-test; Levene's test (item 9.4) |

Here μ_d is the mean of the differences within pairs, p₁ and p₂ are the population proportions (the share defective, the share that pass), and p̂₁ = x₁/n₁ and p̂₂ = x₂/n₂ are the sample proportions: x items with the attribute out of n.

## The shape of every two-sample statistic

statistic = (observed difference − difference under H₀) / standard error of the difference

The observed difference is x̄₁ − x̄₂, d̄ or p̂₁ − p̂₂. The difference under H₀ is usually 0; Module 9 called it Δ₀ (item 10.4 tests against a Δ₀ that is not 0). The **standard error** (SE) is how much the observed difference varies from sample to sample. The statistic counts how many standard errors the data lie from H₀, and the reference distribution (t or the standard normal Z) turns that into a p-value.

## Six steps

Topic 1 of Module 10 on the course website (reaglin.github.io/EGN3443) gives six steps. They follow Module 8's steps (the Reference page *The steps of a hypothesis test*), with the parameter, hypotheses and α together as step 1, the check of assumptions added as step 2, and the conclusion written in context as step 6:

1. **State H₀ and H₁, and choose α** before collecting the data.
2. **Check the design and the assumptions** (item 10.3), and collect the data.
3. **Compute the test statistic.**
4. **Find the p-value** (or compare the statistic with the critical value).
5. **Decide:** reject H₀ when p < α; otherwise do not reject it.
6. **Interpret in context:** the size of the difference with its confidence interval, and what it means for the decision (item 10.6).

## Which group is 1, and which direction

Name the groups first and keep the order. With group 1 the old process, p̂₁ − p̂₂ > 0 means the old process has the higher rate; swap the names and every difference, statistic and interval changes sign.

H₁ is one of three:

| H₁ | Use when | p-value |
|---|---|---|
| μ₁ − μ₂ ≠ 0 (two-sided) | a difference either way matters, or no direction was predicted | 2 × P(T > \|t\|) |
| μ₁ − μ₂ > 0 (one-sided) | only an increase of group 1 over group 2 matters, and this was decided before the data were seen | P(T > t) |
| μ₁ − μ₂ < 0 (one-sided) | only a decrease matters, decided in advance | P(T < t) |

Here t is the statistic the data gave, and T stands for a value drawn from the t distribution with the test's degrees of freedom. The same three forms hold for p₁ − p₂, where the p-value comes from the standard normal Z and the observed z instead.

Choosing the direction **after** seeing which way the data point is called HARKing (hypothesizing after the results are known). It halves the p-value without any new evidence, so the real chance of a Type I error is double the α you state.

## Two things a test cannot say

- **"Do not reject H₀" is not "the groups are equal".** It means the data cannot tell them apart, often because the samples are small. Item 10.6 shows how the confidence interval tells the two apart.
- **A small p is not a big difference.** It says the difference is probably real, not that it matters.

## What to do next

On the **Exercise** tab, six questions ask you to choose the parameter, write the hypotheses and compute a statistic from its parts. They are practice questions: after a wrong try the explanation shows. You need 80 % to complete the item.
