Statistle - Statistics for engineers
Test build {VERSION}, {DATE}

Thank you for trying Statistle. This is a pre-release build for feedback. It is
free software that follows EGN3443 "Probability and Statistics for Engineers"
and will be published in the Microsoft Store.


RUNNING IT
----------
1. Unzip this folder anywhere (your Desktop or Documents is fine).
2. Double-click Statistle.exe. Nothing is installed and no administrator rights
   are needed; it needs Windows 10 or 11 (64-bit) and nothing else.
3. Windows SmartScreen may show "Windows protected your PC" the first time,
   because this test build is not yet signed. Click "More info", then
   "Run anyway". The Store release will be signed and will not show this.
4. Optional: double-click "Create desktop shortcut.cmd" to put a Statistle
   shortcut on your desktop.

Your files: Statistle keeps its files in Documents\Statistle. Projects
(.statistle files) open and save there by default.

Sample projects are in the "samples" folder: open demo-statistics.statistle for
two data tables with a descriptive-statistics analysis and a regression already
set up on them.


WHAT TO TRY
-----------
- Add a data table (+ Data table) and paste your measurements into it: the
  first line is the column names, then one row per line. Tabs, commas and
  semicolons all work, so a paste straight out of a spreadsheet is fine.
  Columns may be different lengths - leave a cell blank where a column has no
  value in that row. Import... reads a text, CSV, .xd2 or .xlsx file.
- Open Descriptive statistics from the left rail and pick a column of that
  table. The count, mean, median and mode; range, standard deviation and
  variance (sample and population forms), standard error, coefficient of
  variation; quartiles two ways (interpolated, and Tukey's hinges), the
  interquartile range, the fences and any outliers; skewness and kurtosis; and
  a frequency table all follow. The Plots tab draws the histogram with a
  normal curve of the same mean and s, and the box plot. Edit the table and
  the analysis follows with it.
- Probability rules (Module 3): P(A), P(B) and how the events relate -
  independent, mutually exclusive, a known joint or conditional probability,
  or the two likelihoods for Bayes - give the complements, intersection, union,
  both conditionals and an independence check; nPr and nCr below.
- Discrete distribution (Module 4): binomial, Poisson, geometric, negative
  binomial, hypergeometric or a typed table. Enter k (and k2 for a range) and
  read P(X = k) and every tail, the mean, variance and SD. Plots: the PMF with
  the probability asked for highlighted, and the CDF.
- Continuous distribution (Module 5): uniform, normal, exponential, lognormal,
  t, chi-squared and F. Enter x (and x2) and a probability q: density, tails,
  the quantile (critical values: q = 0.975 for two-sided 5 %), mean, variance,
  median; z-score and the 68-95-99.7 rule for the normal. Plots: the density
  with the area shaded, and the CDF.
- Sampling distribution (Module 6): the mean and standard error of x-bar or
  p-hat for a sample size, the CLT probability of a sample statistic, the
  middle 95 % range, and whether the normal approximation holds. The plot
  shows the population and the sampling distribution on one axis.
- Every one of these writes its Python (scipy.stats) and R on the Code tab.
- Open Simple linear regression, pick an x column and a y column, and read the
  fitted line: its equation, r squared and the standard error of the estimate.
  Five curved forms are fitted beside it so a relation that is not straight can
  be recognised; compare them by standard error, which is in y units.
- Every screen has four tabs: Properties (entry), Report (values, plots,
  sources and the code as an appendix, with Open in browser / Save as PDF),
  Plots, and Code.
- The Code tab is the point of the program: the Python (numpy, scipy.stats)
  and the R that reproduce whatever is selected - a data table as variables,
  an analysis as its data followed by every result printed in the same order
  with the same labels. Copy it, or Save as... a .py or .R file, run it in
  Colab or R, and check it against the Results column. It is what goes in the
  appendix of an EGN3443 report. Please tell us where the code is wrong,
  unidiomatic, or explains too little.
- Click into any field: the panel under the form describes it - what it is,
  what it means, how to read it. Click empty space for an overview of the whole
  item. Please tell us where this text is wrong or unhelpful; it is meant for
  the student, not the programmer.


KNOWN LIMITS IN THIS BUILD
--------------------------
- Modules 1-6 and simple regression are in. Confidence intervals, hypothesis
  tests, multiple regression, validation and ANOVA (Modules 7-14) are still
  to come.
- Simple linear regression does not write code yet; Descriptive statistics
  and data tables do.
- Reports show a data table as one line per column (its name and how many
  values), not as a table of numbers. Export the CSV for the numbers.


SENDING FEEDBACK
----------------
Email ron.eaglin@gmail.com. Most useful: what you were trying to do, what you
expected, what happened, and the .statistle file if one is involved (it is a
small zip of XML). Screenshots help. Wrong or confusing help text, a number you
believe is off, a crash - all wanted.

If Statistle fails to start, or closes suddenly, it keeps a recovery copy of
your work at %LOCALAPPDATA%\Statistle\autosave.statistle.
