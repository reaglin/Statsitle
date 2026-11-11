# Statistics+ — how content is written

The lessons are files, not code: an author edits Markdown and JSON, runs the validator, and the program shows the
change (plan D6). The module, item, quiz and reference formats are the **shared Learning Core's**, exactly as PLC+
uses them (`..\..\PLCPlus\docs\CONTENT-FORMAT.md` is the full description of those). This page repeats what a
Statistics+ author needs and adds the statistics half: **tasks** (the Examples and Exercises) and the **widgets**.

## The package

```
content/egn3443/
  manifest.json          { "id": "egn3443", "version": "…", "title": "…", "modules": ["modules/00-start-here", "modules/02-descriptive", …] }
  modules/
    02-descriptive/
      module.md          the module: settings, then its introduction
      01-introduction.md an item: settings, then its Learn page
      …
      06-check.md        the module check, last
  reference/             Reference pages, shared by every item that lists them
  quizzes/               question sets (an item's Exercise when it has questions, or the module check)
  tasks/                 Examples and Exercises: one analysis each, with its data (below)
```

Items are shown in file-name order and numbered from it, so start each file name `01-`, `02-`, …
Ids never change once published: progress and reports key off them. Statistics+ uses `st.mN` for a module,
`st.mN.oK` for the course's outcome MO K, `st.mN.<slug>` for an item, and `st.mN.<slug>.ex` / `.eg` for its
exercise and example tasks.

## A module (`module.md`)

```
---
id: st.m2
number: 2
title: Descriptive Statistics
status: draft
minutes: 240
prerequisites: [st.m1]
outcomes:
  - st.m2.o1: Calculate the mean, median and mode of a data set …   (the course's MO list, in its words)
topics:
  - Measures of central tendency
---

The introduction, in Markdown.
```

## An item

```
---
id: st.m2.central-tendency
title: Measures of central tendency
minutes: 30
objectives: [st.m2.o1, st.m2.o4]
builds-on: st.m2.introduction
example: st.m2.central-tendency.eg
exercise: st.m2.central-tendency.ex
reference: [mean-median-mode]
---

# Measures of central tendency

The Learn page …
```

| Setting | The tab it fills |
|---|---|
| (the Markdown after the settings) | **Learn** |
| `example` (a task id) | **Example**: the task's analysis, worked: its data, results, figures, and the Python and R. *Open in the Workbench* copies it there to change the inputs |
| `exercise` (a task id with an `ask` list) | **Exercise**: the data and a box for each asked result; *Check my answers*. **The item is complete when it passes** |
| `quiz` (`quizzes/….json`) | **Exercise** as questions instead (an item has an exercise or a quiz, not both) |
| `reference` (page names) | **Reference** |
| `objectives` | the module outcomes this item teaches (mastery uses them) |

With neither `exercise` nor `quiz`, an item is complete on *I have read this*. The **module check** is the last item:
`kind: assess`, `covers: [item ids]`, and usually a `quiz`.

**The Example is never the Exercise's answer** (PLC+ D21): use a similar dataset, so the Example teaches the method and
the Exercise checks it.

## A task (`tasks/*.json`)

```json
{
  "id": "st.m2.central-tendency.ex",
  "title": "Bolt lengths",
  "analysis": "DescriptiveStatistics",
  "intro": "A machine cuts bolts to 50 mm. Twelve were measured (mm). Calculate the mean and the median.",
  "settings": { "Sample": [50.2, 49.8, 50.1, 50.4, 49.9, 50.0, 50.3, 49.7, 50.2, 50.1, 51.6, 50.0] },
  "ask": [ "Mean", { "result": "Median", "prompt": "Median (mm)" } ],
  "hints": [ "The mean is the sum divided by n.", "Sort the values first for the median." ],
  "passThreshold": 1.0
}
```

| Field | Meaning |
|---|---|
| `analysis` | The analysis class: `DescriptiveStatistics`, `ProbabilityRules`, `DiscreteDistribution`, `ContinuousDistribution`, `SamplingDistribution`, `ConfidenceInterval`, `SampleSize`, `OneSampleTest`, `TwoSampleTest`, `SimpleRegression`, `MultipleRegression`, `OneWayAnova`, `TwoWayAnova` (and those phase 10 adds) |
| `settings` | Property name → value, **as the analysis's project file writes it** (the property names in `src/Statistle.Core/Analyses/*.cs`). A list of numbers is a series, one value per line. An enum is its name. A setting not given keeps the analysis's default; `null` leaves a number blank (no alternative mean, so no power; no upper bound x₂) |
| `tables` | Data tables the task brings: `[{ "name": "Alloys", "columns": { "A": [..], "B": [..] } }]`. A setting `"@Alloys/A"` points the analysis at that column |
| `ask` | The results asked, by property name (only numbers). A string, or `{ "result", "prompt", "tolerance", "wrong" }`. `wrong` is what a wrong answer that matches no known mistake should check, in place of "Check the formula in the Learn tab and your arithmetic" (for a number read in the Workbench: "Check the Workbench settings against the list above.") |
| `mistakes` | Common mistakes the exercise recognizes, by asked result (below) |
| `hints` | Shown one at a time on request |
| `passThreshold` | The share of answers that must be right (default 1: all) |

A **contingency table** for Chi-square tests (`"Test": "Independence"`) comes from a task table:
`"tables": [{ "name": "Pension", "columns": { "Plan 1": [160, 40], "Plan 2": [140, 60], "Plan 3": [40, 60] } }]` and
`"CountsTable": "@Pension/"` — each column of the table is a column of counts, one value per row category
(`"RowNames": "Salaried, Hourly"` names the rows). Goodness of fit takes `"Observed"` as a list of counts.

**There is no answer in the file.** The program runs the analysis and calculates every asked result, so the key always
matches the engine (plan D5). An answer is accepted within half a unit in the second decimal or 0.5 % of the value,
whichever is larger; a count must be exact; `tolerance` overrides that.

A task without `ask` is an Example.

### Mistakes an exercise recognizes

A wrong answer normally says "too high" or "too low". When a common mistake gives a number of its own, name it, and a
learner who makes it reads what went wrong instead:

```json
"mistakes": {
  "Variance": [
    { "result": "PopulationVariance", "says": "You divided by n = 12. These bars are a sample, so divide by n − 1 = 11." },
    { "result": "StandardDeviation", "says": "That is the standard deviation. The variance is its square, s²." }
  ],
  "Median": [
    { "value": 50.15, "says": "You took the middle two values as they are listed. Sort the values first." }
  ],
  "CoefficientOfVariation": { "scale": 0.01, "says": "Give it as a percentage: multiply by 100." }
}
```

| Field | Meaning |
|---|---|
| the key | An asked result (a name in `ask`). Its value is one mistake or a list of them, checked in order |
| `result` | Another result of the same analysis that the mistake gives, by property name (the program calculates it, as it does the answer) |
| `scale` | Multiplies `result`, or the asked result itself when `result` is left out (a percentage typed as a fraction) |
| `value` | A fixed number, for a mistake no result gives (the median of the unsorted values). Not with `result` or `scale` |
| `says` | The sentence the learner reads after "Mean: 50.1 is not right.". Say what went wrong and what to do; never the answer |

An answer counts as the mistake when it is within the asked result's tolerance of the mistake's number. The validator
refuses a mistake that names a result the analysis does not have, a key the task does not ask, and a mistake whose number
is accepted as the right answer (the learner would never read it).

## Widgets in a Learn page

A line on its own:

| Widget | Draws |
|---|---|
| `[[figure: task-id 1]]` | the task's analysis's figure number 1 (histogram, box plot, distribution curve, regression line, …) |
| `[[data: task-id]]` | the task's data as a table |
| `[[results: task-id]]` | the task's results: label, value, unit |
| `[[code: task-id]]` | the Python and the R that reproduce the task's analysis |

## Questions

As PLC+: `choice`, `multiple`, `numeric` (`answer`, `tolerance`, `unit`), `match`, `text` (`answers`). Every question has
`id`, `prompt` (Markdown), `explanation` and `objective`. `practice: true` shows the answer after a wrong try.

On a module check a wrong answer says "Look again at …". By default that is the first item listing the question's
`objective`, which is often the module's introduction. Give a question `"lookAgain": "<item id>"` to name the item
that teaches it; the validator checks that it is a lesson in the package. Statistics+ reads this field itself (the
Learning Core ignores it).

The answer box's tooltip says how precise to be: a count is a whole number; a result whose tolerance is below 0.005
asks for at least three decimals; otherwise two are usually enough. Say the same in the exercise's intro.

A **widget** may be written in a question too, on a line of its own (`\n` in the JSON string), so a question can show
the picture it asks about:

```json
"prompt": "Look at the histogram of the twenty reply times. How is this distribution shaped?\n[[figure: st.m2.selecting.eg 1]]"
```

In the `explanation`, put the widget after a line of text (it is drawn under the feedback, when the feedback shows the
explanation). Keep the answer right for the picture: the figure is drawn from the task's data.

## The validator

`dotnet test tests/Statistle.Learning.Tests --filter Content` loads `content/egn3443`, runs the Learning Core's checks and
the statistics ones (every task builds and calculates; every exercise asks for numbers the analysis has; every mistake names
a result and is not the right answer; every widget, in a page or a question, names a task and a figure it draws), and prints
one sentence per problem. A problem fails the test.

It also prints **warnings**, which do not fail it: things to look at. Today one: two results an exercise asks for lie within
each other's tolerance (a mean and a median 0.009 apart), so a learner who types one for the other passes. Change the data or
give a smaller `tolerance`. Read them in the test's output (`--logger "console;verbosity=detailed"`).
