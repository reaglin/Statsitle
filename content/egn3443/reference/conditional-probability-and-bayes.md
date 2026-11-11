---
title: Conditional probability and Bayes' theorem
---

# Conditional probability and Bayes' theorem

| Quantity | Formula |
|---|---|
| Conditional probability | P(A ∣ B) = P(A ∩ B) / P(B) |
| Total probability | P(B) = P(B ∣ A)·P(A) + P(B ∣ A′)·P(A′) |
| Bayes' theorem | P(A ∣ B) = P(B ∣ A)·P(A) / P(B) |

| Word | In a test for a fault A, with B = a positive result |
|---|---|
| Prior | P(A), the chance of the fault before the test (the base rate) |
| Sensitivity | P(B ∣ A), positives among the faulty |
| False-positive rate | P(B ∣ A′), positives among the sound |
| Posterior | P(A ∣ B), the chance of the fault after a positive result |

**The base-rate effect:** when the fault is rare, even a good test gives many false positives, because they come from the many sound items. A weld line with 2 % cracked welds, a test with sensitivity 0.90 and false-positive rate 0.05: P(crack | flag) = 0.018 / 0.067 = 0.27.

**Natural frequencies** make it easy by hand: take 10 000 items, split them by the prior, then each group by the test, and read the share of the positives that are faulty.

## Saying it in plain words

- Name the event, the group and the time frame.
- Use a frequency: "about 1 in 4 flagged welds is cracked".
- Give both sides, and the consequence of each.
- Never read P(B | A) as P(A | B).

## Python and R

| Quantity | Python | R |
|---|---|---|
| P(B) | `p_b_a * p_a + p_b_not_a * (1 - p_a)` | `p_b_a * p_a + p_b_not_a * (1 - p_a)` |
| P(A ∣ B) | `p_b_a * p_a / p_b` | `p_b_a * p_a / p_b` |

In the Workbench, **Probability rules** with "P(B | A) and P(B | A′) are known — Bayes" gives P(B) and P(A | B).

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 2-4 (conditional probability), 2-5 (total probability rule) and 2-7 (Bayes' theorem).
- EGN3443 Module 3, Topic 5: https://reaglin.github.io/EGN3443/week-4module-3-probability-concepts/
