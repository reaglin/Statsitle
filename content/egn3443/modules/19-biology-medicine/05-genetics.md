---
id: st.m19.genetics
title: Genetics and genomics
minutes: 50
objectives: [st.m19.o5]
example: st.m19.genetics.eg
exercise: st.m19.genetics.ex
reference: [genetics-hardy-weinberg-fdr]
---

# Genetics and genomics

Genetics was statistical from its start: Mendel's pea ratios were counts compared with what a theory predicts. Two uses stand out today: checking that genotype counts fit the **Hardy–Weinberg** model, a chi-square goodness-of-fit test (item 8.5); and testing **thousands of genes at once**, which needs a new way to control false positives.

## Hardy–Weinberg equilibrium

A gene with two alleles, A and a, with frequencies p and q = 1 − p. If mating is random and nothing (selection, migration, mutation) disturbs the population, the genotypes occur in the proportions

**AA: p², Aa: 2pq, aa: q².**

Geneticists test genotype counts against these proportions. A clear departure can point to a real effect (inbreeding, selection), or, very often, to **genotyping errors**: large genome studies drop genetic markers that fail the test.

## The Example

1,000 people genotyped at one gene: **298 AA, 489 Aa, 213 aa** (illustrative data, made up for this book).

1. The allele frequency from the data: p = (2 × 298 + 489) / 2000 = **0.5425**, q = 0.4575.
2. Expected proportions: p² = 0.2943, 2pq = 0.4964, q² = 0.2093; expected counts 294.3, 496.4, 209.3.
3. The chi-square goodness-of-fit test with **3 − 1 − 1 = 1 degree of freedom**: one more is lost because p was estimated from the same counts.

[[results: st.m19.genetics.eg]]

χ² = 0.22, p = 0.64: the counts fit Hardy–Weinberg proportions well.

## Many tests at once

A genome-wide study tests **hundreds of thousands** of genetic markers, each for an association with a disease. At α = 0.05, 5 % of the markers with no effect at all will give p < 0.05 by chance: thousands of false discoveries.

- **Bonferroni**: test each at α/m. With m = 500,000, that is 10⁻⁷ (genome-wide studies use **5 × 10⁻⁸**). Safe, but it misses many real effects.
- The **false discovery rate** (FDR; Benjamini and Hochberg, 1995): control the expected **share of false discoveries among the discoveries**, say 5 %, rather than the chance of any false one.

**The Benjamini–Hochberg procedure.** Sort the m p-values, p₍₁₎ ≤ p₍₂₎ ≤ … ≤ p₍ₘ₎. Find the largest k with p₍ₖ₎ ≤ (k/m)·q, and call the first k discoveries.

Ten genes, q = 0.05 (illustrative p-values):

| Rank k | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
|---|---|---|---|---|---|---|---|---|---|---|
| p₍ₖ₎ | 0.001 | 0.008 | 0.012 | 0.030 | 0.041 | 0.09 | 0.20 | 0.35 | 0.60 | 0.88 |
| (k/10)·0.05 | 0.005 | 0.010 | 0.015 | 0.020 | 0.025 | 0.030 | 0.035 | 0.040 | 0.045 | 0.050 |

The largest k with p₍ₖ₎ below its line is **k = 3**: three discoveries. Uncorrected (p < 0.05): five. Bonferroni (p < 0.005): one.

[[see-example: 1]]

## In Python and R

```python
from scipy import stats
obs = [298, 489, 213]; p = (2*298 + 489) / 2000
exp = [1000*p*p, 1000*2*p*(1-p), 1000*(1-p)**2]
chi2, pval = stats.chisquare(obs, exp, ddof=1)        # ddof=1: p was estimated
from statsmodels.stats.multitest import multipletests
reject, p_adj, _, _ = multipletests(pvals, alpha=0.05, method="fdr_bh")
```

```r
obs <- c(298, 489, 213); p <- (2*298 + 489) / 2000
e <- 1000 * c(p^2, 2*p*(1-p), (1-p)^2)
x2 <- sum((obs - e)^2 / e); pchisq(x2, df = 1, lower.tail = FALSE)
p.adjust(pvals, method = "BH") < 0.05
```

[[code: st.m19.genetics.eg]]

## Common mistakes

- **Two degrees of freedom** for Hardy–Weinberg: one more is lost for the estimated p.
- **p < 0.05 for every one of thousands of genes**, uncorrected.
- **The FDR read as the chance a given discovery is false.** It is the expected share among all discoveries.

## What to do next

On the **Exercise** tab, 400 plants genotyped: test Hardy–Weinberg equilibrium.

[[example: Many genes tested at once]]

A simulation: **20 tests**, each of a gene with no real effect, at α = 0.05. In **644 of 1,000** simulated studies at least one gene came out "significant" (theory: 1 − 0.95²⁰ = 0.64).

1. **Move the number of tests to 100.** Nearly every study now "finds" something: about 99 %.
2. **Move α to 0.0025** (Bonferroni for 20 tests, 0.05/20) with 20 tests. The chance of any false discovery falls back to about 5 %.
3. **Change the seed** to run another 1,000 studies: the share moves a little; the theory does not.

[[try: st.m19.fdr.try Tests=1..200 Alpha=0.0005..0.1 Seed | Occurrences ExperimentalProbability TheoreticalProbability figure=1]]
