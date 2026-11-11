---
id: st.m8.formulating
title: Formulating the null and alternative hypotheses
minutes: 40
objectives: [st.m8.o1]
builds-on: st.m8.foundations
example: st.m8.formulating.eg
quiz: quizzes/m8-formulating.json
reference: [hypothesis-testing-steps]
---

# Formulating the null and alternative hypotheses

## From a claim to two hypotheses

Every test starts from a question in words: a specification, a supplier's claim, a suspicion. Turning it into H₀ and H₁ is the step that most often goes wrong, and a test of the wrong hypotheses answers the wrong question.

1. **Name the parameter.** A mean μ (a measurement: strength, time, length), a proportion p (a share: defective, failed, on time), or a standard deviation σ.
2. **Find the value in the claim.** The target, the specification, the historical rate: this is μ₀ or p₀.
3. **Write H₀ with the equals sign:** μ = μ₀. H₀ is the status quo, the claim being checked, or "no effect".
4. **Write H₁ from the question.** What do you need the data to show?

## Two-tailed and one-tailed

| The question | H₁ | Called | Evidence against H₀ is |
|---|---|---|---|
| Has it **changed**? Is it **off** target? | μ ≠ μ₀ | non-directional, two-tailed | far from μ₀ on either side |
| Is it **more** than claimed? Too **high**? | μ > μ₀ | directional, one-tailed (upper) | far above μ₀ only |
| Is it **less**? Has it been **reduced**? | μ < μ₀ | directional, one-tailed (lower) | far below μ₀ only |

**Choosing.** Use a one-tailed H₁ only when the direction is fixed by the question **before** the data are seen, and a result in the other direction would lead to the same action as no result. "Does the new coating **reduce** corrosion?" is one-tailed: a coating that increases corrosion is rejected just like one that does nothing. "Is the machine on target?" is two-tailed: drifting either way matters.

Never pick the tail after looking at the data. A sample mean above μ₀ tempts you to test μ > μ₀, which halves the p-value; that doubles the real risk of a Type I error.

Some books write a one-tailed H₀ as μ ≤ μ₀ (against μ > μ₀). The test is calculated at the boundary μ = μ₀ either way, so this course writes H₀: μ = μ₀, as the Workbench does.

## A worked case: four claims

1. *"The bolts have a mean breaking load of at least 25 kN."* A buyer wants to know whether they fall short. Parameter μ, value 25. H₀: μ = 25; H₁: **μ < 25** (lower tail).
2. *"No more than 2 % of the chips are defective."* An auditor checks whether the rate is higher. Parameter p, value 0.02. H₀: p = 0.02; H₁: **p > 0.02** (upper tail).
3. *"The filling machine puts 500 mL in each bottle."* Overfilling wastes product and underfilling breaks the law, so both matter. H₀: μ = 500; H₁: **μ ≠ 500** (two tails).
4. *"The new firmware cuts the mean boot time below the old 8.4 s."* The developer must show an improvement. H₀: μ = 8.4; H₁: **μ < 8.4**.

In 1 and 2 the claim itself goes in H₀ and is given the benefit of the doubt. In 4 the claim is what must be shown, so it goes in H₁: the burden of proof is on whoever wants to change the status quo.

## The Example: resin cure time, one tail or two

A supplier says its resin cures in 30 minutes on average. An engineer suspects it takes longer. She times 25 batches (σ = 4 minutes) and gets x̄ = 31.4 minutes, so z = (31.4 − 30)/(4/√25) = 1.4/0.8 = **1.75**.

Her question was "does it take longer?", so H₁: μ > 30, and only the upper tail counts:

[[figure: st.m8.formulating.eg 1]]

[[results: st.m8.formulating.eg]]

The p-value is the area above z = 1.75: **0.0401**, below 0.05, so H₀ is rejected. Had any change mattered, H₁ would be μ ≠ 30, both tails would count, and the same z gives twice the p-value:

[[figure: st.m8.formulating.two 1]]

Now p = **0.0801** and H₀ is not rejected. The same data, a different question, a different decision. That is why the question, and with it the tail, is fixed before the data are seen. The one-tailed test puts all of α in one tail (z > 1.645), so it detects a shift in that direction more easily, and cannot detect one in the other.

## In Python and R

```python
from scipy import stats
z = (31.4 - 30) / (4 / 25 ** 0.5)          # 1.75
print(stats.norm.sf(z))                    # H1: mu > 30,  p = 0.0401
print(stats.norm.cdf(z))                   # H1: mu < 30,  p = 0.9599
print(2 * stats.norm.sf(abs(z)))           # H1: mu != 30, p = 0.0801
```

```r
z <- (31.4 - 30) / (4 / sqrt(25))
pnorm(z, lower.tail = FALSE)               # mu > 30:  0.0401
pnorm(z)                                   # mu < 30:  0.9599
2 * pnorm(abs(z), lower.tail = FALSE)      # mu != 30: 0.0801
```

With data, `stats.ttest_1samp(x, mu0, alternative='greater')` in Python and `t.test(x, mu = mu0, alternative = "greater")` in R choose the tail the same way.

[[code: st.m8.formulating.eg]]

## Common mistakes

- Putting the equals sign in H₁, or writing H₀: μ ≠ μ₀. The equals sign always goes in H₀.
- Writing hypotheses about x̄ or p̂ instead of μ or p.
- Using the sample to choose the tail.
- Putting what you hope to show in H₀. The test can only reject H₀, so what needs evidence goes in H₁.

## What to do next

The **Exercise** tab has six claims to turn into hypotheses, and questions on which tail to use.
