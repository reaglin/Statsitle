---
id: st.m3.sample-spaces
title: Sample spaces and events
minutes: 45
objectives: [st.m3.o1, st.m3.o2]
builds-on: st.m3.introduction
example: st.m3.sample-spaces.eg
quiz: quizzes/m3-sample-spaces.json
reference: [sample-spaces-and-counting]
---

# Sample spaces and events

## The sample space

The **sample space** S is the list of every outcome an experiment can have. Writing it down is the first step of almost every probability problem: an outcome you forget is a probability you get wrong.

- One die: S = {1, 2, 3, 4, 5, 6}.
- A weld is inspected: S = {pass, fail}.
- The number of cars waiting at a light: S = {0, 1, 2, …}.

An **event** is a part of the sample space, a set of outcomes. For one die, A = "an even number" = {2, 4, 6}. The event happens when the outcome is one of its members.

## Three systematic ways to list it

**1. A list**, when the outcomes are few: S = {pass, fail}.

**2. A table**, when the experiment has two parts. Two dice give 6 × 6 = 36 outcomes, one per cell, each cell holding the sum:

| Die 1 \ Die 2 | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| **1** | 2 | 3 | 4 | 5 | 6 | 7 |
| **2** | 3 | 4 | 5 | 6 | 7 | 8 |
| **3** | 4 | 5 | 6 | 7 | 8 | 9 |
| **4** | 5 | 6 | 7 | 8 | 9 | 10 |
| **5** | 6 | 7 | 8 | 9 | 10 | 11 |
| **6** | 7 | 8 | 9 | 10 | 11 | 12 |

**3. A tree diagram**, when the experiment happens in stages. An inspector checks three parts in turn, and each passes (P) or fails (F). Each stage splits every branch in two:

```
Part 1    Part 2    Part 3    Outcome
  P ───┬── P ───┬── P        PPP
       │        └── F        PPF
       └── F ───┬── P        PFP
                └── F        PFF
  F ───┬── P ───┬── P        FPP
       │        └── F        FPF
       └── F ───┬── P        FFP
                └── F        FFF
```

S has 2 × 2 × 2 = 8 outcomes. The event "exactly one part fails" is {PPF, PFP, FPP}: three outcomes.

## Combining events

| In words | Written | The outcomes in it |
|---|---|---|
| not A (the **complement**) | A′ | every outcome not in A |
| A or B (or both) | A ∪ B (union) | outcomes in A, in B, or in both |
| A and B | A ∩ B (intersection) | outcomes in both A and B |

Two events are **mutually exclusive** (disjoint) when they share no outcome: A ∩ B is empty, so they cannot both happen. For one die, "a 1" and "an even number" are mutually exclusive; "a 2" and "an even number" are not.

## Probability by counting

When every outcome in S is **equally likely**:

P(A) = (outcomes in A) ÷ (outcomes in S)

**A worked case by hand.** Two fair dice are rolled.

1. **The sample space:** 36 equally likely ordered pairs (the table above).
2. **A = "the sum is 7":** the cells (1,6), (2,5), (3,4), (4,3), (5,2), (6,1). Six outcomes, so P(A) = 6/36 = **0.1667**.
3. **B = "the sum is 11":** (5,6) and (6,5). P(B) = 2/36 = **0.0556**.
4. **"7 or 11":** A and B are mutually exclusive, so 6 + 2 = 8 outcomes, P = 8/36 = **0.2222**.

The ordered pairs are what make the outcomes equally likely. The **sums** 2 to 12 are not: there is one way to roll a 2 and six ways to roll a 7. Treating the 11 sums as equally likely (P(7) = 1/11) is a classic mistake.

Counting works only when the outcomes are equally likely. If parts fail 5 % of the time, the eight outcomes of the tree above are not equally likely (FFF is far rarer than PPP); item 3.4 multiplies the probabilities along each branch instead.

## The Example: two dice rolled 1000 times

The computer rolls two dice 1000 times. The second figure puts each sum's share of the rolls (blue and grey bars) beside its theoretical probability from the table (red bars):

[[figure: st.m3.sample-spaces.eg 2]]

The red bars rise from 1/36 at a sum of 2 to 6/36 at 7 and fall back to 1/36 at 12, exactly as the table's diagonals do. The simulated shares follow them closely. The event "the sum is 7" (the dark blue bar) came up 168 times in 1000:

[[results: st.m3.sample-spaces.eg]]

## Counting larger sample spaces

When S is too big to list, count it.

- **Multiplication principle.** If one choice can be made n₁ ways and a second n₂ ways, together they can be made n₁ × n₂ ways. A bracket made in 3 materials, 4 finishes and 2 sizes comes in 3 × 4 × 2 = **24** versions.
- **Permutations** (order matters). The number of ordered arrangements of r items from n: nPr = n! / (n − r)!. Three of 8 tests run one after another in a chosen order: 8 × 7 × 6 = **336** orders.
- **Combinations** (order does not matter). The number of groups of r items from n: nCr = C(n, r) = n! / (r!(n − r)!). Three of 8 samples sent to the lab: 336 ÷ 3! = **56** groups.

**A worked case by hand.** A box of 10 relays holds 2 defective ones. A technician takes 3 at random. What is the probability that none is defective?

1. Every group of 3 is equally likely: C(10, 3) = 120 groups in S.
2. The groups with no defective relay take all 3 from the 8 good ones: C(8, 3) = 56.
3. P(none defective) = 56 / 120 = **0.4667**.

The Workbench's **Probability rules** counts permutations and combinations too: enter n and r at the bottom of its form.

## In Python and R

```python
import itertools, math
rolls = list(itertools.product(range(1, 7), repeat=2))   # the 36 outcomes
sevens = [r for r in rolls if sum(r) == 7]
print(len(sevens) / len(rolls))          # 0.1667
print(math.perm(8, 3), math.comb(8, 3))  # 336 56
print(math.comb(8, 3) / math.comb(10, 3))   # 0.4667
```

```r
rolls <- expand.grid(d1 = 1:6, d2 = 1:6)   # the 36 outcomes
mean(rolls$d1 + rolls$d2 == 7)             # 0.1667
factorial(8) / factorial(5); choose(8, 3)  # 336 56
choose(8, 3) / choose(10, 3)               # 0.4667
```

The Example's script, which also simulates the rolls:

[[code: st.m3.sample-spaces.eg]]

## Common mistakes

- Leaving outcomes out: listing {PPF} as the only way for one part of three to fail, when there are three orders.
- Counting outcomes that are not equally likely, such as the sums of two dice, or the pass/fail patterns of parts that rarely fail.
- Using permutations when order does not matter (a sample of parts), or combinations when it does (an order of tests).

## What to do next

Look at the **Example** tab and open it in the Workbench: set the event to another sum and compare its theoretical probability with the table. Then answer the questions on the **Exercise** tab.
