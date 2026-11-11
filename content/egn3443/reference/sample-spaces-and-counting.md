---
title: Sample spaces and counting
---

# Sample spaces and counting

| Term | Meaning |
|---|---|
| Experiment | A process whose result is decided by chance |
| Outcome | One possible result |
| Sample space S | The set of every outcome |
| Event A | A set of outcomes (a part of S) |
| Complement A′ | The outcomes not in A |
| Union A ∪ B | Outcomes in A, in B, or in both ("A or B") |
| Intersection A ∩ B | Outcomes in both ("A and B") |
| Mutually exclusive | A ∩ B is empty: A and B cannot both happen |

**Listing S:** a list for a few outcomes; a table for two parts (two dice: 6 × 6 = 36 cells); a tree diagram for stages (three parts, each pass or fail: 2³ = 8 branches).

**Equally likely outcomes:** P(A) = (outcomes in A) ÷ (outcomes in S). Check that the outcomes really are equally likely: the 36 ordered pairs of two dice are, the 11 sums are not.

## Counting

| Rule | Formula | Example |
|---|---|---|
| Multiplication principle | n₁ × n₂ × … | 3 materials × 4 finishes × 2 sizes = 24 |
| Permutations (order matters) | nPr = n! / (n − r)! | 3 of 8 tests in order: 336 |
| Combinations (order does not matter) | nCr = n! / (r!(n − r)!) | 3 of 8 samples: 56 |
| Probability by counting | favourable groups ÷ all groups | no defective in 3 of 10 relays, 2 defective: C(8, 3)/C(10, 3) = 56/120 |

## Python and R

| Task | Python | R |
|---|---|---|
| All outcomes of two dice | `list(itertools.product(range(1, 7), repeat=2))` | `expand.grid(d1 = 1:6, d2 = 1:6)` |
| Share of outcomes in an event | `sum(1 for r in rolls if sum(r) == 7) / len(rolls)` | `mean(rolls$d1 + rolls$d2 == 7)` |
| n! | `math.factorial(n)` | `factorial(n)` |
| nPr | `math.perm(n, r)` | `factorial(n) / factorial(n - r)` |
| nCr | `math.comb(n, r)` | `choose(n, r)` |

In the Workbench, **Probability rules** gives nPr and nCr for the n and r at the bottom of its form, and **Simulation** with two dice draws every sum's theoretical probability.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 2-1 (sample spaces and events, counting techniques) and 2-2 (interpretations of probability).
- EGN3443 Module 3, Topic 2: https://reaglin.github.io/EGN3443/week-4module-3-probability-concepts/
