---
id: st.m19.trials
title: Clinical trials
minutes: 50
objectives: [st.m19.o1]
example: st.m19.trials.eg
exercise: st.m19.trials.ex
reference: [clinical-trials]
---

# Clinical trials

Does a new treatment work better than the standard one? A **clinical trial** answers that with an experiment on people, and its design is what lets the answer be trusted. The analysis is often one you know: Module 10's comparison of two proportions or two means. The design is the field's own.

## The design

- **A control group.** Patients on the new treatment are compared with patients on the standard treatment or a **placebo** (a dummy treatment). Without a control, recovery that would have happened anyway is credited to the drug.
- **Randomisation.** Each patient is assigned to a group by chance, not by the doctor's choice. Then the groups differ only by chance in age, severity and everything else, measured or not, and a difference in outcome can be credited to the treatment. This is what an observational study cannot do (item 19.2).
- **Blinding.** In a **single-blind** trial the patients do not know which treatment they get; in a **double-blind** trial their doctors and the people who judge the outcome do not know either. Knowing changes how patients feel and how outcomes are judged.
- **A primary outcome, fixed in advance.** Recovery within 30 days, say. The trial is **registered** with its outcome and its analysis before it starts, so the researchers cannot pick the outcome that happened to look best afterwards.
- **A planned sample size.** Enough patients to detect the smallest benefit that matters, with high **power** (item 8.6).

## The Example: recovery on two treatments

400 patients were randomised, 200 to each treatment, double-blind (illustrative data, made up for this book). Recovered within 30 days: **142 of 200 (71 %)** on the new treatment, **118 of 200 (59 %)** on the standard one.

[[results: st.m19.trials.eg]]

[[figure: st.m19.trials.eg 1]]

- z = 2.52, p = 0.012: the recovery rates differ at α = 0.05 (two-proportion z-test, item 10.5).
- The difference is **12 percentage points**, with a 95 % interval from **2.7 to 21.3 points**: the new treatment helps, but the trial cannot say whether by a little or a lot.

Medicine reports the same result in its own measures:

| Measure | Formula | Here |
|---|---|---|
| Absolute risk difference | p₁ − p₂ | 0.71 − 0.59 = 0.12 |
| Relative risk (ratio) | p₁ / p₂ | 0.71 / 0.59 = 1.20: 20 % more recoveries |
| Number needed to treat | 1 / (p₁ − p₂) | 1 / 0.12 ≈ 8.3: treat about 8 patients for one extra recovery |

The **number needed to treat** is what a doctor weighs against the treatment's cost and side effects.

[[see-example: 1]]

## Sample size and power

Before the trial, the planners asked: how many patients per group to detect a rise from 59 % to 71 % with 80 % power at α = 0.05 (two-sided)? For two proportions,

n per group = (z_{α/2} + z_β)² [p₁(1 − p₁) + p₂(1 − p₂)] / (p₁ − p₂)²
= (1.960 + 0.842)² × (0.71 × 0.29 + 0.59 × 0.41) / 0.12² = 7.85 × 0.448 / 0.0144 ≈ **245**.

With 200 per group this trial had somewhat less than 80 % power: it was lucky to reach p < 0.05. A trial that is too small can miss a real benefit, and a "no significant difference" from it is not evidence that the treatment does not work (item 8.6).

## In Python and R

```python
from statsmodels.stats.proportion import proportions_ztest
from statsmodels.stats.power import NormalIndPower
from statsmodels.stats.proportion import proportion_effectsize
z, p = proportions_ztest([142, 118], [200, 200])
n = NormalIndPower().solve_power(proportion_effectsize(0.71, 0.59), alpha=0.05, power=0.8)
```

```r
prop.test(c(142, 118), c(200, 200), correct = FALSE)
power.prop.test(p1 = 0.71, p2 = 0.59, power = 0.8)    # n per group
```

[[code: st.m19.trials.eg]]

## Common mistakes

- **No control group**, or a control chosen by the doctor rather than at random.
- **Changing the primary outcome** after seeing the data.
- **"Not significant" read as "no effect"** from a trial too small to detect one.
- **Relative risk without the absolute risk.** "Halves the risk" sounds large; from 2 in 1,000 to 1 in 1,000 is small.

## What to do next

On the **Exercise** tab, a vaccine trial with 1,500 volunteers in each group: compare the infection rates.

[[example: The same recovery rates, more or fewer patients]]

The trial's result: **71 %** against **59 %** recovered, 200 patients in each group, z = **2.52**, p = **0.012**.

1. **Half the trial:** set both sample sizes to 100 and the recoveries to 71 and 59. The same 12-point difference is no longer clear at α = 0.05, and the interval for it includes zero.
2. **Twice the trial:** sample sizes 400 and recoveries 284 and 236. The p-value is far smaller and the interval narrower: the difference is the same, the evidence stronger.
3. **Back to 200 each**, and change the new treatment's recoveries to 125 (62.5 %). A 3.5-point benefit, if real, would need thousands of patients to show.

[[try: st.m19.trial-size.try Successes SampleSize Successes2 Sample2Size | Statistic PValue CiLower CiUpper figure=1]]
