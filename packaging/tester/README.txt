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
  table. The count, mean, median, standard deviation, variance, standard error,
  coefficient of variation, range, quartiles and skewness all follow. Edit the
  table and the analysis follows with it.
- Open Simple linear regression, pick an x column and a y column, and read the
  fitted line: its equation, r squared and the standard error of the estimate.
  Five curved forms are fitted beside it so a relation that is not straight can
  be recognised; compare them by standard error, which is in y units.
- Every screen has three tabs: Properties (entry), Report (values, plots and
  sources, with Open in browser / Save as PDF) and Plots.
- Click into any field: the panel under the form describes it - what it is,
  what it means, how to read it. Click empty space for an overview of the whole
  item. Please tell us where this text is wrong or unhelpful; it is meant for
  the student, not the programmer.


KNOWN LIMITS IN THIS BUILD
--------------------------
- Two analyses so far: Descriptive statistics and Simple linear regression.
  Confidence intervals, hypothesis tests, ANOVA, control charts and the
  probability distributions are still to come.
- There is no Code tab yet: the plan is for each analysis to show the R or
  Python that would reproduce it.
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
