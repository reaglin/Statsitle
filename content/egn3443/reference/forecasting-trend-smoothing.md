---
title: Forecasting: trend, moving averages, exponential smoothing
---

# Forecasting: trend, moving averages, exponential smoothing

A **time series** is one quantity at regular times. It is usually trend + season + noise. Full time-series models (ARIMA,
autocorrelation) are Part II; these tools need only Part I.

| Tool | Formula | Notes |
|---|---|---|
| Linear trend | ŷₜ = b₀ + b₁ t (regression on time, item 11.2) | forecast ŷ at t₀ with its **prediction interval** (item 11.5) |
| Seasonal index | average of yₜ / trendₜ for each season | adjusted forecast = trend forecast × index |
| Moving average (k periods) | (yₜ + yₜ₋₁ + … + yₜ₋ₖ₊₁)/k | k = 4 for quarters or 12 for months removes the season |
| Simple exponential smoothing | Lₜ = α yₜ + (1 − α) Lₜ₋₁ | forecast for every future period = Lₜ; 0 < α < 1 |
| Mean absolute error | MAE = (1/n) Σ \|yₜ − ŷₜ\| | compare methods on data they did not see |
| Root mean squared error | RMSE = √((1/n) Σ (yₜ − ŷₜ)²) | penalises large misses more |

**A forecast is a value and an interval.** The prediction interval widens the further t₀ is from the data.

## Python and R

| Task | Python | R |
|---|---|---|
| Trend and prediction interval | `smf.ols("y ~ t", d).fit().get_prediction({"t": [17]}).summary_frame()` | `predict(lm(y ~ t), data.frame(t = 17), interval = "prediction")` |
| Moving average | `pd.Series(y).rolling(4).mean()` | `stats::filter(y, rep(1/4, 4), sides = 1)` |
| Exponential smoothing | `statsmodels.tsa.holtwinters.SimpleExpSmoothing(y).fit(smoothing_level=0.3)` | `HoltWinters(ts(y), alpha = 0.3, beta = FALSE, gamma = FALSE)` |

**Read more:** Hyndman and Athanasopoulos, *Forecasting: Principles and Practice*, 3rd ed. (free at otexts.com/fpp3),
chapters 3 (decomposition), 5 (forecast errors) and 8 (exponential smoothing).
