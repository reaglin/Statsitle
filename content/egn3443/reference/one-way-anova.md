---
title: One-way ANOVA
---

# One-way ANOVA

H₀: μ₁ = μ₂ = … = μₖ. H₁: at least one mean differs. k groups, nᵢ in group i, N = Σ nᵢ, ȳᵢ the group means, ȳ the grand mean.

| Source | df | SS | MS | F |
|---|---|---|---|---|
| Between groups | k − 1 | Σ nᵢ(ȳᵢ − ȳ)² | SS between/(k − 1) | MS between/MS within |
| Within groups | N − k | Σ Σ (yᵢⱼ − ȳᵢ)² = Σ (nᵢ − 1)sᵢ² | SS within/(N − k) | |
| Total | N − 1 | Σ Σ (yᵢⱼ − ȳ)² | | |

- **p-value:** the area above F under the F distribution with k − 1 and N − k df. Reject H₀ when p < α, or when F > F(α; k − 1, N − k).
- **MS within** is the pooled variance; √(MS within) estimates the common σ.
- **η²** = SS between / SS total, the share of the variation the factor explains (about 0.01 small, 0.06 medium, 0.14 large).
- **ω²** = (SS between − (k − 1) MS within) / (SS total + MS within), a less optimistic estimate.
- With k = 2, F = t² of the pooled t-test.

## Steps

1. State H₀ and H₁; choose α.
2. Check independence (design), normality (residuals), equal variances (Levene).
3. Build the ANOVA table; find F and p.
4. Decide; give η² and the group means.
5. If H₀ is rejected, compare the pairs with Tukey's HSD.

## Python and R

| | Python | R |
|---|---|---|
| F and p | `stats.f_oneway(a, b, c)` | `summary(aov(y ~ factor(g)))` |
| The table | `sm.stats.anova_lm(ols("y ~ C(g)", data=d).fit())` | `anova(lm(y ~ factor(g)))` |
| Critical value | `stats.f.ppf(1 - alpha, k - 1, N - k)` | `qf(1 - alpha, k - 1, N - k)` |
| p from F | `stats.f.sf(F, k - 1, N - k)` | `pf(F, k - 1, N - k, lower.tail = FALSE)` |

Level codes must be categories: `C(g)` in Python, `factor(g)` in R. Check that df between is k − 1.

## In the Workbench

**One-way ANOVA**. **Layout**: *Each column is a group*, or *A factor column and a response column* (then also **Factor column** and **Response column**). It gives the table, F, F critical, p, η², ω², power, Tukey's HSD, the assumption checks and Kruskal–Wallis, and draws the box plots, the group means with intervals and the residuals' Q–Q plot.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 13.1–13.2.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 7.4.3 (are the means equal?): https://www.itl.nist.gov/div898/handbook/prc/section4/prc43.htm
