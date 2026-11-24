---
id: st.m20.forecasting
title: Economic forecasting
minutes: 50
objectives: [st.m20.o2]
example: st.m20.forecasting.eg
exercise: st.m20.forecasting.ex
reference: [forecasting-trend-smoothing]
---

# Economic forecasting

Businesses plan on forecasts: next quarter's sales, next year's electricity demand, the number of staff a store will need at Christmas. An economic **time series** is one quantity measured at regular times, and a forecast extends it into the future. Part II will treat time series in full (autocorrelation, ARIMA models). This page uses three tools you can already work with: a **trend line**, a **moving average** and **exponential smoothing**, plus the one thing every forecast needs and many leave out: **how wrong it might be**.

## The parts of a series

Most business series are made of:

- a **trend**: the long-run rise or fall;
- **seasonality**: a pattern that repeats every year (more hardware sold in summer, more heating in winter);
- **noise**: what is left, the part no model predicts.

## The Example: a hardware store's quarterly sales

Sixteen quarters of sales ($ thousand), four years (illustrative data, made up for this book):

[[data: st.m20.forecasting.eg]]

### 1. The trend: a regression on time

A trend line is Module 11's regression with time as x: quarter number 1, 2, 3, … (item 11.2).

[[figure: st.m20.forecasting.eg 1]]

[[results: st.m20.forecasting.eg]]

- ŷ = 209.8 + 6.54 × quarter: sales grow about **$6.5 thousand a quarter**.
- The forecast for quarter 17 is **$320.9 thousand**. The 95 % **prediction interval** for that one quarter is **$272.6 to $369.2 thousand** (item 11.5: one future value needs the prediction interval, not the interval for the line).
- R² = 0.72, and Shapiro–Wilk's p = 0.035. Look at the residuals: quarter 3 of every year is above the line and quarter 4 below. The trend leaves the **seasonality** in the residuals.

### 2. Seasonality: a seasonal index

Divide each quarter's sales by the trend's value for it, and average by quarter: Q1 0.946, Q2 1.016, **Q3 1.104**, Q4 0.934. Summer (Q3) runs about 10 % above trend. Quarter 17 is a first quarter, so the forecast **with the season put back in** is trend × index = 320.9 × 0.946 = **$303.6 thousand**, not 320.9. Going the other way, data ÷ index are **seasonally adjusted** figures: official statistics publish them so this month can be compared with last month without the season in the way.

### 3. A moving average

The average of the last four quarters smooths out the season (one of each quarter is always in it): quarters 13 to 16 give (277.5 + 303.1 + 340.4 + 298.6)/4 = **304.9**. A moving average follows the level of the series and lags behind a trend.

### 4. Exponential smoothing

Each new level is a weighted average of the newest value and the old level:

Lₜ = α yₜ + (1 − α) Lₜ₋₁, with 0 < α < 1.

With α = 0.3, starting at L₁ = 211.4: L₂ = 0.3 × 234.2 + 0.7 × 211.4 = 218.2, and so on to L₁₆ = **299.8**, the forecast for every future quarter. A large α follows recent values closely; a small one smooths more. (Holt and Holt–Winters smoothing add a trend and a season; Part II.)

### 5. Forecast error

A forecast method is judged on the errors it would have made. The **mean absolute error** (MAE) of the one-step-ahead forecasts of the smoothing above, over quarters 2 to 16, is **$22.4 thousand**. Compare methods by their errors on data they did not see (item 13.4's idea of a hold-out set).

[[see-example: 1]]

## In Python and R

```python
import numpy as np
from scipy import stats
fit = stats.linregress(quarter, sales)                 # the trend
# exponential smoothing with alpha = 0.3
level = [sales[0]]
for y in sales[1:]:
    level.append(0.3 * y + 0.7 * level[-1])
```

```r
fit <- lm(sales ~ quarter)
predict(fit, data.frame(quarter = 17), interval = "prediction")
HoltWinters(ts(sales, frequency = 4), beta = FALSE, gamma = FALSE)   # simple exponential smoothing
```

[[code: st.m20.forecasting.eg]]

## Common mistakes

- **A forecast without an interval.** "Sales will be $321 thousand" hides that they could be $273 or $369 thousand.
- **The interval for the line instead of the prediction interval** for one future value.
- **Extrapolating far ahead.** The prediction interval widens with every quarter past the data, and the trend may not last.
- **Ignoring the season** in the residuals, so the forecast is too high in one quarter and too low in the next.

## What to do next

On the **Exercise** tab, a town's electricity sales from 2010 to 2023: fit the trend and forecast 2026, with its prediction interval.

[[example: Forecasting further ahead]]

The store's sixteen quarters with the straight trend. It starts at quarter 17: ŷ = **320.9**, prediction interval **272.6 to 369.2**.

1. **Move Predict at x₀ to 20** (the end of year 5). The forecast rises with the trend and the interval widens: the further from the data, the less sure.
2. **Move it to 30.** Three years past the data. The interval is wider still, and the line now promises growth nobody has checked.
3. **Back to 17**, and change quarter 16's sales (298.6) to 380: one strong quarter pulls the trend up and the forecast with it.

[[try: st.m20.store.try "@Store/Sales ($k)" PredictAt=1..40 | Equation FittedAt NewLower NewUpper RSquared figure=1]]
