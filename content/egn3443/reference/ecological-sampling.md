---
title: Ecological sampling: mark–recapture and diversity
---

# Ecological sampling: mark–recapture and diversity

| Quantity | Formula |
|---|---|
| Lincoln–Petersen | N̂ = M·C/R (M marked, C caught later, R of them marked) |
| Chapman | N̂ = (M + 1)(C + 1)/(R + 1) − 1 |
| Recaptures | hypergeometric: mean C·M/N (item 4.6) |
| Shannon index | H = −Σ pᵢ ln pᵢ; maximum ln S for S species |
| Evenness (Pielou) | J = H / ln S |
| Simpson index | 1 − Σ pᵢ²: the chance two individuals are of different species |

**Assumptions of mark–recapture:** a closed population, no lost marks, every animal equally catchable.

## Python and R

| Task | Python | R |
|---|---|---|
| Recaptures | `stats.hypergeom(M=N, n=marked, N=catch).pmf(k)` | `dhyper(k, marked, N - marked, catch)` |
| Shannon | `-(p * np.log(p)).sum()` | `-sum(p * log(p))` or `vegan::diversity(counts)` |

**Read more:** Krebs, *Ecological Methodology* (Addison-Wesley Longman), chapters on mark–recapture and on species diversity.
