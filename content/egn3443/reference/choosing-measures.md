---
title: Choosing the measures to report
---

# Choosing the measures to report

| Data | Center | Spread | Example |
|---|---|---|---|
| Nominal (categories) | mode | count or % in each category | failure cause |
| Ordinal (ordered categories) | median | range of categories, IQR | condition rating 0 to 9 |
| Numerical, about symmetric | mean | standard deviation | machined diameters |
| Numerical, skewed or with outliers | median | IQR (with Q1 and Q3) | repair times |
| Any numerical, when a total matters | mean (total = n × x̄) | standard deviation | daily energy use |
| Comparing spread across scales | | coefficient of variation | bolt lengths against concrete strengths |

**Pairs that belong together:** mean with standard deviation; median with IQR. Add the minimum and maximum when the extremes matter.

## A checklist for a written summary

1. The quantity and its **unit**.
2. The **number of values**, n.
3. A **center** and a **spread** that match each other and the shape.
4. The **shape**, when it is not symmetric.
5. Any **unusual values**, and what was done about them.
6. Sensible **rounding**: one or two digits more than the data were measured to.
7. Words the **reader** uses, and what the numbers mean for their decision.

## Sources

- Montgomery and Runger, *Applied Statistics and Probability for Engineers*, 7th ed., Chapter 6.
- NIST/SEMATECH *e-Handbook of Statistical Methods*, Section 1.3.5.1, Measures of Location (which estimate of location to use), and Section 1.3.5.6, Measures of Scale: https://www.itl.nist.gov/div898/handbook/
