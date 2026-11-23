---
id: st.m20.capm
title: Asset pricing
minutes: 50
objectives: [st.m20.o5]
example: st.m20.capm.eg
exercise: st.m20.capm.ex
reference: [capm-beta-portfolio]
---

# Asset pricing

Why does one stock pay a higher return than another, on average? Finance's answer is **risk**: investors are paid for bearing risk they cannot avoid. Asset pricing measures that risk with two tools from Part I: the **variance** of returns (Module 2) and a **regression slope** (Module 11).

## Returns

A stock's **return** over a month is its gain as a share of its price at the start (with dividends): R = (P₁ − P₀ + D)/P₀. Prices are not compared; returns are. The **excess return** is the return minus the **risk-free rate** (a short-term government bill): what the investor earns for taking the risk.

## Diversification: the variance of a portfolio

A portfolio holds several assets. With weights w₁ and w₂ (adding to 1), standard deviations σ₁ and σ₂, and correlation ρ, the portfolio's variance is

σₚ² = w₁²σ₁² + w₂²σ₂² + 2w₁w₂ρσ₁σ₂.

Two assets with σ₁ = 20 % and σ₂ = 10 % a year, half in each:

- if ρ = 1 (they always move together): σₚ = 0.5 × 20 + 0.5 × 10 = **15 %**, the average;
- if ρ = 0.3: σₚ² = 0.25 × 400 + 0.25 × 100 + 2 × 0.25 × 0.3 × 20 × 10 = 155, **σₚ = 12.4 %**.

Less than the average: because the two do not always move together, some of their ups and downs cancel. This is **diversification**. Hold enough assets and the risk special to each company (a failed product, a strike) almost cancels out. What remains is **market risk**, the part all stocks share, and it cannot be diversified away.

## The CAPM: beta as a regression slope

The **capital asset pricing model** (CAPM; Sharpe, 1964) says a stock's expected excess return is proportional to its market risk, measured by **beta**:

E[Rᵢ − R_f] = β (E[R_m − R_f]).

Beta is estimated by regressing the stock's excess return on the market's (Module 11):

(Rᵢ − R_f) = α + β (R_m − R_f) + error

- **β** (the slope): how much the stock moves when the market moves 1 %. β > 1: more volatile than the market (technology stocks); β < 1: less (utilities, food).
- **α** (the intercept): the average return beyond what beta explains. The CAPM says α = 0; a fund manager who earns α > 0 reliably has beaten the market.
- **R²**: the share of the stock's variance that is market risk; 1 − R² is the company's own risk, which diversification removes.

## The Example: a technology stock

Twenty-four months of excess returns (%), the stock and the market index (illustrative data, made up for this book):

[[figure: st.m20.capm.eg 1]]

[[results: st.m20.capm.eg]]

- **β = 1.13** (95 % interval 0.86 to 1.40): the stock moves about 1.13 % for each 1 % of the market.
- **α = 0.48 % a month**, with p = 0.46: not distinguishable from zero. Two years of data cannot show that this stock beats the market.
- **R² = 0.78**: about three quarters of the stock's monthly variance is market risk; the other quarter is its own and could be diversified away.

[[see-example: 1]]

## How finance uses beta

- **Cost of capital.** A company with β = 1.13 must offer investors more expected return than one with β = 0.6. Firms use beta to set the return a new project must clear.
- **Portfolio beta** is the weighted average of the holdings' betas.
- **Caution.** Beta changes over time, depends on the period and the index chosen, and the CAPM is a simple model: other factors (company size, value) also explain returns (Fama and French, 1993).

Real monthly returns are public: Kenneth French's data library (Dartmouth) publishes market and factor returns, and stock prices are on many financial sites.

## In Python and R

```python
from scipy import stats
fit = stats.linregress(market_excess, stock_excess)
print(fit.slope, fit.intercept, fit.rvalue**2)      # beta, alpha, R²
```

```r
fit <- lm(stock_excess ~ market_excess)
coef(fit); confint(fit); summary(fit)$r.squared
```

[[code: st.m20.capm.eg]]

## Common mistakes

- **Regressing the market on the stock.** The stock is y; the market is x. Swapped, the slope is not beta.
- **Raw returns instead of excess returns** (small difference when the risk-free rate is low, but not the model).
- **Reading a large α from a short period** as skill: check its p-value.
- **Adding standard deviations** for a portfolio: variances (with the correlation term) add, standard deviations do not.

## What to do next

On the **Exercise** tab, thirty months of an electric-utility stock: estimate its beta, alpha and R², and say whether it is more or less risky than the market.

[[example: Beta: one market crash]]

The technology stock's 24 months. It starts at **β = 1.131**, **α = 0.478**, R² = **0.776**.

1. **A crash month.** Change the 16th market return (−9.09) to −25 and the stock's (−10.92) to −35. One extreme month pulls the line toward it: beta rises to **1.28** and R² to **0.89**, and that month is named the most influential.
2. **Put them back**, and change the stock's 8th return (0.14), a month when the market barely moved (+0.10 %), to 15: a company announcement, not the market. Beta hardly moves (**1.12**), but α rises to **1.10 %** and R² falls to **0.64**: the stock's own risk grew, not its market risk.

[[try: st.m20.beta.try "@Returns/Market excess (%)" "@Returns/Stock excess (%)" | Equation Slope Intercept InterceptP RSquared Influential figure=1]]
