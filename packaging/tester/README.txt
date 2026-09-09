Statistle - Statistics for engineers
Test build {VERSION}, {DATE}

Thank you for trying Statistle. This is a pre-release build for feedback. It is
free software that follows EGN3443 "Probability and Statistics for Engineers"
and will be published in the Microsoft Store.


RUNNING IT
----------
You have one of two forms of this build; both hold the same program.

The installer (Statistle-<version>-setup.exe):
1. Double-click it. It installs for you alone, without administrator rights,
   into your local programs folder, with a Start-menu entry, an optional
   desktop icon and an optional association for .statistle files. An
   administrator can choose "all users" instead.
2. Uninstall from Settings > Apps, or the Start-menu entry. Your projects in
   Documents\Statistle are left in place.

The zip (Statistle-<version>-win-x64.zip):
1. Unzip this folder anywhere (your Desktop or Documents is fine).
2. Double-click Statistle.exe. Nothing is installed and no administrator rights
   are needed; it needs Windows 10 or 11 (64-bit) and nothing else.
3. Optional: double-click "Create desktop shortcut.cmd" to put a Statistle
   shortcut on your desktop.

Either way, Windows SmartScreen may show "Windows protected your PC" the first
time, because this test build is not yet signed. Click "More info", then
"Run anyway". The Store release will be signed and will not show this.

Your files: Statistle keeps its files in Documents\Statistle. Projects
(.statistle files) open and save there by default.

The first time Statistle runs it puts the sample project
demo-statistics.statistle into Documents\Statistle (a copy is also in this
folder's "samples"): four data tables with a descriptive-statistics analysis,
two regressions and two ANOVAs already set up on them. File > Open lands in
Documents\Statistle, so it is the first thing you see. Edit it freely; a
changed copy is never overwritten, and deleting it brings a fresh one back on
the next start.


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
- Confidence interval (Module 7): for a mean (z or t), a proportion (Wald,
  with Wilson beside it), a variance, or the difference of two means (pooled,
  Welch, paired) or two proportions - from summary statistics you type, or
  from columns of a data table (the summaries are computed and shown locked).
- Sample size (Module 7): the n a margin of error needs, for a mean or a
  proportion.
- One-sample test (Modules 8-9): z or t on a mean, z on a proportion, chi-
  squared on a variance; one- or two-sided; the rejection region, p-value,
  decision and confidence interval side by side; power against a stated mean
  and the n for a target power. The plot shades the rejection region on the
  reference distribution with the observed statistic.
- Two-sample test (Modules 9-10): pooled t, Welch t, paired t, z, two
  proportions, F for two variances, Mann-Whitney U and Wilcoxon signed-rank.
  With data columns: Cohen's d, Shapiro-Wilk normality and Levene equal-
  variance checks, and box plots of the two samples.
- Every one of these writes its Python (scipy.stats) and R on the Code tab,
  with the library call (ttest_ind, t.test, wilcox.test...) as a check.
- Simple linear regression (Modules 11, 13): pick an x and a y column, or type
  the pairs, and read the least-squares line: slope and intercept with their
  standard errors, the t-test and confidence interval on the slope, r, R
  squared, the residual standard error, the ANOVA F, and a prediction at a
  chosen x with the interval on the mean response and the wider one on a new
  observation. Below: Durbin-Watson, Shapiro-Wilk on the residuals, the largest
  Cook's distance and leverage. Transform x or y (ln, square root, reciprocal)
  when the residual plot curves; the fit is drawn back in the original units.
  Plots: the data with the fit, residuals against fitted, normal Q-Q.
- Multiple regression (Modules 12-13): choose a table, the response column and
  the predictor columns (by name; blank means all the others). Read the
  coefficient table with standard errors, t, p, confidence intervals and VIFs,
  R squared and adjusted R squared, the F-test, the same residual diagnostics,
  and k-fold cross-validated RMSE beside the in-sample RMSE. Forward selection
  and backward elimination at 0.05 are on the Variable selection drop-down,
  with a note of each step taken. The sample project's Housing table has four
  predictors of price.
- Both regressions write statsmodels (Python) and lm (R) on the Code tab,
  including the prediction, the diagnostics and the cross-validation loop.
- One-way ANOVA (Module 14): choose a table whose columns are the groups (the
  sample's three alloys), or a factor column of level codes with a response
  column. Read the ANOVA table, F and its p-value, the decision, eta squared,
  Cohen's f and the power, then Tukey's HSD on every pair with simultaneous
  intervals, Bartlett and Levene for equal variances, Shapiro-Wilk on the
  residuals, and Kruskal-Wallis as the rank-based fallback. Plots: the groups
  as box plots, the means with intervals, the residual Q-Q.
- Two-way ANOVA (Module 14): a table with two columns of level codes and a
  response, the same number of rows in every cell. Read SS, df, MS, F and p
  for factor A, factor B and their interaction, with a decision on each, and
  the interaction plot (parallel lines mean no interaction). With one row per
  cell the additive model is fitted. The sample's Battery life table is a
  3 x 2 design with three replicates.
- Both ANOVAs write scipy / statsmodels (f_oneway, tukey_hsd, anova_lm,
  pairwise_tukeyhsd) and R (aov, TukeyHSD, kruskal.test).
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
- All fourteen modules are in. Store packaging is the remaining phase.
- Two-way ANOVA needs a balanced design (equal replicates per cell) and
  numeric level codes for the factors; unbalanced data is reported, not
  fitted.
- Multiple regression takes its columns from one table; predictors are named
  by typing them (a drop-down for several columns is coming).
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
