---
id: st.m11.applying
title: Applying regression to real data
minutes: 45
objectives: [st.m11.o5, st.m11.o4]
builds-on: st.m11.interpreting
example: st.m11.applying.eg
exercise: st.m11.applying.ex
reference: [reading-regression-output, regression-fit-and-residuals]
---

# Applying regression to real data

Items 11.1 to 11.5 each took one step. Real data need all of them, in order, and a result someone can use. This item works through a published data set, from the question to a paragraph a manager can read.

## The steps

1. **The question.** What is to be predicted or explained (y), from what (x), and for whom? "How much fuel economy does each extra 1,000 lb of car weight cost?"
2. **The data.** Where did they come from, how were they measured, in what units, over what range? Was x **set** in an experiment, or only **recorded**?
3. **Plot.** The scatter plot first: direction, form, strength, unusual points (item 11.1).
4. **Fit.** The least-squares line (item 11.2).
5. **Check.** The residual plot and the Q–Q plot (item 11.4). If a check fails, fix the model or say what it limits.
6. **Interpret.** The slope with its confidence interval, R², σ̂, the test, a prediction with its prediction interval (items 11.4 and 11.5).
7. **Communicate.** A few plain sentences: what was found, how sure it is, what to do, and the limits.

## The Example: car weight and fuel economy

In 1973–74 *Motor Trend* magazine road-tested 32 cars. Their data, known as **mtcars**, come with R. The table has fuel economy (miles per US gallon), weight (thousands of pounds) and engine power (horsepower).

**1–2. Question and data.** How does fuel economy change with weight? The weights run from 1,513 to 5,424 lb. Weight was recorded, not set: these are observational data.

[[data: st.m11.applying.eg]]

**3. Plot.**

[[figure: st.m11.applying.eg 1]]

A clear falling relation, roughly straight, with no wild points.

**4. Fit, and 6. interpret.**

[[results: st.m11.applying.eg]]

- ŷ = 37.29 − 5.344 × weight: each 1,000 lb more, about 5.3 mpg less. The 95 % interval for the slope is −6.49 to −4.20 mpg per 1,000 lb.
- t = −9.56 on 30 df, p < 0.0001 (the same as the F-test, F = 91.4): the relation is real.
- R² = 0.753: weight explains about three quarters of the variation in fuel economy.
- σ̂ = 3.05 mpg is the typical miss; about 95 % of cars are within ±6 mpg of the line (±2σ̂).
- For a 3,000 lb car (x₀ = 3.0): ŷ₀ = 21.3 mpg, and one such car is expected to get between 14.9 and 27.6 mpg (95 % prediction interval).

**5. Check.**

[[figure: st.m11.applying.eg 2]]

[[figure: st.m11.applying.eg 3]]

Most residuals are within ±4 mpg, and the Q–Q plot is close to its line (Shapiro–Wilk p = 0.104). There is a slight bend, though. The lightest and the heaviest cars mostly have positive residuals, and the cars in the middle mostly negative ones. A gentle curve would fit a little better (item 13.6 shows how to try one, with **Transform x**). The straight line is still a fair summary over this range.

**7. Communicate.** For a manager or a car buyer, without the jargon:

> Heavier cars use more fuel. Among 32 cars tested by *Motor Trend* in 1973–74, each extra 1,000 lb of weight cost about 5.3 miles per gallon (likely between 4.2 and 6.5). Weight alone accounts for about three quarters of the differences in fuel economy between the cars. A typical 3,000 lb car got about 21 mpg, but a single car of that weight could get anywhere from about 15 to 28 mpg. These are 1970s cars from 1,500 to 5,400 lb. The figures should not be applied to modern cars, or to weights outside that range. Heavier cars also had bigger engines, so part of this effect may be engine size rather than weight itself.

What makes it work for a non-technical reader:

- **lead with the finding**, in everyday words ("heavier cars use more fuel");
- give the **size in the reader's units**, with a plain range for the uncertainty ("likely between …") rather than "95 % CI" or a p-value;
- put R² as **"accounts for about three quarters of the differences"**;
- give a **prediction with its spread** for one unit, not just the line's value;
- state **the limits**: the range of the data, when and where they came from, and what was not controlled;
- **no causal claim from observational data**. Weight and engine power go together in these cars (r = 0.66), so "weight causes …" would overstate it. Module 12 shows how a multiple regression separates two predictors that move together (item 12.1).

## Working in the Workbench with a data table

Real data usually come as a table with more columns than one regression uses. In the Workbench: press **Copy the data**, press Ctrl+B, click **+ Data table** and paste (Ctrl+V). The first row becomes the column names. Choose **Simple linear regression** in the list on the left, and set **x column** and **y column** to the columns you want. Change **x column** to try another predictor; the line, the results and the plots update at once. Set **Predict at x₀, optional** for a prediction with its intervals. The **Code** tab writes the Python and R for exactly what is on the screen. Ctrl+L brings you back.

## In Python and R

```python
import statsmodels.formula.api as smf
from statsmodels.datasets import get_rdataset
cars = get_rdataset("mtcars").data        # downloads R's data set
fit = smf.ols("mpg ~ wt", data=cars).fit()
print(fit.summary())
print(fit.get_prediction({"wt": [3.0]}).summary_frame(alpha=0.05))
```

```r
fit <- lm(mpg ~ wt, data = mtcars)     # mtcars is built into R
summary(fit)
confint(fit)
predict(fit, data.frame(wt = 3.0), interval = "prediction")   # 14.93 to 27.57
plot(fit, which = 1:2)                 # the residual checks
```

[[code: st.m11.applying.eg]]

## Common mistakes

- **Skipping the plot or the residual check** and reporting only the numbers.
- **The wrong column as x.** With several columns, check that x is the predictor your question names, and y the response.
- **Jargon for a non-technical reader.** "p < 0.0001, R² = 0.753" tells a manager nothing. Say what changes, by how much, how sure, and within what limits.
- **Claiming cause from observational data**, or leaving out a variable that moves with x.
- **Leaving out the range and the source** of the data.

## What to do next

On the **Exercise** tab, a forester's question: regress the timber volume of 31 black cherry trees on their diameter, and predict the volume of one 15-inch tree. Look at the residual plot too: it is not a shapeless band. Then write the two or three sentences you would send the forester, including what the residual plot means for the prediction (they are not graded, but item 11.7 asks about such a summary).
