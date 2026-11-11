---
id: st.m3
number: 3
title: Probability Concepts
status: draft
minutes: 240
prerequisites: [st.m2]
outcomes:
  - st.m3.o1: Construct a sample space for a given probability experiment using systematic methods such as lists, tables, or tree diagrams.
  - st.m3.o2: Calculate event probabilities using the complementary probability rule, the addition rule, and the multiplication rule.
  - st.m3.o3: Differentiate between theoretical and experimental probability by comparing their definitions, derivation methods, and the role of the Law of Large Numbers.
  - st.m3.o4: Interpret a calculated probability value in plain language within a real-world engineering or decision-making context.
  - st.m3.o5: Select the appropriate probability rule — complementary, general addition, or general multiplication — for solving a multi-step probability problem.
topics:
  - Introduction to Probability
  - Sample Spaces and Events
  - Theoretical vs. Experimental Probability
  - Core Probability Rules
  - Interpreting and Applying Probability in Context
---

Module 2 described data that had already been measured. Before a part is made or a test is run, an engineer has to reason about what **might** happen: will this weld crack, will both pumps fail on the same day, is a positive inspection result a real defect? **Probability** is the language for those questions. It puts a number between 0 and 1 on how likely an event is, and a few rules combine those numbers.

Everything after this module rests on it. The distributions of Modules 4 and 5, the confidence intervals of Module 7 and every hypothesis test are built from the rules you learn here.

This module has five topics and a module check. Each topic is an item with four tabs:

- **Learn** explains the idea, gives the rule, works a small case by hand, and shows the Python and R that do the same.
- **Example** shows a worked case: an experiment repeated many times on the computer, or two events and every probability that follows from them, with the code. Press **Open in the Workbench** to change the numbers and watch the results change.
- **Exercise** gives you a new case to calculate, or a set of questions. Work out the answers by hand, in Python or R, or in the Workbench, type them in, and press **Check my answers**. The item is complete when you pass. Hints are there if you are stuck.
- **Reference** has the rules, the Python and R, and where to read more.

Two analyses in the **Workbench** go with this module:

- **Simulation** flips coins, rolls dice, deals cards or throws darts as many times as you ask, counts how often an event happens, and draws that proportion against the theoretical probability. It shows the Law of Large Numbers at work. It is seeded: the same seed gives the same counts every time, so a question can say "with seed 1" and everyone sees the same numbers.
- **Probability rules** takes P(A), P(B) and what you know about how A and B relate, and gives the complements, P(A and B), P(A or B), both conditional probabilities, Bayes' theorem and an independence check. It also counts permutations and combinations.

Use them to check your hand calculations, not to skip them: the module check asks you to work small cases yourself.

The module check, the last item, asks ten questions over all five outcomes, including the course's three-card 21 problem. You need 80 % to pass, and you can try again.
