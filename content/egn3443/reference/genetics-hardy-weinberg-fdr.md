---
title: Genetics: Hardy–Weinberg and the false discovery rate
---

# Genetics: Hardy–Weinberg and the false discovery rate

| Quantity | Formula |
|---|---|
| Allele frequency | p = (2·n_AA + n_Aa)/(2n), q = 1 − p |
| Hardy–Weinberg proportions | AA p², Aa 2pq, aa q² |
| Test | chi-square goodness of fit (item 8.5), df = 3 − 1 − 1 = 1 (p estimated) |
| Bonferroni | test each of m at α/m (genome-wide: 5 × 10⁻⁸) |
| Benjamini–Hochberg (FDR q) | sort p-values; find the largest k with p₍ₖ₎ ≤ (k/m)q; reject the first k |
| Chance of at least one false positive in m independent tests | 1 − (1 − α)^m |

## Python and R

| Task | Python | R |
|---|---|---|
| Hardy–Weinberg test | `scipy.stats.chisquare(obs, expected, ddof=1)` | `pchisq(sum((obs - e)^2/e), df = 1, lower.tail = FALSE)` |
| FDR | `statsmodels.stats.multitest.multipletests(p, method="fdr_bh")` | `p.adjust(p, method = "BH")` |
| Bonferroni | `multipletests(p, method="bonferroni")` | `p.adjust(p, method = "bonferroni")` |

**Read more:** Benjamini and Hochberg (1995), "Controlling the false discovery rate", *Journal of the Royal Statistical
Society B* 57, 289–300.
