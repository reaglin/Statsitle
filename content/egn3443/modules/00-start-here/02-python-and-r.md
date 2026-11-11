---
id: st.m0.python-and-r
title: Python and R for this course
minutes: 35
objectives: [st.m0.o2]
builds-on: st.m0.how-it-works
example: st.m0.python-and-r.eg
exercise: st.m0.python-and-r.ex
reference: [python-and-r]
---

# Python and R for this course

The course uses two tools for calculation: **Python** (with the numpy and scipy libraries) and **R**. You do not need both. Choose one and stay with it. Python is the usual choice if you used it in an earlier course such as EGN3214. R was written for statistics, and many engineering statistics books use it.

Statistics+ writes the code for you: every Example, and every analysis in the Workbench, shows the Python and the R that give the same numbers. Your assignments put that code in the report's appendix, so a reader can check your numbers (item 0.3).

## Where to run the code

| Tool | How | What you need |
|---|---|---|
| **Google Colab** (Python) | In a web browser at https://colab.research.google.com: New notebook, paste the code in a cell, press ▶ (or Shift+Enter) | A Google account. numpy and scipy are already installed |
| **Anaconda** (Python) | Installed on your computer from https://www.anaconda.com/download; run the code in Jupyter or Spyder | numpy and scipy come with it |
| **R** | Installed from https://www.r-project.org; run the code in the R console, or in RStudio if you have it | Nothing else: the course's R code uses base R, no packages |
| **R online** | https://rdrr.io, or W3Schools' tryR, for quick calculations without installing | Nothing; files are not saved |

The course recommends **Google Colab** for Python. It needs no installation, works on any computer with a browser, and saves your notebooks in your Google Drive.

## Your first run, step by step (Colab)

1. Open https://colab.research.google.com and sign in with your Google account.
2. Press **New notebook**. A notebook is a list of cells; each cell holds code.
3. Open the **Example** tab of this item. Under **The same in Python and R**, choose **Python** and press **Copy Python**.
4. Click in the first cell of the notebook, paste (Ctrl+V) and press ▶ to the left of the cell.
5. The results print under the cell, one per line, each with the same label the program uses: `Mean: 10.1750`, `Median: 10.1500`, and so on.
6. Compare them with the results on the Example tab. They must agree to the decimals shown.

In R the steps are the same: choose **R**, press **Copy R**, paste it into the R console (or an RStudio script) and run it.

## What the code looks like

The Example's script is long because it prints every result. The core of it is a few lines. Here it is for the eight battery run times of the Example, in hours:

[[data: st.m0.python-and-r.eg]]

```python
import numpy as np
x = np.array([9.8, 10.4, 10.1, 9.6, 10.9, 10.2, 9.9, 10.5])
print(np.mean(x))          # 10.175
print(np.median(x))        # 10.149999999999999, that is 10.15
print(np.std(x, ddof=1))   # 0.42003401222826164 (sample standard deviation)
```

```r
x <- c(9.8, 10.4, 10.1, 9.6, 10.9, 10.2, 9.9, 10.5)
mean(x)     # 10.175
median(x)   # 10.15
sd(x)       # 0.4200 (sample standard deviation)
```

Read it line by line:

- `import numpy as np` loads the numpy library under the short name `np`. R needs no such line for these functions.
- `x = np.array([...])` in Python, or `x <- c(...)` in R, stores the data in a variable called `x`.
- `print(...)` in Python shows a result. In R, a line on its own shows its result; inside a script, `cat(...)` or `print(...)` does.
- Everything after `#` is a comment, for the reader. It is not run.
- Python prints every digit it has, so the median comes out as 10.149999999999999. Computers store decimals in binary, and 10.15 cannot be stored exactly; the difference is far below anything you measure. Round when you report: 10.15 h. R rounds to 7 significant digits when it prints, so it shows 10.15.

**Watch the one difference that catches everyone.** numpy's `np.std` divides by n unless you write `ddof=1`; R's `sd` always divides by n − 1. The course, Statistics+ and R all use n − 1 for a sample (Module 2 explains why). Without `ddof=1`, Python gives 0.3929 h for these batteries instead of 0.4200 h.

Here is the full script from the Example:

[[code: st.m0.python-and-r.eg]]

## Getting data into the code

- On an Example or an Exercise, **Copy as Python** (or **Copy as R**) beside the data copies it as code: in Python `sample = np.array([...])`, in R `sample <- c(...)`. Paste it into a cell, and the data is in a variable called `sample`.
- In the Workbench, the **Code** tab shows the code for whatever you have chosen. **Copy code** puts it on the clipboard, and **Save code as…** saves it as a `.py` or `.R` file.
- For your own data in a spreadsheet, the simplest way is to copy the column and type it into `np.array([...])` or `c(...)`, with commas between the values.

## Common mistakes

- Running only part of a notebook. In Colab, a cell that uses `x` fails with `NameError` if the cell that defines `x` has not been run. **Runtime ▸ Run all** runs every cell in order.
- Leaving out `import numpy as np`, which gives `NameError: name 'np' is not defined`.
- Typing a decimal comma (9,8) instead of a decimal point (9.8). Both Python and R read the comma as a separator between two numbers.
- Forgetting `ddof=1` in numpy's `std` and `var`.
- In R, typing the Python form `np.mean(x)`, or in Python the R form `mean(x)`. Each language has its own names.

## What to do next

On the **Exercise** tab are six temperature readings. Press **Copy as Python** (or **Copy as R**) beside them, paste the data into Colab or R, print the mean and the sample standard deviation, and type the two numbers your code prints. Press **Check my answers**.
