---
id: st.m0.report-template
title: The report template
minutes: 20
objectives: [st.m0.o3]
builds-on: st.m0.python-and-r
quiz: quizzes/m0-report.json
reference: [course-report]
---

# The report template

Every assignment in EGN3443 is handed in as a **report**. An engineer's numbers are only useful when someone else can read what was asked, how it was answered, and what the answer is, and can check the work. The course gives a template with seven sections. You may copy the template or use your own design, as long as the sections are there.

## The seven sections

| Section | What goes in it |
|---|---|
| **Title** | The title (you may center it), your name, the date, and who it is submitted to or for |
| **Introduction** | A brief introduction. A single sentence is enough, such as "This report fulfills the requirements of Assignment 3 of EGN3443." |
| **Problem statement** | Usually the problem from the assignment, copied. Only the core of the problem: leave out the outcomes and the grading policy |
| **Methodology** | How you approached and solved the problem. Name the statistical method, and the decisions you made and why. Equations are not always needed |
| **Conclusions** | The answer to what the problem statement asked. Plots go here when they are part of the answer |
| **References** | Only when the problem asks you to use sources |
| **Appendices** | The code, any hand calculations, and anything else that takes the reader from the methodology to the conclusions |

## Writing each part well

**Methodology** says what you did and why, in words. For example: *"The strengths are a sample of the day's pour, so the sample standard deviation (dividing by n − 1) was used. The histogram is strongly skewed, so the median, not the mean, is reported as the typical value."* It is not a copy of your code.

**Conclusions** answer the question in the problem statement, directly, with numbers and units. If the problem asks "Does the concrete meet the 30 MPa design strength?", the first sentence of the conclusions says yes or no, and the numbers that show it. Round sensibly: 32.3 MPa, not 32.2550 MPa.

**Appendices** hold the evidence. In this course that is usually the Python or R you ran, and its printed output, so that a reader can run it again and get your numbers.

## How Statistics+ helps

The Workbench does the calculation; you write the report. Its tabs give you pieces for the sections:

| Section | What Statistics+ gives you |
|---|---|
| Problem statement | The **Properties** tab lists the data and settings you entered: check them against the problem |
| Methodology | The **Instructions** tab (F1) says what the analysis does and when to use it, and the **Report** tab names the methods it used and their sources |
| Conclusions | The results on the **Properties** and **Report** tabs, and the figures on the **Plots** tab |
| Appendices | The **Code** tab: **Copy code** or **Save code as…** the Python or R. The **Report** tab also prints the code as appendices |

**File ▸ Save report as PDF…** saves the Report tab as a PDF. It is the calculation's record, not your report: it has no problem statement and no conclusions in your words. Paste from it, or attach it as an appendix if your instructor allows.

## Common mistakes

- Conclusions that do not answer the question that was asked, or answer it without numbers.
- Numbers without units, or with ten decimals.
- A methodology that is only code. Say in words what the method is and why it fits.
- Leaving out the appendix. Without the code or the hand calculation, nobody can check the numbers.
- Pasting a whole page of output into the conclusions. Put the full output in the appendix and the few numbers that answer the question in the conclusions.

## What to do next

Answer the questions on the **Exercise** tab. Then go on to Module 1, *Statistics for Engineers*. Its first assignment in the course is a short report showing that you can run Python or R, so this item and the last one are its preparation.
