---
id: st.m21.ecology
title: Ecological sampling
minutes: 45
objectives: [st.m21.o6]
example: st.m21.ecology.eg
exercise: st.m21.ecology.ex
reference: [ecological-sampling]
---

# Ecological sampling

An ecologist cannot count every fish in a lake or every bird in a forest. Population sizes and the variety of species are **estimated from samples**, and the methods come straight from probability: the **hypergeometric distribution** (item 4.6) for marked animals, and a **diversity index** for the mix of species.

## Mark–recapture

1. Catch M animals, **mark** them (a tag, a fin clip) and release them.
2. Later, catch C animals. R of them are marked.
3. If the second catch is a random sample, the share marked in it estimates the share marked in the whole population: R/C ≈ M/N. So

**N̂ = M·C / R** (the Lincoln–Petersen estimate).

The **Chapman** version, N̂ = (M + 1)(C + 1)/(R + 1) − 1, is less biased when R is small.

The assumptions are the statistics: the population is **closed** (no births, deaths or moves between the catches), marks are **not lost**, and marked and unmarked animals are **equally likely to be caught** (a trap-shy animal breaks it).

## The Example: fish in a pond

Suppose a pond holds 500 fish. 100 are caught, marked and released; a second catch takes 80 (illustrative, made up for this book). The number of marked fish in the second catch is **hypergeometric** (sampling without replacement from 500 with 100 marked):

[[figure: st.m21.ecology.eg 1]]

[[results: st.m21.ecology.eg]]

- The expected number marked is 80 × 100/500 = **16**, with standard deviation **3.3**.
- If the catch holds exactly 16 marked fish, N̂ = 100 × 80/16 = **500**: right on. Chapman: 101 × 81/17 − 1 = **480**.
- But R varies from catch to catch: with R = 12 the estimate is 667, with R = 20 it is 400. The spread of the hypergeometric is the uncertainty of N̂. With few recaptures, the estimate is poor: plan the catches so that R is at least 10 or so.

[[see-example: 1]]

## Diversity indices

Two woods can hold the same number of species and differ: one dominated by a single species, one evenly mixed. The **Shannon index** measures both richness and evenness:

H = −Σ pᵢ ln pᵢ, with pᵢ the share of individuals of species i.

A survey counts 50 trees: 30 oak, 15 maple, 5 birch. pᵢ = 0.6, 0.3, 0.1:

H = −(0.6 ln 0.6 + 0.3 ln 0.3 + 0.1 ln 0.1) = −(−0.306 − 0.361 − 0.230) = **0.898**.

Three species equally common would give ln 3 = 1.099, the maximum for three; the **evenness** is 0.898/1.099 = **0.82**. The **Simpson index**, 1 − Σ pᵢ² (here 1 − 0.46 = 0.54), is the chance that two individuals picked at random belong to different species.

## In Python and R

```python
from scipy import stats
import numpy as np
recaptured = stats.hypergeom(M=500, n=100, N=80)   # population, marked, catch
print(recaptured.mean(), recaptured.std(), recaptured.cdf(12))
N_hat = 100 * 80 / 16
p = np.array([30, 15, 5]) / 50; H = -(p * np.log(p)).sum()
```

```r
dhyper(16, 100, 400, 80)            # P(exactly 16 marked): marked, unmarked, catch
100 * 80 / 16                       # Lincoln–Petersen
p <- c(30, 15, 5) / 50; -sum(p * log(p))    # Shannon (vegan::diversity does it too)
```

[[code: st.m21.ecology.eg]]

## Common mistakes

- **A population that is not closed**: births and moves between the catches bias N̂.
- **Too few recaptures**: N̂ swings wildly with each one.
- **Richness alone** as diversity, ignoring evenness.
- **Comparing diversity indices** from samples of very different sizes.

## What to do next

On the **Exercise** tab, marked turtles in a wetland: the hypergeometric probabilities of the second catch.

[[example: Mark–recapture: the population size decides]]

100 fish marked, a second catch of 80, in a pond of **500**: on average **16** marked fish come back, and P(16 or fewer) = **0.569**.

1. **Move the population to 1,000.** Only 8 marked fish are expected: half as many recaptures for twice the fish. N̂ = 100 × 80/R tracks it.
2. **Move it to 250.** 32 expected: in a small population many marked fish come back, and N̂ is precise.
3. **Back to 500, and move k** to see P(at most k) for other numbers of recaptures: how likely is a catch that would make N̂ wrong by a third?

[[try: st.m21.recapture.try PopulationSize=200..2000 PopulationSuccesses Draws K | Exactly AtMost Mean StandardDeviation figure=1]]
