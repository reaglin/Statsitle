---
title: The steps of a hypothesis test
---

# The steps of a hypothesis test

| Step | What to do |
|---|---|
| 1. Parameter | μ (a measurement), p (a proportion), σ, or a set of category proportions |
| 2. Hypotheses | H₀ holds the equals sign: μ = μ₀. H₁ comes from the question: μ ≠ μ₀, μ < μ₀ or μ > μ₀ |
| 3. α | chosen before the data: 0.05 usually, 0.01 when a false alarm is costly, 0.10 for screening |
| 4. Statistic | (estimate − value in H₀) / standard error; χ² for counts |
| 5. p-value | P(a statistic at least this extreme \| H₀ true), in the direction(s) of H₁ |
| 6. Decision | p < α: reject H₀. p ≥ α: do not reject H₀ |
| 7. Conclusion | in words, about the population, with the statistic, df, p, the size of the effect, and what it means |

## Tails

| H₁ | Tail | p-value from z | Critical region at α |
|---|---|---|---|
| μ > μ₀ | upper | 1 − Φ(z) | z > z_α (1.645 at 0.05) |
| μ < μ₀ | lower | Φ(z) | z < −z_α |
| μ ≠ μ₀ | both | 2(1 − Φ(\|z\|)) | \|z\| > z_{α/2} (1.960 at 0.05) |

Choose the tail from the question, before the data. Use one tail only when a result in the other direction would lead to the same action as no result.

## Wording

- Reject: "The data give evidence that … (statistic, df, p)."
- Do not reject: "The data do not give evidence that …". Never "accept H₀" or "proves".
- Report the size of the effect in its units, and compare it with a tolerance or a cost: statistical significance is not practical significance.
- After not rejecting, give the power against the smallest difference that matters.

## Python and R

| Task | Python (scipy.stats) | R |
|---|---|---|
| Upper tail of z | `stats.norm.sf(z)` | `pnorm(z, lower.tail = FALSE)` |
| Lower tail of z | `stats.norm.cdf(z)` | `pnorm(z)` |
| Two-sided p from t | `2 * stats.t.sf(abs(t), df)` | `2 * pt(abs(t), df, lower.tail = FALSE)` |
| Critical z, t | `stats.norm.ppf(1 - alpha)`, `stats.t.ppf(1 - alpha/2, df)` | `qnorm(1 - alpha)`, `qt(1 - alpha/2, df)` |

## In the Workbench

**One-sample test** writes the hypotheses, the statistic, the rejection region, the p-value and the decision side by side, and draws the reference curve with the rejection region shaded and the observed statistic marked.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 9-1 (hypothesis testing) and 9-2 to 9-4 (tests on a mean, a variance).
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.1.3, What are statistical tests?: https://www.itl.nist.gov/div898/handbook/prc/section1/prc13.htm
- EGN3443 Module 8 (Week 9, Hypothesis Testing) on the course website, reaglin.github.io/EGN3443.
