---
id: st.m19.dose
title: Dose–response and bioassay
minutes: 50
objectives: [st.m19.o6]
example: st.m19.dose.eg
exercise: st.m19.dose.ex
reference: [dose-response-bioassay]
---

# Dose–response and bioassay

How much of a drug, a poison or a pesticide does it take? **Bioassay** measures the effect of a substance on living things at several doses. The answer is usually given as one number: the **LD50** (lethal dose 50 %), the dose that kills half of those exposed, or the **EC50** (effective concentration 50 %), the concentration that produces the effect in half. Regulators use them to set safe limits for pesticides, chemicals in water and drugs.

## The dose–response curve

Expose groups of animals to increasing doses and count how many respond (die, are affected) in each. The share responding rises with the dose along an **S-shaped** curve: few at low doses, nearly all at high ones. Two steps straighten it:

1. **Dose on a log scale**: effects grow with the dose's *ratio*, so log dose is the usual x.
2. **The logit of the share responding**: logit(p) = ln(p/(1 − p)). The S-curve becomes a straight line:

logit(p) = b₀ + b₁ · log₁₀(dose).

The LD50 is where p = 0.5, so logit(p) = 0: **log₁₀(LD50) = −b₀/b₁**, and LD50 = 10^(−b₀/b₁).

The proper fit is **logistic regression** by maximum likelihood (Part II), which weights each group by its size and handles 0 % and 100 % groups. A quick, usually close, version is a least-squares line through the **empirical logits** ln((k + 0.5)/(n − k + 0.5)) (k responding of n; the 0.5 keeps 0 % and 100 % groups finite). That is what this page does, with Module 11's regression.

## The Example: Bliss's beetles

A classic published data set (Bliss, 1935): flour beetles exposed for five hours to carbon disulphide gas at eight doses (log₁₀ mg per litre), with the numbers tested and killed.

[[data: st.m19.dose.eg]]

[[figure: st.m19.dose.eg 1]]

[[results: st.m19.dose.eg]]

- logit = −63.25 + **35.82** × log₁₀(dose), R² = 0.960: the logit scale straightens the curve well.
- log₁₀(LD50) = 63.25 / 35.82 = **1.766**, so **LD50 = 10^1.766 = 58.3 mg per litre**.
- The steep slope means a small change in dose moves the kill rate a long way: from 10 % to 90 % killed between about 1.70 and 1.83 on the log scale.

A maximum-likelihood logistic fit of the same data gives log₁₀(LD50) ≈ 1.77, very close: the empirical-logit line is a good quick estimate here.

[[see-example: 1]]

## How the numbers are used

- **Toxicity classes.** A substance's LD50 (in mg per kg of body weight) places it in a hazard class on its label.
- **Relative potency.** Two substances with parallel lines differ by a constant dose ratio: one is "3 times as potent".
- **Safe limits.** Ecotoxicology tests (water fleas, fish, algae) give EC50s from which a "predicted no-effect concentration" in rivers is set, dividing by safety factors.

## In Python and R

```python
import numpy as np, statsmodels.api as sm
logit = np.log((killed + 0.5) / (tested - killed + 0.5))
fit = sm.OLS(logit, sm.add_constant(logdose)).fit()
ld50 = 10 ** (-fit.params[0] / fit.params[1])
# maximum likelihood: sm.GLM(np.column_stack([killed, tested - killed]), sm.add_constant(logdose), family=sm.families.Binomial()).fit()
```

```r
logit <- log((killed + 0.5) / (tested - killed + 0.5))
fit <- lm(logit ~ logdose); 10^(-coef(fit)[1] / coef(fit)[2])
glm(cbind(killed, tested - killed) ~ logdose, family = binomial)   # maximum likelihood
```

[[code: st.m19.dose.eg]]

## Common mistakes

- **Dose on its own scale** rather than log dose: the line will bend.
- **The share killed as y** rather than its logit.
- **Forgetting the log**: LD50 = 10^(−b₀/b₁), not −b₀/b₁.
- **Extrapolating** far below the lowest dose tested to claim a "safe" level.

## What to do next

On the **Exercise** tab, a pesticide tested on water fleas at six concentrations: fit the logit line and read its intercept and slope.

[[example: Dose–response: the logit line]]

Bliss's beetles: logit = −63.25 + **35.82** × log₁₀(dose), R² = **0.960**, LD50 = 10^(63.25/35.82) ≈ **58.3 mg/L**.

1. **Change the first group's logit** (−2.108, 6 of 59 killed) to −4 (as if almost none were killed). The line steepens to a slope of **41.92**, yet log₁₀(LD50) = 74.43/41.92 = **1.776** barely moves from 1.766: the slope changes how fast the kill rate rises, not where it reaches half.
2. **Put it back**, and change the logit at the fifth dose (52 of 63 killed, 1.519) to 0. The line flattens a little (slope **34.96**) and R² drops to **0.907**: that point no longer sits on the line. log₁₀(LD50) = 61.91/34.96 = **1.771**, still close. The LD50 is the best-determined number in a bioassay.

[[try: st.m19.beetles.try @Beetles/Logit | Equation Intercept Slope RSquared figure=1]]
