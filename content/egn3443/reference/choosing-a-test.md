---
title: Choosing a hypothesis test
---

# Choosing a hypothesis test

Ask three questions: what kind of data, how many groups, and (for two) independent or paired?

| Data | Groups | Test | Workbench | Module |
|---|---|---|---|---|
| Measurements | one, against a target | one-sample t (z if σ known) | One-sample test | 8 |
| A proportion | one, against a rate | one-sample z on a proportion | One-sample test | 8 |
| A spread | one, against a σ₀ | χ² test on a variance | One-sample test | — |
| Measurements | two independent samples | two-sample t (pooled or Welch) | Two-sample test | 9, 10 |
| Measurements | the same units twice | paired t | Two-sample test | 9 |
| Two proportions | two independent samples | two-proportion z | Two-sample test | 10 |
| Measurements | three or more groups | one-way ANOVA | One-way ANOVA | 14 |
| Counts of one variable | against stated proportions | χ² goodness of fit | Chi-square tests | 8 |
| Counts of two variables | a contingency table | χ² independence | Chi-square tests | 8 |

**Measurements vs counts.** A length, time or strength is a measurement: compare means. "How many fell in each category" is a count: use χ².

**Paired vs independent.** Paired when each value in one sample is matched to one in the other (same part, same specimen, same day). Pairing removes the unit-to-unit variation; analysing paired data as independent wastes it.

**Not normal, small samples?** Module 9 gives the rank tests (Mann–Whitney, Wilcoxon).

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapters 9, 10 and 13 (summary tables at the end of each).
- EGN3443 Module 8, topic 5, Applying Common Hypothesis Tests, on reaglin.github.io/EGN3443.
