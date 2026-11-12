---
id: st.m14.one-way
title: One-way ANOVA
minutes: 50
objectives: [st.m14.o2, st.m14.o3]
builds-on: st.m14.f-statistic
example: st.m14.one-way.eg
exercise: st.m14.one-way.ex
reference: [one-way-anova, anova-assumptions]
---

# One-way ANOVA

Item 14.3 built the ANOVA table. This item puts the whole analysis together, as you would run it on a real study, and says how to read and report it.

## The design

A **one-way** (single-factor) design has one factor with k levels and a measured response. Units are assigned to the levels (at random, if you can), and each unit gives one observation. The groups may differ in size; the design is **balanced** when they do not. Balanced designs are the most robust to unequal variances, so aim for equal groups when you plan.

## The steps

1. **Hypotheses.** H₀: μ₁ = … = μₖ; H₁: at least one mean differs. Choose α, usually 0.05.
2. **Look and check** (item 14.2). Box plots of the groups; Levene's test for equal variances; Shapiro–Wilk and the Q–Q plot of the residuals for normality; independence from the design.
3. **The ANOVA table** (item 14.3): SS, df, MS, F and its p-value.
4. **Decide.** Reject H₀ when p < α.
5. **Size of the effect.** **η² (eta squared)** = SS between / SS total is the share of the response's variation that the factor accounts for, from 0 to 1. Cohen's rough guide calls 0.01 small, 0.06 medium and 0.14 large. A tiny p with a tiny η² is a real but unimportant difference. (The Workbench also gives ω², a less optimistic estimate for small samples.)
6. **Which means differ?** If H₀ is rejected, follow with Tukey's HSD (item 14.6).
7. **Report** it in words an engineer can act on.

## Two ways to lay out the data

- **Each column is a group** (wide): one column per level, as in items 14.2 and 14.3. Columns may have different lengths.
- **A factor column and a response column** (long): one row per observation, a column of level codes (150, 170, 190, or 1, 2, 3) and a column of measurements. This is how most data files and most software store it, and how the two-way ANOVA (item 14.5) needs it.

In the Workbench, **One-way ANOVA**'s **Layout** setting chooses between them; with *A factor column and a response column*, also set **Factor column** and **Response column**.

## The Example: bond strength at three curing temperatures

[[data: st.m14.one-way.eg]]

[[figure: st.m14.one-way.eg 1]]

The joints cured at 150 °C are weaker than the others; 170 and 190 °C look alike. The three spreads are similar.

[[results: st.m14.one-way.eg]]

1. **Checks.** Levene p = 0.855 and Bartlett p = 0.956: no evidence that the variances differ (the standard deviations are 0.56, 0.60 and 0.64 MPa). Shapiro–Wilk of the residuals p = 0.491, and the Q–Q plot is close to its line. Independence: each joint was made and tested separately.
2. **Table.** SS between = 14.098 on 2 df and SS within = 5.412 on 15 df, so MS between = 7.049, MS within = 0.3608 and F = 19.54.
3. **Decision.** p = 0.00007 < 0.05: reject H₀. The curing temperature changes the mean bond strength.
4. **Effect size.** η² = 14.098/19.509 = 0.72: the temperature accounts for 72 % of the variation in strength, a very large effect.

In a report: "A one-way ANOVA showed that the curing temperature affected the bond strength, F(2, 15) = 19.54, p < 0.001, η² = 0.72. The mean strengths were 18.3, 20.4 and 19.9 MPa at 150, 170 and 190 °C." Item 14.6 adds which temperatures differ (150 °C from both others; 170 and 190 °C cannot be told apart).

[[figure: st.m14.one-way.eg 3]]

The residuals' Q–Q plot: close to the line, as normal errors should be.

**Power.** The Workbench also reports the **power** the F test would have against an effect the size of the one observed (here 0.9999). Power is most useful when **planning**: choose the group sizes so a difference that matters would be detected with, say, 80 % power (Module 8). Computed after the fact from the observed effect, it adds little to the p-value.

## In Python and R

```python
import pandas as pd
from scipy import stats
import statsmodels.api as sm
from statsmodels.formula.api import ols
d = pd.DataFrame({"Temp": [150]*6 + [170]*6 + [190]*6,
                  "Strength": [18.2, 17.5, 19.1, 18.6, 17.9, 18.4, 20.3, 19.8, 21.0, 20.6, 19.5, 20.9,
                               19.6, 20.4, 19.1, 20.0, 19.4, 20.8]})
groups = [g["Strength"] for _, g in d.groupby("Temp")]
print(stats.f_oneway(*groups))                       # F = 19.54, p = 6.65e-05
model = ols("Strength ~ C(Temp)", data=d).fit()
print(sm.stats.anova_lm(model))                      # the ANOVA table
```

```r
Temp <- rep(c(150, 170, 190), each = 6)
Strength <- c(18.2, 17.5, 19.1, 18.6, 17.9, 18.4, 20.3, 19.8, 21.0, 20.6, 19.5, 20.9,
              19.6, 20.4, 19.1, 20.0, 19.4, 20.8)
fit <- aov(Strength ~ factor(Temp))
summary(fit)                                         # F value 19.54, Pr(>F) 6.65e-05
```

`C(...)` in Python and `factor(...)` in R say that the codes are **categories**. Leave them out and the program fits a straight line in temperature instead: one df, F = 10.39, p = 0.0053, and it misses that the strength rises from 150 to 170 °C and then levels off.

[[code: st.m14.one-way.eg]]

## Common mistakes

- **Numeric level codes treated as numbers.** Check that df between is k − 1. With 1 df for three levels, the software fitted a line.
- **Stopping at "reject H₀".** Say how big the effect is (η², the group means) and which groups differ (item 14.6).
- **Skipping the checks.** The p-value is only as good as the assumptions under it.
- **"Not significant" read as "the same".** Small groups may simply lack the power to show a real difference.

## What to do next

On the **Exercise** tab, four suppliers' resin with groups of different sizes. Find MS within, F and η².
