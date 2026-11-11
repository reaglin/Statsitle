---
title: Python and R: where to run them and the first functions
---

# Python and R: where to run them and the first functions

## Where to run the code

| Tool | Address | Notes |
|---|---|---|
| Google Colab (Python) | https://colab.research.google.com | The course's recommendation. Needs a Google account; numpy and scipy are installed |
| Anaconda (Python) | https://www.anaconda.com/download | Python on your own computer, with Jupyter and Spyder |
| R | https://www.r-project.org | R on your own computer; RStudio is an optional editor for it |
| mycompiler | https://www.mycompiler.io | Python and R online; needs a free account to save files |
| rdrr.io, W3Schools tryR | https://rdrr.io | R online for quick calculations; nothing is saved |

## The first functions

| Task | Python (numpy, scipy.stats) | R (base) |
|---|---|---|
| Load the libraries | `import numpy as np` and `from scipy import stats` | nothing needed |
| Store data | `x = np.array([9.8, 10.4, 10.1])` | `x <- c(9.8, 10.4, 10.1)` |
| Number of values | `len(x)` | `length(x)` |
| Mean | `np.mean(x)` | `mean(x)` |
| Median | `np.median(x)` | `median(x)` |
| Sample standard deviation | `np.std(x, ddof=1)` | `sd(x)` |
| Sample variance | `np.var(x, ddof=1)` | `var(x)` |
| Show a result | `print(np.mean(x))` | `mean(x)`, or `cat(mean(x), "\n")` in a script |
| Round for a report | `round(np.mean(x), 2)` | `round(mean(x), 2)` |

**ddof=1.** numpy divides by n unless told `ddof=1` (delta degrees of freedom); R's `sd` and `var` always divide by n − 1. For a sample, use n − 1.

## Errors you will meet first

| Message | Usual cause |
|---|---|
| `NameError: name 'np' is not defined` | `import numpy as np` was not run |
| `NameError: name 'x' is not defined` | the cell that stores `x` was not run (Colab: Runtime ▸ Run all) |
| `Error: object 'x' not found` (R) | the line that stores `x` was not run, or was spelled differently (R is case sensitive) |
| `SyntaxError` | a missing bracket or comma, or a decimal comma such as 9,8 |

## Sources

- The course page "Software for the Class": https://reaglin.github.io/EGN3443/week-1-welcome-start-here/content/Software_for_the_Class.html
- numpy documentation, `numpy.std`: https://numpy.org/doc/stable/reference/generated/numpy.std.html
- *An Introduction to R*, the R Core Team: https://cran.r-project.org/doc/manuals/r-release/R-intro.html
