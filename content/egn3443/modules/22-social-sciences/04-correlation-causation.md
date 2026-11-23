---
id: st.m22.correlation
title: Correlation, causation and confounding
minutes: 50
objectives: [st.m22.o4]
example: st.m22.correlation.eg
exercise: st.m22.correlation.ex
reference: [correlation-causation]
---

# Correlation, causation and confounding

Pupils who spend more time on screens get lower grades. Richer people report being happier. Countries that eat more chocolate win more Nobel prizes. All three are **correlations** in observational data, and the social sciences are full of them. The hard question, and the field's constant one, is whether one variable **causes** the other.

## The Example: screen time and grades

Thirty-five pupils' daily hours of recreational screen time (self-reported) and their end-of-year grade (illustrative data, made up for this book):

[[figure: st.m22.correlation.eg 1]]

[[results: st.m22.correlation.eg]]

- r = **−0.28**: a weak negative correlation. R² = 0.08: screen time accounts for 8 % of the variation in grades.
- The slope's p = **0.10**: with 35 pupils, a correlation this weak could be chance.
- Even if a larger study found the same r with p < 0.001, it would not show that screens **cause** lower grades.

[[see-example: 1]]

## Why correlation is not causation

An association between X and Y can arise in several ways:

1. **X causes Y**: screen time takes time from homework.
2. **Y causes X** (reverse causation): pupils who struggle at school turn to screens.
3. **A confounder causes both**: home circumstances (parents' hours, a quiet place to study) affect both screen time and grades.
4. **Chance**, especially when many correlations are tested.
5. **Selection**: the sample was chosen in a way that creates the link.

Only a **randomised experiment** (item 22.3) separates the first from the rest. When an experiment is impossible or unethical, researchers measure possible confounders and **adjust** for them in a multiple regression (Module 12), look for a dose–response pattern, check the timing (cause before effect), and see whether many different studies agree.

## Simpson's paradox

A confounder can even **reverse** an association. Two job-training programs (illustrative):

| | Program A | Program B |
|---|---|---|
| Young applicants placed | 90 of 100 (90 %) | 240 of 300 (80 %) |
| Older applicants placed | 120 of 300 (40 %) | 35 of 100 (35 %) |
| **All applicants placed** | **210 of 400 (52.5 %)** | **275 of 400 (68.8 %)** |

Program A places a larger share of the young **and** a larger share of the older applicants. Yet overall, Program B looks far better. The reason is the mix: B enrolled mostly young people, who are easier to place, and A mostly older ones. Age is a confounder, and the overall comparison points the opposite way from every group. The lesson: compare like with like, and look at the groups before the total.

## In Python and R

```python
from scipy import stats
r, p = stats.pearsonr(hours, grade)
import statsmodels.formula.api as smf
smf.ols("grade ~ hours + parent_hours", data=df).fit()    # adjusting for a measured confounder
```

```r
cor.test(hours, grade)
lm(grade ~ hours + parent_hours, data = df)               # adjusting for a measured confounder
```

[[code: st.m22.correlation.eg]]

## Common mistakes

- **"X is linked to Y" written as "X causes Y".**
- **A tiny p-value read as a strong relation**: with 100,000 people, r = 0.02 is "significant" and explains 0.04 % of the variation.
- **Adjusting only for the confounders measured**, and calling the rest settled.
- **The overall table without the groups**, Simpson's trap.

## What to do next

On the **Exercise** tab, 28 households' income and life satisfaction: the correlation, R² and the slope's p-value, then what else could explain it.

[[example: A correlation: one pupil can move it]]

Screen time and grades: r = **−0.28**, R² = **0.08**, p = **0.10**.

1. **One unusual pupil.** Change the first pupil's screen time to 14 hours and grade to 30 %. One point far from the rest makes the correlation look much stronger: r = **−0.44** and p = **0.009**. Always look at the scatter plot.
2. **Put them back**, and change the grade of the pupil with the most screen time (12.7 hours, the 12th pupil) from 66 to 95. The correlation all but vanishes (r = **−0.01**). With 35 pupils, a single point can make or break a weak correlation.

[[try: st.m22.screen.try "@Pupils/Screen hours" "@Pupils/Grade (%)" | Correlation RSquared SlopeP Influential figure=1]]
