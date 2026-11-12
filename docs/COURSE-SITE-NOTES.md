# Notes on the course site (for Ron)

Slips found on https://reaglin.github.io/EGN3443/ while writing Statistics+. Ron fixes the site
himself, later (Ron, 2026-11-11: *"Make a note of issues with the course site for me to look at"*).
Statistics+ does not repeat any of them: its own text gives the right figure or the missing formula.

Add to this list when writing or reviewing content turns up something new. Tick an item off once the
site is fixed.

| # | Module | Where | What | Suggested fix |
|---|---|---|---|---|
| 1 | 10 | headings | Labelled "Week 11 / Module 10" in one place and "Module 11" in another | One module number throughout |
| 2 | 10 | two-proportion test | No formula for the confidence interval of p₁ − p₂ | Add (p̂₁ − p̂₂) ± z·√(p̂₁(1 − p̂₁)/n₁ + p̂₂(1 − p̂₂)/n₂) (the unpooled SE, as Statistics+ 10.5 uses) |
| 3 | 10 | Example 2 | Claims a direction ("significantly higher in Group 1") from a two-sided test | Say "differs", or run the one-sided test |
| 4 | 10 | equal-variance checks | Levene's test is called an "F-test" | Call it Levene's test (its statistic follows an F distribution, which may be the source of the name) |
| 5 | 10 | paired test | d is x₂ − x₁ on one page and x₁ − x₂ on another | One direction throughout |
| 6 | 11 | interval example | A CI of [0.2, 0.4] with SE = 0.1 is inconsistent: ±1.96·SE is about ±0.2, so the interval should be about 0.4 wide | Make the SE 0.05, or the interval [0.1, 0.5] |
| 7 | 11 | Topic 4, adjusted R² table | The values 0.774, 0.799, 0.791 do not match the page's own formula, which gives 0.775, 0.802, 0.800 | Recompute the column |
| 8 | 12 | landing page | The lecture link is titled "Module 12 - Hypothesis Testing for 2 Samples" | Retitle it for multiple regression |
| 9 | 12 | study page code | The Python uses `pd`, `X` and `y` without defining them | Add `import pandas as pd` and the lines that build `X` and `y` |
| 10 | 14 | familywise error | 1 − (1 − α)ᵐ is given without saying it assumes independent tests; pairwise tests are correlated, so the real rate is lower (about 0.28 for five groups, not 0.40) | One sentence saying the formula is an upper bound for pairwise comparisons |
