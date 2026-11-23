---
id: st.m19.diagnostic
title: Diagnostic tests
minutes: 45
objectives: [st.m19.o3]
example: st.m19.diagnostic.eg
exercise: st.m19.diagnostic.ex
reference: [diagnostic-tests]
---

# Diagnostic tests

A test comes back positive. How likely is it that the patient has the disease? Many people, doctors included, answer with the test's accuracy, and they are often far wrong. The right answer is **Bayes' rule** (item 3.4), and it depends on how common the disease is.

## The test's own accuracy

A test is checked against a "gold standard" diagnosis on people known to have the disease and people known not to:

| | Disease (A) | No disease (not A) |
|---|---|---|
| Test positive (B) | true positive | false positive |
| Test negative | false negative | true negative |

- **Sensitivity** = P(positive | disease): the share of sick people the test catches.
- **Specificity** = P(negative | no disease): the share of healthy people it clears. 1 − specificity is the **false positive rate**.

These are properties of the test. What a patient and a doctor want is the reverse:

- **Positive predictive value**, PPV = P(disease | positive);
- **Negative predictive value**, NPV = P(no disease | negative).

## Bayes' rule

With prevalence P(A):

P(B) = P(B|A)P(A) + P(B|not A)P(not A), and **PPV = P(A|B) = P(B|A)P(A) / P(B)**.

## The Example: screening for a rare disease

A disease affects **1 %** of the people screened; the test has sensitivity **95 %** and specificity **95 %** (illustrative numbers, made up for this book).

[[results: st.m19.diagnostic.eg]]

- P(B) = 0.95 × 0.01 + 0.05 × 0.99 = **0.059**: about 6 % of the people screened test positive.
- **PPV = 0.0095 / 0.059 = 0.161**. Only about **one positive in six** has the disease.

Think of 10,000 people: 100 have the disease and the test finds 95 of them. Of the 9,900 without it, 5 % (495) also test positive. So 590 positives, of whom 95 are sick: 95/590 = 16 %. The false positives outnumber the true ones because healthy people are so many.

The NPV is 0.9405 / (0.9405 + 0.0005) = **0.9995**: a negative result is very reassuring.

[[see-example: 1]]

This is why a positive **screening** result is followed by a second, more specific test, and why screening is aimed at groups where the disease is more common.

## The ROC curve

Many tests give a number (a blood level), and a **cut-off** turns it into positive or negative. Lowering the cut-off catches more sick people (higher sensitivity) but flags more healthy ones (lower specificity). The **ROC curve** (receiver operating characteristic) plots sensitivity against 1 − specificity for every cut-off.

- A useless test lies on the diagonal; a perfect one goes through the top-left corner.
- The **area under the curve** (AUC) summarises it: 0.5 is chance, 1.0 perfect, and above 0.8 is usually called good.
- The cut-off is chosen by weighing the cost of a missed case against the cost of a false alarm.

Item 23.2 uses the same ideas, under other names (recall and precision), for computer classifiers.

## In Python and R

```python
prev, sens, spec = 0.01, 0.95, 0.95
p_pos = sens * prev + (1 - spec) * (1 - prev)
ppv = sens * prev / p_pos
npv = spec * (1 - prev) / (spec * (1 - prev) + (1 - sens) * prev)
# ROC: from sklearn.metrics import roc_curve, roc_auc_score
```

```r
prev <- 0.01; sens <- 0.95; spec <- 0.95
p_pos <- sens * prev + (1 - spec) * (1 - prev)
ppv <- sens * prev / p_pos
# ROC: library(pROC); roc(truth, score)
```

[[code: st.m19.diagnostic.eg]]

## Common mistakes

- **Sensitivity taken as the PPV**: "the test is 95 % accurate, so a positive is 95 % sure".
- **Ignoring the prevalence.** The same test has a very different PPV in a clinic of sick patients and in a screening of the healthy.
- **Specificity confused with the false positive rate** (they add to 1).

## What to do next

On the **Exercise** tab, a rapid test in a clinic where 8 % of patients are infected: P(positive) and the PPV.

[[example: Screening: the prevalence decides]]

The screening test, sensitivity and specificity **95 %**, at a prevalence of **1 %**: PPV = **0.161**. (This example has no figure: Probability rules draws none. Watch the numbers.)

1. **Move P(A), the prevalence, to 0.10** (a clinic where one patient in ten has the disease). The PPV rises to about **0.68**: the same test is now right two times in three.
2. **Move it to 0.5.** PPV = **0.95**, the same as the sensitivity, only because sensitivity and specificity are equal here.
3. **Back to 0.01, and make the test more specific: P(B|not A) = 0.01** (specificity 99 %). The PPV rises to about **0.49**: for a rare disease, specificity matters most.

[[try: st.m19.screening.try ProbabilityA=0.001..0.6 LikelihoodGivenA=0.5..1 LikelihoodGivenNotA=0.001..0.3 | ProbabilityBResult IntersectionResult AGivenB figure=none]]
