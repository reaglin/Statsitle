---
id: st.m14.multi-factor
title: Multi-factor designs
minutes: 60
objectives: [st.m14.o4]
builds-on: st.m14.one-way
example: st.m14.multi-factor.eg
exercise: st.m14.multi-factor.ex
reference: [two-way-anova]
---

# Multi-factor designs

## From one factor to two

A process usually has more than one setting that matters: the catalyst **and** the temperature, the mold design **and** the resin. You could study each in its own one-way experiment, but a **two-way** (two-factor) design that runs every combination is better:

- every run tells you about **both** factors, so it needs fewer runs for the same precision;
- the variation due to the second factor is taken out of the error, so the F tests are more sensitive;
- it is the only way to see an **interaction**.

## The design

Factor A has a levels, factor B has b levels, and every one of the a × b combinations (the **cells**) is run n times (the **replicates**). With the same n in every cell the design is **balanced**; the Workbench's Two-way ANOVA needs that. Here N = abn.

Three kinds of mean appear:

- the **cell mean** ȳᵢⱼ, of the n runs at level i of A and level j of B;
- the **level means** of A, written ȳᵢ. (the dot stands for "averaged over B"), and of B, written ȳ.ⱼ (averaged over A);
- the grand mean ȳ.

## Main effects and interaction

- The **main effect** of A is how the level means of A differ: the effect of A **averaged over** the levels of B. Likewise for B.
- An **interaction** (A × B) means that the effect of A **depends on** the level of B. If catalyst 2 beats catalyst 1 at low temperature but loses at high temperature, catalyst and temperature interact, and "which catalyst is better?" has no answer without "at which temperature?".

The **interaction plot** shows it: the cell means against the levels of A, one line for each level of B. **Parallel lines: no interaction** (the effect of A is the same at every level of B). Lines that converge, diverge or cross **suggest** an interaction. The F test for the interaction says whether the departure from parallel is more than the scatter within the cells would give by chance.

## The two-way ANOVA table

The total sum of squares splits into four parts:

| Source | SS | df |
|---|---|---|
| A | bn Σ (ȳᵢ. − ȳ)² | a − 1 |
| B | an Σ (ȳ.ⱼ − ȳ)² | b − 1 |
| A × B | n Σ Σ (ȳᵢⱼ − ȳᵢ. − ȳ.ⱼ + ȳ)² | (a − 1)(b − 1) |
| Error | Σ Σ Σ (y − ȳᵢⱼ)², each run around its own cell mean | ab(n − 1) |
| Total | Σ (y − ȳ)² over all N runs | N − 1 |

Each MS is its SS over its df, and each of A, B and A × B has its own **F = MS / MS error**, tested on its df and the error df. There are three null hypotheses: no A effect, no B effect, no interaction. The interaction term ȳᵢⱼ − ȳᵢ. − ȳ.ⱼ + ȳ is how far a cell mean is from what the two main effects alone would predict.

With only **one run per cell** (n = 1) there is nothing left to estimate the error from inside the cells; the interaction cannot be tested, and the Workbench then fits the model without it.

## A worked case by hand

A 2 × 2 design with n = 2 runs per cell (a = b = 2, N = 8):

| | B = 1 | B = 2 | Mean of A |
|---|---|---|---|
| A = 1 | 10, 12 (mean 11) | 14, 16 (mean 15) | 13 |
| A = 2 | 13, 15 (mean 14) | 13, 15 (mean 14) | 14 |
| Mean of B | 12.5 | 14.5 | ȳ = 13.5 |

1. **SS A** = bn Σ (ȳᵢ. − ȳ)² = 2 × 2 × [(13 − 13.5)² + (14 − 13.5)²] = 4 × 0.5 = **2**.
2. **SS B** = an Σ (ȳ.ⱼ − ȳ)² = 4 × [(12.5 − 13.5)² + (14.5 − 13.5)²] = 4 × 2 = **8**.
3. **SS A × B.** For cell (1, 1): 11 − 13 − 12.5 + 13.5 = −1. The other three cells give +1, +1 and −1. SS A × B = n × (1 + 1 + 1 + 1) = 2 × 4 = **8**.
4. **SS error.** Every run is 1 from its cell mean, so SS error = 8 × 1² = **8**, on ab(n − 1) = 4 df, and MS error = 2.
5. **F's.** A: 2/2 = 1.0; B: 8/2 = 4.0; A × B: 8/2 = 4.0, each on 1 and 4 df. The critical value F(0.05; 1, 4) is 7.71, so none is significant (p = 0.374, 0.116 and 0.116): eight runs are too few to tell.

Read it with the plot in mind: at A = 1, B raises the mean by 4; at A = 2, B changes nothing. That is an interaction, whether or not this small study can prove it.

## The Example: yield by catalyst and temperature

[[data: st.m14.multi-factor.eg]]

[[figure: st.m14.multi-factor.eg 1]]

The catalyst is on the axis, with one line per temperature. The lines are far from parallel; they cross. At 160 °C catalyst 2 gives 6.2 points more yield (77.6 against 71.4 %), at 180 °C 2.3 points more, and at 200 °C 5.0 points **less** (76.3 against 81.3 %).

[[results: st.m14.multi-factor.eg]]

| Source | SS | df | MS | F | p |
|---|---|---|---|---|---|
| Catalyst (A) | 6.125 | 1 | 6.125 | 5.38 | 0.039 |
| Temperature (B) | 56.573 | 2 | 28.287 | 24.82 | 0.00005 |
| Interaction | 95.853 | 2 | 47.927 | 42.06 | 0.000004 |
| Error | 13.673 | 12 | 1.139 | | |
| Total | 172.225 | 17 | | | |

**Read the interaction first.** p = 0.000004: the catalyst's effect depends on the temperature. The main effect of catalyst (means 76.3 and 77.5 %, p = 0.039) is an average over a positive effect at two temperatures and a negative one at the third. On its own it would recommend catalyst 2, the wrong choice at 200 °C. When the interaction is significant, compare the **cell means**: the best combination is catalyst 1 at 200 °C (81.3 %).

A one-way ANOVA on catalyst alone would be worse still: F = 0.59, p = 0.454, "the catalyst makes no difference", because the temperature and interaction variation is left in its error.

[[figure: st.m14.multi-factor.eg 2]]

The Q–Q plot of the residuals (each run minus its cell mean) is close to its line, and Shapiro–Wilk gives p = 0.140.

## In Python and R

```python
import pandas as pd
import statsmodels.api as sm
from statsmodels.formula.api import ols
d = pd.DataFrame({
    "Catalyst": [1]*9 + [2]*9,
    "Temperature": ([160]*3 + [180]*3 + [200]*3) * 2,
    "Yield": [71.6, 72.4, 70.3, 76.4, 74.9, 77.3, 81.0, 80.2, 82.6,
              78.3, 76.6, 77.9, 78.1, 77.6, 79.8, 76.9, 75.4, 76.6]})
model = ols("Yield ~ C(Catalyst) * C(Temperature)", data=d).fit()
print(sm.stats.anova_lm(model, typ=2))   # A, B, A:B and Residual rows
```

```r
Catalyst <- factor(rep(c(1, 2), each = 9))
Temperature <- factor(rep(rep(c(160, 180, 200), each = 3), 2))
Yield <- c(71.6, 72.4, 70.3, 76.4, 74.9, 77.3, 81.0, 80.2, 82.6,
           78.3, 76.6, 77.9, 78.1, 77.6, 79.8, 76.9, 75.4, 76.6)
summary(aov(Yield ~ Catalyst * Temperature))       # * fits A, B and A:B
interaction.plot(Catalyst, Temperature, Yield)
```

`A * B` asks for both main effects and the interaction; `A + B` leaves the interaction out and puts its variation in the error.

[[code: st.m14.multi-factor.eg]]

## In the Workbench

Choose **Two-way ANOVA**. Set **Data table**, then **Factor A column**, **Factor B column** and **Response column**. The data must be in the long layout, one row per run, and balanced.

## Common mistakes

- **Reading the main effects first.** If the interaction is significant, a main effect is an average of effects that differ, and it can point the wrong way. Look at the interaction plot and the cell means.
- **"The lines are not parallel, so there is an interaction."** Sample means are never exactly parallel. The F test for the interaction decides.
- **Two one-way ANOVAs instead of one two-way.** The other factor's variation stays in the error, and the interaction is never seen.
- **Leaving out the interaction term** (A + B instead of A * B) when there are replicates: its variation inflates the error.

## What to do next

On the **Exercise** tab, three mold designs and two resins. Find the three F's and the interaction's p-value, and decide which effects are real.
