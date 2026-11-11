---
id: st.m8.foundations
title: Foundations of hypothesis testing
minutes: 40
objectives: [st.m8.o1, st.m8.o4, st.m8.o5]
example: st.m8.foundations.eg
quiz: quizzes/m8-foundations.json
reference: [hypothesis-testing-steps, inference-preview]
---

# Foundations of hypothesis testing

## A decision made from a sample

A hypothesis test answers a yes-or-no question about a **population** (every part a machine makes, every batch a supplier ships) from a **sample** of it. Because the sample varies by chance (Module 6), the answer is never certain. The test is a rule for deciding, with a known risk of being wrong.

A **statistical hypothesis** is a statement about a population **parameter**, such as the mean μ or the proportion p. A test always weighs two of them:

- The **null hypothesis H₀** is the "nothing has changed" or "the claim holds" statement: the machine is on target, μ = 250. It always contains an equals sign.
- The **alternative hypothesis H₁** (also written Hₐ) is what you would conclude if the data argue strongly against H₀: μ ≠ 250.

The test starts by assuming H₀ is true. It then asks: **if H₀ were true, how surprising would this sample be?**

## The burden of proof

A courtroom works the same way. The defendant is presumed innocent (H₀). The prosecution must bring evidence strong enough to convict (reject H₀). If the evidence is not strong enough, the verdict is "not guilty", **not** "innocent".

| Court | Hypothesis test |
|---|---|
| Presumed innocent | Assume H₀ is true |
| Evidence | The sample, summed up in a test statistic |
| "Beyond reasonable doubt" | A p-value smaller than α |
| Guilty | Reject H₀ |
| Not guilty | Do not reject H₀ (this does **not** prove H₀) |
| Convicting an innocent person | **Type I error**: rejecting a true H₀ |
| Letting a guilty person go | **Type II error**: not rejecting a false H₀ |

Item 8.6 treats the two errors in full.

## The words of a test

- **Test statistic:** how far the sample lies from H₀, in standard errors. For a mean with σ known, z = (x̄ − μ₀)/(σ/√n), where μ₀ is the value H₀ states.
- **p-value:** the probability, **if H₀ were true**, of a test statistic at least as far from H₀ as the one observed. A small p-value means the sample would be surprising under H₀.
- **Significance level α:** the risk of a Type I error you accept, chosen **before** the data are seen; usually 0.05.
- **Decision:** if p < α, reject H₀. If p ≥ α, do not reject H₀.

## A worked case by hand

A lab checks its balance with a 100.000 g standard mass. The balance's readings have σ = 0.5 g. A technician weighs the mass n = 4 times and gets a mean of x̄ = 100.6 g. Is the balance reading high, or is this chance?

1. **Hypotheses:** H₀: μ = 100 (the balance is right); H₁: μ ≠ 100 (it is off, either way).
2. **α = 0.05**, chosen before weighing.
3. **Standard error:** σ/√n = 0.5/√4 = 0.25 g.
4. **Test statistic:** z = (100.6 − 100)/0.25 = **2.40**. The mean is 2.4 standard errors above 100.
5. **p-value:** a z this far out on either side has probability 2 × (1 − Φ(2.40)) = 2 × 0.0082 = **0.0164**. (Φ(z) is the area under the standard normal curve to the left of z, as in Module 5.)
6. **Decision:** 0.0164 < 0.05, so reject H₀. The balance reads high; recalibrate it.

## The Example: fuel injectors

A batch of injectors meant to flow 250 mL/min was tested: 16 injectors, mean 253.3 mL/min, σ = 6 mL/min known from years of records.

[[results: st.m8.foundations.eg]]

The standard error is 6/√16 = 1.5 mL/min, so z = 3.3/1.5 = **2.20**. The curve below is the standard normal, the distribution of z **if H₀ is true**. The shaded tails are the values of z beyond ±1.96 that would lead to rejecting H₀ at α = 0.05; the orange vertical line is the observed z.

[[figure: st.m8.foundations.eg 1]]

z = 2.20 lies in the shaded region, and p = 0.0278 < 0.05: reject H₀. The batch's mean flow is not 250 mL/min. Notice that 3.3 mL/min is smaller than σ: one injector 3.3 mL/min high would be ordinary. The mean of 16 is held much closer to μ, so the same gap is evidence.

## In Python and R

```python
import numpy as np
from scipy import stats
xbar, mu0, sigma, n = 253.3, 250, 6, 16
z = (xbar - mu0) / (sigma / np.sqrt(n))     # 2.20
p = 2 * stats.norm.sf(abs(z))               # two-sided p-value, 0.0278
print(z, p, "reject H0" if p < 0.05 else "do not reject H0")
```

```r
xbar <- 253.3; mu0 <- 250; sigma <- 6; n <- 16
z <- (xbar - mu0) / (sigma / sqrt(n))       # 2.20
p <- 2 * pnorm(abs(z), lower.tail = FALSE)  # 0.0278
z; p
```

`stats.norm.sf(z)` and `pnorm(z, lower.tail = FALSE)` are the area to the right of z, 1 − Φ(z).

[[code: st.m8.foundations.eg]]

## Try it in the Workbench

Press **Open in the Workbench** on the Example tab. Change **Sample mean (x̄)** to 252.0: z falls to 1.33, p rises to 0.18, and the decision becomes "do not reject". The batch may still be off target; 16 injectors are just not enough evidence of a 2 mL/min shift.

## Common mistakes

- Writing the hypotheses about the sample: "H₀: x̄ = 250". Hypotheses are about the population, μ. The sample mean is known; there is nothing to test about it.
- Saying "we accept H₀" or "the test proves μ = 250". Not rejecting means the evidence is not strong enough, nothing more.
- Choosing α after seeing the p-value. α is part of the rule, set before the data.

## What to do next

The **Exercise** tab has five questions on the logic of a test: what H₀ is, what the p-value means, and what each decision says.
