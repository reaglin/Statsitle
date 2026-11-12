---
title: Two-way ANOVA
---

# Two-way ANOVA

Factor A with a levels, factor B with b levels, n runs in each of the ab cells (balanced), N = abn. ȳᵢⱼ cell means, ȳᵢ. level means of A (averaged over B), ȳ.ⱼ level means of B (averaged over A), ȳ grand mean.

| Source | df | SS | F |
|---|---|---|---|
| A | a − 1 | bn Σ (ȳᵢ. − ȳ)² | MS A / MS error |
| B | b − 1 | an Σ (ȳ.ⱼ − ȳ)² | MS B / MS error |
| A × B | (a − 1)(b − 1) | n Σ Σ (ȳᵢⱼ − ȳᵢ. − ȳ.ⱼ + ȳ)² | MS AB / MS error |
| Error | ab(n − 1) | Σ Σ Σ (y − ȳᵢⱼ)² | |
| Total | abn − 1 | Σ (y − ȳ)² | |

Each MS = SS / df. Each F is compared with the F distribution on its df and the error df.

- **Main effect:** the level means of one factor differ, averaged over the other factor.
- **Interaction:** the effect of one factor depends on the level of the other. Interaction plot: cell means against the levels of A, one line per level of B; parallel lines, no interaction.
- **Read the interaction first.** If it is significant, interpret the cell means, not the main effects alone.
- With n = 1 the interaction cannot be tested: the additive model (A + B) is fitted, with the interaction's variation as the error.

## Python and R

| | Python (statsmodels) | R |
|---|---|---|
| With interaction | `anova_lm(ols("y ~ C(A) * C(B)", data=d).fit(), typ=2)` | `summary(aov(y ~ factor(A) * factor(B)))` |
| Without | `"y ~ C(A) + C(B)"` | `aov(y ~ factor(A) + factor(B))` |
| Interaction plot | `statsmodels.graphics.factorplots.interaction_plot(d.A, d.B, d.y)` | `interaction.plot(A, B, y)` |

## In the Workbench

**Two-way ANOVA**: **Data table**, **Factor A column**, **Factor B column**, **Response column** (one row per run, balanced). It gives the cell means, the table with an F, p and decision for A, B and the interaction, Shapiro–Wilk of the residuals, the interaction plot and the Q–Q plot.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 14 (two-factor factorial experiments).
