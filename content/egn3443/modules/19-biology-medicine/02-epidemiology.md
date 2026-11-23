---
id: st.m19.epidemiology
title: Epidemiology
minutes: 50
objectives: [st.m19.o2]
example: st.m19.epidemiology.eg
exercise: st.m19.epidemiology.ex
reference: [epidemiology-measures]
---

# Epidemiology

**Epidemiology** studies how disease is spread through a population and what raises or lowers the risk: smoking and lung cancer, a contaminated water supply and cholera, a solvent and liver disease. Most of its data are **observational**: nobody can randomise people to smoke. Its tools are counts in a table, a handful of ratios, and constant attention to **confounding**.

## Measuring how much disease there is

- **Prevalence**: the share of a population that has the disease **now**. 300 people with diabetes in a town of 5,000: prevalence 6 %.
- **Incidence** (risk): the share of people free of the disease who **develop it** over a period. 50 new cases among 4,700 people over a year: incidence about 1.1 % a year.

A long-lasting disease can have a high prevalence and a low incidence; a short one the reverse.

## The 2 × 2 table

Exposure (yes / no) against disease (yes / no):

| | Disease | No disease | Total |
|---|---|---|---|
| Exposed | a | b | a + b |
| Unexposed | c | d | c + d |

From it:

- **Relative risk**: RR = [a/(a + b)] / [c/(c + d)], the risk if exposed divided by the risk if not. RR = 2: exposure doubles the risk.
- **Odds ratio**: OR = (a·d)/(b·c), the odds of disease if exposed divided by the odds if not. For a rare disease, OR ≈ RR.
- **The chi-square test of independence** (item 8.5) asks whether the association could be chance.

## The Example: a cohort study

A **cohort study** follows people, exposed and not, forward in time and counts who falls ill. 1,000 factory workers were followed for ten years: 400 exposed to a solvent, 600 not (illustrative data, made up for this book).

| | Liver disease | None | Total |
|---|---|---|---|
| Exposed | 40 | 360 | 400 |
| Unexposed | 30 | 570 | 600 |

- Risk if exposed 40/400 = **10 %**; if not, 30/600 = **5 %**. **RR = 2.0**: the solvent doubles the ten-year risk. The extra risk, 5 percentage points, is the **attributable risk**.
- OR = (40 × 570)/(360 × 30) = **2.11**, close to the RR because the disease is fairly rare.

[[results: st.m19.epidemiology.eg]]

χ² = 9.22 on 1 degree of freedom, p = 0.0024: the association is unlikely to be chance.

[[see-example: 1]]

## Case-control studies

For a rare disease a cohort would need huge numbers. A **case-control study** starts from the disease: it finds people who have it (cases) and similar people who do not (controls), and asks about their past exposure. Because the study chose how many cases and controls to take, the risk of disease cannot be estimated, so the **relative risk cannot be calculated**. The **odds ratio** can, and for a rare disease it estimates the relative risk. That is why case-control studies report odds ratios.

## Confounding

A **confounder** is a third variable related to both the exposure and the disease. If older workers are more often exposed to the solvent and older people also get more liver disease, part of the RR of 2.0 is age, not the solvent. Epidemiologists deal with it by:

- **stratifying**: calculating the RR within age groups and combining them (the Mantel–Haenszel method);
- **adjusting** in a regression model with the confounder as another predictor (Module 12's idea; for a yes/no outcome, logistic regression, Part II);
- **matching**: choosing controls of the same age as each case.

Randomisation (item 19.1) removes confounding by design; observational studies can only adjust for the confounders they measured.

## In Python and R

```python
from scipy import stats
table = [[40, 360], [30, 570]]
chi2, p, dof, expected = stats.chi2_contingency(table, correction=False)
rr = (40/400) / (30/600); odds_ratio = (40*570) / (360*30)
```

```r
tab <- matrix(c(40, 360, 30, 570), nrow = 2, byrow = TRUE)
chisq.test(tab, correct = FALSE)
(40/400) / (30/600); (40*570) / (360*30)        # RR, OR
```

[[code: st.m19.epidemiology.eg]]

## Common mistakes

- **A relative risk from a case-control study.** Only the odds ratio.
- **Prevalence and incidence confused.**
- **Association taken as cause** without asking what confounds it.
- **The odds ratio read as a relative risk** for a common disease, where it exaggerates it.

## What to do next

On the **Exercise** tab, a food-poisoning outbreak: a case-control table of who ate the chicken salad. Test the association.

[[example: A cohort study: change the counts]]

The solvent cohort: 40 of 400 exposed and 30 of 600 unexposed workers fell ill, **RR = 2.0**, χ² = **9.22**, p = **0.0024**.

1. **A smaller study:** divide every count by 4 (10, 90 exposed; 7.5 is not a count, so use 8, 142 unexposed). The risks are about the same, but χ² falls well below 3.84: a real doubling of risk can be missed by a study that is too small.
2. **Back to the full counts**, and change the unexposed cases to 60 (and the unexposed without disease to 540). Both groups now have a 10 % risk, RR = 1, and χ² is 0.
3. **Change the exposed cases to 80 (and 320 without disease).** RR = 4 if the unexposed are back at 30 of 600: the stronger the association, the larger χ² and Cramér's V.

[[try: st.m19.cohort.try "@Cohort/Disease" "@Cohort/No disease" | Statistic P CramersV figure=1]]
