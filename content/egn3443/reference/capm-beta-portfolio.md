---
title: Asset pricing: portfolios, beta and the CAPM
---

# Asset pricing: portfolios, beta and the CAPM

| Quantity | Formula |
|---|---|
| Return | R = (P₁ − P₀ + D)/P₀ |
| Excess return | R − R_f (R_f the risk-free rate) |
| Portfolio mean | E[Rₚ] = Σ wᵢ E[Rᵢ] |
| Portfolio variance (two assets) | σₚ² = w₁²σ₁² + w₂²σ₂² + 2w₁w₂ρσ₁σ₂ |
| CAPM (Sharpe, 1964) | E[Rᵢ − R_f] = βᵢ E[R_m − R_f] |
| Beta | βᵢ = Cov(Rᵢ, R_m)/Var(R_m): the slope of the regression of Rᵢ − R_f on R_m − R_f (item 11.2) |
| Alpha | the intercept of that regression; CAPM says 0 |
| Systematic share of risk | R² of that regression; 1 − R² is diversifiable |

**Diversification:** with ρ < 1 the portfolio's SD is less than the weighted average of the SDs; company-specific risk
cancels across many holdings, market risk does not.

**Real data:** Kenneth R. French's data library (mba.tuck.dartmouth.edu/pages/faculty/ken.french/data_library.html) for market
and factor returns.

## Python and R

| Task | Python | R |
|---|---|---|
| Beta and alpha | `stats.linregress(market_excess, stock_excess)` | `lm(stock_excess ~ market_excess)` |
| Portfolio SD | `np.sqrt(w @ cov @ w)` | `sqrt(t(w) %*% cov %*% w)` |

**Read more:** Bodie, Kane and Marcus, *Investments* (McGraw-Hill), the chapters on diversification and the CAPM;
Fama and French (1993), "Common risk factors in the returns on stocks and bonds", *Journal of Financial Economics* 33, 3–56.
