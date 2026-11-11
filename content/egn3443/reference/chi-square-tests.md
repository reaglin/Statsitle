---
title: Chi-square tests on counts
---

# Chi-square tests on counts

Both tests work on **counts** (never percentages or measurements), compare each observed count O with the count E that H₀ predicts, and add:

χ² = Σ (O − E)² / E

The p-value is always the **upper** tail, the area above the observed χ² under the χ² curve with df degrees of freedom. The approximation is trusted when every E is at least 5; otherwise combine neighboring categories (or, for a 2 × 2 table, use Fisher's exact test).

| | Goodness of fit | Independence (or homogeneity) |
|---|---|---|
| Data | one list of k counts | an r × c contingency table |
| H₀ | the category proportions are p₁, …, pₖ | the row and column variables are independent |
| Expected count | Eᵢ = n pᵢ | Eᵢⱼ = (row i total)(column j total) / n |
| df | k − 1 − m (m parameters estimated from the data) | (r − 1)(c − 1) |
| Size of the effect | the largest contributions (O − E)²/E | Cramér's V = √(χ² / (n (min(r, c) − 1))) |

Cramér's V: about 0.1 weak, 0.3 moderate, 0.5 strong.

## Critical values χ²(0.05, df)

| df | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| χ²(0.05, df) | 3.841 | 5.991 | 7.815 | 9.488 | 11.070 | 12.592 |

## Python and R

| Task | Python (scipy.stats) | R |
|---|---|---|
| Goodness of fit | `stats.chisquare(observed, f_exp=n * p)` | `chisq.test(observed, p = p)` |
| Independence | `stats.chi2_contingency(table, correction=False)` | `chisq.test(table, correct = FALSE)` |
| Cramér's V | `stats.contingency.association(table, method="cramer")` | `sqrt(X2 / (n * (min(dim(table)) - 1)))` |
| p from χ² | `stats.chi2.sf(chi2, df)` | `pchisq(chi2, df, lower.tail = FALSE)` |
| Critical value | `stats.chi2.ppf(1 - alpha, df)` | `qchisq(1 - alpha, df)` |

Without `correct = FALSE`, R applies Yates' continuity correction to a 2 × 2 table; the course does not.

## In the Workbench

**Chi-square tests**: **Test** *Goodness of fit* takes the observed counts and Equal or Entered proportions; *Independence or homogeneity* takes a table of counts. The results list each expected count and contribution; the Plots tab draws the χ² curve with the rejection region and the observed χ², and the observed against the expected counts.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Sections 9-7 (goodness of fit) and 9-8 (contingency tables).
- NIST/SEMATECH *e-Handbook*, Section 1.3.5.15, Chi-square goodness-of-fit test: https://www.itl.nist.gov/div898/handbook/eda/section3/eda35f.htm
