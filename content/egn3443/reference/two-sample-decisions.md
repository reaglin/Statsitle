---
title: Two-sample results and decisions
---

# Two-sample results and decisions

**Before the data:** the question, which group is 1, H₀ and H₁ (one- or two-sided), α from the costs of the two errors, and any practical threshold Δ₀.

**The written conclusion, in order:**

1. The conclusion in plain words: is there a difference, which way, how big, and what it means.
2. The design and the data: who or what was measured, how many, independent or paired.
3. The test and why; how the assumptions were checked (Shapiro–Wilk, Levene, success–failure counts).
4. The statistic, its degrees of freedom and the exact p-value.
5. The confidence interval for the difference, in the reader's units.
6. The decision, its risks (Type I or II), and the next step.

**Reading the interval against a threshold:**

| Interval | Meaning |
|---|---|
| excludes 0, all values matter | real and important: act |
| excludes 0, no value matters | real but too small to matter |
| contains 0, narrow | no difference that matters |
| contains 0, holds values that matter | too few data: collect more |

**Testing "better by enough":** H₀: μ₁ − μ₂ = Δ₀ against H₁: μ₁ − μ₂ > Δ₀, t = (x̄₁ − x̄₂ − Δ₀)/SE (Workbench: Hypothesised difference (Δ₀)).

**Avoid:** "not significant, so equal"; "significant, so important"; p as the probability that H₀ is true; choosing α or the direction after seeing the data; "p < 0.05" without the number.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 9.2 (P-values, practical and statistical significance) and 10.1–10.6.
- Wasserstein, R. L. and Lazar, N. A. (2016), The ASA statement on p-values: context, process, and purpose, *The American Statistician* 70, 129–133.
