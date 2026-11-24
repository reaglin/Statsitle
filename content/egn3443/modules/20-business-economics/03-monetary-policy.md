---
id: st.m20.monetary
title: Monetary policy
minutes: 50
objectives: [st.m20.o3]
example: st.m20.monetary.eg
exercise: st.m20.monetary.ex
reference: [monetary-policy-taylor-rule]
---

# Monetary policy

A central bank (the Federal Reserve in the United States, the European Central Bank, the Bank of England) sets a short-term interest rate, the **policy rate**, to keep inflation low and steady without choking growth. It decides on statistics: a price index, measures of output and jobs, and models fitted to them. This page shows two of them: **measuring inflation** with a price index, and **describing a central bank's decisions** with a regression, the Taylor rule.

## Inflation from a price index

A **price index** prices the same basket of goods and services at different times, relative to a base period set to 100. The best known is the **consumer price index** (CPI), built from thousands of prices collected every month by a national statistics office (in the US, the Bureau of Labor Statistics).

**Inflation** is the percentage change in the index:

inflation = (index now − index a year ago) / index a year ago × 100 %

An index that goes from 250.0 to 262.5 in a year: (262.5 − 250.0)/250.0 = **5.0 % inflation**. Two statistical points matter here:

- The index is an **estimate**: prices are sampled, so the index has sampling error, and statistics offices publish how it is sampled and weighted.
- The **basket's weights** come from surveys of what households buy. When people switch to cheaper goods as prices rise, a fixed basket overstates the cost of living. This is one reason several indices exist.

## The Taylor rule

In 1993 the economist John Taylor proposed a simple description of how the Federal Reserve set its rate:

i = r* + π + 0.5 (π − π*) + 0.5 y

with i the policy rate, π inflation, π* the inflation target (2 %), r* the neutral real rate (2 %) and y the **output gap** (how far output is above or below its sustainable level, % of potential GDP). With those numbers it simplifies to

**i = 1 + 1.5 π + 0.5 y.**

The coefficient on inflation, 1.5, is the heart of it: when inflation rises by one point, the bank raises the rate by **more** than one point, so the real interest rate (i − π) rises and cools the economy. This is the **Taylor principle** (b₁ > 1).

A Taylor rule is a **multiple regression** with two predictors (Module 12): Rate = b₀ + b₁·Inflation + b₂·Output gap. Fitted to a bank's past decisions, its coefficients say how strongly the bank reacted to each.

## The Example: an imaginary central bank

Twenty quarters of an imaginary country (illustrative data, made up for this book):

[[data: st.m20.monetary.eg]]

[[results: st.m20.monetary.eg]]

- Rate = 1.00 + **1.50**·Inflation + **0.49**·Output gap. This bank follows Taylor's rule almost exactly: the data were made that way, so the fit shows what the regression recovers when the rule holds.
- The inflation coefficient's 95 % interval is 1.41 to 1.59 (in the coefficient table): clearly above 1, so the bank follows the Taylor principle.
- R² = 0.990. Real central banks fit less tightly: they also react to financial crises, the exchange rate and their forecasts, not only to today's inflation and output gap.
- The two predictors are almost unrelated here (VIF = 1.03, item 12.5), so each coefficient is well determined.

[[figure: st.m20.monetary.eg 1]]

[[see-example: 1]]

## Reading the data a central bank watches

- **Inflation**: headline (all items) and **core** (without food and energy, which jump around); the bank targets a rate over a year or two.
- **Output and jobs**: GDP growth, the unemployment rate, the output gap. The output gap is itself an estimate from a model, and is revised.
- **Expectations**: surveys of what households and markets expect inflation to be.

Every one of these is a statistic with sampling error and revisions. A good analyst asks of each number: how is it measured, how often is it revised, and how big is its uncertainty?

Real data are public: the US Federal Reserve Bank of St. Louis publishes the federal funds rate, the CPI and estimates of the output gap in its free FRED database (fred.stlouisfed.org).

## In Python and R

```python
import statsmodels.formula.api as smf
fit = smf.ols("rate ~ inflation + gap", data=bank).fit()
print(fit.summary())          # b1 > 1: the Taylor principle
```

```r
fit <- lm(rate ~ inflation + gap, data = bank)
summary(fit); confint(fit)
```

[[code: st.m20.monetary.eg]]

## Common mistakes

- **Inflation as the index's level** rather than its percentage change.
- **Reading the regression as a law.** A Taylor rule describes past decisions; a bank can change how it reacts.
- **Forgetting the real rate.** A 5 % policy rate with 6 % inflation is a negative real rate: loose policy, not tight.

## What to do next

On the **Exercise** tab, another imaginary central bank's 24 quarters: fit its Taylor rule and say whether it follows the Taylor principle.

[[example: The Taylor rule: change one quarter]]

The imaginary bank's twenty quarters. It starts at Rate = 1.00 + **1.50**·Inflation + **0.49**·Output gap, R² = **0.990**.

1. **A crisis quarter.** Change the 12th policy rate (9.69) to 2.0, as if the bank cut sharply in a financial crisis though inflation was high. The inflation coefficient falls to **1.14** and R² to **0.59**: one quarter the rule does not describe pulls the whole fit.
2. **Put it back to 9.69**, and change the first quarter's rate (4.28) to 2.0. The fit barely moves (inflation coefficient **1.51**, R² **0.97**): two things differ: quarter 12's change was larger (9.69 to 2.0, against 4.28 to 2.0), and quarter 12 had unusually high inflation, far from the other quarters in x. Both the size of a miss and the point's leverage (item 13.5) decide how much one quarter can move the fit.

[[try: st.m20.bank.try "@Bank/Policy rate" | Equation ChosenEstimate RSquared Influential figure=1]]
