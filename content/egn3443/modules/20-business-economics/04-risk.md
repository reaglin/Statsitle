---
id: st.m20.risk
title: Risk and uncertainty
minutes: 45
objectives: [st.m20.o4]
example: st.m20.risk.eg
exercise: st.m20.risk.ex
reference: [risk-expected-value-var]
---

# Risk and uncertainty

Every business decision is made before its outcome is known. A new product may sell well or badly; an investment may gain or lose. Business and finance describe such decisions with a probability distribution of the outcome (Modules 4 and 5) and three numbers from it: the **expected value**, the **risk**, and the **loss that is unlikely to be exceeded**.

## Expected value: the long-run average

The planners of a new product line give five scenarios for next year's profit, with their probabilities (illustrative, made up for this book):

| Profit ($k) | −400 | −100 | 200 | 500 | 900 |
|---|---|---|---|---|---|
| Probability | 0.10 | 0.15 | 0.35 | 0.30 | 0.10 |

The **expected profit** is the mean of this distribution (item 4.3):

E[X] = Σ x·p(x) = (−400)(0.10) + (−100)(0.15) + 200(0.35) + 500(0.30) + 900(0.10) = −40 − 15 + 70 + 150 + 90 = **$255 thousand**.

It is what the line would earn on average if the year could be repeated many times. No single year earns exactly $255 thousand.

## Risk: the standard deviation

Two projects with the same expected profit are not equally attractive if one can lose far more. Finance measures **risk** by the standard deviation of the outcome:

Var(X) = Σ (x − E[X])²·p(x) = 122,475 ($k)², SD = **$350 thousand**.

The chance of a loss is P(X ≤ 0) = 0.10 + 0.15 = **0.25**: one year in four.

[[figure: st.m20.risk.eg 1]]

[[results: st.m20.risk.eg]]

A **risk-averse** decision maker accepts a lower expected value for a smaller standard deviation. Comparing projects by expected value per unit of risk (E/SD) is the idea behind the Sharpe ratio in finance.

## Scenarios

The five numbers above are **scenarios**: what the planners think could happen, with probabilities they judged. They are only as good as that judgement. A careful analysis asks how the decision changes if the probabilities change (a **sensitivity analysis**), and adds a "stress" scenario that is unlikely but very bad.

## Value at Risk

Banks and investment funds summarise the bad end of a distribution with the **Value at Risk** (VaR): the loss that will not be exceeded with a stated probability over a stated time.

> The one-day 95 % VaR is $41 thousand: on 95 days in 100 the portfolio loses less than $41 thousand; on 5 days in 100 it loses more.

If the day's change in value is modelled as normal (item 5.3) with mean μ and standard deviation σ, the 95 % VaR is the 5 % quantile turned into a loss: VaR = −(μ − 1.645 σ). For μ = $0.2 thousand and σ = $25 thousand, the 5 % quantile is −$40.9 thousand, so **VaR = $40.9 thousand**. The Example tab moves μ and σ.

VaR's weaknesses are statistical: daily returns have **heavier tails** than the normal, so the normal VaR understates the chance of a big loss; and VaR says nothing about **how much** is lost on the bad 5 % of days. The **expected shortfall** (the average loss beyond the VaR) answers that, and banking rules now ask for it.

[[see-example: 1]]

## In Python and R

```python
import numpy as np
from scipy import stats
x = np.array([-400, -100, 200, 500, 900]); p = np.array([0.10, 0.15, 0.35, 0.30, 0.10])
ev = (x * p).sum(); sd = np.sqrt(((x - ev)**2 * p).sum())
var95 = -stats.norm.ppf(0.05, loc=0.2, scale=25)      # one-day 95 % VaR
```

```r
x <- c(-400, -100, 200, 500, 900); p <- c(0.10, 0.15, 0.35, 0.30, 0.10)
ev <- sum(x * p); sd <- sqrt(sum((x - ev)^2 * p))
-qnorm(0.05, mean = 0.2, sd = 25)                     # one-day 95 % VaR
```

[[code: st.m20.risk.eg]]

## Common mistakes

- **Expecting the expected value.** It is a long-run average; this year's outcome is one of the scenarios.
- **The variance as the risk.** It is in squared dollars; the standard deviation is in dollars.
- **VaR as the worst case.** It is a quantile: 5 % of days are worse, and it does not say by how much.
- **Probabilities that do not add up to 1.**

## What to do next

On the **Exercise** tab, a plan to automate a warehouse: its expected gain, its risk, and the chance it loses money.

[[example: Value at Risk: the mean, the spread and the tail]]

A portfolio's one-day change in value, modelled as normal with mean **$0.2 thousand** and standard deviation **$25 thousand**. The 5 % quantile is **−$40.9 thousand**: the one-day 95 % VaR is $40.9 thousand.

1. **Double the risk:** move σ to 50. The VaR roughly doubles: VaR grows in proportion to σ.
2. **Back to 25, and set the probability to 0.01**: the 99 % VaR, the 1 % quantile, is a larger loss (about $58 thousand).
3. **Move μ to 5.** A positive expected gain shifts the whole curve right, and the VaR falls a little: over one day, the spread matters far more than the mean.

[[try: st.m20.var.try Mu=-10..10 Sigma=5..60 Probability | Quantile AtMost ModelDescription figure=1]]
