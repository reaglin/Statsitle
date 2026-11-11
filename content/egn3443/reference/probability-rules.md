---
title: The probability rules
---

# The probability rules

| Rule | Formula | When |
|---|---|---|
| Range | 0 ≤ P(A) ≤ 1; P(S) = 1 | always |
| Complement | P(A′) = 1 − P(A) | "not A"; **"at least one" = 1 − P(none)** |
| Addition, mutually exclusive | P(A ∪ B) = P(A) + P(B) | A and B cannot both happen |
| General addition | P(A ∪ B) = P(A) + P(B) − P(A ∩ B) | any two events |
| Conditional | P(A ∣ B) = P(A ∩ B) / P(B), P(B) > 0 | A when B is known to have happened |
| General multiplication | P(A ∩ B) = P(A ∣ B)·P(B) = P(B ∣ A)·P(A) | any two events; draws without replacement |
| Multiplication, independent | P(A ∩ B) = P(A)·P(B) | knowing one says nothing about the other |
| Independence test | A, B independent ⇔ P(A ∩ B) = P(A)·P(B) ⇔ P(A ∣ B) = P(A) | check it, do not assume it |

## Choosing the rule

1. **"Not", "at least one", "none":** the complement.
2. **"Or":** the addition rule. Subtract the overlap unless the events are mutually exclusive.
3. **"And", one after the other:** the multiplication rule. Use the conditional probability unless the events are independent.
4. **"Given", "of those that …":** conditional probability. To turn one around, Bayes' theorem.
5. Several steps: draw a tree or a two-way table, and work one rule at a time.

Mutually exclusive is not the same as independent. Mutually exclusive events (with positive probabilities) are always dependent: if one happens, the other cannot.

## Python and R

| Quantity | Python | R |
|---|---|---|
| P(A ∪ B) | `p_a + p_b - p_ab` | `p_a + p_b - p_ab` |
| P(A ∣ B) | `p_ab / p_b` | `p_ab / p_b` |
| At least one of n independent, each p | `1 - (1 - p)**n` | `1 - (1 - p)^n` |
| Independent? | `math.isclose(p_ab, p_a * p_b)` | `isTRUE(all.equal(p_ab, p_a * p_b))` |

In the Workbench, **Probability rules** takes P(A), P(B) and how they relate, and gives every one of these.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 2-3 (addition rules), 2-4 (conditional probability), 2-5 (multiplication and total probability rules) and 2-6 (independence).
- EGN3443 Module 3, Topic 4: https://reaglin.github.io/EGN3443/week-4module-3-probability-concepts/
