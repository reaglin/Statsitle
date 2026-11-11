---
id: st.m3.interpreting
title: Interpreting probability in context
minutes: 45
objectives: [st.m3.o4, st.m3.o5]
builds-on: st.m3.rules
example: st.m3.interpreting.eg
quiz: quizzes/m3-interpreting.json
reference: [conditional-probability-and-bayes, probability-rules]
---

# Interpreting probability in context

A probability is only useful if the person deciding understands it. "P = 0.0013" means nothing to a city council; "about one year in 770" does.

## Saying a probability in plain words

1. **Name the event, the group and the time.** Not "the failure probability is 0.03" but "about 3 % of these pumps fail **in their first year**".
2. **Use a frequency people can picture.** 0.03 is "3 in 100"; 0.0013 is "about 1 in 770".
3. **Give both sides.** A 4 % chance that a part fails in a year is a 96 % chance that it lasts the year. Either alone can mislead.
4. **Pair it with the consequence.** A 1 % chance of a cosmetic scratch and a 1 % chance of a gas leak call for very different actions.
5. **Say where it came from.** "From 600 installations" is worth more than "from 10 tests"; a small sample is an uncertain estimate.

## Conditional probabilities are easy to turn around

A weld inspection by ultrasound flags 90 % of cracked welds. That is P(flag | crack) = 0.90. It is **not** the probability that a flagged weld is cracked, P(crack | flag), and the two can be very different. Mixing them up is the most common and most costly error in interpreting probability.

## Bayes' theorem

To turn a conditional probability around, combine the multiplication rule with the **total probability rule**:

P(B) = P(B | A)·P(A) + P(B | A′)·P(A′)

P(A | B) = P(B | A)·P(A) / P(B)

P(A) is the **prior**, the chance before the test; P(A | B) is the **posterior**, the chance after it.

## A worked case by hand

2 % of the welds on a line are cracked. The ultrasonic test flags 90 % of cracked welds, and also flags 5 % of sound welds (false alarms). A weld is flagged. How likely is it to be cracked?

Follow 10 000 welds down the tree:

```
10 000 welds ──┬── 200 cracked (2 %) ──┬── 180 flagged (90 %)
               │                       └──  20 missed
               └── 9 800 sound (98 %) ──┬── 490 flagged (5 %)
                                        └── 9 310 clear
```

1. **Flagged welds:** 180 + 490 = 670 of 10 000. P(flag) = 0.9 × 0.02 + 0.05 × 0.98 = 0.018 + 0.049 = **0.067**.
2. **Cracked among the flagged:** 180 of 670. P(crack | flag) = 0.018 / 0.067 = **0.2687**.
3. **In plain words:** only about 1 flagged weld in 4 is really cracked. The test is good, but cracks are rare, so the 5 % false alarms on the many sound welds outnumber the real finds.

The decision follows: a flag should send the weld to a second, more exact test (radiography), not straight to a repair crew. The rarer the fault, the more a positive result needs confirming. This is the **base rate** effect.

## The Example: the weld inspection in the Workbench

**Probability rules** with "P(B | A) and P(B | A′) are known — Bayes", A = cracked and B = flagged:

[[results: st.m3.interpreting.eg]]

P(B) used is the total probability, 0.067, and P(A | B) is Bayes' answer, 0.2687. P(B | A) = 0.90 is the entered sensitivity, which the program also gets back from P(A ∩ B) / P(A).

## Probability and decisions

When outcomes have costs, multiply each by its probability and compare the **expected cost**. A bridge bearing can be the standard model, which seizes in its design life with probability 0.02, or an upgraded one at $6 000 more, which seizes with probability 0.005. A seizure costs $500 000 to repair.

- Standard: 0.02 × $500 000 = **$10 000** expected repair cost.
- Upgraded: 0.005 × $500 000 = **$2 500**, plus $6 000 = **$8 500**.

The upgrade is worth it on average. A high probability alone does not justify an action, nor a low one rule it out: the consequence decides. Module 4 makes the expected value precise.

## In Python and R

```python
p_crack, p_flag_crack, p_flag_sound = 0.02, 0.90, 0.05
p_flag = p_flag_crack * p_crack + p_flag_sound * (1 - p_crack)
print(p_flag)                              # 0.067
print(p_flag_crack * p_crack / p_flag)     # P(crack | flag) = 0.2687
```

```r
p_crack <- 0.02; p_flag_crack <- 0.90; p_flag_sound <- 0.05
p_flag <- p_flag_crack * p_crack + p_flag_sound * (1 - p_crack)
p_flag                                     # 0.067
p_flag_crack * p_crack / p_flag            # 0.2687
```

[[code: st.m3.interpreting.eg]]

## Common mistakes

- Reading P(flag | crack) as P(crack | flag), and ignoring how rare the fault is.
- A number with no event, group or time frame: "the risk is 2 %" of what, over how long?
- Treating an estimate from a few trials as exact.
- Deciding on the probability alone, without the consequence.

## What to do next

Answer the questions on the **Exercise** tab: a leak detector, a flood level and a choice between two designs.
