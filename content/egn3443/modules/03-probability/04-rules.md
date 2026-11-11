---
id: st.m3.rules
title: Core probability rules
minutes: 50
objectives: [st.m3.o2, st.m3.o5]
builds-on: st.m3.theoretical-experimental
example: st.m3.rules.eg
exercise: st.m3.rules.ex
reference: [probability-rules, conditional-probability-and-bayes]
---

# Core probability rules

Four rules combine the probabilities of events. With them, a few known probabilities give many others.

## 1. The complement rule

An event either happens or it does not, so

P(A′) = 1 − P(A)

It is most useful for **"at least one"** questions. "At least one" covers many cases; its complement, "none", is a single case.

## 2. The addition rule: A or B

For **any** two events:

P(A ∪ B) = P(A) + P(B) − P(A ∩ B)

Adding P(A) and P(B) counts the outcomes in both twice, so the overlap is taken away once. When A and B are **mutually exclusive** the overlap is empty, P(A ∩ B) = 0, and the rule is just P(A ∪ B) = P(A) + P(B).

## 3. Conditional probability: A given B

P(A | B) is the probability of A **when B is known to have happened**. Knowing B shrinks the sample space to B, so

P(A | B) = P(A ∩ B) / P(B), when P(B) > 0

P(A | B) and P(B | A) are different numbers: each divides the same overlap by a different event.

## 4. The multiplication rule: A and B

Turn the conditional probability around:

P(A ∩ B) = P(A | B) · P(B) = P(B | A) · P(A)

When A and B are **independent**, knowing one says nothing about the other, P(A | B) = P(A), and the rule becomes

P(A ∩ B) = P(A) · P(B)

This is also the **test** of independence: A and B are independent exactly when P(A ∩ B) = P(A)·P(B). Do not assume it. Two pumps on the same power supply, or two welds by the same welder, often fail together.

## Worked cases by hand

**Complement, "at least one".** A bracket has three welds, and each has a flaw with probability 0.05, independently. What is the probability that at least one weld is flawed?

1. Complement: "no weld flawed". Each weld is sound with probability 1 − 0.05 = 0.95.
2. Independent, so multiply: P(none flawed) = 0.95 × 0.95 × 0.95 = 0.857375.
3. P(at least one flawed) = 1 − 0.857375 = **0.1426**.

Adding 0.05 + 0.05 + 0.05 = 0.15 is wrong: the welds are not mutually exclusive (two can be flawed at once), and with 25 welds the sum would pass 1.

**Multiplication without replacement.** A box of 10 relays holds 2 defective ones. Two are taken, one after the other. What is the probability that both are defective?

1. P(first defective) = 2/10.
2. With one defective relay gone, P(second defective | first defective) = 1/9.
3. P(both) = 2/10 × 1/9 = 2/90 = **0.0222**.

The second draw depends on the first, so the general rule is needed. Multiplying 2/10 × 2/10 = 0.04 would treat the draws as independent.

## The Example: two defects on a machined part

At a machine shop, A = "a part has a surface defect" and B = "a part is out of dimensional tolerance". Records show P(A) = 0.12, P(B) = 0.08, and P(A ∩ B) = 0.03 for both at once. Per 1000 parts:

| | Out of tolerance (B) | In tolerance (B′) | Total |
|---|---|---|---|
| **Surface defect (A)** | 30 | 90 | 120 |
| **No surface defect (A′)** | 50 | 830 | 880 |
| **Total** | 80 | 920 | 1000 |

[[results: st.m3.rules.eg]]

- **A or B:** P(A ∪ B) = 0.12 + 0.08 − 0.03 = **0.17**. 17 % of parts need rework for one reason or the other; 83 % have neither.
- **A given B:** P(A | B) = 0.03 / 0.08 = **0.375**. Of the parts out of tolerance, 37.5 % also have a surface defect (30 of 80 in the table).
- **B given A:** P(B | A) = 0.03 / 0.12 = **0.25** (30 of 120).
- **Independent?** If they were, P(A ∩ B) would be 0.12 × 0.08 = 0.0096. It is 0.03, about three times that. The two defects tend to come together, perhaps from a worn tool. Knowing a part is out of tolerance raises the chance of a surface defect from 12 % to 37.5 %.

## In Python and R

```python
p_a, p_b, p_ab = 0.12, 0.08, 0.03
print(p_a + p_b - p_ab)        # P(A ∪ B) = 0.17
print(p_ab / p_b, p_ab / p_a)  # P(A | B) = 0.375, P(B | A) = 0.25
print(abs(p_ab - p_a * p_b) < 1e-9)   # independent? False
print(1 - 0.95**3)             # at least one flawed weld: 0.1426
```

```r
p_a <- 0.12; p_b <- 0.08; p_ab <- 0.03
p_a + p_b - p_ab               # 0.17
p_ab / p_b; p_ab / p_a         # 0.375 and 0.25
isTRUE(all.equal(p_ab, p_a * p_b))   # FALSE
1 - 0.95^3                     # 0.1426
```

[[code: st.m3.rules.eg]]

In the Workbench, **Probability rules** does all of this: choose how A and B relate (independent, mutually exclusive, P(A ∩ B) known, P(A | B) known, or Bayes) and enter what you know.

## Common mistakes

- Adding the probabilities of events that can happen together. The sum counts the overlap twice and can even pass 1.
- Multiplying the probabilities of events that are not independent, such as draws without replacement.
- Mixing up P(A | B) and P(B | A). Say each in words: "of the parts out of tolerance, the share with a surface defect".
- Working out "at least one" case by case when 1 − P(none) is one line.

## What to do next

On the **Exercise** tab, a bridge inspection finds two kinds of damage. Use the rules to find the probabilities asked, and press **Check my answers**.
