---
id: st.m20.econometrics
title: Econometrics
minutes: 50
objectives: [st.m20.o1]
example: st.m20.econometrics.eg
exercise: st.m20.econometrics.ex
reference: [econometrics-elasticity]
---

# Econometrics

**Econometrics** is regression applied to economic data: estimating how much one economic quantity moves with another, from data the economist did not get to control. The method is Module 11's least-squares line and Module 12's multiple regression. What is new is the question, the units, and two problems economic data bring with them: the effects people want are usually **percentages**, and the data almost always come from **observation**, not experiment.

## The question: how much does demand fall when the price rises?

A café chain wants to know what happens to sales if it raises the price of a large coffee. The economist's number for this is the **price elasticity of demand**:

> elasticity = (% change in quantity) / (% change in price)

An elasticity of −1.3 means a 1 % price rise loses about 1.3 % of the cups sold. Demand elasticities are negative (higher price, fewer sales). The size decides what happens to revenue:

| Elasticity | Called | A price rise … |
|---|---|---|
| between 0 and −1 | inelastic | loses fewer sales than it gains in price: revenue **rises** |
| −1 | unit elastic | leaves revenue unchanged |
| below −1 | elastic | loses more sales than it gains: revenue **falls** |

## The log–log model

An elasticity is a ratio of **percentage** changes, and a straight line measures **unit** changes. Taking logarithms of both variables turns one into the other:

ln(quantity) = b₀ + b₁ ln(price)

A small change in ln(x) is a proportional change in x, so the slope b₁ is the elasticity directly. This is Module 11's regression with both variables transformed (item 13.6 introduced transformations); nothing else changes. The curve q = e^b₀ · p^b₁ is the demand curve.

## The Example: coffee at twelve prices

The café chain set twelve prices, one a week, from $2.50 to $5.25, and counted the cups sold (illustrative data, made up for this book).

[[data: st.m20.econometrics.eg]]

[[figure: st.m20.econometrics.eg 1]]

[[results: st.m20.econometrics.eg]]

- ln(cups) = 9.128 − 1.322 ln(price). **The price elasticity is −1.32**, with a 95 % interval from −1.43 to −1.22 (item 11.5 for the interval).
- R² = 0.987: price explains nearly all the week-to-week change in sales here. The chain varied only the price, so there is little else to explain.
- A 10 % price rise: cups × 1.10^(−1.322) = 0.882, about **11.8 % fewer cups**. Revenue (price × cups) is × 1.10 × 0.882 = 0.970, **3 % lower**. Demand is elastic, so a price rise does not pay.

[[see-example: 1]]

## Observational data and omitted variables

The café could set its prices, which is an experiment. Most economic data are not: prices, incomes and quantities are **recorded** as the economy produces them. Then a variable left out of the regression can bias the slope. This is **omitted-variable bias**.

Suppose regions with high gasoline prices also have good public transport. People there buy less gasoline because of the price *and* because they can take the train. A regression of gallons on price alone gives the price the credit for both, and the elasticity looks larger than it is. The remedy is Module 12's: put the other variable in the model (item 12.3 reads a coefficient "with the other held fixed"). An economist asks of every regression: **what else moves with x, and affects y?**

Two other habits of econometrics:

- **Units and logs.** A log–log slope is an elasticity; a log–linear slope (only y logged) is the proportional change in y per unit of x (× 100 for a percentage).
- **Correlation is not policy.** A slope from observational data says what goes together. Whether changing x *causes* y to change needs an experiment, or an argument that nothing omitted moves with x.

## In Python and R

```python
import numpy as np, statsmodels.api as sm
X = sm.add_constant(np.log(price))
fit = sm.OLS(np.log(cups), X).fit()
print(fit.params, fit.conf_int(), fit.rsquared)   # the slope is the elasticity
```

```r
fit <- lm(log(cups) ~ log(price))
coef(fit); confint(fit); summary(fit)$r.squared
```

[[code: st.m20.econometrics.eg]]

## Common mistakes

- **Reading the slope of the untransformed line as an elasticity.** The slope of cups on price is cups per dollar, not a percentage.
- **Forgetting the sign.** "An elasticity of 1.3" for demand means −1.3.
- **Logging only one variable** and calling the slope an elasticity.
- **Ignoring omitted variables** in observational data: income, the season, a competitor's price.

## What to do next

On the **Exercise** tab, fourteen regions' gasoline prices and the gallons bought per person: fit the log–log model, and say whether gasoline demand is elastic or inelastic.

[[example: Coffee demand: logs or no logs]]

This is the Learn page's coffee data. It starts with both variables logged: the slope is the elasticity, **−1.322**, and R² = **0.987**.

1. **Set Transform y to None** (x still logged). The slope is now cups per unit of ln(price): not an elasticity, and the curve fits less well.
2. **Set Transform x to None too.** The plain line of cups on price has a slope of cups per dollar. It is a fair summary in the middle, but the scatter plot shows the bend the log–log model takes in: demand falls fast at low prices and slowly at high ones.
3. **Put both back to Log**, and change the last week's cups (1070) to 1500. The elasticity moves toward zero and R² drops: one unusual week (a holiday, a broken machine) shifts the estimate. Look at the residual plots before you trust it.

[[try: st.m20.coffee.try "@Coffee/Cups sold" XTransformation YTransformation | Equation Slope SlopeLower SlopeUpper RSquared figure=1]]
